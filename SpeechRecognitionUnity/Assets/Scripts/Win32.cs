using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class Win32 : MonoBehaviour {


    [DllImport("User32.Dll")]
    public static extern long SetCursorPos(int x, int y);

    [DllImport("User32.Dll")]
    public static extern bool ClientToScreen(IntPtr hWnd, ref POINT point);

    [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
    public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);
    //Mouse actions
    private const int MOUSEEVENTF_LEFTDOWN = 0x02;
    private const int MOUSEEVENTF_LEFTUP = 0x04;
    private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
    private const int MOUSEEVENTF_RIGHTUP = 0x10;

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int x;
        public int y;
    }

    public static void LeftMouseClick(uint xpos, uint ypos)
    {
        mouse_event(MOUSEEVENTF_LEFTDOWN, xpos, ypos, 0, 0);
    }

    public static void LeftMouseUp(uint xpos, uint ypos)
    {
        mouse_event(MOUSEEVENTF_LEFTUP, xpos, ypos, 0, 0);
    }

    public static void RightMouseClick(uint xpos, uint ypos)
    {
        mouse_event(MOUSEEVENTF_RIGHTDOWN, xpos, ypos, 0, 0);
    }

    public static void RightMouseUp(uint xpos, uint ypos)
    {
        mouse_event(MOUSEEVENTF_RIGHTUP, xpos, ypos, 0, 0);
    }


}
