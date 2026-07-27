// MainForm.cs 07242026 15:14:00
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CursorMagnifier
{
    public partial class MainForm : Form
    {
        private const int WM_MOUSEWHEEL = 0x020A;
        private const int WH_MOUSE_LL = 14;
        private const int WM_HOTKEY = 0x0312;

        private IntPtr hookID = IntPtr.Zero;
        private LowLevelMouseProc mouseProc;

        private OverlayForm overlay;
        private bool magnifierVisible = false;

        private int hotkeyIdToggle;
        private int hotkeyIdExit;

        public MainForm()
        {
            this.ShowInTaskbar = false;
            this.WindowState = FormWindowState.Minimized;
            this.Opacity = 0;

            RegisterHotKeys();

            mouseProc = MouseHookCallback;
            hookID = SetHook(mouseProc);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            UnregisterHotKeys();
            UnhookWindowsHookEx(hookID);
            base.OnFormClosed(e);
        }

        private void RegisterHotKeys()
        {
            hotkeyIdToggle = 1;
            RegisterHotKey(this.Handle, hotkeyIdToggle, (uint)KeyModifiers.None, (uint)Keys.F8);

            hotkeyIdExit = 2;
            RegisterHotKey(this.Handle, hotkeyIdExit, (uint)KeyModifiers.Control, (uint)Keys.F8);
        }

        private void UnregisterHotKeys()
        {
            if (hotkeyIdToggle != 0)
            {
                UnregisterHotKey(this.Handle, hotkeyIdToggle);
                hotkeyIdToggle = 0;
            }

            if (hotkeyIdExit != 0)
            {
                UnregisterHotKey(this.Handle, hotkeyIdExit);
                hotkeyIdExit = 0;
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_HOTKEY)
            {
                int id = m.WParam.ToInt32();
                int lParam = m.LParam.ToInt32();

                Keys key = (Keys)((lParam >> 16) & 0xFFFF);
                KeyModifiers mods = (KeyModifiers)(lParam & 0xFFFF);

                if (id == hotkeyIdToggle && key == Keys.F8 && mods == KeyModifiers.None)
                {
                    ToggleMagnifier();
                }
                else if (id == hotkeyIdExit && key == Keys.F8 && mods == KeyModifiers.Control)
                {
                    Application.Exit();
                }
            }

            base.WndProc(ref m);
        }

        private void ToggleMagnifier()
        {
            if (!magnifierVisible)
            {
                overlay = new OverlayForm();
                overlay.Show();
                magnifierVisible = true;
            }
            else
            {
                overlay.Close();
                overlay = null;
                magnifierVisible = false;
            }
        }

        private IntPtr SetHook(LowLevelMouseProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_MOUSE_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_MOUSEWHEEL)
            {
                MSLLHOOKSTRUCT hookStruct =
                    (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));

                int delta = (short)((hookStruct.mouseData >> 16) & 0xffff);

                if (magnifierVisible && overlay != null)
                {
                    overlay.AdjustZoom(delta);
                    return (IntPtr)1; // consume the wheel so the underlying window does not scroll
                }
            }

            return CallNextHookEx(hookID, nCode, wParam, lParam);
        }

        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public System.Drawing.Point pt;
            public int mouseData;
            public int flags;
            public int time;
            public IntPtr dwExtraInfo;
        }

        [Flags]
        private enum KeyModifiers
        {
            None = 0,
            Alt = 1,
            Control = 2,
            Shift = 4,
            Windows = 8
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk,
            int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
    }
}
