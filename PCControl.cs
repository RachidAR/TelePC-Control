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
			var screenPath = string.Format(AppDomain.CurrentDomain.BaseDirectory) + @"screenshot\";
            Bitmap memoryImage;
            memoryImage = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            Size s = new Size(memoryImage.Width, memoryImage.Height);

            Graphics memoryGraphics = Graphics.FromImage(memoryImage);

            memoryGraphics.CopyFromScreen(0, 0, 0, 0, s);

            filePath = screenPath + "ScreenShot" + ".jpg";

            if (!Directory.Exists(screenPath))
                Directory.CreateDirectory(screenPath);

            memoryImage.Save(filePath);
        }
        public static void ListProcesses()
        {
			var tempPath = string.Format(AppDomain.CurrentDomain.BaseDirectory) + @"temp\";
            Process[] processCollection = Process.GetProcesses();
            foreach (Process p in processCollection)
            {
                listProcess += $"Process : {p.ProcessName} ID : {p.Id}\n";
            }

            filePathListProc = tempPath + "listProc" + ".txt";

            if (!Directory.Exists(tempPath))
                Directory.CreateDirectory(tempPath);

            File.WriteAllTextAsync(filePathListProc, listProcess);
            listProcess = string.Empty;
            return;
        }

    }
}
