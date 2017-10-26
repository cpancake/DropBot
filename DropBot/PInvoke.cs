using System;
using System.Runtime.InteropServices;

namespace DropBot
{
	public class PInvoke
	{
		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool GetWindowRect(HandleRef hWnd, out RECT lpRect);

		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

		[DllImport("user32.dll", SetLastError = true)]
		static extern bool SetForegroundWindow(IntPtr hWnd);



		[StructLayout(LayoutKind.Sequential)]
		public struct RECT
		{
			public int Left;        // x position of upper-left corner
			public int Top;         // y position of upper-left corner
			public int Right;       // x position of lower-right corner
			public int Bottom;      // y position of lower-right corner
		}
	
		public static void FocusWindow(IntPtr hWnd)
		{
			ShowWindow(hWnd, 1);
			SetForegroundWindow(hWnd);
		}	
	}
}
