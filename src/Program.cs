// Program.cs 07242026 10:25:00
using System;
using System.Windows.Forms;

namespace CursorMagnifier
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
