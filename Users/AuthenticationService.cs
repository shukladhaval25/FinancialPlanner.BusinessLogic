using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FinancialPlanner.Common.Model;
using System.Net.Mail;

namespace FinancialPlanner.BusinessLogic.Users
{
    public class AuthenticationService
    {
        private UserService _userService;
        public AuthenticationService()
        {
            _userService = new UserService();
        }
        public User ValidateAuthentication(User user)
        {
            var userData =  _userService.GetUserByName(user.UserName);
            if (user.Password == userData.Password)
            {
                //Activity.ActivitiesService.Add(ActivityType.ServerLogin, EntryStatus.Success,
                //    Source.Server, user.UserName, string.Empty, user.MachineName);
                //sendAutheticationFailEmail();
                return userData;
            }
            else
            {
                Activity.ActivitiesService.Add(ActivityType.LoginFail, EntryStatus.Success,
                   Source.Server, user.UserName, string.Empty, user.MachineName);
                //sendAutheticationFailEmail();
                throw new Exception("Authentication Fail due to invalid credential");
            }
        }

        private static void sendAutheticationFailEmail()
        {
            EmailService emailService = new EmailService();
            MailMessage mailMsg = new MailMessage();
            mailMsg.To.Add("shukla_dhaval@yahoo.com");
            mailMsg.Subject = "Authentication Fail.";
            mailMsg.Body = "This is email to inform you that authentication process fail.";

            emailService.SendEmail(mailMsg);
        }

        public User ValidateClientAuthentication(User user)
        {
            var userData =  _userService.GetUserByName(user.UserName);
            if (user.Password == userData.Password)
            {
                //Activity.ActivitiesService.Add(ActivityType.ClientLogin, EntryStatus.Success,
                //    Source.Client, user.UserName, string.Empty,user.MachineName);
                return userData;
            }
            else
            {
                Activity.ActivitiesService.Add(ActivityType.LoginFail, EntryStatus.Success,
                   Source.Client, user.UserName, string.Empty, string.Empty);
                throw new Exception("Authentication Fail due to invalid credential");
            }
        }
    }
}
