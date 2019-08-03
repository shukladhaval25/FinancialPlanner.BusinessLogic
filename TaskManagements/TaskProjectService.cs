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
    public class TaskProjectService
    {
        const string GET_PROJECT_NAME_QUERY = "SELECT NAME FROM TASKPROJECT ID = {0}";
        const string SELECT_ALL = "SELECT[ID],[NAME],[INITIALID],[DETAILS],[ISCUSTOMTYPE],[CREATEDON],[CREATEDBY]" +
            ",[UPDATEDON] ,[UPDATEDBY] FROM [TASKPROJECT]";

        readonly string SELECT_BY_ID = SELECT_ALL + "WHERE [ID] = {0}";
        const string INSERT_PROJECT = "INSERT INTO TASKPROJECT VALUES (" +
            "'{0}','{1}','{2}','{3}'," +
            "'{4}',{5},'{6}',{7})";
        const string UPDATE_PROJECT = "UPDATE TASKPROJECT SET " +
            "[NAME] = '{0}', [INITIALID] ='{1}', [DETAILS] = '{2}',[ISCUSTOMTYPE] = '{3}', " +
            "[UpdatedOn] = '{4}', [UpdatedBy] ={5} " +
            "WHERE ID = {6}";

        const string DELETE_PROJECT = "DELETE FROM TASKPROJECT WHERE ID = {0}";

        public IList<Common.Model.TaskManagement.Project> GetAll()
        {
            try
            {
                Logger.LogInfo("Get: Task Project process start");
                IList<Project> projects =
                    new List<Project>();

                DataTable dtAppConfig = DataBase.DBService.ExecuteCommand(SELECT_ALL);
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    Project project = convertToProject(dr);
                    projects.Add(project);
                }
                Logger.LogInfo("Get: Task Project process completed.");
                return projects;
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

        private Project convertToProject(DataRow dr)
        {
            Project project = new Project();
            project.Id = dr.Field<int>("ID");
            project.Name = dr.Field<string>("NAME");
            project.InitialId = dr.Field<string>("INITIALID");
            project.Description = dr.Field<string>("Details");
            project.IsCustomType = dr.Field<bool>("IsCustomType");
            project.UpdatedBy = dr.Field<int>("UpdatedBy");
            project.UpdatedOn = dr.Field<DateTime>("UpdatedOn");
            //project.UpdatedByUserName = dr.Field<string>("UpdatedByUserName");
            return project;
        }

        public Project GetById(int id)
        {
            try
            {
                Logger.LogInfo("Get: Task project by id process start");
                Project project = new Project();

                DataTable dtAppConfig = DataBase.DBService.ExecuteCommand(string.Format(SELECT_BY_ID, id));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    project = convertToProject(dr);
                }
                Logger.LogInfo("Get: Task project by id process completed.");
                return project;
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

        public void Add(Project project)
        {
            try
            {
                //string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_PROJECT_NAME_QUERY, project.Id));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(INSERT_PROJECT,
                      project.Name, project.InitialId,
                      project.Description,project.IsCustomType,
                      project.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss"), project.CreatedBy,
                      project.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), project.UpdatedBy), true);

                Activity.ActivitiesService.Add(ActivityType.CreateTaskProject, EntryStatus.Success,
                         Source.Server, project.UpdatedByUserName, project.Name, project.MachineName);
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

        public void Update(Project project)
        {
            try
            {
                //string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_PROJECT_NAME_QUERY, project.Id));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(UPDATE_PROJECT,
                      project.Name, project.InitialId,
                      project.Description, project.IsCustomType,                     
                      project.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), project.UpdatedBy,
                      project.Id), true);

                Activity.ActivitiesService.Add(ActivityType.UpdateTaskProject, EntryStatus.Success,
                         Source.Server, project.UpdatedByUserName, project.Name, project.MachineName);
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

        public void Delete(Project project)
        {
            try
            {
                //string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_PROJECT_NAME_QUERY, project.Id));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(DELETE_PROJECT,                     
                      project.Id), true);

                Activity.ActivitiesService.Add(ActivityType.DeleteTaskProject, EntryStatus.Success,
                         Source.Server, project.UpdatedByUserName, project.Name, project.MachineName);
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
