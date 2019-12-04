using FinancialPlanner.Common;
using FinancialPlanner.Common.Model.TaskManagement;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FinancialPlanner.BusinessLogic.TaskManagements
{
    public class TaskHistoryService
    {
        private readonly string SELECT_TASK_HISTORY = "SELECT TH.*, U.UserName FROM[FinancialPlanner].[dbo].[TaskHistory] TH, " +
            "Users U WHERE TH.UPDATEDBY = U.ID AND TH.TaskId = {0} order by UpdatedOn desc";

        public object GetAll(int taskId)
        {
            try
            {
                Logger.LogInfo("Get: Task history process start");
                IList<TaskHistory> taskHistories =
                    new List<TaskHistory>();

                DataTable dtAppConfig = DataBase.DBService.ExecuteCommand(
                   string.Format(SELECT_TASK_HISTORY, taskId));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    TaskHistory task = convertToTaskHistory(dr);
                    taskHistories.Add(task);
                }
                Logger.LogInfo("Get: Task history process completed.");
                return taskHistories;
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

        private TaskHistory convertToTaskHistory(DataRow dr)
        {
            TaskHistory taskHistory = new TaskHistory();
            taskHistory.Id = dr.Field<int>("ID");
            taskHistory.TaskId = dr.Field<int>("TaskId");
            taskHistory.FieldName = dr.Field<string>("FieldName");
            taskHistory.OldValue = dr.Field<string>("OldValue");
            taskHistory.NewValue = dr.Field<string>("NewValue");
            taskHistory.Username = dr.Field<string>("UserName");
            taskHistory.UpdatedBy = dr.Field<int>("UpdatedBy");
            taskHistory.UpdatedOn = dr.Field<DateTime>("UpdatedOn");
            return taskHistory;
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
