// OverlayForm.cs 07262026 14:33:00
using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace CursorMagnifier
{
    public partial class OverlayForm : Form
    {
        private Timer timer;

        private const int MAG_SIZE = 600;

        private double magFactor = 2.0;
        private const double MIN_MAG = 1.0;
        private const double MAX_MAG = 6.0;

        private const int MIN_CAPTURE_SIZE = 50;

        private Bitmap buffer;
        private Graphics bufferGraphics;

        private bool usingLeftSide = false;

        private const int DangerRadius = 420;
        private const double SlideFactor = 0.15;
        private const double EmergencySlideFactor = 0.92;

        private double currentX;
        private double currentY;

        private Rectangle lastCaptureRect;

        private bool elevationWarning = false;

        private const int BannerOpacity = 200;

        private const int WH_MOUSE_LL = 14;
        private const int WM_MOUSEWHEEL = 0x020A;

        private static IntPtr hookHandle = IntPtr.Zero;
        private static LowLevelMouseProc hookCallback;

        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public int mouseData;
            public int flags;
            public int time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll")]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll")]
        private static extern IntPtr WindowFromPoint(POINT pt);

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        public OverlayForm()
        {
            InitializeComponent();

            this.FormBorderStyle = FormBorderStyle.None;
            this.TopMost = true;
            this.ShowInTaskbar = false;

            this.Width = MAG_SIZE;
            this.Height = MAG_SIZE;

            usingLeftSide = false;

            Point cursor = Cursor.Position;
            Rectangle screen = Screen.FromPoint(cursor).Bounds;

            currentX = screen.Right - MAG_SIZE - 20;
            currentY = screen.Bottom - MAG_SIZE - 20;

            buffer = new Bitmap(MAG_SIZE, MAG_SIZE);
            bufferGraphics = Graphics.FromImage(buffer);

            timer = new Timer();
            timer.Interval = 16;
            timer.Tick += Timer_Tick;
            timer.Start();

            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.UserPaint |
                          ControlStyles.OptimizedDoubleBuffer, true);
            this.DoubleBuffered = true;

            lastCaptureRect = new Rectangle(Cursor.Position.X, Cursor.Position.Y, MIN_CAPTURE_SIZE, MIN_CAPTURE_SIZE);

            CheckElevation();
            InstallMouseHook();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            timer.Stop();
            timer.Dispose();

            bufferGraphics.Dispose();
            buffer.Dispose();

            RemoveMouseHook();

            base.OnFormClosed(e);
        }

        private void CheckElevation()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);

            elevationWarning = !principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private void InstallMouseHook()
        {
            hookCallback = HookProc;
            IntPtr hModule = GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName);
            hookHandle = SetWindowsHookEx(WH_MOUSE_LL, hookCallback, hModule, 0);
        }

        private void RemoveMouseHook()
        {
            if (hookHandle != IntPtr.Zero)
            {
                UnhookWindowsHookEx(hookHandle);
                hookHandle = IntPtr.Zero;
            }
        }

        private IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam.ToInt32() == WM_MOUSEWHEEL && this.Visible)
            {
                MSLLHOOKSTRUCT data = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);

                int delta = (short)((data.mouseData >> 16) & 0xFFFF);

                Rectangle mvRect = new Rectangle(
                    (int)currentX,
                    (int)currentY,
                    MAG_SIZE,
                    MAG_SIZE);

                Point cursorPoint = new Point(data.pt.X, data.pt.Y);
                bool cursorInsideMV = mvRect.Contains(cursorPoint);

                if (cursorInsideMV)
                    return (IntPtr)1;

                if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
                {
                    AdjustZoom(delta > 0 ? +1 : -1);
                    return (IntPtr)1;
                }
                else
                {
                    POINT pt;
                    pt.X = data.pt.X;
                    pt.Y = data.pt.Y;

                    IntPtr hWnd = WindowFromPoint(pt);
                    if (hWnd != IntPtr.Zero)
                    {
                        SendMessage(hWnd, WM_MOUSEWHEEL, (IntPtr)data.mouseData, (IntPtr)((pt.Y << 16) | (pt.X & 0xFFFF)));
                        return (IntPtr)1;
                    }
                }
            }

            return CallNextHookEx(hookHandle, nCode, wParam, lParam);
        }

        public void AdjustZoom(int delta)
        {
            if (delta > 0)
                magFactor += 0.1;
            else
                magFactor -= 0.1;

            if (magFactor < MIN_MAG)
                magFactor = MIN_MAG;

            if (magFactor > MAX_MAG)
                magFactor = MAX_MAG;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (this.IsDisposed)
                return;

            Point cursor = Cursor.Position;
            Rectangle screen = Screen.FromPoint(cursor).Bounds;

            int captureSize = (int)(MAG_SIZE / magFactor);
            if (captureSize < MIN_CAPTURE_SIZE)
                captureSize = MIN_CAPTURE_SIZE;

            Rectangle captureRect = new Rectangle(
                cursor.X - captureSize / 2,
                cursor.Y - captureSize / 2,
                captureSize,
                captureSize);

            if (captureRect.Left < screen.Left) captureRect.X = screen.Left;
            if (captureRect.Top < screen.Top) captureRect.Y = screen.Top;
            if (captureRect.Right > screen.Right) captureRect.X = screen.Right - captureRect.Width;
            if (captureRect.Bottom > screen.Bottom) captureRect.Y = screen.Bottom - captureRect.Height;

            lastCaptureRect = captureRect;

            int targetX_Right = screen.Right - MAG_SIZE - 20;
            int targetX_Left = screen.Left + 20;
            int targetY = screen.Bottom - MAG_SIZE - 20;

            Rectangle mvRect = new Rectangle(
                (int)currentX,
                (int)currentY,
                MAG_SIZE,
                MAG_SIZE);

            Point mvCenter = new Point(
                mvRect.Left + mvRect.Width / 2,
                mvRect.Top + mvRect.Height / 2);

            double dx = cursor.X - mvCenter.X;
            double dy = cursor.Y - mvCenter.Y;
            double distance = Math.Sqrt(dx * dx + dy * dy);

            bool cursorInsideMV = mvRect.Contains(cursor);

            if (distance < DangerRadius && !cursorInsideMV)
                usingLeftSide = !usingLeftSide;

            int targetX = usingLeftSide ? targetX_Left : targetX_Right;

            double factor = cursorInsideMV ? EmergencySlideFactor : SlideFactor;

            currentX = currentX + (targetX - currentX) * factor;
            currentY = currentY + (targetY - currentY) * factor;

            this.Location = new Point((int)currentX, (int)currentY);

            using (Bitmap bmp = new Bitmap(captureRect.Width, captureRect.Height))
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(captureRect.Location, Point.Empty, captureRect.Size);

                bufferGraphics.Clear(Color.Black);
                bufferGraphics.DrawImage(bmp, 0, 0, MAG_SIZE, MAG_SIZE);
            }

            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            e.Graphics.DrawImage(buffer, 0, 0);

            DrawMVBorder(e);
            DrawReticle(e);
            DrawBanner(e);
        }

        private void DrawMVBorder(PaintEventArgs e)
        {
            Color yellow = Color.FromArgb(255, 255, 240, 0);

            using (var borderPen = new Pen(yellow, 5f))
            {
                e.Graphics.DrawRectangle(borderPen, 0, 0, MAG_SIZE - 1, MAG_SIZE - 1);
            }
        }

        private void DrawReticle(PaintEventArgs e)
        {
            Color coreColor = GetReticleColor(lastCaptureRect);

            Color ringColor = Color.FromArgb(90, 255, 240, 0);

            float centerX = MAG_SIZE / 2f;
            float centerY = MAG_SIZE / 2f;

            float outerRadius = 48f;
            float innerRadius = outerRadius - 5f;

            using (GraphicsPath ringPath = new GraphicsPath())
            {
                ringPath.AddEllipse(centerX - outerRadius, centerY - outerRadius, outerRadius * 2, outerRadius * 2);
                ringPath.AddEllipse(centerX - innerRadius, centerY - innerRadius, innerRadius * 2, innerRadius * 2);

                using (Brush ringBrush = new SolidBrush(ringColor))
                {
                    e.Graphics.FillPath(ringBrush, ringPath);
                }
            }

            using (var corePen = new Pen(coreColor, 4f))
            {
                e.Graphics.DrawLine(corePen, centerX - 6, centerY, centerX + 6, centerY);
                e.Graphics.DrawLine(corePen, centerX, centerY - 6, centerX, centerY + 6);
            }
        }

        private Color GetReticleColor(Rectangle captureRect)
        {
            if (captureRect.Width <= 0 || captureRect.Height <= 0)
                return Color.White;

            int cx = captureRect.Width / 2;
            int cy = captureRect.Height / 2;

            using (Bitmap sample = new Bitmap(captureRect.Width, captureRect.Height))
            using (Graphics g = Graphics.FromImage(sample))
            {
                g.CopyFromScreen(captureRect.Location, Point.Empty, captureRect.Size);
                Color c = sample.GetPixel(cx, cy);

                double brightness =
                    (c.R * 0.2126) +
                    (c.G * 0.7152) +
                    (c.B * 0.0722);

                return brightness > 128 ? Color.Black : Color.White;
            }
        }

        private void DrawBanner(PaintEventArgs e)
        {
            string line1 = "F8 to exit";
            string line2 = "Ctrl+Scroll to Zoom ±";
            string line3 = $"Current zoom level: {(int)(magFactor * 100)}%";
            string line4 = elevationWarning
                ? "Not running as admin, Run as admin for full scroll-through support"
                : "Running as admin full scroll-through works in elevated windows";

            using (Font f = new Font("Segoe UI", 10, FontStyle.Regular))
            {
                int bannerHeight = 100;

                Rectangle bannerRect = new Rectangle(
                    0,
                    MAG_SIZE - bannerHeight,
                    MAG_SIZE,
                    bannerHeight);

                using (GraphicsPath path = new GraphicsPath())
                {
                    int radius = 12;
                    int x = bannerRect.X;
                    int y = bannerRect.Y;
                    int w = bannerRect.Width;
                    int h = bannerRect.Height;

                    path.AddArc(x, y, radius * 2, radius * 2, 180, 90);
                    path.AddArc(x + w - radius * 2, y, radius * 2, radius * 2, 270, 90);
                    path.AddLine(x + w, y + radius * 2, x + w, y + h);
                    path.AddLine(x + w, y + h, x, y + h);
                    path.AddLine(x, y + h, x, y + radius * 2);
                    path.CloseFigure();

                    using (Brush bg = new SolidBrush(Color.FromArgb(BannerOpacity, 180, 180, 180)))
                    {
                        e.Graphics.FillPath(bg, path);
                    }
                }

                using (Brush fg = new SolidBrush(Color.FromArgb(240, 20, 20, 40)))
                {
                    float xText = 20;
                    float yText = MAG_SIZE - bannerHeight + 10;

                    e.Graphics.DrawString(line1, f, fg, xText, yText);
                    e.Graphics.DrawString(line2, f, fg, xText, yText + 20);
                    e.Graphics.DrawString(line3, f, fg, xText, yText + 40);
                    e.Graphics.DrawString(line4, f, fg, xText, yText + 60);
                }
            }
        }
    }
}
