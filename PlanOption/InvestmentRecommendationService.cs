using FinancialPlanner.Common;
using FinancialPlanner.Common.Model;
using FinancialPlanner.Common.Model.PlanOptions;
using System;
using System.Data;
using System.Diagnostics;
using System.Reflection;

namespace FinancialPlanner.BusinessLogic.PlanOption
{
    public class InvestmentRecommendationService
    {
        private readonly string SELECT_ID = "SELECT N1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM InvestmentRatio N1, USERS U WHERE N1.UPDATEDBY = U.ID AND N1.PID = {0}";

        const string INSERT_INVESTMENTRATIO= "INSERT INTO InvestmentRatio VALUES (" +
            "{0},{1},{2},'{3}',{4},'{5}',{6})";


        const string DELETE_INVESTMENTRECOMMENDATIONRATIO = "DELETE FROM InvestmentRatio WHERE ID = {0}";

        public InvestmentRecommendationRatio Get(int pid)
        {
            try
            {
                Logger.LogInfo("Get: Investment recommendation ratio process start");
                InvestmentRecommendationRatio InvestmentRecommendation = new InvestmentRecommendationRatio();

                DataTable dtAppConfig = DataBase.DBService.ExecuteCommand(string.Format(SELECT_ID, pid));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    InvestmentRecommendation = convertToInvestmentRecommendation(dr);
                }
                Logger.LogInfo("Get: Investment recommendation ratio process completed");
                return InvestmentRecommendation;
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

        public void Add(InvestmentRecommendationRatio investmentRecommendationRatio)
        {
            try
            {
                //DataBase.DBService.BeginTransaction();

                DataBase.DBService.ExecuteCommandString(string.Format(DELETE_INVESTMENTRECOMMENDATIONRATIO, investmentRecommendationRatio.Pid));

                DataBase.DBService.ExecuteCommandString(string.Format(INSERT_INVESTMENTRATIO,
                      investmentRecommendationRatio.Pid, investmentRecommendationRatio.EquityRatio,
                      investmentRecommendationRatio.DebtRatio,
                      investmentRecommendationRatio.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss"),
                      investmentRecommendationRatio.CreatedBy,
                      investmentRecommendationRatio.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"),
                      investmentRecommendationRatio.UpdatedBy));

                Activity.ActivitiesService.Add(ActivityType.CreateInvestmentRecommendation, EntryStatus.Success,
                         Source.Server, investmentRecommendationRatio.UpdatedByUserName, "Investment Ratio", investmentRecommendationRatio.MachineName);
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

        private InvestmentRecommendationRatio convertToInvestmentRecommendation(DataRow dr)
        {
            InvestmentRecommendationRatio InvestmentRecommendation = new InvestmentRecommendationRatio();
            InvestmentRecommendation.Id = dr.Field<int>("ID");
            InvestmentRecommendation.Pid = dr.Field<int>("PID");
            InvestmentRecommendation.EquityRatio = double.Parse(dr["EquityRatio"].ToString());
            InvestmentRecommendation.DebtRatio = double.Parse(dr["DebtRatio"].ToString());
            InvestmentRecommendation.UpdatedBy = dr.Field<int>("UpdatedBy");
            InvestmentRecommendation.UpdatedOn = dr.Field<DateTime>("UpdatedOn");
            InvestmentRecommendation.UpdatedByUserName = dr.Field<string>("UpdatedByUserName");
            return InvestmentRecommendation;
        }

        public void Add(InvestmentRecommendation InvestmentRecommendation)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(SELECT_ID, InvestmentRecommendation.Id));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(INSERT_INVESTMENTRATIO,
                      InvestmentRecommendation.Pid,InvestmentRecommendation.AmcId,
                      InvestmentRecommendation.SchemeId,InvestmentRecommendation.Amount,
                      InvestmentRecommendation.Category,InvestmentRecommendation.ChequeInFavourOf,
                      InvestmentRecommendation.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss"), 
                      InvestmentRecommendation.CreatedBy,
                      InvestmentRecommendation.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), 
                      InvestmentRecommendation.UpdatedBy), true);

                Activity.ActivitiesService.Add(ActivityType.CreateInvestmentRecommendation, EntryStatus.Success,
                         Source.Server, InvestmentRecommendation.UpdatedByUserName, "InvestmentRecommendation", InvestmentRecommendation.MachineName);
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
