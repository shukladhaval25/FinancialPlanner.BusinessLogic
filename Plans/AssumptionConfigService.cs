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

namespace FinancialPlanner.BusinessLogic.Plans
{
    public class AssumptionConfigService
    {
        private const string GET_CLIENT_NAME_QUERY = "SELECT C.NAME FROM CLIENT C, PLANNER P  WHERE P.CLIENTID = C.ID AND P.ID = {0}";

        private readonly string SELECT_ALL = "SELECT * FROM [FinancialPlanner].[dbo].[AssumptionConfig] where planId = {0}";

        const string SELECT_COUNT = "SELECT COUNT(*) FROM AssumptionConfig N1 WHERE N1.PlanId = {0}";

        const string INSERT_QUERY = "INSERT INTO [FinancialPlanner].[dbo].[AssumptionConfig]  VALUES (" +
            "{0},{1},{2},{3},{4})";

        const string UPDATE_QUERY = "UPDATE [FinancialPlanner].[dbo].[AssumptionConfig] SET " +
            "RateOfInflation = {0}," +
            "PostTaxRateOfReturn = {1}," +
            "RegularOngoingExp = {2}," +
            "PostRetirementInvestmentReturn = {3}" +
            " WHERE PLANID ={4}";

        public AssumptionConfig GetAll(int plannerId)
        {
            try
            {
                Logger.LogInfo("Get: AssumptionConfig process start");
                AssumptionConfig plannerAssumption = new AssumptionConfig();

                DataTable dtAppConfig = DataBase.DBService.ExecuteCommand(string.Format(SELECT_ALL, plannerId));
                if (dtAppConfig.Rows.Count == 0)
                {
                    plannerAssumption.PlannerId = plannerId;
                    plannerAssumption.RegularOngoingExp = true;
                    plannerAssumption.PostRetirementInvestmentReturn = true;
                    plannerAssumption.PostTaxRateOfReturn = true;
                    plannerAssumption.RateOfInflation = true;
                }
                else
                {
                    foreach (DataRow dr in dtAppConfig.Rows)
                    {
                        plannerAssumption = convertToAssumptionConfigObject(dr);
                    }
                }
                Logger.LogInfo("Get: AssumptionConfig process completed.");
                return plannerAssumption;
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

        public void Update(AssumptionConfig assumptionConfig)
        {
            try
            {
                string clientName =
                    DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY, assumptionConfig.PlannerId));
                string recordCount = DataBase.DBService.ExecuteCommandScalar(string.Format(SELECT_COUNT, assumptionConfig.PlannerId));
                DataBase.DBService.BeginTransaction();
                if (recordCount != "0")
                {
                    DataBase.DBService.ExecuteCommandString(string.Format(UPDATE_QUERY,
                       (assumptionConfig.RateOfInflation) ? 1 : 0,
                       (assumptionConfig.PostTaxRateOfReturn) ? 1 : 0,
                       (assumptionConfig.RegularOngoingExp) ? 1 : 0,
                       (assumptionConfig.PostRetirementInvestmentReturn) ? 1 : 0,
                       assumptionConfig.PlannerId), true);
                }
                else
                {
                    DataBase.DBService.ExecuteCommandString(string.Format(INSERT_QUERY,
                      assumptionConfig.PlannerId,
                      (assumptionConfig.RateOfInflation) ? 1 : 0,
                      (assumptionConfig.PostTaxRateOfReturn) ? 1 : 0,
                      (assumptionConfig.RegularOngoingExp) ? 1 : 0,
                      (assumptionConfig.PostRetirementInvestmentReturn) ? 1 : 0), true);
                }
                //Activity.ActivitiesService.Add(ActivityType.UpdateAssumptionConfig, EntryStatus.Success,
                //            Source.Server, assumptionConfig.UpdatedByUserName, clientName, assumptionConfig.MachineName);
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

        private AssumptionConfig convertToAssumptionConfigObject(DataRow dr)
        {
            AssumptionConfig assumptionConfig = new AssumptionConfig();
            assumptionConfig.PlannerId = dr.Field<int>("PlanId");
            assumptionConfig.RateOfInflation = bool.Parse(dr["RateOfInflation"].ToString());
            assumptionConfig.PostTaxRateOfReturn = bool.Parse(dr["PostTaxRateOfReturn"].ToString());
            assumptionConfig.RegularOngoingExp = bool.Parse(dr["RegularOngoingExp"].ToString());
            assumptionConfig.PostRetirementInvestmentReturn =bool.Parse(dr["PostRetirementInvestmentReturn"].ToString());
            return assumptionConfig;
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
