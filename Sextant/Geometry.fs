namespace Sextant

open System

module Geometry =
    let inline private square x = x*x

    let vectorTo target source =
        let (xt,yt) = target
        let (xs,ys) = source
        (xt-xs, yt-ys)

    let distanceSquaredTo target source =
        let (dx,dy) = source |> vectorTo target
        square(dx)+square(dy)

    let distanceTo target source =
        source |> distanceSquaredTo target |> Math.Sqrt