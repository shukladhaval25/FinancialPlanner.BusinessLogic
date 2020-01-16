using FinancialPlanner.Common;
using FinancialPlanner.Common.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Reflection;

namespace FinancialPlanner.BusinessLogic.PlanOption
{
    public class QuarterlyReviewTemplateService
    {
        private const string INSERT_QUARTERLY_REVIEW_TEMPLATE = "INSERT INTO " + "[QuarterlyReviewTemplate] ([cid],[IsSelected],[InvestmentType], " +
            "[IsLoanSelected]) VALUES (" +
            "{0},'{1}','{2}','{3}')";

        private const string GET_QUARTERLY_REVIEW_TEMPLATE = "SELECT *  FROM [QuarterlyReviewTemplate] WHERE CID = {0}";
        private const string DELETE_QUARTERLY_REVIEW_TEMPLATE = "DELETE FROM [QuarterlyReviewTemplate] WHERE CID = {0}";
        public void Add(IList<QuarterlyReviewTemplate> quarterlyReviewTemplates)
        {
            try
            {
                DataBase.DBService.BeginTransaction();
                if (quarterlyReviewTemplates.Count > 0)
                    DataBase.DBService.ExecuteCommandString(string.Format(DELETE_QUARTERLY_REVIEW_TEMPLATE,
                        quarterlyReviewTemplates[0].Cid), true);

                foreach (QuarterlyReviewTemplate quarterlyReviewTemplate in quarterlyReviewTemplates)
                {
                    DataBase.DBService.ExecuteCommandString(string.Format(INSERT_QUARTERLY_REVIEW_TEMPLATE,
                     quarterlyReviewTemplate.Cid,
                     quarterlyReviewTemplate.IsSelected,
                     quarterlyReviewTemplate.InvestmentType,
                     quarterlyReviewTemplate.IsLoanSelected),true);
                }

                if (quarterlyReviewTemplates.Count > 0)
                Activity.ActivitiesService.Add(ActivityType.UpdateQuarterlyReviewTemplate, EntryStatus.Success,
                         Source.Server, quarterlyReviewTemplates[0].UpdatedByUserName, "", quarterlyReviewTemplates[0].MachineName);
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

        public IList<QuarterlyReviewTemplate> Get(int clientId)
        {
            try
            {
                Logger.LogInfo("Get: Quarterly review template process start");
                IList<QuarterlyReviewTemplate> quarterlyReviewTemplates = new List<QuarterlyReviewTemplate>();

                DataTable dtAppConfig = DataBase.DBService.ExecuteCommand(string.Format(GET_QUARTERLY_REVIEW_TEMPLATE,
                    clientId));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    QuarterlyReviewTemplate quarterlyReviewTemplate = convertToQuarterlyReviewTemplateObject(dr);
                    quarterlyReviewTemplates.Add(quarterlyReviewTemplate);
                }
                Logger.LogInfo("Get: Quarterly review template process completed.");
                return quarterlyReviewTemplates;
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

        private QuarterlyReviewTemplate convertToQuarterlyReviewTemplateObject(DataRow dr)
        {
            QuarterlyReviewTemplate quarterlyReviewTemplate = new QuarterlyReviewTemplate();
            quarterlyReviewTemplate.Id = dr.Field<int>("ID");
            quarterlyReviewTemplate.Cid = dr.Field<int>("cid");
            quarterlyReviewTemplate.IsSelected = dr.Field<bool>("IsSelected");
            quarterlyReviewTemplate.InvestmentType = dr.Field<string>("InvestmentType");
            quarterlyReviewTemplate.IsLoanSelected = dr.Field<bool>("IsLoanSelected");
            return quarterlyReviewTemplate;
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
