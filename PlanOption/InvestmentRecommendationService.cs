using FinancialPlanner.BusinessLogic.ApplictionConfiguration;
using FinancialPlanner.Common;
using FinancialPlanner.Common.Model;
using FinancialPlanner.Common.Model.PlanOptions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace FinancialPlanner.BusinessLogic.PlanOption
{
    public class InvestmentRecommendationService
    {
        private readonly string SELECT_ID = "SELECT N1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM InvestmentRatio N1, USERS U WHERE N1.UPDATEDBY = U.ID AND N1.PID = {0}";

        const string INSERT_INVESTMENTRATIO= "INSERT INTO InvestmentRatio VALUES (" +
            "{0},{1},{2},'{3}',{4},'{5}',{6})";


        const string DELETE_INVESTMENTRECOMMENDATIONRATIO = "DELETE FROM InvestmentRatio WHERE ID = {0}";

        const string ADD_SEND_INVESTMENT_RECOMMENDATION = "INSERT" +
            "[InvestmentRecommendationSend] ([PID],[SendDate],[ReportDataPath],[FileName]) VALUES ({0},'{1}','{2}','{3}')";

        const string GET_SEND_INVESTMENT_RECOMMENDATION = "SELECT *   FROM[FinancialPlanner].[dbo].[InvestmentRecommendationSend] " +
          "WHERE PID = {0}";

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
                DataBase.DBService.BeginTransaction();

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

        public IList<InvRecommendationSend> GetSendReports(int plannerId)
        {
            IList<InvRecommendationSend> recommendationSends = new List<InvRecommendationSend>();

            DataTable dtAppConfig = DataBase.DBService.ExecuteCommand(string.Format(GET_SEND_INVESTMENT_RECOMMENDATION,plannerId));
            foreach (DataRow dr in dtAppConfig.Rows)
            {
                InvRecommendationSend recommendationSend = convertToInvRecSendObject(dr);
                recommendationSends.Add(recommendationSend);
            }
            return recommendationSends;
        }

        private InvRecommendationSend convertToInvRecSendObject(DataRow dr)
        {
            InvRecommendationSend recommendationSend = new InvRecommendationSend();
            recommendationSend.Id = dr.Field<int>("Id");
            recommendationSend.Pid = dr.Field<int>("PID");
            recommendationSend.SendDate = dr.Field<DateTime>("SendDate");
            recommendationSend.ReportDataPath = dr.Field<string>("ReportDataPath");
            recommendationSend.FileName = dr.Field<string>("FileName");
            return recommendationSend;
        }

        public void AddSendReport(InvRecommendationSend invRecommendationSend)
        {
            try
            {
               // DataBase.DBService.BeginTransaction();
                string filePath = getInvestmentRecommendationSendReportPath(invRecommendationSend.ClientId.ToString(), invRecommendationSend.Pid.ToString());

                if (invRecommendationSend.ReportDataPath != null)
                {
                    byte[] arrBytes = Convert.FromBase64String(invRecommendationSend.ReportDataPath);
                    File.WriteAllBytes(filePath, arrBytes);
                }
                
                DataBase.DBService.ExecuteCommandString(string.Format(ADD_SEND_INVESTMENT_RECOMMENDATION, invRecommendationSend.Pid, invRecommendationSend.SendDate.ToString("yyyy-MM-dd hh:mm:ss"), filePath, Path.GetFileName(filePath)));
               // DataBase.DBService.CommitTransaction();
            }
            catch(Exception ex)
            {
                //DataBase.DBService.RollbackTransaction();
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

        private string getInvestmentRecommendationSendReportPath(string clientId, string plannerId)
        {
            string applicationPath = getApplicationPath();

            if (applicationPath == null)
                return null;
            if (!Directory.Exists(Path.Combine(applicationPath, clientId, plannerId)))
            {
                System.IO.Directory.CreateDirectory(
                    Path.Combine(applicationPath, clientId, plannerId));
            }
            string fileName = "InvRec - " + DateTime.Now.Date.ToString("dd-MM-yyyy") + ".pdf";
            return Path.Combine(applicationPath, clientId,plannerId,fileName );
        }

        private string getApplicationPath()
        {
            ApplicationConfiService appConfig = new ApplicationConfiService();
            IList<ApplicationConfiguration> appConfigs = appConfig.Get();
            var resultConfig = appConfigs.First(i => i.SettingName == "Application Path");
            if (resultConfig != null)
                return resultConfig.SettingValue.ToString();
            return null;
        }
    }
}
