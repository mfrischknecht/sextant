namespace Sextant

open System
open System.Text.RegularExpressions

module Text =
    let private lineEnding = Regex ("\r?\n")
    let private whitespaceOnly  = Regex ("^\\s*$")

    [<Struct>]
    type Line private (line:string) =
        member this.Text = line
        member this.IsEmpty = line |> String.IsNullOrEmpty
        member this.IsWhitespaceOnly = line |> whitespaceOnly.IsMatch

        static member empty = Line ""
        static member sequence = lineEnding.Split >> (Array.map Line)
        static member single str =
            if str |> lineEnding.IsMatch |> not then Line str |> Ok
            else Error "Text contains unexpected newlines"

        static member map fn (line:Line) = line.Text |> fn |> Line.single
        static member toString (line:Line) = line.Text
        static member indent = Line.map (sprintf "\t%s") >> Result.unwrap "Indentation broke line"

    type Lines private () =
        static member split = Line.sequence
        static member concat (lines:Line seq) =
            lines 
            |> Seq.map (fun l -> l.Text)
            |> String.concat "\r\n"

    let indent = 
        Lines.split 
        >> Seq.map Line.indent 
        >> Lines.concat

    let stripEmptyHeader (lines:Line seq) =
        lines |> Seq.skipWhile (fun l -> l.IsWhitespaceOnly)

    let stripEmptyFooter (lines:Line seq) =
        lines 
        |> Seq.rev
        |> stripEmptyHeader
        |> Seq.rev
