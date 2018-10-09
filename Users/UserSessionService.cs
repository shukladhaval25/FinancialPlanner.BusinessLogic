using FinancialPlanner.Common.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FinancialPlanner.BusinessLogic.Users
{
    public class UserSessionService
    {

        private const string INSERT_QUERY = "INSERT INTO USERSESSION VALUES ('{0}','{1}','{2}','{3}')";
        private const string SELECT_BY_NAME = "SELECT * FROM USERSESSION WHERE UID = {0}";
        private const string UPDATE_QUERY = "UPDATE USERSESSION SET USERTOKEN = '{0}'," +
                "LASTPING = '{1}',HostName ='{2}' WHERE UID= {3}";
        private const string DELETE_QUERY = "DELETE FROM USERSESSION WHERE UID = {0}";
        private const string GET_SESSION_COUNT = "SELECT COUNT(*) FROM USERSESSION WHERE UID = {0}";

        public UserSession Get(int userId)
        {
            UserSession userSession = new UserSession();
            DataTable dtAppConfig =  DataBase.DBService.ExecuteCommand(string.Format(SELECT_BY_NAME,userId));
            foreach (DataRow dr in dtAppConfig.Rows)
            {
                userSession = convertToUserSeession(dr);
            }
            return userSession;
        }

        private UserSession convertToUserSeession(DataRow dr)
        {
            UserSession userSession = new UserSession();
            userSession.Id = dr.Field<int>("ID");
            userSession.UserId = dr.Field<int>("UId");
            userSession.UserToken = dr.Field<string>("UserToken");
            userSession.LastPingTime = dr.Field<DateTime>("LastPing");
            userSession.MachineName = dr.Field<string>("HostName");
            return userSession;
        }

        public void AddSession(UserSession userSession)
        {
            try
            {
               string countResult = DataBase.DBService.ExecuteCommandScalar(
                   string.Format(GET_SESSION_COUNT,userSession.UserId));
                if (countResult != "0")
                {
                    DataBase.DBService.ExecuteCommandString(string.Format(DELETE_QUERY, userSession.UserId));
                }

                DataBase.DBService.ExecuteCommandString(string.Format(INSERT_QUERY,
                    userSession.UserId, userSession.UserToken, 
                    userSession.LastPingTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    userSession.MachineName));                
            }
            catch (Exception ex)
            {
                StackTrace st = new StackTrace ();
                StackFrame sf = st.GetFrame (0);
                MethodBase  currentMethodName = sf.GetMethod();
                LogDebug(currentMethodName.Name, ex);
                throw ex;
            }
        }

        private void LogDebug(string name, Exception ex)
        {
            throw new NotImplementedException();
        }

        public void UpdateSession(UserSession userSession)
        {
            try
            {
                DataBase.DBService.ExecuteCommandString(string.Format(UPDATE_QUERY,
                      userSession.UserToken,
                      userSession.LastPingTime.ToString("yyyy-MM-dd HH:mm:ss"),
                      userSession.MachineName, userSession.UserId));
            }
            catch(Exception ex)
            {
                StackTrace st = new StackTrace ();
                StackFrame sf = st.GetFrame (0);
                MethodBase  currentMethodName = sf.GetMethod();
                LogDebug(currentMethodName.Name, ex);
                throw ex;
            }
        }
    }
}
