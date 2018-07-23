namespace Sextant

module Map =
    let keys   map = map |> Map.toSeq |> Seq.map fst
    let values map = map |> Map.toSeq |> Seq.map snd