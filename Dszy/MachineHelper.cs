using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Management;
using System.Text;

namespace Dszy
{
    public enum WmiType
    {
        Win32_Processor,
        Win32_PerfFormattedData_PerfOS_Memory,
        Win32_PhysicalMemory,
        Win32_NetworkAdapterConfiguration,
        Win32_LogicalDisk
    }

    public class MachineHelper
    {
        static Dictionary<string, ManagementObjectCollection> WmiDict = new Dictionary<string, ManagementObjectCollection>();

        static MachineHelper()
        {
            var names = Enum.GetNames(typeof(WmiType));
            foreach (string name in names)
            {
                WmiDict.Add(name, new ManagementObjectSearcher("SELECT * FROM " + name).Get());
            }
        }

        /// <summary>
        /// 获取硬盘号码
        /// </summary>
        /// <returns></returns>
        public static string GetHardDiskNumber()
        {
            var query = WmiDict[WmiType.Win32_LogicalDisk.ToString()];
            //var collection = query.Get();

            string result = string.Empty;
            foreach (var obj in query)
            {
                result = obj["VolumeSerialNumber"].ToString();
                break;
            }

            return result;
        }

        /// <summary>
        /// 获取CPU号码
        /// </summary>
        /// <returns></returns>
        public static string GetCPUNumber()
        {
            var query = WmiDict[WmiType.Win32_Processor.ToString()];
            //var collection = query.Get();

            string result = string.Empty;
            foreach (var obj in query)
            {
                result = obj["Processorid"].ToString();
                break;
            }

            return result;
        }

        /// <summary>
        /// 获取内存编号
        /// </summary>
        /// <returns></returns>
        public static string GetMemoryNumber()
        {
            var query = WmiDict[WmiType.Win32_PhysicalMemory.ToString()];
            //var collection = query.Get();

            string result = string.Empty;
            foreach (var obj in query)
            {
                result = obj["PartNumber"].ToString();
                break;
            }
            return result;
        }

        /// <summary>
        /// 获取硬盘信息
        /// </summary>
        /// <returns></returns>
        public static string HardDiskInfo()
        {
            DriveInfo[] drives = DriveInfo.GetDrives();
            StringBuilder sr = new StringBuilder();
            foreach (DriveInfo drive in drives)
            {
                if (drive.IsReady)
                {
                    var val1 = (double)drive.TotalSize / 1024 / 1024;
                    var val2 = (double)drive.TotalFreeSpace / 1024 / 1024;
                    sr.AppendLine(string.Format("盘符:[{0}] 格式:[{1}] 容量:{2}MB/{3}MB  可用比例{4}%", drive.Name, drive.DriveFormat, (long)val1, (long)val2, string.Format("{0:F2}", val2 / val1 * 100)));
                }
            }
            return sr.ToString();
        }

        /// <summary>
        /// 获取操作系统信息
        /// </summary>
        /// <returns></returns>
        public static string OSInfo()
        {
            StringBuilder sr = new StringBuilder();
            sr.AppendLine(string.Format("机器名:{0};", Environment.MachineName));
            sr.AppendLine(string.Format("操作系统:{0};", Environment.OSVersion));
            sr.AppendLine(string.Format("系统文件夹:{0};", Environment.SystemDirectory));
            sr.AppendLine(string.Format("语言:{0};", CultureInfo.InstalledUICulture.EnglishName));
            sr.AppendLine(string.Format(".NET:{0};", Environment.Version));
            sr.AppendLine(string.Format("当前目录:{0};", Environment.CurrentDirectory));
            sr.AppendLine(string.Format("当前用户:{0};", Environment.UserName));
            return sr.ToString();
        }

        /// <summary>
        /// 获取网卡信息
        /// </summary>
        /// <returns></returns>
        //public static string NetworkInfo()
        //{
        //    StringBuilder sr = new StringBuilder();

        //    string host = Dns.GetHostName();
        //    IPHostEntry ipEntry = Dns.GetHostByName(host);
        //    sr.Append("IPv4:" + ipEntry.AddressList[0] + "/");

        //    sr.Append("IPv6:");
        //    ipEntry = Dns.GetHostEntry(host);
        //    sr.Append("IPv6:" + ipEntry.AddressList[0] + ";");

        //    sr.Append("MAC:");
        //    var query = WmiDict[WmiType.Win32_NetworkAdapterConfiguration.ToString()];
        //    foreach (var obj in query)
        //    {
        //        if (obj["IPEnabled"].ToString() == "True")
        //            sr.Append(obj["MacAddress"] + ";");
        //    }

        //    return sr.ToString();
        //}

        /// <summary>
        /// 获取内存信息
        /// </summary>
        /// <returns></returns>
        public static string MemoryInfo()
        {
            StringBuilder sr = new StringBuilder();
            long capacity = 0;
            var query = WmiDict[WmiType.Win32_PhysicalMemory.ToString()];
            int index = 1;
            foreach (var obj in query)
            {
                sr.AppendLine("内存" + index + " -> 频率:" + obj["ConfiguredClockSpeed"] + ";");
                capacity += Convert.ToInt64(obj["Capacity"]);
                index++;
            }
            sr.AppendLine("总物理内存:"+ capacity / 1024 / 1024 + "MB;");
            query = WmiDict[WmiType.Win32_PerfFormattedData_PerfOS_Memory.ToString()];
            sr.AppendLine("总可用内存:");
            long available = 0;
            foreach (var obj in query)
            {
                available += Convert.ToInt64(obj.Properties["AvailableMBytes"].Value);
            }
            sr.Append(available + "MB;");
            sr.AppendFormat("{0:F2}%可用; ", (double)available / (capacity / 1024 / 1024) * 100);

            return sr.ToString();
        }

        /// <summary>
        /// 获取CPU信息
        /// </summary>
        /// <returns></returns>
        public static string CpuInfo()
        {
            StringBuilder sr = new StringBuilder();

            var query = WmiDict[WmiType.Win32_Processor.ToString()];
            foreach (var obj in query)
            {
                sr.AppendLine("厂商:" + obj["Manufacturer"] + ";");
                sr.AppendLine("产品名称:" + obj["Name"] + ";");
                sr.AppendLine("最大频率:" + obj["MaxClockSpeed"] + ";");
                sr.AppendLine("当前频率:" + obj["CurrentClockSpeed"] + ";");
            }

            return sr.ToString();
        }
    }
}
