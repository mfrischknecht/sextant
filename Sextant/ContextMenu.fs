namespace Sextant

open System
open System.Windows
open System.Windows.Forms

module ContextMenu =
    type Item =
        | MenuEntry of (string * (unit -> unit))
        | SubMenu   of (string * Item[])
        | Divider

    let rec private createItems (items:Item seq): MenuItem[] = 
        items
        |> Seq.map (function
              | Divider                    -> new MenuItem("-")
              | MenuEntry (name, callback) -> new MenuItem(name, EventHandler(fun _ _ -> callback()))
              | SubMenu   (name, items   ) ->
                   let subitems = items |> createItems
                   new MenuItem(name,subitems))
        |> Array.ofSeq

    let create items =
        items
        |> Option.nonEmptySeq
        |> Result.nonEmptyOption
        |> Result.mapError (fun _ -> "No menu items provided")
        |> Result.map createItems
        |> Result.map (fun items -> new ContextMenu(items))
