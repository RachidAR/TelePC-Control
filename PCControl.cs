using System.Diagnostics;
using System.Runtime.InteropServices;

namespace PC_Control
{
    public static class PC
    {
        [DllImport("PowrProf.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern bool SetSuspendState(bool hiberate, bool forceCritical, bool disableWakeEvent);

        public static string filePath = string.Empty;
        public static string filePathListProc = string.Empty;
        private static string listProcess = string.Empty;

        public static void Lock()
        {
            Process.Start(@"C:\WINDOWS\system32\rundll32.exe", "user32.dll,LockWorkStation");
        }
        public static void ShutDown()
        {
            Process.Start("shutdown", "/s /t 0");
        }
        public static void Restart()
        {
            Process.Start("shutdown", "/r /t 0");
        }

        public static void Screenshot()
        {
            Bitmap memoryImage;
            memoryImage = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            Size s = new Size(memoryImage.Width, memoryImage.Height);

            Graphics memoryGraphics = Graphics.FromImage(memoryImage);

            memoryGraphics.CopyFromScreen(0, 0, 0, 0, s);

            filePath = string.Format(AppDomain.CurrentDomain.BaseDirectory) +
                      @"screenshot\" + "ScreenShot" + ".jpg";

            if (!Directory.Exists(string.Format(AppDomain.CurrentDomain.BaseDirectory) + @"screenshot\"))
                Directory.CreateDirectory(string.Format(AppDomain.CurrentDomain.BaseDirectory) + @"screenshot\");

            memoryImage.Save(filePath);
        }
        public static void ListProcesses()
        {
            Process[] processCollection = Process.GetProcesses();
            foreach (Process p in processCollection)
            {
                listProcess += $"Process : {p.ProcessName} ID : {p.Id}\n";
            }

            filePathListProc = string.Format(AppDomain.CurrentDomain.BaseDirectory) +
                      @"temp\" + "listProc" + ".txt";

            if (!Directory.Exists(string.Format(AppDomain.CurrentDomain.BaseDirectory) + @"temp\"))
                Directory.CreateDirectory(string.Format(AppDomain.CurrentDomain.BaseDirectory) + @"temp\");

            File.WriteAllTextAsync(filePathListProc, listProcess);
            listProcess = string.Empty;
            return;
        }

    }
}
