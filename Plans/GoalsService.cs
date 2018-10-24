using FinancialPlanner.Common;
using FinancialPlanner.Common.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FinancialPlanner.BusinessLogic
{
    public class GoalsService
    {
        private const string GET_CLIENT_NAME_QUERY = "SELECT C.NAME FROM CLIENT C, PLANNER P  WHERE P.CLIENTID = C.ID AND P.ID = {0}";
        const string SELECT_ALL = "SELECT N1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM Goals N1, USERS U WHERE N1.UPDATEDBY = U.ID AND N1.PID = {0}";
        const string SELECT_BYID = "SELECT N1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM Goals N1, USERS U WHERE N1.UPDATEDBY = U.ID AND N1.ID = {0} AND N1.PID ={1}";

        const string SELECT_LOANGFORGOAL_BYID = "SELECT N1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM LOANFORGOALS N1, USERS U WHERE N1.UPDATEDBY = U.ID AND N1.GOALID ={0}";

        const string INSERT_QUERY = "INSERT INTO Goals VALUES (" +
            "{0},'{1}','{2}',{3},'{4}','{5}',{6},{7},'{8}','{9}',{10},'{11}',{12},{13})";
        const string INSERT_GOALLOAN_QUERY = "INSERT INTO LOANFORGOALS " + 
            "VALUES ({0},{1},{2},{3},{4},{5},{6},'{7}',{8},'{9}',{10})";

        const string UPDATE_QUERY = "UPDATE Goals SET " +
            "CATEGORY = '{0}',NAME ='{1}',AMOUNT = {2}, " +
            "STARTYEAR = '{3}', ENDYEAR = '{4}',RECURRENCE ={5}," +
            "PRIORITY ={6},DESCRIPTION ='{7}',UPDATEDON = '{8}'," +
            "UPDATEDBY={9},INFLATIONRATE = {10} WHERE ID ={11}";

        const string UPDATE_LOANFORGOAL_QUERY = "UPDATE LOANFORGOALS SET LOANAMOUNT = {0}," +
            "EMI = {1}, ROI = {2}, LOANYEARS = {3}, STARTYEAR = {4}, ENDYEAR = {5}, " +
            "UPDATEDON = '{6}', UPDATEDBY = {7} WHERE GOALID = {8}";

        const string DELET_QUERY = "DELETE FROM Goals WHERE ID ={0}";
        private readonly string DELETE_LOANFORGOAL_QUERY = "DELETE FROM LOANFORGOALS WHERE GOALID ={0}";

        public IList<Goals> GetAll(int plannerId)
        {
            try
            {
                Logger.LogInfo("Get: Goals process start");
                IList<Goals> lstGoals = new List<Goals>();

                DataTable dtGoals =  DataBase.DBService.ExecuteCommand(string.Format(SELECT_ALL,plannerId));
                foreach (DataRow dr in dtGoals.Rows)
                {
                    Goals Goals = convertToGoalsObject(dr);
                    Goals.LoanForGoal =  GetLoansForGoalInfo(Goals.Id);
                    lstGoals.Add(Goals);
                }
                Logger.LogInfo("Get: Goals process completed.");
                return lstGoals;
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

        private LoanForGoal GetLoansForGoalInfo(int goalId)
        {
            DataTable dtLoanForGoal =  DataBase.DBService.ExecuteCommand(string.Format(SELECT_LOANGFORGOAL_BYID,goalId));
            if (dtLoanForGoal != null && dtLoanForGoal.Rows.Count > 0)
            {
                LoanForGoal loanForGoal = convertToLoanForGoalsObject(dtLoanForGoal.Rows[0]);
                return loanForGoal;
            }
            return null;
        }

        private LoanForGoal convertToLoanForGoalsObject(DataRow dr)
        {
            LoanForGoal loanForGoal = new LoanForGoal();
            loanForGoal.Id = dr.Field<int>("ID");
            loanForGoal.GoalId = dr.Field<int>("GoalID");            
            loanForGoal.LoanAmount = double.Parse(dr["LoanAmount"].ToString());
            loanForGoal.EMI = double.Parse( dr["EMI"].ToString());
            loanForGoal.ROI = decimal.Parse(dr["ROI"].ToString());
            loanForGoal.LoanYears = dr.Field<int>("LoanYears");
            loanForGoal.StratYear = dr.Field<int>("StartYear");
            loanForGoal.EndYear = dr.Field<int>("EndYear");

            return loanForGoal;
        }

        public Goals GetById(int id, int plannerId)
        {
            try
            {
                Logger.LogInfo("Get: Goals process start");
                Goals Goals = new Goals();

                DataTable dtAppConfig =  DataBase.DBService.ExecuteCommand(string.Format(SELECT_BYID,id,plannerId));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    Goals = convertToGoalsObject(dr);
                    Goals.LoanForGoal = GetLoansForGoalInfo(Goals.Id); 
                }
                Logger.LogInfo("Get: Goals process completed.");
                return Goals;
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

        public void Add(Goals Goals)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY,Goals.Pid));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(INSERT_QUERY,
                      Goals.Pid, Goals.Category, Goals.Name,
                      Goals.Amount,Goals.StartYear,Goals.EndYear,
                      Goals.Recurrence,Goals.Priority,Goals.Description,                      
                      Goals.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss"), Goals.CreatedBy,
                      Goals.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), Goals.UpdatedBy,
                      Goals.InflationRate), true);

                if (Goals.LoanForGoal != null)
                    addLoanForGoal(Goals);

                Activity.ActivitiesService.Add(ActivityType.CreateGoals, EntryStatus.Success,
                         Source.Server, Goals.UpdatedByUserName, clientName, Goals.MachineName);
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

        private static void addLoanForGoal(Goals Goals)
        {
            DataBase.DBService.ExecuteCommandString(string.Format(INSERT_GOALLOAN_QUERY,
                                  Goals.LoanForGoal.GoalId, Goals.LoanForGoal.LoanAmount,
                                  Goals.LoanForGoal.EMI, Goals.LoanForGoal.ROI,
                                  Goals.LoanForGoal.LoanYears,
                                  Goals.LoanForGoal.StratYear, Goals.LoanForGoal.EndYear,
                                  Goals.LoanForGoal.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss"),
                                  Goals.LoanForGoal.CreatedBy,
                                  Goals.LoanForGoal.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"),
                                  Goals.LoanForGoal.UpdatedBy), true);
        }

        public void Update(Goals Goals)
        {
            try
            {
                string clientName =
                    DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY,Goals.Pid));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(UPDATE_QUERY,
                   Goals.Category,Goals.Name,Goals.Amount,
                   Goals.StartYear,Goals.EndYear,
                   Goals.Recurrence, Goals.Priority,Goals.Description,
                   Goals.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), 
                   Goals.UpdatedBy,Goals.InflationRate, Goals.Id), true);

                if (Goals.LoanForGoal != null && Goals.LoanForGoal.Id == 0)
                    addLoanForGoal(Goals);
                else if (Goals.LoanForGoal != null && Goals.LoanForGoal.Id != 0)
                    updateLoansForGoal(Goals);
                else
                    DataBase.DBService.ExecuteCommandString(string.Format(DELETE_LOANFORGOAL_QUERY, Goals.Id), true);

                Activity.ActivitiesService.Add(ActivityType.UpdateGoals, EntryStatus.Success,
                         Source.Server, Goals.UpdatedByUserName, clientName, Goals.MachineName);
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

        private void updateLoansForGoal(Goals goals)
        {
            DataBase.DBService.ExecuteCommandString(string.Format(UPDATE_LOANFORGOAL_QUERY,
                  goals.LoanForGoal.LoanAmount, goals.LoanForGoal.EMI, goals.LoanForGoal.ROI,
                  goals.LoanForGoal.LoanYears,
                  goals.LoanForGoal.StratYear , goals.LoanForGoal.EndYear,
                  goals.LoanForGoal.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"),
                  goals.LoanForGoal.UpdatedBy, goals.LoanForGoal.GoalId), true);
        }

        public void Delete(Goals Goals)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY,Goals.Pid));
                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(DELETE_LOANFORGOAL_QUERY, Goals.Id), true);
                DataBase.DBService.ExecuteCommandString(string.Format(DELET_QUERY, Goals.Id), true);

                Activity.ActivitiesService.Add(ActivityType.DeleteGoals, EntryStatus.Success,
                         Source.Server, Goals.UpdatedByUserName, clientName, Goals.MachineName);
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
        private Goals convertToGoalsObject(DataRow dr)
        {
            Goals Goals = new Goals();
            Goals.Id = dr.Field<int>("ID");
            Goals.Pid = dr.Field<int>("PID");
            Goals.Category = dr.Field<string>("Category");
            Goals.Name = dr.Field<string>("Name");
            Goals.Amount = double.Parse(dr["Amount"].ToString());
            Goals.StartYear = dr.Field<string>("StartYear");
            Goals.EndYear = dr.Field<string>("EndYear");
            Goals.Recurrence = dr.Field<int>("Recurrence");
            Goals.Priority = dr.Field<int>("Priority");
            Goals.Description = dr.Field<string>("Description");
            Goals.UpdatedOn = dr.Field<DateTime>("UpdatedOn");
            Goals.UpdatedBy = dr.Field<int>("UpdatedBy");
            Goals.InflationRate = dr.Field<decimal>("InflationRate");
            return Goals;
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
