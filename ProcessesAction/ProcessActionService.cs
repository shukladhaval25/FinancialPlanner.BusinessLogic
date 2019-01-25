using FinancialPlanner.BusinessLogic.ApplictionConfiguration;
using FinancialPlanner.Common;
using FinancialPlanner.Common.Model;
using FinancialPlanner.Common.Model.PlannerProcess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FinancialPlanner.BusinessLogic.ProcessesAction
{
    public class ProcessActionService
    {
        private const string GET_CLIENT_NAME_QUERY = "SELECT C.NAME FROM CLIENT C, PLANNER P  WHERE P.CLIENTID = C.ID AND P.ID = {0}";
        const string SELECT_ALL = "SELECT N1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM PROCESSACTION N1, USERS U WHERE N1.UPDATEDBY = U.ID";
        const string SELECT_BYID = "SELECT N1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM PROCESSACTION N1, USERS U WHERE N1.UPDATEDBY = U.ID AND N1.ID = {0}";

        const string INSERT_QUERY = "INSERT INTO PROCESSACTION VALUES (" +
            "'{0}','{1}','{2}',{3},'{4}',{5})";
        const string UPDATE_QUERY = "UPDATE PROCESSACTION SET NAME = '{0}',DESCRIPTION = '{1}',UPDATEDON = '{2}'," +
            "UPDATEDBY={3} WHERE ID ={4}";
        const string DELET_QUERY = "DELETE FROM PROCESSACTION WHERE ID ={0}";
        public IList<ProcessAction> GetAll()
        {
            try
            {
                Logger.LogInfo("Get: ProcessAction process start");
                IList<ProcessAction> lstProcessAction = new List<ProcessAction>();

                DataTable dtAppConfig =  DataBase.DBService.ExecuteCommand(SELECT_ALL);
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    ProcessAction ProcessAction = convertToProcessActionObject(dr);
                    lstProcessAction.Add(ProcessAction);
                }
                Logger.LogInfo("Get: ProcessAction process completed.");
                return lstProcessAction;
            }
            catch (Exception ex)
            {
                StackTrace st = new StackTrace ();
                StackFrame sf = st.GetFrame (0);
                MethodBase  currentMethodName = sf.GetMethod();
                LogDebug(currentMethodName.Name, ex);
                return null;
            }
        }

        public ProcessAction GetById(int id)
        {
            try
            {
                Logger.LogInfo("Get: ProcessAction process start");
                ProcessAction ProcessAction = new ProcessAction();

                DataTable dtAppConfig =  DataBase.DBService.ExecuteCommand(string.Format(SELECT_BYID,id));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    ProcessAction = convertToProcessActionObject(dr);
                }
                Logger.LogInfo("Get: ProcessAction process completed.");
                return ProcessAction;
            }
            catch (Exception ex)
            {
                StackTrace st = new StackTrace ();
                StackFrame sf = st.GetFrame (0);
                MethodBase  currentMethodName = sf.GetMethod();
                LogDebug(currentMethodName.Name, ex);
                return null;
            }
        }

        public void Add(ProcessAction ProcessAction)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY,ProcessAction.Id));
                DataBase.DBService.BeginTransaction();

                DataBase.DBService.ExecuteCommandString(string.Format(INSERT_QUERY,
                      ProcessAction.Name, ProcessAction.Description,
                      ProcessAction.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss"), ProcessAction.CreatedBy,
                      ProcessAction.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), ProcessAction.UpdatedBy), true);

                Activity.ActivitiesService.Add(ActivityType.CreateProcessAction, EntryStatus.Success,
                         Source.Server, ProcessAction.UpdatedByUserName, ProcessAction.Name, ProcessAction.MachineName);
                              
                DataBase.DBService.CommitTransaction();
            }
            catch (Exception ex)
            {
                DataBase.DBService.RollbackTransaction();
                StackTrace st = new StackTrace ();
                StackFrame sf = st.GetFrame (0);
                MethodBase  currentMethodName = sf.GetMethod();
                LogDebug(currentMethodName.Name, ex);
                throw ex;
            }
        }
        
        public void Update(ProcessAction ProcessAction)
        {
            try
            {
                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(UPDATE_QUERY,
                   ProcessAction.Name,
                   ProcessAction.Description, 
                   ProcessAction.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"),
                   ProcessAction.UpdatedBy, ProcessAction.Id), true);

                Activity.ActivitiesService.Add(ActivityType.UpdateProcessAction, EntryStatus.Success,
                         Source.Server, ProcessAction.UpdatedByUserName, ProcessAction.Name, ProcessAction.MachineName);
                DataBase.DBService.CommitTransaction();
            }
            catch (Exception ex)
            {
                DataBase.DBService.RollbackTransaction();
                StackTrace st = new StackTrace ();
                StackFrame sf = st.GetFrame (0);
                MethodBase  currentMethodName = sf.GetMethod();
                LogDebug(currentMethodName.Name, ex);
                throw ex;
            }
        }

        public void Delete(ProcessAction ProcessAction)
        {
            try
            {
                //string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY,ProcessAction.Pid));
                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(DELET_QUERY, ProcessAction.Id), true);

                Activity.ActivitiesService.Add(ActivityType.DeleteProcessAction, EntryStatus.Success,
                         Source.Server, ProcessAction.UpdatedByUserName, ProcessAction.Name, ProcessAction.MachineName);

                DataBase.DBService.CommitTransaction();
            }
            catch (Exception ex)
            {
                DataBase.DBService.RollbackTransaction();
                StackTrace st = new StackTrace ();
                StackFrame sf = st.GetFrame (0);
                MethodBase  currentMethodName = sf.GetMethod();
                LogDebug(currentMethodName.Name, ex);
                throw ex;
            }
        }
        private ProcessAction convertToProcessActionObject(DataRow dr)
        {
            ProcessAction ProcessAction = new ProcessAction();
            ProcessAction.Id = dr.Field<int>("ID");
            ProcessAction.Name = dr.Field<string>("NAME");
            ProcessAction.Description = dr.Field<string>("DESCRIPTION");
            ProcessAction.UpdatedOn = dr.Field<DateTime>("UpdatedOn");
            ProcessAction.UpdatedBy = dr.Field<int>("UpdatedBy");
            return ProcessAction;
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
