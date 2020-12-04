using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;

namespace Sextant.NativeAPI.ComImports
{
	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("F31574D6-B682-4CDC-BD56-1827860ABEC6")]
	internal interface IVirtualDesktopManagerInternal
	{
		int GetCount();
		void MoveViewToDesktop(IApplicationView view, IVirtualDesktop desktop);
		bool CanViewMoveDesktops(IApplicationView view);
		IVirtualDesktop GetCurrentDesktop();
		void GetDesktops(out IObjectArray desktops);
		[PreserveSig]
		int GetAdjacentDesktop(IVirtualDesktop from, int direction, out IVirtualDesktop desktop);
		void SwitchDesktop(IVirtualDesktop desktop);
		IVirtualDesktop CreateDesktop();
		void RemoveDesktop(IVirtualDesktop desktop, IVirtualDesktop fallback);
		IVirtualDesktop FindDesktop(ref Guid desktopid);
	}
}