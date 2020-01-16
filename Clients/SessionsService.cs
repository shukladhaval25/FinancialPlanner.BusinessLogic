using FinancialPlanner.Common;
using FinancialPlanner.Common.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Reflection;

namespace FinancialPlanner.BusinessLogic.Clients
{
    public class SessionsService
    {
        private readonly string SELECT_ALL = "SELECT [ClientId],[SessionName]," +
            "[SessionDate],[IsCovered],[Note],[CreatedOn],[CreatedBy],[UpdatedOn]," +
            "[UpdatedBy]  FROM[FinancialPlanner].[dbo].[ClientSession] " +
            "where clientId = {0}";

        private readonly string DELETE_BY_ID = "DELETE FROM [dbo].[ClientSession]" +
            " WHERE clientId = {0}";

        public readonly string INSERT_QUERY = "INSERT INTO [dbo].[ClientSession] " +
            "([ClientId],[SessionName],[SessionDate],[IsCovered],[Note],[CreatedOn],[CreatedBy], [UpdatedOn],[UpdatedBy])" +
            " VALUES ({0},'{1}','{2}','{3}','{4}','{5}',{6},'{7}',{8})";

        public IList<Sessions> Get(int clientId)
        {
            try
            {
                Logger.LogInfo("Get: Sessions process start");
                IList<Sessions> lstSessions = new List<Sessions>();

                DataTable dtAppConfig = DataBase.DBService.ExecuteCommand(string.Format(SELECT_ALL, clientId));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    Sessions Sessions = convertToSessionsObject(dr);
                    lstSessions.Add(Sessions);
                }
                Logger.LogInfo("Get: Sessions process completed.");
                return lstSessions;
            }
            catch (Exception ex)
            {
                StackTrace st = new StackTrace();
                StackFrame sf = st.GetFrame(0);
                MethodBase currentMethodName = sf.GetMethod();
                LogDebug(currentMethodName.Name, ex);
                return null;
            }
        }

        private Sessions convertToSessionsObject(DataRow dr)
        {
            Sessions sessions = new Sessions();
            sessions.ClientId = dr.Field<int>("ClientId");
            sessions.SessionName = dr.Field<string>("SessionName");
            sessions.IsCoverd = dr.Field<bool>("IsCovered");
            sessions.SessionDate = dr.Field<DateTime>("SessionDate");
            sessions.Notes = dr.Field<string>("Note");
            return sessions;
        }

        public void Add(IList<Sessions> SessionsList)
        {
            try
            {
                DataBase.DBService.ExecuteCommand(string.Format(DELETE_BY_ID, SessionsList[0].ClientId));

                foreach (Sessions Sessions in SessionsList)
                {
                    if (!string.IsNullOrEmpty(Sessions.SessionName))
                        DataBase.DBService.ExecuteCommand(string.Format(INSERT_QUERY,
                            Sessions.ClientId,
                            Sessions.SessionName,
                            Sessions.SessionDate,
                            Sessions.IsCoverd,
                            Sessions.Notes.Replace("'","''"),
                            Sessions.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss"), Sessions.CreatedBy,
                            Sessions.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), Sessions.UpdatedBy));

                    //Activity.ActivitiesService.Add(ActivityType.CreateSessions, EntryStatus.Success,
                    //         Source.Server, Sessions.UpdatedByUserName, Sessions.SessionName, Sessions.MachineName);
                }
            }
            catch (Exception ex)
            {
                StackTrace st = new StackTrace();
                StackFrame sf = st.GetFrame(0);
                MethodBase currentMethodName = sf.GetMethod();
                LogDebug(currentMethodName.Name, ex);
                throw ex;
            }
        }

        public void Delete(Sessions Sessions)
        {
            try
            {
                DataBase.DBService.ExecuteCommand(string.Format(DELETE_BY_ID, Sessions.ClientId, Sessions.SessionName));
                //Activity.ActivitiesService.Add(ActivityType.DeleteSessions, EntryStatus.Success,
                //         Source.Server, Sessions.UpdatedByUserName, Sessions.SessionName, Sessions.MachineName);
            }
            catch (Exception ex)
            {
                StackTrace st = new StackTrace();
                StackFrame sf = st.GetFrame(0);
                MethodBase currentMethodName = sf.GetMethod();
                LogDebug(currentMethodName.Name, ex);
                throw ex;
            }
        }


        private void LogDebug(string methodName, Exception ex)
        {
            DebuggerLogInfo debuggerInfo = new DebuggerLogInfo();
            debuggerInfo.ClassName = this.GetType().Name;
            debuggerInfo.Method = methodName;
            debuggerInfo.ExceptionInfo = ex;
            Logger.LogDebug(debuggerInfo);
        }
    }
}
