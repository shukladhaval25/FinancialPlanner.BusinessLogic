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
        private readonly string SELECT_ID = "SELECT N1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM InvestmentRecommendation N1, USERS U WHERE N1.UPDATEDBY = U.ID AND N1.PID = {0}";

        const string INSERT_CASHFLOW = "INSERT INTO InvestmentRecommendation VALUES (" +
            "{0},{1},{2},{3},{4},'{5}','{6}',{7},'{8}',{9})";

        const string UPDATE_CASHFLOW = "UPDATE InvestmentRecommendation SET " +
            "[AMCID] = {0}, SCHEMEID = {1}, AMOUNT = {2}, INVESTMENTCATEGORYID = {3}," +
            " CHEQUEINFAVOUROF = '{4}', [UpdatedOn] = '{5}', [UpdatedBy] ={6} " +
            "WHERE PID = {7} AND ID = {8}";

        const string DELETE_CASHFLOW = "DELETE FROM InvestmentRecommendation WHERE ID = {0}";

        public InvestmentRecommendation Get(int pid)
        {
            try
            {
                Logger.LogInfo("Get: Cash flow by id process start");
                InvestmentRecommendation InvestmentRecommendation = new InvestmentRecommendation();

                DataTable dtAppConfig = DataBase.DBService.ExecuteCommand(string.Format(SELECT_ID, pid));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    InvestmentRecommendation = convertToInvestmentRecommendation(dr);
                }
                Logger.LogInfo("Get: Cash flow by id process completed");
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

        private InvestmentRecommendation convertToInvestmentRecommendation(DataRow dr)
        {
            InvestmentRecommendation InvestmentRecommendation = new InvestmentRecommendation();
            InvestmentRecommendation.Id = dr.Field<int>("ID");
            InvestmentRecommendation.Pid = dr.Field<int>("PID");
            InvestmentRecommendation.AmcId = dr.Field<int>("AMCID");
            InvestmentRecommendation.SchemeId = dr.Field<int>("SchemeId");
            InvestmentRecommendation.Category = dr.Field<string>("Category");

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
                DataBase.DBService.ExecuteCommandString(string.Format(INSERT_CASHFLOW,
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

        public void Update(InvestmentRecommendation InvestmentRecommendation)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(SELECT_ID, InvestmentRecommendation.Id));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(UPDATE_CASHFLOW,
                      InvestmentRecommendation.AmcId,
                      InvestmentRecommendation.SchemeId,
                      InvestmentRecommendation.Amount,
                      InvestmentRecommendation.Category,
                      InvestmentRecommendation.ChequeInFavourOf,
                      InvestmentRecommendation.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), InvestmentRecommendation.UpdatedBy,
                      InvestmentRecommendation.Pid,
                      InvestmentRecommendation.Id), true);

                Activity.ActivitiesService.Add(ActivityType.UpdateInvestmentRecommendation, EntryStatus.Success,
                         Source.Server, InvestmentRecommendation.UpdatedByUserName, "Investment Recomendation", InvestmentRecommendation.MachineName);
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

        public void Delete(InvestmentRecommendation InvestmentRecommendation)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(SELECT_ID, InvestmentRecommendation.Id));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(DELETE_CASHFLOW,
                      InvestmentRecommendation.Id), true);

                Activity.ActivitiesService.Add(ActivityType.DeleteInvestmentRecommendation, EntryStatus.Success,
                         Source.Server, InvestmentRecommendation.UpdatedByUserName, "Investment Recommendation", InvestmentRecommendation.MachineName);
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
