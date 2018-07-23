namespace Sextant

open System
open System.Runtime.InteropServices

open Sextant.NativeAPI
open Sextant.NativeErrors
open Sextant.NativeWindow

module Workspace =
    [<Struct>]
    type WindowStation = { Id:IntPtr }

    let getCurrentWindowStation () =
        let handle = Desktop.GetProcessWindowStation ()
        if handle <> IntPtr.Zero then Ok { WindowStation.Id = handle }
        else Error (NativeError.Last |> annotate "Failed to determine process window station")

    [<Struct>]
    type WorkspaceHandle = { Id:IntPtr; MustClose:bool; }

    type Workspace(handle) = 
        let mutable handle = Some handle
        member this.Handle = 
            if handle.IsSome then handle.Value.Id
            else raise (ObjectDisposedException "The workspace has already been disposed")

        interface IDisposable with
            member this.Dispose () =
                if handle.IsSome then
                    if handle.Value.MustClose then
                        Desktop.CloseDesktop (handle.Value.Id) |> ignore
                    handle <- None
    let workspaces station =
        let names = Desktop.GetWindowStationDesktopNames (station.Id)
        let workspaces = 
            names 
            |> Seq.map (fun name ->
               Desktop.OpenDesktop(name,0,false,int64 Desktop.ACCESS_MASK.DESKTOP_ALL))
            |> Seq.filter (fun h -> h <> IntPtr.Zero)
            |> Seq.map (fun h -> new Workspace ({ Id = h; MustClose = true }))
            |> Array.ofSeq

        workspaces

    let windowWorkspace (window:Window) =
        window.GetProcessAndThreadIds()
        |> Result.bind (fun (proc,thread) -> 
            let handle = Desktop.GetThreadDesktop (int thread)
            if handle <> IntPtr.Zero then Ok (new Workspace ({ Id = handle; MustClose = false }))
            else Error (NativeError.Last |> annotate "Failed to determine window thread desktop"))

    let currentWorkspace () =
        let handle = Desktop.OpenInputDesktop(0,false,int64 Desktop.ACCESS_MASK.DESKTOP_ALL)
        if handle <> IntPtr.Zero then Ok (new Workspace ({ Id = handle; MustClose = true }))
        else Error (NativeError.Last |> annotate "Failed to open current input desktop")

    let makeCurrentWorkspace (workspace:Workspace) =
        if Desktop.SwitchDesktop workspace.Handle then Ok ()
        else Error (NativeError.Last |> annotate "Failed to switch input desktop")

    let windows (workspace:Workspace) =
        let windows = Desktop.GetDesktopWindows(workspace.Handle)
        windows |> Seq.map Window.fromHandle |> Array.ofSeq

