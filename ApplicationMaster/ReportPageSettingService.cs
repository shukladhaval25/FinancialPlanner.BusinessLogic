using FinancialPlanner.Common;
using FinancialPlanner.Common.Model.Masters;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FinancialPlanner.BusinessLogic.ApplicationMaster
{
    public class ReportPageSettingService
    {
        private const string GET_CLIENT_NAME_QUERY = "SELECT NAME FROM CLIENT WHERE ID = {0}";
        private const string SELECT_ALL = "SELECT * FROM ReportPageSetting ";

        private const string UPDATE_QUERY = "UPDATE ReportPageSetting SET IsSelected = {0} WHERE ReportPageName = '{1}'";

        public IList<ReportPageSetting> Get()
        {
            try
            {
                Logger.LogInfo("Get: Bank process start");
                IList<ReportPageSetting> reportPageSettings = new List<ReportPageSetting>();

                DataTable dtAppConfig = DataBase.DBService.ExecuteCommand(string.Format(SELECT_ALL));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    ReportPageSetting reportPage = convertToReportSettingObject(dr);
                    reportPageSettings.Add(reportPage);
                }
                Logger.LogInfo("Get: Bank process completed.");
                return reportPageSettings;
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
                   (reportPageSetting.IsSelected) ?  1 : 0,
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

        private ReportPageSetting convertToReportSettingObject(DataRow dr)
        {
            ReportPageSetting reportPage = new ReportPageSetting();
            reportPage.ReportPageName = dr.Field<string>("ReportPageName");
            reportPage.IsSelected = bool.Parse(dr["IsSelected"].ToString());
            return reportPage;
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
