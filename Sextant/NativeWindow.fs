namespace Sextant

open System
open System.Text
open System.Diagnostics
open System.Diagnostics.CodeAnalysis
open System.Drawing
open System.Runtime.InteropServices

open Sextant
open Sextant.Monitor
open Sextant.NativeErrors
open Sextant.NativeAPI
open Sextant.Rectangle

module NativeWindow =
    let private rectangle (rect:Windows.Rectangle) =
        { Left   = rect.Left   |> float
          Top    = rect.Top    |> float
          Right  = rect.Right  |> float
          Bottom = rect.Bottom |> float }

    type private Index = | GCLP of Windows.GCLPIndex | SM of Windows.SMIndex

    let private iconFromHandle handle = 
        Icon.FromHandle handle

    let private bitmapFromIcon (icon:Icon) =
        let bitmap = new System.Drawing.Bitmap(icon.Size.Width, icon.Size.Height)
        use graphics = Graphics.FromImage (bitmap)
        graphics.DrawIcon (icon, 0, 0)
        bitmap

    let private currentProcess = Process.GetCurrentProcess ()

    type private ThreadAction = Attach | Detach
    let private syncThreadInput action (windowProcessId, windowThreadId)  =
        let currentThreadId = Threads.GetCurrentThreadId ()
        let currentProcessId = uint32 currentProcess.Id
        if windowThreadId = currentThreadId && windowProcessId = currentProcessId then Ok ()
        else
            let flag = match action with | Attach -> true | Detach -> false
            match Windows.AttachThreadInput (currentThreadId, windowThreadId, flag) with
            | true  -> Ok ()
            | false -> Error (NativeError.Last |> annotate "Failed to synchronize with window thread")

    [<Struct>]
    type Window  = 
        { Handle: IntPtr }

          [<SuppressMessage("NameConventions","*")>]
          static member fromHandle handle = 
            { Window.Handle = handle }

          [<SuppressMessage("NameConventions","*")>]
          static member fromWPF window =
            { Window.Handle = window |> WPF.handle }

          member this.GetProcessAndThreadIds () =
              let mutable processId = 0u
              let threadId = Windows.GetWindowThreadProcessId (this.Handle, &processId)
              match threadId with
              | 0u -> Error (NativeError.Last |> annotate "Failed to get thread id")
              | _  -> Ok (processId, threadId)

          member this.GetWindowTitle () =
              let handle = this.Handle
              let length = Math.Max ((Windows.GetWindowTextLength handle), 1024)
              let builder = new StringBuilder (length)
              Windows.GetWindowText (handle, builder, length) |> ignore
              builder.ToString()

          member this.Descriptor =
              let pid = 
                this.GetProcessAndThreadIds() 
                |> Option.ofResult
                |> Option.map fst

              let processName =
                pid
                |> Option.bind (int >> Process.getById >> Option.ofResult)
                |> Option.bind (Process.name >> Option.nonEmptyString)

              let title = 
                  this.GetWindowTitle()
                  |> Option.nonEmptyString

              pid, processName, title

          member this.Description =
                let pid, name, title = this.Descriptor
                let pid   = pid |> Option.map (fun p -> p.ToString()) |> Option.defaultValue "???"
                let name  = name |> Option.defaultValue "<Unknown process name>"
                let title = title |> Option.defaultValue "<Unknown window title>"
                sprintf "[%s] %s | %s" pid name title

          member internal this.AnnotateError message error =
              error |> annotate (sprintf "Window \"%s\": %s" this.Description message)

          member this.AttatchToThread () =
              this.GetProcessAndThreadIds ()
              |> Result.bind (syncThreadInput Attach)
              |> Result.mapError (this.AnnotateError "Failed to attach to thread")

          member this.DetachFromThread () =
              this.GetProcessAndThreadIds ()
              |> Result.bind (syncThreadInput Detach)
              |> Result.mapError (this.AnnotateError "Failed to detach from thread")

          member this.SynchronizeThreadGuard () =
              let window = this
              let result = window.AttatchToThread ()

              match result with
              | Error error -> Console.Error.WriteLine (error |> text)
              | _ -> ()

              { new IDisposable with
                  member this.Dispose () =
                      match result with
                      | Ok _ -> window.DetachFromThread() |> ignore
                      | _ -> () }

    let synchronize (window:Window) =
        window.SynchronizeThreadGuard ()

    let handle (window:Window) = window.Handle

    let children window =
        window
        |> handle
        |> Windows.GetChildWindows
        |> Seq.map Window.fromHandle 
        |> Array.ofSeq

    type Window with member this.Children = children this

    let findProcess (window:Window) =
        window.GetProcessAndThreadIds ()
        |> Result.mapError (window.AnnotateError "Unable to query process id")
        |> Result.mapError ``exception``
        |> Result.bind (fst >> int >> Process.getById)

    type Window with member this.Process = findProcess this

    let placement (window:Window) =
        let mutable placement = Windows.Placement ()
        placement.length <- Marshal.SizeOf (placement)

        match Windows.GetWindowPlacement (window |> handle, &placement) with
        | true  -> placement |> Ok
        | false ->
            NativeError.Last
            |> window.AnnotateError "Failed to get placement info"
            |> Error

    let restoreBounds window =
        window 
        |> placement
        |> Result.map (fun placement ->
            placement.rcNormalPosition |> rectangle)

    let windowBounds window =
        let mutable rect = Windows.Rectangle ()

        Windows.GetWindowRect (window |> handle, &rect)
        |> function
        | true  -> Ok (rect |> rectangle)
        | false ->
            NativeError.Last
            |> window.AnnotateError "Failed to detect boundaries"
            |> Error

    let clientBounds (window:Window) =
        let mutable rect = Windows.Rectangle ()

        Windows.GetClientRect (window |> handle, &rect)
        |> function 
        | true  -> Ok (rect |> rectangle)
        | false ->
            NativeError.Last
            |> window.AnnotateError "Failed to detect client boundaries"
            |> Error

    let isVisible window =
        Windows.IsWindowVisible (window |> handle)

    type Window with member this.IsVisible = isVisible this

    let title (window:Window) = window.GetWindowTitle()
    type Window with member this.Title = title this

    let hasTitle window = 
        let title = window |> title
        not (String.IsNullOrEmpty title)

    type Window with member this.HasTitle = hasTitle this

    let icon window =
         let handle = window |> handle

         let gclIndexes = [ 
             GCLP Windows.GCLPIndex.GCL_HICON 
             GCLP Windows.GCLPIndex.GCL_HICONSM ]

         let smIndexes = [
             SM Windows.SMIndex.ICON_BIG
             SM Windows.SMIndex.ICON_SMALL
             SM Windows.SMIndex.ICON_SMALL2 ]

         let iconPtr =
             [ gclIndexes; smIndexes ]
             |> Seq.concat
             |> Seq.map (function 
                 | GCLP idx -> Windows.GetClassLongPtr (handle, idx)
                 | SM   idx -> Windows.SendMessage (handle, Windows.SMMessage.WM_GETICON, idx, 0))
             |> Seq.filter (fun ptr -> ptr <> IntPtr.Zero)
             |> Seq.tryHead

         iconPtr
         |> Option.map (fun ptr ->
              use icon   = iconFromHandle ptr
              let bitmap = bitmapFromIcon icon
              let bg = bitmap.GetPixel (1, 1)
              bitmap.MakeTransparent (bg)
              bitmap)

    type Window with member this.Icon = icon this

    let windowClass window =
        let handle = window |> handle
        let builder = new StringBuilder(512)
        let tryGetClass () =
            let numChars = Windows.GetClassName(handle, builder, builder.Capacity)
            if numChars = builder.Capacity then
                builder.Capacity = 2 * builder.Capacity |> ignore
                builder.Clear () |> ignore
                false //try again
            else
                true

        while not (tryGetClass()) do ignore ()
        builder.ToString()

    type Window with member this.WindowClass = windowClass this

    let windowStyle window =
        let handle = window |> handle
        let index = NativeAPI.Windows.WindowLongIndex.GWL_STYLE
        let result = NativeAPI.Windows.GetWindowLongPtr (handle, index)
        let result = result.ToInt64 ()
        LanguagePrimitives.EnumOfValue<_,NativeAPI.Windows.WindowStyle> (result)

    type Window with member this.WindowStyle = windowStyle this

    let extendedWindowStyle window =
        let handle = window |> handle
        let index = NativeAPI.Windows.WindowLongIndex.GWL_EXSTYLE
        let result = NativeAPI.Windows.GetWindowLongPtr (handle, index)
        let result = result.ToInt64 ()
        LanguagePrimitives.EnumOfValue<_,NativeAPI.Windows.WindowStyle> (result)

    type Window with member this.ExtendedWindowStyle = extendedWindowStyle this

    let isEnabled window = 
        let style = window |> windowStyle
        let flag = style &&& NativeAPI.Windows.WindowStyle.WS_DISABLED
        LanguagePrimitives.EnumToValue<_,int64>(flag) = 0L

    type Window with member this.IsEnabled = isEnabled this

    let isMinimized window = 
        let style = window |> windowStyle
        let flag = style &&& NativeAPI.Windows.WindowStyle.WS_ICONIC
        LanguagePrimitives.EnumToValue<_,int64>(flag) <> 0L

    type Window with member this.IsMinimized = isMinimized this

    let isMaximized window = 
        let style = window |> windowStyle
        let flag = style &&& NativeAPI.Windows.WindowStyle.WS_MAXIMIZE
        LanguagePrimitives.EnumToValue<_,int64>(flag) <> 0L

    type Window with member this.IsMaximized = isMaximized this

    let monitor window =
        let handle = window |> handle
        let ``default`` = NativeAPI.Windows.DefaultMonitor.DefaultToNearest
        let monitorHandle = NativeAPI.Windows.MonitorFromWindow (handle, ``default``)
        { Monitor.Id = monitorHandle }

    type Window with member this.Monitor = monitor this

    let screenPosition window =
        let ox, oy = window |> monitor |> bounds |> topLeft
        let  x,  y = window |> windowBounds |> Result.map topLeft |> Result.defaultValue (infinity,infinity)
        (x-ox, y-oy)

    type Window with member this.ScreenPosition = screenPosition this

    let show flag window = 
        match Windows.ShowWindowAsync(window |> handle, flag) with
        | true -> Ok ()
        | false ->
            NativeError.Last
            |> window.AnnotateError "Failed to show window"
            |> Error

    let restore window =
        window
        |> show Windows.ShowWindowCommands.Restore
        |> Result.mapError (window.AnnotateError "Failed to restore window")

    let toForeground window =
        match window |> handle |> Windows.SetForegroundWindow with
        | true -> Ok ()
        | false ->
            NativeError.Last
            |> window.AnnotateError "Failed to send window to the foreground"
            |> Error

    let focus window =
        match window |> handle |> Windows.SetFocus |> int with
        | 0 -> NativeError.Last |> window.AnnotateError "Failed to focus window" |> Error
        | _ -> Ok ()

    let activate window =
        match window |> handle |> Windows.SetActiveWindow |> int with
        | 0 -> NativeError.Last |> window.AnnotateError "Failed to activate window" |> Error
        | _ -> Ok ()
        
    let private moveBelowReferenceWindow reference window =
        let flags = 
            NativeAPI.Windows.SetWindowPosFlags.IgnoreMove    |||
            NativeAPI.Windows.SetWindowPosFlags.IgnoreResize  |||
            NativeAPI.Windows.SetWindowPosFlags.DoNotActivate |||
            NativeAPI.Windows.SetWindowPosFlags.AsynchronousWindowPosition

        use sync = window |> synchronize
        let hWin = window    |> handle
        let hRef = reference |> handle

        match NativeAPI.Windows.SetWindowPos (hWin, hRef, 0, 0, 0, 0, flags) with
        | true -> Ok ()
        | false ->
            NativeError.Last
            |> window.AnnotateError "Failed to change window z position"
            |> Error

    let placeBelow reference window =
        window |> moveBelowReferenceWindow reference
        |> Result.mapError (window.AnnotateError "Unable to place below reference window")

    let placeAbove reference window =
        (window |> moveBelowReferenceWindow reference)
        |> Result.map (fun _ -> reference |> moveBelowReferenceWindow window)
        |> Result.mapError (window.AnnotateError "Unable to place above reference window")

    let desktopWindow =
        Windows.GetDesktopWindow ()
        |> Window.fromHandle

    let getRootWindows () = 
        Windows.GetRootWindows ()
        |> Seq.map Window.fromHandle 
        |> Array.ofSeq

    let getActiveWindow () =
        let handle = NativeAPI.Windows.GetForegroundWindow ()
        { Handle = handle }

    let rec iterateDescendants predicate window =
        let mutable windows = []

        if predicate window then
             let descendants = 
                 window
                 |> children
                 |> Seq.collect (iterateDescendants predicate)

             windows <- descendants :: windows
             windows <- Seq.singleton window :: windows

        windows |> Seq.concat

    let parent window =
        let handle = window |> handle |> Windows.GetParent

        match handle |> int with
        | 0 -> NativeError.Last |> window.AnnotateError "Failed to get window parent" |> Error
        | _ -> handle |> Window.fromHandle |> Result.Ok

    [<SuppressMessage("NameConventions","*")>]
    type WindowEvent =
        | EVENT_MIN                                    = 0x00000001
        | EVENT_MAX                                    = 0x7FFFFFFF
        | EVENT_OBJECT_ACCELERATORCHANGE               = 0x8012
        | EVENT_OBJECT_CLOAKED                         = 0x8017
        | EVENT_OBJECT_CONTENTSCROLLED                 = 0x8015
        | EVENT_OBJECT_CREATE                          = 0x8000
        | EVENT_OBJECT_DEFACTIONCHANGE                 = 0x8011
        | EVENT_OBJECT_DESCRIPTIONCHANGE               = 0x800D
        | EVENT_OBJECT_DESTROY                         = 0x8001
        | EVENT_OBJECT_DRAGSTART                       = 0x8021
        | EVENT_OBJECT_DRAGCANCEL                      = 0x8022
        | EVENT_OBJECT_DRAGCOMPLETE                    = 0x8023
        | EVENT_OBJECT_DRAGENTER                       = 0x8024
        | EVENT_OBJECT_DRAGLEAVE                       = 0x8025
        | EVENT_OBJECT_DRAGDROPPED                     = 0x8026
        | EVENT_OBJECT_END                             = 0x80FF
        | EVENT_OBJECT_FOCUS                           = 0x8005
        | EVENT_OBJECT_HELPCHANGE                      = 0x8010
        | EVENT_OBJECT_HIDE                            = 0x8003
        | EVENT_OBJECT_HOSTEDOBJECTSINVALIDATED        = 0x8020
        | EVENT_OBJECT_IME_HIDE                        = 0x8028
        | EVENT_OBJECT_IME_SHOW                        = 0x8027
        | EVENT_OBJECT_IME_CHANGE                      = 0x8029
        | EVENT_OBJECT_INVOKED                         = 0x8013
        | EVENT_OBJECT_LIVEREGIONCHANGED               = 0x8019
        | EVENT_OBJECT_LOCATIONCHANGE                  = 0x800B
        | EVENT_OBJECT_NAMECHANGE                      = 0x800C
        | EVENT_OBJECT_PARENTCHANGE                    = 0x800F
        | EVENT_OBJECT_REORDER                         = 0x8004
        | EVENT_OBJECT_SELECTION                       = 0x8006
        | EVENT_OBJECT_SELECTIONADD                    = 0x8007
        | EVENT_OBJECT_SELECTIONREMOVE                 = 0x8008
        | EVENT_OBJECT_SELECTIONWITHIN                 = 0x8009
        | EVENT_OBJECT_SHOW                            = 0x8002
        | EVENT_OBJECT_STATECHANGE                     = 0x800A
        | EVENT_OBJECT_TEXTEDIT_CONVERSIONTARGETCHANED = 0x8030
        | EVENT_OBJECT_TEXTSELECTIONCHANGED            = 0x8014
        | EVENT_OBJECT_UNCLOAKED                       = 0x8018
        | EVENT_OBJECT_VALUECHANGE                     = 0x800E
        | EVENT_SYSTEM_ALERT                           = 0x0002
        | EVENT_SYSTEM_ARRANGMENTPREVIEW               = 0x8016
        | EVENT_SYSTEM_CAPTUREEND                      = 0x0009
        | EVENT_SYSTEM_CAPTURESTART                    = 0x0008
        | EVENT_SYSTEM_CONTEXTHELPEND                  = 0x000D
        | EVENT_SYSTEM_CONTEXTHELPSTART                = 0x000C
        | EVENT_SYSTEM_DESKTOPSWITCH                   = 0x0020
        | EVENT_SYSTEM_DIALOGEND                       = 0x0011
        | EVENT_SYSTEM_DIALOGSTART                     = 0x0010
        | EVENT_SYSTEM_DRAGDROPEND                     = 0x000F
        | EVENT_SYSTEM_DRAGDROPSTART                   = 0x000E
        | EVENT_SYSTEM_END                             = 0x00FF
        | EVENT_SYSTEM_FOREGROUND                      = 0x0003
        | EVENT_SYSTEM_MENUPOPUPEND                    = 0x0007
        | EVENT_SYSTEM_MENUPOPUPSTART                  = 0x0006
        | EVENT_SYSTEM_MENUEND                         = 0x0005
        | EVENT_SYSTEM_MENUSTART                       = 0x0004
        | EVENT_SYSTEM_MINIMIZEEND                     = 0x0017
        | EVENT_SYSTEM_MINIMIZESTART                   = 0x0016
        | EVENT_SYSTEM_MOVESIZEEND                     = 0x000B
        | EVENT_SYSTEM_MOVESIZESTART                   = 0x000A
        | EVENT_SYSTEM_SCROLLINGEND                    = 0x0013
        | EVENT_SYSTEM_SCROLLINGSTART                  = 0x0012
        | EVENT_SYSTEM_SOUND                           = 0x0001
        | EVENT_SYSTEM_SWITCHEND                       = 0x0015
        | EVENT_SYSTEM_SWITCHSTART                     = 0x0014

    // type ObjectID =
    //     | OBJID_WINDOW            = 0x00000000l
    //     | OBJID_SYSMENU           = 0xFFFFFFFFl
    //     | OBJID_TITLEBAR          = 0xFFFFFFFEl
    //     | OBJID_MENU              = 0xFFFFFFFDl
    //     | OBJID_CLIENT            = 0xFFFFFFFCl
    //     | OBJID_VSCROLL           = 0xFFFFFFFBl
    //     | OBJID_HSCROLL           = 0xFFFFFFFAl
    //     | OBJID_SIZEGRIP          = 0xFFFFFFF9l
    //     | OBJID_CARET             = 0xFFFFFFF8l
    //     | OBJID_CURSOR            = 0xFFFFFFF7l
    //     | OBJID_ALERT             = 0xFFFFFFF6l
    //     | OBJID_SOUND             = 0xFFFFFFF5l
    //     | OBJID_QUERYCLASSNAMEIDX = 0xFFFFFFF4l
    //     | OBJID_NATIVEOM          = 0xFFFFFFF0l

    let private windowEvent = new Event<_> ()
    let WindowEvent = windowEvent.Publish

    let private onWinEvent 
        (hook:nativeint) (eventType:uint32) (windowHandle:nativeint) 
        (objectId:int) (childId:int) (eventThread:uint32) (timestamp:uint32) =
            if objectId = 0 && childId = 0 then
                let window = { Window.Handle = windowHandle }
                let event:WindowEvent = eventType |> int |> enum
                windowEvent.Trigger (window, event)

    let private eventDelegate = new Windows.WinEventCallback(onWinEvent)

    let hook = 
        Windows.SetWinEventHook 
            (Windows.EVENT_MIN, Windows.EVENT_MAX, IntPtr.Zero, 
            eventDelegate, 0u, 0u, Windows.WINEVENT_OUTOFCONTEXT) 
