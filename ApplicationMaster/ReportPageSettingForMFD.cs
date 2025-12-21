using FinancialPlanner.Common.Model.Masters;
using System;
using System.Collections.Generic;
using FinancialPlanner.Common;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using Newtonsoft.Json;

namespace FinancialPlanner.BusinessLogic.ApplicationMaster
{
    public class ReportPageSettingForMFD
    {
        private const string GET_CLIENT_NAME_QUERY = "SELECT NAME FROM CLIENT WHERE ID = {0}";
        private const string SELECT_ALL = "SELECT * FROM ReportPageSettingMFD ";

        private const string UPDATE_QUERY = "UPDATE ReportPageSettingMFD SET IsSelected = {0} WHERE ReportPageName = '{1}'";

        public string Get()
        {
            try
            {
                Logger.LogInfo("Get: Report Page setting for MFD process start");
                IList<ReportPageSetting> reportPageSettings = new List<ReportPageSetting>();

                DataTable dtAppConfig = DataBase.DBService.ExecuteCommand(string.Format(SELECT_ALL));
                Logger.LogInfo("Get: Report Page setting for MFD process completed.");
                string json = JsonConvert.SerializeObject(dtAppConfig, Formatting.Indented);
                return json;
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

        public void Update(ReportPageSetting reportPageSetting)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY, 0));

                DataBase.DBService.ExecuteCommand(string.Format(UPDATE_QUERY,
                   (reportPageSetting.IsSelected) ? 1 : 0,
                   reportPageSetting.ReportPageName));
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
