namespace Sextant

open System

module Rectangle =
    let inline private max a b = if a > b then a else b
    let inline private min a b = if a < b then a else b

    type Rectangle = 
        { Left  :float 
          Top   :float 
          Right :float 
          Bottom:float }

        static member None = 
            { Left   = 0.0
              Top    = 0.0
              Right  = 0.0
              Bottom = 0.0 }

    let create (x,y) (w,h) =
        { Left   = x
          Top    = y
          Right  = x+w
          Bottom = y+h }

    let topLeft     (r:Rectangle) = (r.Left,  r.Top   )
    let topRight    (r:Rectangle) = (r.Right, r.Top   )
    let bottomLeft  (r:Rectangle) = (r.Left,  r.Bottom)
    let bottomRight (r:Rectangle) = (r.Right, r.Bottom)

    let width  (r:Rectangle) = r.Right  - r.Left |> max LanguagePrimitives.GenericZero
    let height (r:Rectangle) = r.Bottom - r.Top  |> max LanguagePrimitives.GenericZero
    let size     r = (r |> width, r |> height)
    let area     r = (r |> width) * (r |> height)

    let moveTo pos r = 
        let x,y = pos
        let w,h = r |> size
        { r with 
              Left   = x
              Top    = y
              Right  = x+w
              Bottom = y+h }

    let move delta r = 
        let dx, dy = delta
        let  x,  y = r |> topLeft
        r |> moveTo (x + dx, y + dy)

    let withWidth  w r = { r with Right  = r.Left + w }
    let withHeight h r = { r with Bottom = r.Top  + h }
    let withSize size r = 
        let w,h = size
        r |> withWidth w |> withHeight h

    let scale scale r = 
        let w, h = r |> size
        let w = scale * w
        let h = scale * h
        r |> withSize (w,h)

    let intersect (r1:Rectangle) (r2:Rectangle) =
        let l = max r1.Left   r2.Left
        let t = max r2.Top    r2.Top
        let r = min r1.Right  r2.Right
        let b = min r1.Bottom r1.Bottom
        { Left = l; Top = t; Right = r; Bottom = b }

    let pad padding (r:Rectangle) =
        { Left   = r.Left   + padding
          Top    = r.Top    + padding
          Right  = r.Right  - padding
          Bottom = r.Bottom - padding }

    let center rectangle =
        let dx = rectangle |> width  |> (*) 0.5
        let dy = rectangle |> height |> (*) 0.5
        let x,y = rectangle |> topLeft
        (x+dx, y+dy)
