using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Web;

namespace TestProject.Models.MailTest
{
    public class sendGmail
    {
        public bool Send_Gmail(string AquariunNum, string Email,string Message)
        {
            MailMessage mail = new MailMessage();

            //前面是發信email
            //後面是顯示的名稱
            mail.From = new MailAddress("tcher.2009@gmail.com", DateTime.Now.ToString("HH: MM")+"(_NotifyTest)");

            //收信者email
            mail.To.Add(Email);

            //設定優先權
            mail.Priority = MailPriority.Normal;

            //標題
            mail.Subject = "AutoEmail";

            //內容
            mail.Body = "<h1>"+"魚缸編號 : "+ AquariunNum + "</h1>" +
                        "<h2>" + Message+"</h2>";

            //內容使用html
            mail.IsBodyHtml = true;

            //設定gmail的smtp (這是google的)
            SmtpClient MySmtp = new SmtpClient("smtp.gmail.com", 587);

            //您在gmail的帳號
            //和應用程式專屬的密碼
            MySmtp.Credentials = new System.Net.NetworkCredential("tcher.2009@gmail.com", "buibadkqenxlyumh");

            //開啟ssl
            MySmtp.EnableSsl = true;


            try
            {
                MySmtp.Send(mail);

                //釋放資源
                MySmtp.Dispose();
            }
            catch (Exception ex)
            {
                ex.ToString();
                return false;
            }
            return true;
        }

        public bool Send_Gmail(string AquariunNum, string Email, List<string> QualityMessage)
        {
            MailMessage mail = new MailMessage();

            //前面是發信email
            //後面是顯示的名稱
            mail.From = new MailAddress("tcher.2009@gmail.com", DateTime.Now.ToString("HH: MM") + "(_NotifyTest)");

            //收信者email
            mail.To.Add(Email);

            //設定優先權
            mail.Priority = MailPriority.Normal;

            //標題
            mail.Subject = "AutoEmail" + DateTime.Now.ToString("HH:MM");

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("魚缸編號 : " + AquariunNum + "<br/>");
            for (int i = 0; i < QualityMessage.Count; i++) {
                sb.AppendLine(QualityMessage[i]+ "<br/>");
            }
            string sbString = sb.ToString();
            //內容
            mail.Body = "<h1>" + sbString + "</h1>";

            //內容使用html
            mail.IsBodyHtml = true;

            //設定gmail的smtp (這是google的)
            SmtpClient MySmtp = new SmtpClient("smtp.gmail.com", 587);

            //您在gmail的帳號
            //和應用程式專屬的密碼
            MySmtp.Credentials = new System.Net.NetworkCredential("tcher.2009@gmail.com", "buibadkqenxlyumh");

            //開啟ssl
            MySmtp.EnableSsl = true;


            try
            {
                MySmtp.Send(mail);

                //釋放資源
                MySmtp.Dispose();
            }
            catch (Exception ex)
            {
                ex.ToString();
                return false;
            }
            return true;
        }

    }
}