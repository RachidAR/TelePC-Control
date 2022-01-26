using System.Diagnostics;
using System.Management;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;

public static class HardwareInfo
{

    public static string UserName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
    public static string GetAccountName()
    {

        ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_UserAccount");

        foreach (ManagementObject wmi in searcher.Get())
        {
            try
            {

                return wmi.GetPropertyValue("Name").ToString();
            }
            catch { }
        }
        return "User Account Name: Unknown";

    }
    public static string GetPhysicalMemory()
    {
        ManagementScope oMs = new ManagementScope();
        ObjectQuery oQuery = new ObjectQuery("SELECT Capacity FROM Win32_PhysicalMemory");
        ManagementObjectSearcher oSearcher = new ManagementObjectSearcher(oMs, oQuery);
        ManagementObjectCollection oCollection = oSearcher.Get();

        long MemSize = 0;
        long mCap = 0;

        foreach (ManagementObject obj in oCollection)
        {
            mCap = Convert.ToInt64(obj["Capacity"]);
            MemSize += mCap;
        }
        MemSize = (MemSize / 1024) / 1024;
        return MemSize.ToString() + "MB";
    }
    public static int GetCPUCurrentClockSpeed()
    {
        int cpuClockSpeed = 0;
        ManagementClass mgmt = new ManagementClass("Win32_Processor");
        ManagementObjectCollection objCol = mgmt.GetInstances();
        foreach (ManagementObject obj in objCol)
        {
            if (cpuClockSpeed == 0)
            {
                cpuClockSpeed = Convert.ToInt32(obj.Properties["CurrentClockSpeed"].Value.ToString());
            }
        }
        return cpuClockSpeed;
    }
    public static double? GetCpuSpeedInGHz()
    {
        double? GHz = null;
        using (ManagementClass mc = new ManagementClass("Win32_Processor"))
        {
            foreach (ManagementObject mo in mc.GetInstances())
            {
                GHz = 0.001 * (UInt32)mo.Properties["CurrentClockSpeed"].Value;
                break;
            }
        }
        return GHz;
    }

    public static string GetOSInformation()
    {
        ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");
        foreach (ManagementObject wmi in searcher.Get())
        {
            try
            {
                return ((string)wmi["Caption"]).Trim() + ", " + (string)wmi["Version"] + ", " + (string)wmi["OSArchitecture"];
            }
            catch { }
        }
        return "BIOS Maker: Unknown";
    }
    public static String GetProcessorInformation()
    {
        ManagementClass mc = new ManagementClass("win32_processor");
        ManagementObjectCollection moc = mc.GetInstances();
        String name = String.Empty;
        foreach (ManagementObject mo in moc)
        {
            name = (string)mo["Name"];

            // old style
            // name = name.Replace("(TM)", "™").Replace("(tm)", "™").Replace("(R)", "®").Replace("(r)", "®").Replace("(C)", "©").Replace("(c)", "©").Replace("    ", " ").Replace("  ", " ");

            name = name.Replace("(TM)", "").Replace("(tm)", "").Replace("(R)", "").Replace("(r)", "").Replace("(C)", "").Replace("(c)", "").Replace("    ", " ").Replace("  ", " ").Replace("@", "").Replace("CPU", "");
        }
        return name;
    }
    public static String GetComputerName()
    {
        ManagementClass mc = new ManagementClass("Win32_ComputerSystem");
        ManagementObjectCollection moc = mc.GetInstances();
        String info = String.Empty;
        foreach (ManagementObject mo in moc)
        {
            info = (string)mo["Name"];
        }
        return info;
    }

    public static string GetSystemUpTimeInfo()
    {
        try
        {
            var time = GetSystemUpTime();
            var upTime = string.Format("{0:D2}h:{1:D2}m:{2:D2}s", time.Hours, time.Minutes, time.Seconds);
            return string.Format("{0}", upTime);
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

    private static TimeSpan GetSystemUpTime()
    {
        try
        {
            using var uptime = new PerformanceCounter("System", "System Up Time");
            uptime.NextValue();
            return TimeSpan.FromSeconds(uptime.NextValue());
        }
        catch (Exception)
        {
            return new TimeSpan(0, 0, 0, 0);
        }
    }

    [DllImport("user32.dll")]
    static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

    public static string GetActiveWindowTitle()
    {
        const int nChars = 256;
        StringBuilder Buff = new StringBuilder(nChars);
        IntPtr handle = GetForegroundWindow();

        if (GetWindowText(handle, Buff, nChars) > 0)
        {
            return Buff.ToString();
        }
        return null;
    }
	public static string PingTest()
	{
		Ping myPing = new Ping();
		string result = string.Empty;
        try
		{
			PingReply reply = myPing.Send("google.com", 500);
			result = reply.RoundtripTime.ToString();
			if(reply.RoundtripTime > 65) result += " [Bad internet ⚠]";
		}
		catch{
            result = "Error";
        }
		return result;
	}

}