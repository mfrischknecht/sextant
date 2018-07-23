namespace Sextant

open System
open System.Threading
open System.Reflection
open System.Diagnostics

module Process =

    let assemblyGuid = Assembly.GetExecutingAssembly().GetType().GUID.ToString()
    let mutable private mutexIsNew = false
    let mutex = new Mutex(false, assemblyGuid, &mutexIsNew)
    let alreadyRunning = not mutexIsNew

    let exitIfAlreadyRunning () =
        if alreadyRunning then
            printfn "Another instance is already running."
            System.Windows.Application.Current.Shutdown ()

    let getById id =
        Result.tryWith (fun () -> 
            Process.GetProcessById id)

    let id (proc:Process) = proc.Id
    let sessionId (proc:Process) = proc.SessionId
    let name (proc:Process) = proc.ProcessName