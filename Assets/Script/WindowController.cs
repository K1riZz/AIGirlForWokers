using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class WindowController : MonoBehaviour
{
    // Import functions from Windows libraries
    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

    [DllImport("user32.dll")]
    private static extern int SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll")]
    private static extern int SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

    [DllImport("dwmapi.dll")]
    private static extern uint DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS margins);

    // Define necessary constants
    private const int GWL_STYLE = -16;
    private const uint WS_POPUP = 0x80000000;
    private const uint WS_VISIBLE = 0x10000000;

    private const int GWL_EXSTYLE = -20;
    private const uint WS_EX_LAYERED = 0x00080000;
    private const uint WS_EX_TRANSPARENT = 0x00000020;

    private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);

    private const uint LWA_COLORKEY = 0x00000001;

    // Define the MARGINS struct for DwmExtendFrameIntoClientArea
    [StructLayout(LayoutKind.Sequential)]
    public struct MARGINS
    {
        public int cxLeftWidth;
        public int cxRightWidth;
        public int cyTopHeight;
        public int cyBottomHeight;
    }

    private IntPtr hWnd;

    void Start()
    {
#if !UNITY_EDITOR // This script should only run in a built application
        hWnd = GetActiveWindow();

        // 1. Make the window background transparent
        // The DWM API is used to extend the non-client frame into the client area, creating a transparent window.
        MARGINS margins = new MARGINS { cxLeftWidth = -1 };
        DwmExtendFrameIntoClientArea(hWnd, ref margins);

        // 2. Make the window borderless and a popup
        // We change the window style to be a borderless popup window.
        SetWindowLong(hWnd, GWL_STYLE, WS_POPUP | WS_VISIBLE);

        // 3. Set the window to be always on top
        // This ensures our pet is not obscured by other windows.
        SetWindowPos(hWnd, HWND_TOPMOST, 0, 0, 0, 0, 0);

        // 4. Enable click-through for a specific color
        // We set a "color key". Any pixel of this color will be fully transparent and click-through.
        // We will set our camera's background to this color.
        // Note: The color is in 0x00BBGGRR format. Here, 0x00000000 is black.
        SetWindowLong(hWnd, GWL_EXSTYLE, WS_EX_LAYERED);
        SetLayeredWindowAttributes(hWnd, 0, 0, LWA_COLORKEY);
#endif
    }
}