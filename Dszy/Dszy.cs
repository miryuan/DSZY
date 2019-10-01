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
            LogFileHelper.WriteLog("启动了服务。");

            ThreadPool.QueueUserWorkItem(new WaitCallback(MonitorTimer_Elapsed));
            ThreadPool.QueueUserWorkItem(new WaitCallback(MonitorClose));
        }

        /// <summary>
        /// 监控开启
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MonitorTimer_Elapsed(object state)
        {
            try
            {
                LogFileHelper.WriteLog("MonitorTimer_Elapsed");
                //延迟2分钟
                int i = 59;
                while (i >= 0 && !isSendStartMail)
                {
                    Thread.Sleep(1000);
                    i--;
                }

                //启动
                Task.Run(SendWakeupMailAsync);

            }
            catch (Exception e)
            {
                LogFileHelper.WriteLog("MonitorTimer_Elapsed 报错：" + e.Message);
            }
            LogFileHelper.WriteLog("MonitorTimer_Elapsed 执行结束.");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        private void MonitorClose(object state)
        {
            LogFileHelper.WriteLog("MonitorClose");
            connectNetTimer.Elapsed += new System.Timers.ElapsedEventHandler((obj, eventArg) =>
            {
                int min = wait.Next(1, 45);
                LogFileHelper.WriteLog("随机值：" + min);
                if (min == 1)
                {
                    CloseOne();
                    //信息
                    Task.Run(SendInfoMailAsync);
                }
            });
            connectNetTimer.Interval = 60000;//毫秒 1秒=1000毫秒
            connectNetTimer.Enabled = true;//必须加上
            connectNetTimer.AutoReset = true;//执行一次 false，一直执行true 
            connectNetTimer.Start();
            LogFileHelper.WriteLog("MonitorClose 执行结束.");
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
            LogFileHelper.WriteLog("SendWakeupMailAsync");
            isSendStartMail = true;
            var subject = "Wakeup";
            var content = MachineInfo();
            await EMailHelper.SendEMailAsync(subject, content, new MailboxAddress[] {
                new MailboxAddress("150101977@qq.com")
            });
        }

        private async Task SendInfoMailAsync()
        {
            LogFileHelper.WriteLog("SendInfoMailAsync");
            var subject = "InfoNow";
            string content = ProcessInfo();
            await EMailHelper.SendEMailAsync(subject, content, new MailboxAddress[] {
                new MailboxAddress("150101977@qq.com")
            });
        }

        private async Task SendShutDownMailAsync()
        {
            LogFileHelper.WriteLog("SendShutDownMailAsync");
            var subject = "ShutDown";
            string content = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " ShutDown";
            await EMailHelper.SendEMailAsync(subject, content, new MailboxAddress[] {
                new MailboxAddress("150101977@qq.com")
            });
        }
    }
}
