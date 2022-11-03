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
        public bool Send_PasswordForgot()
        {
            //宣告Mail的空白設定
            MailMessage MyMail = new MailMessage();

            //寄件者Email
            MyMail.From = new MailAddress("t110318084@ntut.org.tw");

            //收件者Email
            MyMail.To.Add("tcher.2009@gmail.com");

            //加入密件副本的Mail(目前沒有使用)
            //MyMail.Bcc.Add("密件副本的收件者Mail");  

            //Email標題
            MyMail.Subject = "測試發送";

            //在信件內顯示URL(信件主體)        
            MyMail.Body = "<label><font color=" + "red" + " size=" + "5" + ">這只是這次mail發送功能是否合用。</font></label><br />";

            //Body是否使用html格式(預設為false)
            MyMail.IsBodyHtml = true;

            //設定Gmail主機&port(寄件者一定要用Gmail 收件者則隨意) 
            SmtpClient MySMTP = new SmtpClient("smtp.ntut.edu.tw", 25);

            //是否使用SSL(Secure Sockets Layer)加密連線
            MySMTP.EnableSsl = true;

            //驗證寄件者
            MySMTP.Credentials = new NetworkCredential("t110318084@ntut.org.tw", "f1302597");

            try
            {
                MySMTP.Send(MyMail);

                //釋放資源
                MyMail.Dispose();
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