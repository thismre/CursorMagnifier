// HotKeyManager.cs 07242026 15:02:00
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CursorMagnifier
{
    public static class HotKeyManager
    {
        public static event EventHandler<HotKeyEventArgs> HotKeyPressed;

        public static int RegisterHotKey(Keys key, KeyModifiers modifiers)
        {
            int id = System.Threading.Interlocked.Increment(ref _id);
            if (!RegisterHotKey(IntPtr.Zero, id, (uint)modifiers, (uint)key))
                throw new InvalidOperationException("Couldn’t register the hotkey.");
            return id;
        }

        public static void UnregisterHotKey(Keys key, KeyModifiers modifiers)
        {
            // This implementation unregisters all hotkeys.
            // Your earlier version used this exact pattern.
            for (int i = 1; i <= _id; i++)
                UnregisterHotKey(IntPtr.Zero, i);
        }

        private static int _id = 0;

        private static void OnHotKeyPressed(HotKeyEventArgs e)
        {
            HotKeyPressed?.Invoke(null, e);
        }

        private static IntPtr _windowHandle;

        static HotKeyManager()
        {
            MessageWindow window = new MessageWindow();
            _windowHandle = window.Handle;
        }

        private class MessageWindow : NativeWindow
        {
            private const int WM_HOTKEY = 0x0312;

            public MessageWindow()
            {
                CreateHandle(new CreateParams());
            }

            protected override void WndProc(ref Message m)
            {
                if (m.Msg == WM_HOTKEY)
                {
                    Keys key = (Keys)(((int)m.LParam >> 16) & 0xFFFF);
                    KeyModifiers modifiers = (KeyModifiers)((int)m.LParam & 0xFFFF);
                    HotKeyManager.OnHotKeyPressed(new HotKeyEventArgs(key, modifiers));
                }
                base.WndProc(ref m);
            }
        }

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
    }

    public class HotKeyEventArgs : EventArgs
    {
        public Keys Key { get; private set; }
        public KeyModifiers Modifiers { get; private set; }

        public HotKeyEventArgs(Keys key, KeyModifiers modifiers)
        {
            Key = key;
            Modifiers = modifiers;
        }
    }

    [Flags]
    public enum KeyModifiers
    {
        None = 0,
        Alt = 1,
        Control = 2,
        Shift = 4,
        Windows = 8
    }
}
