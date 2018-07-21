namespace Sextant

module Seq =
    let cat seq2 seq1 = Seq.append seq1 seq2

    let appendTo = Seq.append
    let prependTo target source = Seq.append source target

    let appendElementTo target element = [ element ] |> appendTo target
    let prependElementTo target element = [ element ] |> prependTo target