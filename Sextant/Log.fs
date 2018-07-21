namespace Sextant

open System
open System.Collections.Immutable
open System.Drawing
open System.Runtime.CompilerServices
open System.Text.RegularExpressions
open System.Threading
open System.Windows
open System.Windows.Controls
open System.Windows.Media

open Microsoft.FSharp.Reflection

open Sextant.NativeErrors

module Log =
    type Severity =
        | Info
        | Warning
        | Error

    type Source =
        | NativeError of NativeError

    type Entry(shortText:string, ?severity: Severity, ?additionalText:string option, ?source:Source option) =
        let severity       = defaultArg severity       Info
        let additionalText = defaultArg additionalText None
        let source         = defaultArg source         None
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

        static member ofNativeError (error:NativeError) =
            let info = error.Annotations @ [ error.ErrorText ]
            let shortText = info |> Seq.head
            let text = info |> String.concat "\r\n"
            Entry(shortText, Error, text |> Option.Some, NativeError error |> Option.Some)

    type LogWindow() as this =
        inherit Window()

        static let windowsMonitor = Object ()
        static let mutable windows = ImmutableArray.Create ()

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
                let headline = Label ()
                headline.Content <- entry.ShortText

                let bg =
                    match entry.Severity with
                    | Error   -> System.Windows.Media.Brushes.Red
                    | Warning -> System.Windows.Media.Brushes.Yellow
                    | Info    -> System.Windows.Media.Brushes.LightBlue

                let border = Border ()
                border.BorderBrush <- System.Windows.Media.Brushes.Black
                border.BorderThickness <- Thickness (1.0)
                border.CornerRadius <- CornerRadius (0.0)
                border.Background <- bg

                match entry.AdditionalText with
                | None -> 
                    border.Padding <- Thickness(25.0,0.0,0.0,0.0)
                    border.Child <- headline

                | Some text ->
                    let content = Label ()
                    content.Background <- bg
                    content.Margin <- Thickness(25.0,0.0,0.0,0.0)
                    content.Content <- entry.AdditionalText |> Option.defaultValue ""

                    let expander = Expander ()
                    expander.Background <- bg
                    expander.Header <- headline
                    expander.IsExpanded <- false
                    expander.Content <- content

                    border.Child <- expander

                controls.Items.Add border |> ignore
                entryControls <- entryControls.Add (entry, border))

        member this.Remove entry =
            lock itemsMonitor (fun _ ->
                let control = entryControls.[entry]
                controls.Items.Remove (control)
                entryControls = entryControls.Remove entry )

        static member internal log entry =
            let windows = lock windowsMonitor (fun _ -> windows)
            for window in windows do window.Add entry

    let private lineEnding = Regex ("\r?\n")
    let private simpleMessage severity text =
        let lines = lineEnding.Split(text)
        let headline = lines |> Array.head
        let lines = lines |> Array.tail
        let text = lines |> function
            | [| |] -> None
            | _ -> lines |> String.concat "\r\n" |> Some

        Entry(headline, severity, text, None)

    let info = simpleMessage Severity.Info
    let warning = simpleMessage Severity.Warning
    let error = simpleMessage Severity.Error

    let logToStream (stream:IO.TextWriter) (entry:Entry) =
        let severity = entry.Severity
        let severity = 
            FSharpValue.GetUnionFields(severity, typeof<Severity>)
            |> function
            | case, _ -> case.Name

        let severity = severity.[0].ToString()

        let timestamp = entry.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.ffff")

        let text = 
            [ entry.ShortText; entry.AdditionalText |> Option.defaultValue "" ] 
            |> Seq.filter (fun s -> s.Length > 0)
            |> String.concat "\r\n"

        sprintf "[%s] %s: %s" severity timestamp text |> stream.WriteLine

    let logToConsole entry = 
        entry |> logToStream Console.Error

    let log entry =
        entry |> LogWindow.log
        entry |> logToConsole