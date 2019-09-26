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
        bool isSendStartMail = false;

        /// <summary>
        /// 监控定时任务
        /// </summary>
        System.Timers.Timer monitorTimer = new System.Timers.Timer();
        System.Timers.Timer connectNetTimer = new System.Timers.Timer();
        public Dszy()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            EMailHelper.Host = "smtp.qq.com";
            EMailHelper.Port = 465;
            EMailHelper.UseSsl = true;
            EMailHelper.UserName = "390059127@qq.com";
            EMailHelper.Password = "phykougiygxcbjif";
            EMailHelper.UserAddress = "390059127@qq.com";

            monitorTimer.Elapsed += MonitorTimer_Elapsed;
            monitorTimer.Interval = 60000;//毫秒 1秒=1000毫秒
            monitorTimer.Enabled = true;//必须加上
            monitorTimer.AutoReset = false;//执行一次 false，一直执行true 
            monitorTimer.Start();
        }

        /// <summary>
        /// 监控开启
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MonitorTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                int i = 10;
                while (i >= 0 && !isSendStartMail)
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
            Task.Run(SendShutDownMailAsync);

            monitorTimer.Stop();
            monitorTimer.Dispose();
            connectNetTimer.Stop();
            connectNetTimer.Dispose();
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
            info.AppendLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " Wake UP");
            info.AppendLine("CPU:\r\n" + MachineHelper.CpuInfo());
            info.AppendLine("HardDisk:\r\n" + MachineHelper.HardDiskInfo());
            info.AppendLine("Memory:\r\n" + MachineHelper.MemoryInfo());
            info.AppendLine("OS:\r\n" + MachineHelper.OSInfo());
            return info.ToString();
        }

        private async Task SendWakeupMailAsync()
        {
            isSendStartMail = true;
            var subject = "Wakeup";
            var content = MachineInfo();
            await EMailHelper.SendEMailAsync(subject, content, new MailboxAddress[] {
                new MailboxAddress("150101977@qq.com")
            });
        }

        private async Task SendInfoMailAsync()
        {
            var subject = "InfoNow";
            string content = ProcessInfo();
            await EMailHelper.SendEMailAsync(subject, content, new MailboxAddress[] {
                new MailboxAddress("150101977@qq.com")
            });
        }

        private async Task SendShutDownMailAsync()
        {
            var subject = "ShutDown";
            string content = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " ShutDown";
            await EMailHelper.SendEMailAsync(subject, content, new MailboxAddress[] {
                new MailboxAddress("150101977@qq.com")
            });
        }
    }
}
