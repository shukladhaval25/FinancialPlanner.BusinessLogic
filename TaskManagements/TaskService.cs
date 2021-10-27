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

        private readonly string SELECT_ALL_TASKS =
           "SELECT TaskCard.*,TaskProject.Name as ProjectName,Users.UserName AS OwnerName FROM TaskCard " +
           "INNER JOIN TaskProject ON " +
           "TaskCard.ProjectId = TaskProject.ID INNER JOIN Users ON TaskCard.Owner = Users.ID";

        private readonly string SELECT_ALL =
            "SELECT TaskCard.*,TaskProject.Name as ProjectName,Users.UserName AS OwnerName FROM TaskCard " +
            "INNER JOIN TaskProject ON " +
            "TaskCard.ProjectId = TaskProject.ID INNER JOIN Users ON TaskCard.Owner = Users.ID AND " +
            "(TaskCard.TaskStatus <> 4 and TaskCard.TaskStatus<> 5)";

       private readonly string SELECT_ALL_NOTIFIED_BY_USER = "SELECT  TaskCard.*, Users.UserName AS OwnerName, " +
                         "TaskProject.Name AS ProjectName FROM Users INNER JOIN " +
                         "TaskCard ON Users.ID = TaskCard.Owner INNER JOIN " +
                         "TaskNotification ON TaskCard.ID = TaskNotification.TaskId INNER JOIN " +
                         "TaskProject ON TaskCard.ProjectId = TaskProject.ID " +
                         "WHERE (TaskNotification.NotifyTo = {0})";

        //private readonly string SELECT_ALL_BY_TASK_STATUS = "SELECT * FROM [TaskCard] WHERE TASKSTATUS = {0}";
        //private readonly string SELECT_ALL_BY_PROJECT_ID = "SELECT * FROM [TaskCard] WHERE PROJECTID = {0}";
        //private readonly string SELECT_ALL_BY_PROJECTID_TASK_STATUS = "SELECT* FROM[TaskCard] WHERE PROJECTID = {0} AND TASKSTATUS = {1}";

        //private readonly string SELECT_BY_ID = "SELECT * FROM[TaskCard] WHERE TASKID = {0}";

        private string SELECT_OVERDUE_TASKS_BY_USEID = "SELECT TaskCard.*,TaskProject.Name as ProjectName,Users.UserName AS OwnerName " +
            "FROM TASKCARD INNER JOIN TaskProject ON " +
            "TaskCard.ProjectId = TaskProject.ID INNER JOIN Users ON TaskCard.Owner = Users.ID WHERE DueDate < GETDATE() AND " +
            "TASKCARD.ASSIGNTO = {0} AND (TaskCard.TaskStatus <> 4 and TaskCard.TaskStatus<> 5)";

        private const string SELECT_TASK_BYPROJECTNAME_OPENSTATUS_ASSIGNTO = "SELECT Taskcard.* FROM TaskProject " +
            "LEFT OUTER JOIN TaskCard ON TaskProject.ID = TaskCard.ProjectId " +
            "where (TaskCard.AssignTo = {0}) AND (TaskCard.TaskStatus <> 4 and TaskCard.TaskStatus<> 5) " + 
            "and TaskProject.Name ='{1}'";

        private const string SELECT_TASK_BYPROJECTNAME = "SELECT Taskcard.* FROM TaskProject " +
            "LEFT OUTER JOIN TaskCard ON TaskProject.ID = TaskCard.ProjectId " +
            "where (TaskCard.TaskStatus <> 4 and TaskCard.TaskStatus<> 5) " +
            "and TaskProject.Name ='{1}'";

        private readonly string SELECT_BY_ASSIGNTO = "SELECT TaskCard.*, Users.UserName AS OwnerName,TaskProject.Name AS ProjectName " +
            "FROM Users INNER JOIN TaskCard ON Users.ID = TaskCard.Owner INNER JOIN " +
            "TaskProject ON TaskCard.ProjectId = TaskProject.ID WHERE (TaskCard.AssignTo = {0}) AND "+
            "(TaskCard.TaskStatus <> 4 and TaskCard.TaskStatus<> 5)";

        private readonly string SELECT_BY_TASKID = "SELECT TaskCard.*, Users.UserName AS OwnerName,TaskProject.Name AS ProjectName " +
           "FROM Users INNER JOIN TaskCard ON Users.ID = TaskCard.Owner INNER JOIN " +
           "TaskProject ON TaskCard.ProjectId = TaskProject.ID WHERE (TaskCard.TaskId = '{0}') AND " +
           "(TaskCard.TaskStatus <> 4 and TaskCard.TaskStatus<> 5)";

        private const string SELECT_BY_OVERDUE_TASKSTATUS = "SELECT * FROM [TaskCard] WHERE DUEDATE < {0} AND " +
            "(TaskCard.TaskStatus <> 1 or TaskCard.TaskStatus<> 2 or TaskCard.TaskStatus<> 3)";

        private const string SELECT_ID_BY_TASKDETAILS = "SELECT ID FROM TASKCARD WHERE PROJECTID = {0} AND TITLE ='{1}' AND  " +
            "CID ={2} AND CREATEDON ='{3}' AND  ASSIGNTO ={4} AND OWNER = {5} AND TransactionType ='{6}' AND CREATEDBY ={7}";

        

        private readonly string INSERT_TASK = "INSERT INTO TASKCARD " +
            " VALUES ('{0}',{1},'{2}',{3},{4},'{5}','{6}',{7},{8},{9},{10},{11},'{12}',{13},'{14}',{15},'{16}','{17}','{18}')";

        private readonly string UPDATE_TASK = "UPDATE TASKCARD SET TRANSACTIONTYPE = '{0}'," +
            "CARDTYPE ={1},CID = {2},TITLE ='{3}',DESCRIPTION = '{4}',PRIORITY = {5}, TASKSTATUS = {6}," +
            "OWNER = {7},ASSIGNTO = {8},COMPLETEDPERCENTAGE ={9}, UPDATEDON ='{10}',UPDATEDBY = {11}, " +
            "ACTUALCOMPLETEDON ='{12}',DUEDATE = '{13}',OTHERNAME ='{15}' WHERE ID ={14}";

        private readonly string UPDATE_TASKID = "UPDATE TASKCARD SET TASKID ='{0}' WHERE ID ={1}";
        private readonly string DELETE_NOTIFIED_TASK_BY_USER = "DELETE FROM TASKNOTIFICATION WHERE NotifyTo ={0}";

        private readonly string USER_PERFORMANCE_YEARLY_ON_TASK = "SELECT A.AssignTo,Count(A.Id) as CompletedTaskCount," +
                    "(SELECT ISNULL(Count(B.Id) ,0) AS OVERDUECOUNT FROM " +
                    "TaskCard B where (B.ActualCompletedOn > B.DueDate) AND(B.ASSIGNTO = {0}) AND " +
                    "(B.ActualCompletedOn BETWEEN DATEADD(YEAR,-1, GETDATE()) AND GETDATE()) AND " +
                    "B.TaskStatus NOT IN (3,4,5) AND " +
                    "(format(A.ActualCompletedOn,'MMM.yyyy') = format(B.ActualCompletedOn,'MMM.yyyy')) " +
                    "group by AssignTo, format(B.ActualCompletedOn,'MMM.yyyy')) AS OverDueTaskCount, " +
                "format(A.ActualCompletedOn, 'MMM.yyyy') AS PERIOD FROM TaskCard A " +
                "where (A.ActualCompletedOn <= A.DueDate) AND(A.ASSIGNTO = {0}) AND " +
                "A.TaskStatus NOT IN (3,4,5) AND " +
                "(ACTUALCOMPLETEDON BETWEEN DATEADD(YEAR,-1, GETDATE()) AND GETDATE()) " +
                "group by AssignTo, format(ActualCompletedOn,'MMM.yyyy')";

        private readonly string COMPANY_PERFORMANCE_YEARLY_ON_TASK = "SELECT 0 as AssignTo,Count(A.Id) as CompletedTaskCount," +
                    "(SELECT ISNULL(Count(B.Id) ,0) AS OVERDUECOUNT FROM " +
                    "TaskCard B where (B.ActualCompletedOn > B.DueDate) AND " +
                    "(B.ActualCompletedOn BETWEEN DATEADD(YEAR,-1, GETDATE()) AND GETDATE()) AND " +
                    "B.TaskStatus NOT IN (3,4,5) AND " +
                    "(format(A.ActualCompletedOn,'MMM.yyyy') = format(B.ActualCompletedOn,'MMM.yyyy')) " +
                    "group by format(B.ActualCompletedOn,'MMM.yyyy')) AS OverDueTaskCount, " +
                "format(A.ActualCompletedOn, 'MMM.yyyy') AS PERIOD FROM TaskCard A " +
                "where(A.ActualCompletedOn <= A.DueDate) AND " +
                "A.TaskStatus NOT IN (3,4,5) AND " +
                "(ACTUALCOMPLETEDON BETWEEN DATEADD(YEAR,-1, GETDATE()) AND GETDATE()) " +
                "group by format(ActualCompletedOn,'MMM.yyyy')";



        public IList<TaskCard> GetNotified(int userId)
        {
            try
            {
                Logger.LogInfo("Get: Notified task process start");
                IList<TaskCard> taskcards =
                    new List<TaskCard>();

                DataTable dtAppConfig = DataBase.DBService.ExecuteCommand(string.Format(SELECT_ALL_NOTIFIED_BY_USER,userId));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    TaskCard task = convertToTaskCard(dr);
                    taskcards.Add(task);
                }
                DataBase.DBService.ExecuteCommand(string.Format(DELETE_NOTIFIED_TASK_BY_USER,userId));
                Logger.LogInfo("Get: Notified task process completed.");
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

        public object GetAll(string projectName)
        {
            try
            {
                Logger.LogInfo("Get: Task Card process start");
                IList<TaskCard> taskcards =
                    new List<TaskCard>();

                DataTable dtAppConfig = DataBase.DBService.ExecuteCommand(
                   string.Format(SELECT_TASK_BYPROJECTNAME, projectName));
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

        public int Update(TaskCard taskCard)
        {
            try
            {                
                DataBase.DBService.ExecuteCommand(SELECT_ALL);
                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(UPDATE_TASK,                    
                    taskCard.TransactionType,
                    (int)taskCard.Type,
                    taskCard.CustomerId,
                    taskCard.Title,
                    taskCard.Description,
                    (int)taskCard.Priority,
                    (int)taskCard.TaskStatus,
                    taskCard.Owner,
                    taskCard.AssignTo,
                    taskCard.CompletedPercentage,                    
                    taskCard.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"),
                    taskCard.UpdatedBy,
                    taskCard.DueDate.ToString("yyyy-MM-dd hh:mm:ss"),
                    taskCard.DueDate.ToString("yyyy-MM-dd hh:mm:ss"), taskCard.Id,
                    taskCard.OtherName), true);

                if ((taskCard.ProjectId == 1 || taskCard.ProjectId == 2) && !taskCard.TransactionType.Equals("Others"))
                {
                    updateTransactionType(taskCard, taskCard.Id);
                }
                //DataBase.DBService.ExecuteCommandString(string.Format(UPDATE_TASKID, taskCard.TaskId + "-" + id, id), true);
                //Activity.ActivitiesService.Add(ActivityType.CreateTask, EntryStatus.Success,
                //        Source.Server, taskCard.UpdatedByUserName, taskCard.TaskId, taskCard.MachineName);
                DataBase.DBService.CommitTransaction();
                return taskCard.Id;
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

        private void updateTransactionType(TaskCard taskCard, int id)
        {
            TransactionTypeHelper transactionTypeHelper = new TransactionTypeHelper(taskCard, id);
            transactionTypeHelper.UpdateTransaction();
        }

        public IList<TaskCard> GetByAssignTo(int userId)
        {
            try
            {
                Logger.LogInfo("Get: Assign to task process start");
                IList<TaskCard> taskcards =
                    new List<TaskCard>();

                DataTable dtAppConfig = DataBase.DBService.ExecuteCommand(string.Format(SELECT_BY_ASSIGNTO, userId));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    TaskCard task = convertToTaskCard(dr);
                    taskcards.Add(task);
                }
                Logger.LogInfo("Get: Assign to task process completed.");
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

        public IList<TaskCard> GetTaskByTaskId(string taskId)
        {
            try
            {
                Logger.LogInfo("Get: Assign to task process start");
                IList<TaskCard> taskcards =
                    new List<TaskCard>();

                DataTable dtAppConfig = DataBase.DBService.ExecuteCommand(string.Format(SELECT_BY_TASKID, taskId));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    TaskCard task = convertToTaskCard(dr);
                    taskcards.Add(task);
                }
                Logger.LogInfo("Get: Assign to task process completed.");
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

        public IList<TaskCard> GetAllTasks()
        {
            try
            {
                Logger.LogInfo("Get: Task Card process start");
                IList<TaskCard> taskcards =
                    new List<TaskCard>();

                DataTable dtAppConfig = DataBase.DBService.ExecuteCommand(SELECT_ALL_TASKS);
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
                   string.Format(SELECT_TASK_BYPROJECTNAME_OPENSTATUS_ASSIGNTO,userId,projectName));
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
                Logger.LogInfo("Get: Overdue task card process start");
                IList<TaskCard> taskcards =
                    new List<TaskCard>();

                DataTable dtAppConfig = DataBase.DBService.ExecuteCommand(string.Format(SELECT_OVERDUE_TASKS_BY_USEID,userId));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    TaskCard task = convertToTaskCard(dr);
                    taskcards.Add(task);
                }
                Logger.LogInfo("Get: Overdue task card process completed.");
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


        public IList<UserPerformanceOnTask> GetUserPerformanceForYear(int userId)
        {
            try
            {
                Logger.LogInfo("Get: User performance yearly process start");
                IList<UserPerformanceOnTask> taskcards =
                    new List<UserPerformanceOnTask>();

                DataTable dtAppConfig = DataBase.DBService.ExecuteCommand(string.Format(USER_PERFORMANCE_YEARLY_ON_TASK, userId));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    UserPerformanceOnTask task = convertToUserPerformaceData(dr);
                    taskcards.Add(task);
                }
                Logger.LogInfo("Get: User performance yearly process completed.");
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

        public IList<UserPerformanceOnTask> GetCompanyTaskPerformanceForYear()
        {
            try
            {
                Logger.LogInfo("Get: User performance yearly process start");
                IList<UserPerformanceOnTask> taskcards =
                    new List<UserPerformanceOnTask>();

                DataTable dtAppConfig = DataBase.DBService.ExecuteCommand(COMPANY_PERFORMANCE_YEARLY_ON_TASK);
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    UserPerformanceOnTask task = convertToUserPerformaceData(dr);
                    taskcards.Add(task);
                }
                Logger.LogInfo("Get: User performance yearly process completed.");
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

        private UserPerformanceOnTask convertToUserPerformaceData(DataRow dr)
        {
            UserPerformanceOnTask userPerformanceOnTask = new UserPerformanceOnTask();
            userPerformanceOnTask.UserId = dr.Field<int>("AssignTo");
            userPerformanceOnTask.Period = dr.Field<string>("Period");
            userPerformanceOnTask.CompletedTaskCount = dr["CompletedTaskCount"] == DBNull.Value ? 0 : dr.Field<int>("CompletedTaskCount");
            userPerformanceOnTask.OverDueTaskCount = dr["OverDueTaskCount"] == DBNull.Value ? 0 : dr.Field<int>("OverDueTaskCount");
            return userPerformanceOnTask;
        }

        public int Add(TaskCard taskcard)
        {
            try
            {               
                DataBase.DBService.ExecuteCommand(SELECT_ALL);
                DataBase.DBService.BeginTransaction();
                Logger.LogInfo("SQL:" + string.Format(INSERT_TASK,
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
                    taskcard.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss"),
                    taskcard.CreatedBy,
                    taskcard.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"),
                    taskcard.UpdatedBy,
                    taskcard.DueDate.ToString("yyyy-MM-dd hh:mm:ss"),
                    taskcard.DueDate.ToString("yyyy-MM-dd hh:mm:ss"),
                    taskcard.OtherName));
              
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
                    taskcard.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss"),
                    taskcard.CreatedBy,
                    taskcard.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"),
                    taskcard.UpdatedBy,
                    taskcard.DueDate.ToString("yyyy-MM-dd hh:mm:ss"),
                    taskcard.DueDate.ToString("yyyy-MM-dd hh:mm:ss"),
                    taskcard.OtherName), true);

                int id = getTaskID(taskcard);

                if (id > 0 && (taskcard.ProjectId == 1 || taskcard.ProjectId == 2) && !taskcard.TransactionType.Equals("Others"))
                {
                    saveTransactionType(taskcard, id);
                }
                DataBase.DBService.ExecuteCommandString(string.Format(UPDATE_TASKID, taskcard.TaskId + "-" + id, id),true);
                //Activity.ActivitiesService.Add(ActivityType.CreateTask, EntryStatus.Success,
                //        Source.Server, taskcard.UpdatedByUserName, taskcard.TaskId, taskcard.MachineName);
                DataBase.DBService.CommitTransaction();
                return id;
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

        private static int getTaskID(TaskCard taskcard)
        {
            return int.Parse( DataBase.DBService.ExecuteCommandScalar(string.Format(SELECT_ID_BY_TASKDETAILS,
                 taskcard.ProjectId,
                 taskcard.Title,
                 taskcard.CustomerId,
                 taskcard.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss"),
                 taskcard.AssignTo,
                 taskcard.Owner,
                 taskcard.TransactionType,
                 taskcard.CreatedBy
                 )));
        }

        private void saveTransactionType(TaskCard taskcard, int id)
        {
            TransactionTypeHelper transactionTypeHelper = new TransactionTypeHelper(taskcard, id);
            transactionTypeHelper.SaveTransaction();
        }

        private object getTransactionType(TaskCard taskCard,int id)
        {
            TransactionTypeHelper transactionTypeHelper = new TransactionTypeHelper(taskCard,id);

            return (transactionTypeHelper == null) ? null : transactionTypeHelper.GetTransaction();
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
            taskCard.CreatedBy = dr.Field<int>("CreatedBy");
            taskCard.CreatedOn = dr.Field<DateTime>("CreatedOn");
            taskCard.UpdatedOn = dr.Field<DateTime>("UpdatedOn");
            //taskCard.ActualCompletedDate = dr.Field<DateTime>("ActualCompletedDate");
            taskCard.DueDate = dr.Field<DateTime>("DueDate");
            taskCard.ProjectName = dr.Field<string>("ProjectName");
            taskCard.OwnerName = dr.Field<string>("OwnerName");
            taskCard.AssignToName = getAssignTo(dr.Field<int?>("AssignTo"));
            taskCard.CustomerName = getCustomerName(taskCard.CustomerId);
            taskCard.TaskTransactionType = getTransactionType(taskCard, taskCard.Id);
            taskCard.OtherName = dr.Field<string>("OtherName");
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
