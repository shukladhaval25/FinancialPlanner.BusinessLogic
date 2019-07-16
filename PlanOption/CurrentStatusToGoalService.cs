using FinancialPlanner.Common;
using FinancialPlanner.Common.Model;
using FinancialPlanner.Common.Model.PlanOptions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FinancialPlanner.BusinessLogic.PlanOption
{
    public class CurrentStatusToGoalService
    {
        private readonly string SELECT_ID = "SELECT N1.*,U.USERNAME AS UPDATEDBYUSERNAME,G.NAME AS GOALNAME FROM CURRESNTSTATUSTOGOAL N1," +
            " USERS U,GOALS G WHERE N1.UPDATEDBY = U.ID AND N1.OID = {0} AND " +
            " N1.GOALID = G.ID  AND N1.PID ={1}; ";

        const string INSERT_CurrentStatusToGoal= "INSERT INTO CURRESNTSTATUSTOGOAL VALUES (" +
            "{0},{1},{2},{3},'{4}',{5},'{6}',{7})";

        const string UPDATE_CurrentStatusToGoal = "UPDATE CURRESNTSTATUSTOGOAL SET " +
            "[FUNDALLOCATION] = {0}, [UpdatedOn] = '{1}', [UpdatedBy] ={2} " +
            "WHERE OID = {3} AND ID ={4}";

        const string DELETE_CurrentStatusToGoal = "DELETE FROM CURRESNTSTATUSTOGOAL WHERE ID = {0}";

        public IList<CurrentStatusToGoal> Get(int id,int planId)
        {
            try
            {
                Logger.LogInfo("Get: Current status fund allocation process start");
                IList<CurrentStatusToGoal> currentStatusToGoals = new List<CurrentStatusToGoal>();

                CurrentStatusToGoal currentStatusToGoal = new CurrentStatusToGoal();

                DataTable dtAppConfig =  DataBase.DBService.ExecuteCommand(string.Format(SELECT_ID,id,planId));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    currentStatusToGoal = convertToCurrentStatusToGoal(dr);
                    currentStatusToGoals.Add(currentStatusToGoal);
                }
                Logger.LogInfo("Get: Current status fund allocation process completed");
                return currentStatusToGoals;
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

        public void Add(CurrentStatusToGoal CurrentStatusToGoal)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(SELECT_ID,CurrentStatusToGoal.Id,CurrentStatusToGoal.PlannerId));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(INSERT_CurrentStatusToGoal,
                      CurrentStatusToGoal.PlannerId,
                      CurrentStatusToGoal.OptionId, CurrentStatusToGoal.GoalId,
                      CurrentStatusToGoal.FundAllocation,
                      CurrentStatusToGoal.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss"), CurrentStatusToGoal.CreatedBy,
                      CurrentStatusToGoal.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), CurrentStatusToGoal.UpdatedBy), true);

                Activity.ActivitiesService.Add(ActivityType.CreateCurrentStatusToGoal, EntryStatus.Success,
                         Source.Server, CurrentStatusToGoal.UpdatedByUserName, "CurrentStatusToGoal", CurrentStatusToGoal.MachineName);
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

        public void Update(CurrentStatusToGoal CurrentStatusToGoal)
        {
            try
            {
               // string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(SELECT_ID,CurrentStatusToGoal.PlannerId));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(UPDATE_CurrentStatusToGoal,
                      CurrentStatusToGoal.FundAllocation,
                      CurrentStatusToGoal.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), CurrentStatusToGoal.UpdatedBy,
                      CurrentStatusToGoal.OptionId,
                      CurrentStatusToGoal.Id), true);

                Activity.ActivitiesService.Add(ActivityType.UpdateCurrentStatusToGoal, EntryStatus.Success,
                         Source.Server, CurrentStatusToGoal.UpdatedByUserName, "CurrentStatusToGoal", CurrentStatusToGoal.MachineName);
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

        public void Delete(CurrentStatusToGoal CurrentStatusToGoal)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(
                    string.Format(SELECT_ID,CurrentStatusToGoal.Id,CurrentStatusToGoal.PlannerId));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(DELETE_CurrentStatusToGoal,
                      CurrentStatusToGoal.Id), true);

                Activity.ActivitiesService.Add(ActivityType.DeleteCurrentStatusToGoal, EntryStatus.Success,
                         Source.Server, CurrentStatusToGoal.UpdatedByUserName, "CurrentStatusToGoal", CurrentStatusToGoal.MachineName);
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
        private CurrentStatusToGoal convertToCurrentStatusToGoal(DataRow dr)
        {
            CurrentStatusToGoal currentStatusToGoal = new CurrentStatusToGoal();
            currentStatusToGoal.Id = dr.Field<int>("ID");
            currentStatusToGoal.OptionId = dr.Field<int>("OId");
            currentStatusToGoal.PlannerId = dr.Field<int>("PId");
            currentStatusToGoal.GoalName = dr.Field<string>("GoalName");
            currentStatusToGoal.FundAllocation = double.Parse(dr["FundAllocation"].ToString());
            currentStatusToGoal.GoalId = dr.Field<int>("GoalId");
            currentStatusToGoal.UpdatedBy = dr.Field<int>("UpdatedBy");
            currentStatusToGoal.UpdatedOn = dr.Field<DateTime>("UpdatedOn");
            currentStatusToGoal.UpdatedByUserName = dr.Field<string>("UpdatedByUserName");
            return currentStatusToGoal;
        }

    }
}
