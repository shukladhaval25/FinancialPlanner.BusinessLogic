using FinancialPlanner.Common;
using FinancialPlanner.Common.EmailManager;
using FinancialPlanner.Common.Model;
using System;
using System.Data;
using System.Net.Mail;

namespace FinancialPlanner.BusinessLogic
{
    public class EmailService
    {
        //Common.Model.Email  _mailServer;
        DataTable _dtSMTPSetting;
        private const string GET_MAILSERVER_SETTING_QUERY = "SELECT * FROM APPLICATIONCONFIGURATION WHERE CATEGORY = 'Mail Server Setting'";
        public EmailService ()
        {
            //_mailServer = new Common.Model.Email();
            _dtSMTPSetting = DataBase.DBService.ExecuteCommand(GET_MAILSERVER_SETTING_QUERY);
            foreach (DataRow dr in _dtSMTPSetting.Rows)
            {
                if (dr.Field<string>("SettingKey") == "FromEmail")
                {
                   MailServer.FromEmail = dr.Field<string>("Value");
                }
                else if (dr.Field<string>("SettingKey") == "SMTPPort")
                {
                   MailServer.HostPort = int.Parse(dr.Field<string>("Value"));
                }
                else if (dr.Field<string>("SettingKey") == "SMTPHost")
                {
                    MailServer.HostName = dr.Field<string>("Value");
                }
                else if (dr.Field<string>("SettingKey") == "UserName")
                {
                    MailServer.UserName = dr.Field<string>("Value");
                }
                else if (dr.Field<string>("SettingKey") == "Password")
                {
                    MailServer.Password = dr.Field<string>("Value");
                }
                else if (dr.Field<string>("SettingKey") == "IsSSL")
                {
                    MailServer.IsSSL = Boolean.Parse(dr.Field<string>("Value"));
                }
                else if (dr.Field<string>("SettingKey") == "POP3HostName")
                {
                    MailServer.POP3_IMPS_HostName = (dr.Field<string>("Value"));
                }
                else if (dr.Field<string>("SettingKey") == "POP3HostPort")
                {
                    MailServer.POP3_IMPS_HostPort = (dr.Field<string>("Value"));
                }
            }
        }
        public void SendEmail(MailMessage mailMsg)
        {
            try
            {
                MailMessage mail = mailMsg;                              
                SmtpClient SmtpServer = new SmtpClient(MailServer.HostName);
                mail.From = new MailAddress(MailServer.FromEmail);
                SmtpServer.Port = MailServer.HostPort;
                SmtpServer.Credentials = new System.Net.NetworkCredential(MailServer.UserName, 
                   FinancialPlanner.Common.DataEncrypterDecrypter.CryptoEngine.Decrypt(MailServer.Password));
                SmtpServer.EnableSsl = MailServer.IsSSL;
                
                SmtpServer.Send(mail);
            }
            catch (Exception ex)
            {
                Logger.LogDebug(ex.ToString());
                //throw ex;
            }
        }
    }
}
