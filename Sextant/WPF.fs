namespace Sextant

open System
open System.Windows
open System.Windows.Controls
open System.IO
open System.Drawing.Imaging
open System.Windows.Media.Imaging
open System.Windows.Interop

open Sextant.Rectangle

module WPF =
    type System.Windows.Threading.DispatcherObject with
        member this.AsyncDispatch (callback:(unit -> _)) =
            let action = new Action(callback)
            this.Dispatcher.BeginInvoke(action, null)

        member this.SyncDispatch (callback:(unit -> _)) =
            let func = new Func<_>(callback)
            this.Dispatcher.BeginInvoke(func, null)
    end

    let convertBitmap (bitmap:System.Drawing.Bitmap) =
        use stream = new MemoryStream ()
        bitmap.Save (stream, ImageFormat.Png)
        let image = BitmapImage ()
        image.BeginInit ()
        image.CacheOption <- BitmapCacheOption.OnLoad
        image.StreamSource <- stream
        image.EndInit ()
        image

    let handle window =
        let interopHelper = window |> WindowInteropHelper
        let handle = interopHelper.EnsureHandle ()
        handle

    let boundsIn (ancestor:Control) (control:Control) =
        let transform = control.TransformToAncestor ancestor
        let topLeft = (0.0, 0.0) |> Point |> transform.Transform
        let bottomRight = (control.ActualWidth, control.ActualWidth) |> Point |> transform.Transform

        { Left   = topLeft    .X
          Top    = topLeft    .Y
          Right  = bottomRight.X
          Bottom = bottomRight.Y }