using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FinancialPlanner.Common.Model;
using FinancialPlanner.Common;
using System.Data;
using System.Diagnostics;
using System.Reflection;

namespace FinancialPlanner.BusinessLogic.PlanOption
{
    public class PlanOptionService
    {
        private const string GET_CLIENT_NAME_QUERY = "SELECT C.NAME FROM CLIENT C, PLANNER P  WHERE P.CLIENTID = C.ID AND P.ID = {0}";
        const string SELECT_ALL = "SELECT N1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM PLANOPTIONS N1, USERS U WHERE N1.UPDATEDBY = U.ID AND N1.PID = {0}";
        const string INSERT_QUERY = "INSERT INTO PLANOPTIONS VALUES (" +
            "{0},'{1}','{2}',{3},'{4}',{5},{6})";

        const string UPDATE_QUERY = "UPDATE PLANOPTIONS SET Name = '{0}',UPDATEDON = '{1}'," +
            "UPDATEDBY={2},RISKPROFILEID = {3} WHERE ID ={4}";
        const string DELET_QUERY = "DELETE FROM PLANOPTIONS WHERE ID ={0}";
        public IList<Common.Model.PlanOption> GetAll(int plannerId)
        {
            try
            {
                Logger.LogInfo("Get: Plan option process start");
                IList<Common.Model.PlanOption> lstPlanOption = new List<Common.Model.PlanOption>();

                DataTable dtAppConfig =  DataBase.DBService.ExecuteCommand(string.Format(SELECT_ALL,plannerId));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    Common.Model.PlanOption planoption = convertToPlanOptionObject(dr);
                    lstPlanOption.Add(planoption);
                }
                Logger.LogInfo("Get: Plan option process completed.");
                return lstPlanOption;
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

        public void Add(Common.Model.PlanOption planOption)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY,planOption.Pid));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(INSERT_QUERY,
                      planOption.Pid, planOption.Name,
                      planOption.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss"), planOption.CreatedBy,
                      planOption.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), planOption.UpdatedBy,
                      planOption.RiskProfileId), true);

                Activity.ActivitiesService.Add(ActivityType.CreatePlannerOption, EntryStatus.Success,
                         Source.Server, planOption.UpdatedByUserName, planOption.Name, planOption.MachineName);
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

        public void Update(Common.Model.PlanOption planOption)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY,planOption.Pid));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(UPDATE_QUERY,
                      planOption.Name,
                      planOption.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"),
                      planOption.UpdatedBy,planOption.RiskProfileId,planOption.Id), true);

                Activity.ActivitiesService.Add(ActivityType.UpdatePlannerOption, EntryStatus.Success,
                         Source.Server, planOption.UpdatedByUserName, planOption.Name, planOption.MachineName);
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
        public void Delete(Common.Model.PlanOption planOption)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY,planOption.Pid));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(DELET_QUERY,
                     planOption.Id), true);

                Activity.ActivitiesService.Add(ActivityType.DeletePlannerOption, EntryStatus.Success,
                         Source.Server, planOption.UpdatedByUserName, planOption.Name, planOption.MachineName);
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

        private void LogDebug(string methodName, Exception ex)
        {
            DebuggerLogInfo debuggerInfo = new DebuggerLogInfo();
            debuggerInfo.ClassName = this.GetType().Name;
            debuggerInfo.Method = methodName;
            debuggerInfo.ExceptionInfo = ex;
            Logger.LogDebug(debuggerInfo);
        }
        private Common.Model.PlanOption convertToPlanOptionObject(DataRow dr)
        {
            Common.Model.PlanOption planOpt = new Common.Model.PlanOption();
            planOpt.Id = dr.Field<int>("ID");
            planOpt.Pid = dr.Field<int>("PID");
            planOpt.Name = dr.Field<string>("Name");
            planOpt.RiskProfileId = dr.Field<int>("RiskProfileId");
            planOpt.UpdatedOn = dr.Field<DateTime>("UpdatedOn");
            planOpt.UpdatedBy = dr.Field<int>("UpdatedBy");
            
            return planOpt;
        }
    }
}
