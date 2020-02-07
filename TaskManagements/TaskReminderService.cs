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
    public class TaskReminderService
    {
        //private string SELECT_TASK_REMINDER = "SELECT [Id],[TaskId],[ReminderDate],[ReminderTime],[Description],[ReminderDisplayed] FROM [FinancialPlanner].[dbo].[TaskReminder] WHERE ReminderDisplayed = {0} AND CAST(ReminderTime as time) <= '{1}' AND CAST(REMINDERDATE AS date) <= '{2}'";


        private string SELECT_TASK_REMINDER = "SELECT TOP(1000) TaskReminder.Id, TaskReminder.TaskId, TaskReminder.ReminderDate, TaskReminder.ReminderTime, TaskReminder.Description, TaskReminder.ReminderDisplayed, TaskCard.Owner, TaskCard.AssignTo FROM TaskReminder INNER JOIN TaskCard ON TaskReminder.TaskId = TaskCard.ID  WHERE (TaskReminder.ReminderDisplayed = 0) AND (CAST(TaskReminder.ReminderTime AS time) <= '{0}') AND (CAST(TaskReminder.ReminderDate AS date) <= '{1}') AND ((TaskCard.Owner = {2}) OR (TaskCard.AssignTo = {2}))";

        private string SELECT_REMINDER_HISTORY = "SELECT TOP (1000) TaskReminder.Id, TaskReminder.TaskId, TaskReminder.ReminderDate, TaskReminder.ReminderTime, TaskReminder.Description, TaskReminder.ReminderDisplayed FROM  TaskReminder WHERE TaskReminder.TaskId = {0}";

        private string INSERT_REMINDER = "INSERT INTO [dbo].[TaskReminder] ([TaskId],[ReminderDate],[ReminderTime],[Description],[ReminderDisplayed]) VALUES  ({0},'{1}','{2}','{3}','{4}')";

        private string UPDATE_REMINDER = "UPDATE [dbo].[TaskReminder] SET       [ReminderDisplayed] = '{0}' WHERE [TaskId] = {1} AND[ID] = {2}";

        public IList<TaskReminder> GetReminders(int userId)
        {
            try
            {
                Logger.LogInfo("Get: Task reminder process start");
                IList<TaskReminder> taskReminders =
                    new List<TaskReminder>();

                DataTable dtAppConfig = DataBase.DBService.ExecuteCommand(
                   string.Format(SELECT_TASK_REMINDER, DateTime.Now.ToShortTimeString(), DateTime.Now.ToShortDateString(),userId));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    TaskReminder taskReminder = convertToTaskReminder(dr);
                    taskReminders.Add(taskReminder);
                }
                Logger.LogInfo("Get: Task reminder process completed.");
                return taskReminders;
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

        public void UpdateReminder(TaskReminder taskReminder)
        {
            DataBase.DBService.ExecuteCommandString(string.Format(UPDATE_REMINDER,             taskReminder.ReminderDisplayed,
                   taskReminder.TaskId,
                   taskReminder.Id));
        }

        public IList<TaskReminder> GetTaskReminders(int taskId)
        {
            try
            {
                Logger.LogInfo("Get: Task reminder process start");
                IList<TaskReminder> taskReminders =
                    new List<TaskReminder>();

                DataTable dtAppConfig = DataBase.DBService.ExecuteCommand(
                   string.Format(SELECT_REMINDER_HISTORY, taskId));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    TaskReminder taskReminder = convertToTaskReminder(dr);
                    taskReminders.Add(taskReminder);
                }
                Logger.LogInfo("Get: Task reminder process completed.");
                return taskReminders;
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

        public int AddReminder(TaskReminder taskReminder)
        {
            DataBase.DBService.ExecuteCommandString(string.Format(INSERT_REMINDER,
                   taskReminder.TaskId,
                   taskReminder.ReminderDate.ToString("yyyy-MM-dd"),
                   taskReminder.ReminderTime.ToShortTimeString(),
                   taskReminder.Description,
                   taskReminder.ReminderDisplayed), true);

            //int id = getTaskID(taskcard);
            return 0;
        }

        private TaskReminder convertToTaskReminder(DataRow dr)
        {
            TaskReminder taskReminder = new TaskReminder();
            taskReminder.Id = dr.Field<int>("ID");
            taskReminder.TaskId = dr.Field<int>("TaskId");
            taskReminder.ReminderDate = dr.Field<DateTime>("ReminderDate");
            taskReminder.ReminderTime = DateTime.Parse(dr["ReminderTime"].ToString());
            taskReminder.Description = dr.Field<string>("Description");
            taskReminder.ReminderDisplayed = dr.Field<bool>("ReminderDisplayed");
            return taskReminder;
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
