namespace Sextant

open System

module Disposable =
    let dispose<'T when 'T :> IDisposable> (obj:'T) =
        (obj :> IDisposable).Dispose()