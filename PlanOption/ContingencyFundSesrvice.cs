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
    public class ContingencyFundSesrvice
    {
        private readonly string SELECT_ID = "SELECT * FROM CONTINGENCYFUND WHERE OID = {0} AND PID = {1}";

        const string INSERT_CONTINGENCYFUND = "INSERT INTO CONTINGENCYFUND VALUES (" +
            "{0},{1},{2})";

        const string UPDATE_CONTINGENCYFUND = "UPDATE CONTINGENCYFUND SET " +
            "AMOUNT = {0} WHERE OID = {1} AND PID ={2}";

        const string SELECT_COUNT = "SELECT COUNT(*) FROM CONTINGENCYFUND WHERE OID = {0} AND PID = {1}";


        public ContingencyFund GetContingencyFund( int operationId, int plannerId)
        {
            ContingencyFund contingency = new ContingencyFund();
            Logger.LogInfo("Get: Contingency fund process start");

            DataTable dtFund = DataBase.DBService.ExecuteCommand(string.Format(SELECT_ID,operationId, plannerId));
            foreach (DataRow dr in dtFund.Rows)
            {
                contingency.Amount = double.Parse(dr["Amount"].ToString());
                contingency.OptionId = dr.Field<int>("OID");
                contingency.PlannerId = dr.Field<int>("PID");
                contingency.Amount = double.Parse(dr["Amount"].ToString());
            }
            Logger.LogInfo("Get: Contingency fund allocation process completed");
            return contingency;
        }
        public void Update(ContingencyFund contingencyFund)
        {
            try
            {
                 string rowCount = DataBase.DBService.ExecuteCommandScalar(string.Format(SELECT_COUNT,contingencyFund.OptionId,contingencyFund.PlannerId));
                DataBase.DBService.BeginTransaction();
                if (rowCount == "0")
                {
                    DataBase.DBService.ExecuteCommandString(string.Format(INSERT_CONTINGENCYFUND,
                     contingencyFund.PlannerId,
                     contingencyFund.OptionId,
                     contingencyFund.Amount), true);
                }
                else
                {
                    DataBase.DBService.ExecuteCommandString(string.Format(UPDATE_CONTINGENCYFUND,
                          contingencyFund.Amount,
                          contingencyFund.OptionId,
                          contingencyFund.PlannerId), true);
                }
                Activity.ActivitiesService.Add(ActivityType.UpdateCurrentStatusToGoal, EntryStatus.Success,
                         Source.Server, contingencyFund.UpdatedByUserName, "CurrentStatusToGoal", contingencyFund.MachineName);
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
