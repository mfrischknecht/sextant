namespace Sextant

open System
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

    type Entry private (severity: Severity, shortText:Line, additionalText:string option, source:Error.Error option) =
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
        static member ofError (error:Error.Error) =
            let text = error.Message
            let lines = text |> Lines.split 
            let head = lines |> Seq.head
            let tail = 
                lines |> Seq.tail 
                |> stripEmptyHeader 
                |> Option.nonEmptySeq 
                |> Option.map Lines.concat

            Entry(Error, head, tail, error |> Some)

        [<SuppressMessage("NameConventions","*")>]
        static member ofText severity text =
            let lines = text |> Line.sequence
            let headline = lines |> Seq.head
            let text = 
                lines |> Seq.tail 
                |> Option.nonEmptySeq
                |> Option.map Lines.concat

            Entry(severity, headline, text, None)

    type LogWindow() as this =
        inherit Window()

        static let windowsMonitor = Object ()
        static let mutable windows = []

        static let severityColors = 
            [ (Error,   "#FFFF6666")
              (Warning, "#FFFFE866") 
              (Info,    "#00000000") ] 
            |> Seq.map (fun (severity,color) -> 
                (severity, color |> Color.parse |> Result.unwrap "Color parsing failed" |> SolidColorBrush))
            |> Map

        let itemsMonitor = Object ()

        let controls = 
            ItemsControl (
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment   = VerticalAlignment  .Stretch)

        let scrollview = 
            ScrollViewer (
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment   = VerticalAlignment  .Stretch,
                Content             = controls)
                
        let mutable entryControls = [] |> Map

        do
            this.AddChild(scrollview)

            lock windowsMonitor (fun _ ->
                windows <- windows |> List.append [this])

        override this.OnClosed e =
            lock windowsMonitor (fun _ ->
                windows <- windows |> List.filter (fun w -> w <> this))
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

                let border = 
                    Border (
                        BorderBrush     = System.Windows.Media.Brushes.Black,
                        BorderThickness = (1.0 |> Thickness),
                        CornerRadius    = (0.0 |> CornerRadius),
                        Background      = severityColors.[entry.Severity])

                match entry.AdditionalText with
                | None -> 
                    border.Padding <- Thickness(25.0,0.0,0.0,0.0)
                    border.Child   <- firstLine

                | Some text ->
                    let content = 
                        Label (
                            Background = severityColors.[entry.Severity],
                            Margin     = (Thickness(25.0,0.0,0.0,0.0)),
                            Content    = (entry.AdditionalText |> Option.defaultValue ""))

                    let expander = 
                        Expander (
                            HorizontalAlignment = HorizontalAlignment.Stretch,
                            Background          = severityColors.[entry.Severity],
                            IsExpanded          = false,
                            Header              = firstLine,
                            Content             = content)

                    //Hack: Automatically adjust the grid size according to the expander's width
                    ColumnDefinition (Width = GridLength (25.0)) |> firstLine.ColumnDefinitions.Add

                    firstLine.SetBinding (
                        Grid.WidthProperty,
                        Binding (Source=expander, Path=PropertyPath("ActualWidth"), Mode=BindingMode.OneWay))
                        |> ignore;

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

    let info    = Entry.ofText Severity.Info
    let warning = Entry.ofText Severity.Warning
    let error   = Entry.ofText Severity.Error

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