
namespace Sextant

open System.Windows.Media

module Color =
    let parse str = 
        Result.tryWith (fun _ ->
            str |> ColorConverter.ConvertFromString :?> Color)
