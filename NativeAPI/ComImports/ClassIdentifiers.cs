using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;

namespace Sextant.NativeAPI.ComImports
{
	internal static class ClassIdentifiers
	{
		public static readonly Guid ImmersiveShell = new Guid("C2F03A33-21F5-47FA-B4BB-156362A2F239");
		public static readonly Guid VirtualDesktopManagerInternal = new Guid("C5E0CDCA-7B6E-41B2-9FC4-D93975CC467B");
		public static readonly Guid VirtualDesktopManager = new Guid("AA509086-5CA9-4C25-8F95-589D3C07B48A");
		public static readonly Guid VirtualDesktopPinnedApps = new Guid("B5A399E7-1C87-46B8-88E9-FC5747B171BD");
	}
}
