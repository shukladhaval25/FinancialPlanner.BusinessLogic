using FinancialPlanner.BusinessLogic.Clients;
using FinancialPlanner.BusinessLogic.Users;
using FinancialPlanner.Common;
using FinancialPlanner.Common.Model;
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
    public class TaskService
    {
        private readonly string SELECT_ALL =
            "SELECT TaskCard.*,TaskProject.Name as ProjectName,Users.UserName AS OwnerName FROM TaskCard " +
            "INNER JOIN TaskProject ON " +
            "TaskCard.ProjectId = TaskProject.ID INNER JOIN Users ON TaskCard.Owner = Users.ID";

        private readonly string SELECT_ALL_BY_TASK_STATUS = "SELECT * FROM [TaskCard] WHERE TASKSTATUS = {0}";
        private readonly string SELECT_ALL_BY_PROJECT_ID = "SELECT * FROM [TaskCard] WHERE PROJECTID = {0}";
        private readonly string SELECT_ALL_BY_PROJECTID_TASK_STATUS = "SELECT* FROM[TaskCard] WHERE PROJECTID = {0} AND TASKSTATUS = {1}";
        private readonly string SELECT_BY_ID = "SELECT* FROM[TaskCard] WHERE TASKID = { 0 }";

        private string SELECT_OVERDUE_TASKS_BY_USEID = "SELECT TaskCard.*,TaskProject.Name as ProjectName,Users.UserName AS OwnerName " +
            "FROM TASKCARD INNER JOIN TaskProject ON " +
            "TaskCard.ProjectId = TaskProject.ID INNER JOIN Users ON TaskCard.Owner = Users.ID WHERE DueDate < GETDATE() AND " +
            "TASKCARD.ASSIGNTO = {0}";

        private const string SELECT_TASK_BYPROJECTNAME_OPENSTATUS = "SELECT Taskcard.* FROM TaskProject " +
            "LEFT OUTER JOIN TaskCard ON TaskProject.ID = TaskCard.ProjectId " +
            "where (TaskCard.AssignTo = {0}) AND (TaskCard.TaskStatus<> 1 or " +
            "TaskCard.TaskStatus<> 2 or TaskCard.TaskStatus<> 3) and TaskProject.Name ='{1}'";

        private readonly string SELECT_BY_ASSIGNTO = "SELECT * FROM [TaskCard] WHERE ASSIGNTO = {0}";
        private const string SELECT_BY_OVERDUE_TASKSTATUS = "SELECT * FROM [TaskCard] WHERE TASKSTATUS = {0} AND DUEDATE < {1}";
        private const string SELECT_ID_BY_TASKDETAILS = "SELECT ID FROM TASKCARD WHERE PROJECTID = {0} AND TITLE ='{1}' AND  " +
            "CID ={2} AND CREATEDON ='{3}' AND  ASSIGNTO ={4} AND OWNER = {5} AND TransactionType ='{6}' AND CREATEDBY ={7}";
        private readonly string INSERT_TASK = "INSERT INTO TASKCARD " +
            " VALUES ('{0}',{1},'{2}',{3},{4},'{5}','{6}',{7},{8},{9},{10},{11},'{12}',{13},'{14}',{15},'{16}','{17}')";

        public IList<TaskCard> GetAll()
        {
            try
            {
                Logger.LogInfo("Get: Task Card process start");
                IList<TaskCard> taskcards =
                    new List<TaskCard>();

                DataTable dtAppConfig = DataBase.DBService.ExecuteCommand(SELECT_ALL);
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    TaskCard task = convertToTaskCard(dr);
                    taskcards.Add(task);
                }
                Logger.LogInfo("Get: Task Card process completed.");
                return taskcards;
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

        public IList<TaskCard> GetOpenTaskByProjectForUser(string projectName,int userId)
        {
            try
            {
                Logger.LogInfo("Get: Task Card process start");
                IList<TaskCard> taskcards =
                    new List<TaskCard>();

                DataTable dtAppConfig = DataBase.DBService.ExecuteCommand(
                   string.Format(SELECT_TASK_BYPROJECTNAME_OPENSTATUS,userId,projectName));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    TaskCard task = convertToTaskCard(dr);
                    taskcards.Add(task);
                }
                Logger.LogInfo("Get: Task Card process completed.");
                return taskcards;
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

        public object GetOverDueTasks(int userId)
        {
            try
            {
                Logger.LogInfo("Get: Task Card process start");
                IList<TaskCard> taskcards =
                    new List<TaskCard>();

                DataTable dtAppConfig = DataBase.DBService.ExecuteCommand(string.Format(SELECT_OVERDUE_TASKS_BY_USEID,userId));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    TaskCard task = convertToTaskCard(dr);
                    taskcards.Add(task);
                }
                Logger.LogInfo("Get: Task Card process completed.");
                return taskcards;
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

        public void Add(TaskCard taskcard)
        {
            try
            {
                //string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_PROJECT_NAME_QUERY, project.Id));
                //JSONSerialization jSON = new JSONSerialization();
                //TaskCard taskcard = jSON.DeserializeFromString<TaskCard>(jsonString);
                DataBase.DBService.ExecuteCommand(SELECT_ALL);
                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(INSERT_TASK,
                    taskcard.TaskId,
                    taskcard.ProjectId,
                    taskcard.TransactionType,
                    (int)taskcard.Type,
                    taskcard.CustomerId,
                    taskcard.Title,
                    taskcard.Description,
                    (int)taskcard.Priority,
                    (int)taskcard.TaskStatus,
                    taskcard.Owner,
                    taskcard.AssignTo,
                    taskcard.CompletedPercentage,
                    taskcard.CreatedOn,
                    taskcard.CreatedBy,
                    taskcard.UpdatedOn,
                    taskcard.UpdatedBy,
                    taskcard.DueDate,
                    taskcard.DueDate), true);

                string id = getTaskID(taskcard);

                if (!string.IsNullOrEmpty(id))
                    saveTransactionType(taskcard, int.Parse(id));

                //Activity.ActivitiesService.Add(ActivityType.CreateTask, EntryStatus.Success,
                //        Source.Server, taskcard.UpdatedByUserName, taskcard.TaskId, taskcard.MachineName);
                DataBase.DBService.CommitTransaction();
            }
            catch (Exception ex)
            {
                DataBase.DBService.RollbackTransaction();
                StackTrace st = new StackTrace();
                StackFrame sf = st.GetFrame(0);
                MethodBase currentMethodName = sf.GetMethod();
                LogDebug(currentMethodName.Name, ex);
                throw ex;
            }
        }

        private static string getTaskID(TaskCard taskcard)
        {
            return DataBase.DBService.ExecuteCommandScalar(string.Format(SELECT_ID_BY_TASKDETAILS,
                 taskcard.ProjectId,
                 taskcard.Title,
                 taskcard.CustomerId,
                 taskcard.CreatedOn,
                 taskcard.AssignTo,
                 taskcard.Owner,
                 taskcard.TransactionType,
                 taskcard.CreatedBy
                 ));
        }

        private void saveTransactionType(TaskCard taskcard, int id)
        {
            TransactionTypeHelper transactionTypeHelper = new TransactionTypeHelper(taskcard, id);
            transactionTypeHelper.SaveTransaction();
        }

        private TaskCard convertToTaskCard(DataRow dr)
        {
            TaskCard taskCard = new TaskCard();
            taskCard.Id = dr.Field<int>("ID");
            taskCard.TaskId = dr.Field<string>("TaskId");
            taskCard.ProjectId = dr.Field<int>("ProjectId");
            taskCard.TransactionType = dr.Field<string>("TransactionType");
            taskCard.Type = (CardType) dr.Field<int>("CardType");
            taskCard.CustomerId = dr.Field<int>("Cid");
            taskCard.Title = dr.Field<string>("Title");
            taskCard.Description = dr.Field<string>("Description");
            taskCard.Priority = (Priority) dr.Field<int>("Priority");
            taskCard.TaskStatus = (Common.Model.TaskManagement.TaskStatus) dr.Field<int>("TaskStatus");
            taskCard.Owner = dr.Field<int>("Owner");
            taskCard.AssignTo = dr.Field<int?>("AssignTo");
            taskCard.CreatedOn = dr.Field<DateTime>("CreatedOn");
            taskCard.UpdatedOn = dr.Field<DateTime>("UpdatedOn");
            //taskCard.ActualCompletedDate = dr.Field<DateTime>("ActualCompletedDate");
            taskCard.DueDate = dr.Field<DateTime>("DueDate");
            taskCard.ProjectName = dr.Field<string>("ProjectName");
            taskCard.OwnerName = dr.Field<string>("OwnerName");
            taskCard.AssignToName = getAssignTo(dr.Field<int?>("AssignTo"));
            taskCard.CustomerName = getCustomerName(taskCard.CustomerId);
            return taskCard;
        }

        private string getCustomerName(int? customerId)
        {
            if (customerId == 0)
                return string.Empty;
            return new ClientService().Get((int)customerId).Name;
        }

        private string getAssignTo(int? v)
        {
            if (v == 0)
                return string.Empty;

            return new UserService().Get((int)v).UserName;
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
