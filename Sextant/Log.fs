namespace Sextant

open System
open System.Collections.Immutable
open System.Diagnostics.CodeAnalysis
open System.Runtime.CompilerServices
open System.Text
open System.Windows
open System.Windows.Controls
open System.Windows.Data
open System.Windows.Media

open Microsoft.FSharp.Reflection

open Sextant.NativeErrors
open Sextant.Text

module Log =
    type Severity =
        | Info
        | Warning
        | Error

    type Source =
        | NativeError of NativeError
        | Exception   of Exception

    type Entry private (shortText:Line, severity: Severity, additionalText:string option, source:Source option) =
        let timestamp      = DateTime.Now

        member this.Timestamp = timestamp
        member this.Severity  = severity
        member this.ShortText = shortText
        member this.AdditionalText = additionalText
        member this.Source = source

        override this.GetHashCode () = RuntimeHelpers.GetHashCode(this)
        override this.Equals other = Object.ReferenceEquals(this,other)

        interface IEquatable<Entry> with
            member this.Equals other = Object.ReferenceEquals(this,other)

        interface IComparable<Entry> with
            member this.CompareTo other = this.Timestamp.CompareTo other.Timestamp

        interface IComparable with
            member this.CompareTo obj = 
                match obj with
                | :? Entry as other -> this.Timestamp.CompareTo other.Timestamp
                | _ -> invalidArg "obj" "Cannot compare with different types"

        [<SuppressMessage("NameConventions","*")>]
        static member ofException (error:Exception) =
            let text = 
                error 
                |> Exception.chain
                |> Seq.map (fun e -> 
                    sprintf "%s: %s\r\n%s" (e.GetType().Name) e.Message (e.StackTrace.ToString()))
                |> Seq.appendTo [ error.Message; "Stack trace:" ]
                |> String.concat "\r\n\r\n"

            let lines = text |> Lines.split 
            let head = lines |> Seq.head
            let tail = 
                lines |> Seq.tail 
                |> stripEmptyHeader 
                |> Option.nonEmptySeq 
                |> Option.map Lines.concat

            Entry(head, Error, tail, error |> Source.Exception |> Some)

        [<SuppressMessage("NameConventions","*")>]
        static member ofNativeError (error:NativeError) =
            let info = 
                error.Annotations @ [ error.ErrorText ]
                |> Seq.map Line.sequence
                |> Array.concat

            let shortText = info |> Seq.head
            let text = 
                info |> Seq.tail 
                |> Option.nonEmptySeq
                |> Option.map Lines.concat

            Entry(shortText, Error, text, error |> Source.NativeError |> Some)

        [<SuppressMessage("NameConventions","*")>]
        static member simple severity text =
            let lines = text |> Line.sequence
            let headline = lines |> Seq.head
            let text = 
                lines |> Seq.tail 
                |> Option.nonEmptySeq
                |> Option.map Lines.concat

            Entry(headline, severity, text, None)

    type LogWindow() as this =
        inherit Window()

        static let windowsMonitor = Object ()
        static let mutable windows = ImmutableArray.Create ()
        static let severityColors = 
            [ (Error,   "#FFFF6666")
              (Warning, "#FFFFE866") 
              (Info,    "#00000000") ] 
            |> Seq.map (fun (severity,color) -> 
                (severity, ColorConverter.ConvertFromString color :?> Color |> SolidColorBrush))
            |> Map

        let scrollview = ScrollViewer ()
        let itemsMonitor = Object ()
        let controls = ItemsControl ()
                
        let mutable entryControls = [] |> Map

        do
            scrollview.HorizontalAlignment <- HorizontalAlignment.Stretch
            scrollview.VerticalAlignment   <- VerticalAlignment  .Stretch
            this.AddChild(scrollview)

            controls.HorizontalAlignment <- HorizontalAlignment.Stretch
            controls.VerticalAlignment   <- VerticalAlignment  .Stretch
            scrollview.Content <- controls

            lock windowsMonitor (fun _ ->
                windows <- windows.Add this)

        override this.OnClosed e =
            lock windowsMonitor (fun _ ->
                windows <- windows.Remove this)
            base.OnClosed e

        member this.Add (entry:Entry) =
            lock itemsMonitor (fun _ ->
                let firstLine =
                    let grid = Grid (HorizontalAlignment = HorizontalAlignment.Stretch)
                    RowDefinition () |> grid.RowDefinitions.Add
                    ColumnDefinition () |> grid.ColumnDefinitions.Add
                    ColumnDefinition (Width = GridLength (60.0)) |> grid.ColumnDefinitions.Add

                    let headline = Label ()
                    headline.Content <- entry.ShortText.Text
                    headline |> grid.Children.Add |> ignore

                    let timestamp = Label ()
                    timestamp.Content <- entry.Timestamp.ToString("HH:mm:ss")
                    timestamp |> grid.Children.Add |> ignore
                    Grid.SetColumn (timestamp,1)

                    grid

                let border = Border ()
                border.BorderBrush <- System.Windows.Media.Brushes.Black
                border.BorderThickness <- Thickness (1.0)
                border.CornerRadius <- CornerRadius (0.0)
                border.Background <- severityColors.[entry.Severity]

                match entry.AdditionalText with
                | None -> 
                    border.Padding <- Thickness(25.0,0.0,0.0,0.0)
                    border.Child <- firstLine

                | Some text ->
                    let content = Label ()
                    content.Background <- severityColors.[entry.Severity]
                    content.Margin <- Thickness(25.0,0.0,0.0,0.0)
                    content.Content <- entry.AdditionalText |> Option.defaultValue ""

                    let expander = Expander (HorizontalAlignment = HorizontalAlignment.Stretch)
                    expander.Background <- severityColors.[entry.Severity]
                    expander.IsExpanded <- false
                    expander.Content <- content

                    //Hack: Automatically adjust the grid size according to the expander's width
                    ColumnDefinition (Width = GridLength (25.0)) 
                    |> firstLine.ColumnDefinitions.Add

                    firstLine.SetBinding (
                        Grid.WidthProperty,
                        Binding (Source=expander, Path=PropertyPath("ActualWidth"), Mode=BindingMode.OneWay))
                        |> ignore;

                    expander.Header <- firstLine

                    border.Child <- expander

                controls.Items.Add border |> ignore
                entryControls <- entryControls.Add (entry, border))

        member this.Remove entry =
            lock itemsMonitor (fun _ ->
                let control = entryControls.[entry]
                controls.Items.Remove (control)
                entryControls = entryControls.Remove entry )

        [<SuppressMessage("NameConventions","*")>]
        static member internal log entry =
            let windows = lock windowsMonitor (fun _ -> windows)
            for window in windows do window.Add entry

    let info    = Entry.simple Severity.Info
    let warning = Entry.simple Severity.Warning
    let error   = Entry.simple Severity.Error

    let logToWriter (writer:IO.TextWriter) (entry:Entry) =
        let severity = entry.Severity
        let severity = 
            FSharpValue.GetUnionFields(severity, typeof<Severity>)
            |> function
            | case, _ -> case.Name

        let severity = severity.[0].ToString()
        let timestamp = entry.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.ffff")

        let firstLine = 
            entry.ShortText
            |> Line.map (sprintf "[%s] %s: %s" severity timestamp) 
            |> Result.unwrap "Line mapping contains newlines"

        let additionalLines = 
            entry.AdditionalText 
            |> Option.map indent
            |> Option.map Lines.split
            |> Option.defaultValue [||]

        firstLine
        |> Seq.prependElementTo additionalLines 
        |> Lines.concat 
        |> writer.WriteLine

    let logToStream (encoding:Encoding) (stream:IO.Stream) (entry:Entry) =
        use writer = new IO.StreamWriter(stream,encoding)
        logToWriter writer entry

    let logToConsole entry = 
        entry |> logToWriter Console.Error

    let log entry =
        entry |> LogWindow.log
        entry |> logToConsole