namespace Sextant

open System

open Sextant.NativeAPI
open Sextant.NativeWindow
open Sextant.Rectangle

module WindowThumbnail =
    open NativeErrors

    type Properties = 
        { Visible              : bool option
          SourceClientAreaOnly : bool option
          Opacity              : byte option
          Source               : Rectangle option
          Destination          : Rectangle option }

        static member None =
             { Visible              = None
               SourceClientAreaOnly = None
               Opacity              = None
               Source               = None
               Destination          = None }

    let private rect (r:Rectangle) = 
        DwmThumbnails.Rect 
            (r.Left   |> int, 
             r.Top    |> int, 
             r.Right  |> int, 
             r.Bottom |> int)

    type ThumbnailHandle = { Handle:IntPtr; Source:Window; Target:Window }

    let private getHandle (handle:ThumbnailHandle) = handle.Handle

    let register (target:Window) (source:Window) = 
        let src = source |> NativeWindow.handle
        let tgt = target |> NativeWindow.handle
        let mutable handle = IntPtr.Zero 
        let result = DwmThumbnails.DwmRegisterThumbnail (tgt, src, &handle)
        match result with
        | 0 -> Ok { Handle = handle; Source = source; Target = target }
        | _ -> Error (NativeError (0, result) |> annotate "Failed to register a thumbnail handle")

    let unregister handle =
        let handle = handle |> getHandle
        let result = DwmThumbnails.DwmUnregisterThumbnail (handle)
        match result with
        | 0 -> Ok ()
        | _ -> Error (NativeError (0, result) |> annotate "Failed to unregister a thumbnail handle")

    let updateProperties properties handle =
        let handle = handle |> getHandle

        let mutable p = DwmThumbnails.ThumbnailProperties ()
        p.Flags <- 0u

        if properties.Destination.IsSome then
            p.Destination <- properties.Destination.Value |> rect
            p.Flags <- p.Flags ||| DwmThumbnails.DWM_TNP_RECTDESTINATION

        if properties.Source.IsSome then
            p.Source <- properties.Source.Value |> rect
            p.Flags <- p.Flags ||| DwmThumbnails.DWM_TNP_RECTSOURCE

        if properties.Visible.IsSome then
            p.Visible <- properties.Visible.Value
            p.Flags <- p.Flags ||| DwmThumbnails.DWM_TNP_VISIBLE

        if properties.Opacity.IsSome then
            p.Opacity <- properties.Opacity.Value
            p.Flags <- p.Flags ||| DwmThumbnails.DWM_TNP_OPACITY

        let result = DwmThumbnails.DwmUpdateThumbnailProperties (handle, &p)
        match result with
        | 0 -> Ok ()
        | _ -> Error (NativeError (0, result) |> annotate "Failed to update thumbnail properties")

    let sourceSize handle =
        let handle = handle |> getHandle
        let mutable size = DwmThumbnails.Size ()
        let result = DwmThumbnails.DwmQueryThumbnailSourceSize (handle, &size)
        (float size.x, float size.y)


    type Thumbnail(handle) =
        let mutable handle = Some handle
        let mutable properties = { Properties.None with Visible = Some true }

        do 
            handle |> Option.map (updateProperties properties) |> ignore

        interface IDisposable with
            member this.Dispose () =
                handle |> Option.map unregister |> ignore
                handle <- None

        member this.SourceSize =
            handle 
            |> Option.map sourceSize 
            |> Option.defaultValue (0.0,0.0)

        member this.SourceWindow = 
            if not handle.IsSome then
                let error = ObjectDisposedException "Thumbnail"
                raise error
            handle.Value.Source

        member this.TargetWindow = 
            if not handle.IsSome then
                let error = ObjectDisposedException "Thumbnail"
                raise error
            handle.Value.Target

        member this.Properties 
            with get () = properties
            and  set v  = 
                properties <- v
                handle |> Option.map (updateProperties v) |> ignore

    let create target source =
        register target source
        |> Result.map (fun h -> new Thumbnail(h))

    let sourceWindow (thumbnail:Thumbnail) = thumbnail.SourceWindow
    let targetWindow (thumbnail:Thumbnail) = thumbnail.TargetWindow

