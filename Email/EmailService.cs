using FinancialPlanner.Common;
using FinancialPlanner.Common.Model;
using System;
using System.Data;
using System.Net.Mail;

namespace FinancialPlanner.BusinessLogic
{
    public class EmailService
    {
        Common.Model.Email  _emailobj;
        DataTable _dtSMTPSetting;
        private const string GET_SMTP_SETTING_QUERY = "SELECT * FROM APPLICATIONCONFIGURATION WHERE CATEGORY = 'SMTP Setting'";
        public EmailService ()
        {
            _emailobj = new Common.Model.Email();
            _dtSMTPSetting = DataBase.DBService.ExecuteCommand(GET_SMTP_SETTING_QUERY);
            foreach (DataRow dr in _dtSMTPSetting.Rows)
            {
                if (dr.Field<string>("SettingKey") == "FromEmail")
                {
                   _emailobj.FromEmail = dr.Field<string>("Value");
                }
                else if (dr.Field<string>("SettingKey") == "SMTPPort")
                {
                   _emailobj.SMTPPort = int.Parse(dr.Field<string>("Value"));
                }
                else if (dr.Field<string>("SettingKey") == "SMTPHost")
                {
                    _emailobj.SMTPServerHost = dr.Field<string>("Value");
                }
                else if (dr.Field<string>("SettingKey") == "UserName")
                {
                    _emailobj.UserName = dr.Field<string>("Value");
                }
                else if (dr.Field<string>("SettingKey") == "Password")
                {
                    _emailobj.Password = dr.Field<string>("Value");
                }
                else if (dr.Field<string>("SettingKey") == "IsSSL")
                {
                    _emailobj.IsSSL = Boolean.Parse(dr.Field<string>("Value"));
                }
            }
        }
        public void SendEmail(MailMessage mailMsg)
        {
            try
            {
                MailMessage mail = mailMsg;                              
                SmtpClient SmtpServer = new SmtpClient(_emailobj.SMTPServerHost);
                mail.From = new MailAddress(_emailobj.FromEmail);
                SmtpServer.Port = _emailobj.SMTPPort;
                SmtpServer.Credentials = new System.Net.NetworkCredential(_emailobj.UserName, 
                   FinancialPlanner.Common.DataEncrypterDecrypter.CryptoEngine.Decrypt(_emailobj.Password));
                SmtpServer.EnableSsl = _emailobj.IsSSL;
                
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
