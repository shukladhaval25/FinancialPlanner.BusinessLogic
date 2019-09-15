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
    public class TaskCommentService
    {
        private readonly string SELECT_COMMENTS_BY_TASKID = "SELECT TaskComment.ID, TaskComment.TaskId, " +
            "TaskComment.CommentedOn, TaskComment.CommentedBy, TaskComment.comment, TaskComment.IsEdited," +
            "Users.UserName as CommentedByName " +
            "FROM TaskComment INNER JOIN Users ON TaskComment.CommentedBy = Users.ID " +
            "WHERE (TaskComment.TaskId = {0}) ORDER BY TaskComment.CommentedOn";
            
        private readonly string SELECT_COMMENT_BY_ID = "SELECT TaskComment.ID, TaskComment.TaskId, " +
            "TaskComment.CommentedOn, TaskComment.CommentedBy, TaskComment.comment, TaskComment.IsEdited," +
            "Users.UserName as CommentedByName " +
            "FROM TaskComment INNER JOIN Users ON TaskComment.CommentedBy = Users.ID " +
            "WHERE TaskComment.ID ={0}";

        private readonly string INSERT_COMMENTS = "INSERT INTO TASKCOMMENT (" +
            "[TaskId],[CommentedBy],[comment],[IsEdited]) VALUES ({0},{1},'{2}','{3}')";

        public IList<TaskComment> GetTaskComments(int taskId)
        {
            try
            {
                Logger.LogInfo("Get: Task comment process start");

                IList<TaskComment> taskComments =
                                   new List<TaskComment>();

                DataTable dtAppConfig = DataBase.DBService.ExecuteCommand(string.Format(SELECT_COMMENTS_BY_TASKID,taskId));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    TaskComment taskComment = convertToTaskComment(dr);
                    taskComments.Add(taskComment);
                }

                Logger.LogInfo("Get: Task comment process completed.");
                return taskComments;
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

        public TaskComment GetTaskComment(int id)
        {
            try
            {
                Logger.LogInfo("Get: Task comment process start");

                TaskComment taskComment = new  TaskComment();

                DataTable dtAppConfig = DataBase.DBService.ExecuteCommand(string.Format(SELECT_COMMENT_BY_ID, id));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    taskComment = convertToTaskComment(dr);                    
                }

                Logger.LogInfo("Get: Task comment process completed.");
                return taskComment;
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

        public void AddTaskComment(TaskComment taskComment)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar("SELECT COUNT(*) FROM TASKCOMMENT");              

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(INSERT_COMMENTS,
                      taskComment.TaskId,taskComment.CommantedBy,taskComment.Comment,
                      taskComment.IsEditable),true);

               // Activity.ActivitiesService.Add(ActivityType.CreateTaskProject, EntryStatus.Success,
                //         Source.Server, project.UpdatedByUserName, project.Name, project.MachineName);
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

        private TaskComment convertToTaskComment(DataRow dr)
        {
            TaskComment taskComment = new TaskComment();
            taskComment.Id = dr.Field<int>("ID");
            taskComment.TaskId = dr.Field<int>("TaskId");
            taskComment.CommantedBy = dr.Field<int>("CommentedBy");
            taskComment.CommentedByName = dr.Field<string>("CommentedByName");
            taskComment.Comment = dr.Field<string>("Comment");
            taskComment.CommentedOn = dr.Field<DateTime>("CommentedOn");
            taskComment.IsEditable = dr.Field<bool>("IsEdited");
            return taskComment;
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
