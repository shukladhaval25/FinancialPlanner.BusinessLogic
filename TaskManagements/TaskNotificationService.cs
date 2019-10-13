using FinancialPlanner.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FinancialPlanner.BusinessLogic.TaskManagements
{
    public class TaskNotificationService
    {
        private readonly string SELECT_COUNT_NOTIFICATION_BY_NOFITYID = "SELECT COUNT(*) FROM TASKNOTIFICATION WHERE NotifyTo = {0}";

        public int GetNotification(int notifyTo)
        {
            try
            {
                Logger.LogInfo("Get: Task notifaction process start");


                int notificationCount = 0;
                int.TryParse(DataBase.DBService.ExecuteCommandScalar(string.Format(SELECT_COUNT_NOTIFICATION_BY_NOFITYID,notifyTo)),
                    out notificationCount);
               
                Logger.LogInfo("Get: Task notifaction process completed.");
                return notificationCount;
            }
            catch (Exception ex)
            {
                StackTrace st = new StackTrace();
                StackFrame sf = st.GetFrame(0);
                MethodBase currentMethodName = sf.GetMethod();
                LogDebug(currentMethodName.Name, ex);
                return 0;
            }
        }

        private void LogDebug(string name, Exception ex)
        {
            DebuggerLogInfo debuggerInfo = new DebuggerLogInfo();
            debuggerInfo.ClassName = this.GetType().Name;
            debuggerInfo.Method = name;
            debuggerInfo.ExceptionInfo = ex;
            Logger.LogDebug(debuggerInfo);
        }
    }
}
