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

namespace FinancialPlanner.BusinessLogic.PlanOption
{
    public class SIPTypeInvestmentRecomendationService
    {
        private const string GET_CLIENT_NAME_QUERY = "SELECT C.NAME FROM CLIENT C, PLANNER P  WHERE P.CLIENTID = C.ID AND P.ID = {0}";
        const string SELECT_ALL = "SELECT SIPInvestmentRecomendation.Id, SIPInvestmentRecomendation.PId, " +
            "SIPInvestmentRecomendation.SchemeId, SIPInvestmentRecomendation.Amount, " +
            "SIPInvestmentRecomendation.FirstHolder, SIPInvestmentRecomendation.SecondHolder, " +
            "SIPInvestmentRecomendation.ChequeInFavourOff, SIPInvestmentRecomendation.CreatedOn, " +
            "SIPInvestmentRecomendation.CreatedBy, SIPInvestmentRecomendation.UpdatedOn, " +
            "SIPInvestmentRecomendation.UpdatedBy, " +
            "Scheme.Name AS SchemeName, Scheme.Type, SchemeCategory.Name AS Category, Users.UserName " +
            "FROM SchemeCategory INNER JOIN " +
            "Scheme ON SchemeCategory.Id = Scheme.CategoryId INNER JOIN " +
            "SIPInvestmentRecomendation INNER JOIN " +
            "Users ON SIPInvestmentRecomendation.UpdatedBy = Users.ID " +
            "ON Scheme.Id = SIPInvestmentRecomendation.SchemeId WHERE (SIPInvestmentRecomendation.PId = {0})";

        const string INSERT_LUMSUM = "INSERT INTO[dbo].[SIPInvestmentRecomendation] " +
            "([PId],[SchemeId],[Amount],[ChequeInFavourOff],[FirstHolder],[SecondHolder],[CreatedOn],[CreatedBy],[UpdatedOn],[UpdatedBy]) " +
            "VALUES ({0},{1},{2},'{3}','{4}','{5}','{6}',{7},'{8}',{9})";
        const string DELETE_QUERY = "DELETE FROM SIPInvestmentRecomendation WHERE PID = {0} AND SCHEMEID = {1} AND AMOUNT ={2}";

        public IList<SIPTypeInvestmentRecomendation> GetAll(int plannerId)
        {
            IList<SIPTypeInvestmentRecomendation> SIPInvestmentRecomendations = new List<SIPTypeInvestmentRecomendation>();
            try
            {
                Logger.LogInfo("Get: Lumsum investment process start");
                DataTable dtAppConfig = DataBase.DBService.ExecuteCommand(string.Format(SELECT_ALL, plannerId));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    SIPTypeInvestmentRecomendation SIPInvestmentRecomendation = convertToSIPInvestmentRecomendationObject(dr);
                    SIPInvestmentRecomendations.Add(SIPInvestmentRecomendation);
                }
                Logger.LogInfo("Get: Lumsum investment process completed.");
            }
            catch (Exception ex)
            {
                StackTrace st = new StackTrace();
                StackFrame sf = st.GetFrame(0);
                MethodBase currentMethodName = sf.GetMethod();
                LogDebug(currentMethodName.Name, ex);
            }
            return SIPInvestmentRecomendations;
        }

        public void Add(SIPTypeInvestmentRecomendation SIPInvestmentRecomendation)
        {
            try
            {
                //string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY, SIPInvestmentRecomendation.Pid));

                DataBase.DBService.ExecuteCommand(string.Format(INSERT_LUMSUM,
                   SIPInvestmentRecomendation.Pid,
                   SIPInvestmentRecomendation.SchemeId,
                   SIPInvestmentRecomendation.Amount,
                   SIPInvestmentRecomendation.ChequeInFavourOff,
                   SIPInvestmentRecomendation.FirstHolder,
                   SIPInvestmentRecomendation.SecondHolder,
                   SIPInvestmentRecomendation.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss"),
                   SIPInvestmentRecomendation.CreatedBy,
                   SIPInvestmentRecomendation.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"),
                   SIPInvestmentRecomendation.UpdatedBy));

                Activity.ActivitiesService.Add(ActivityType.CreateInvestmentRecommendation, EntryStatus.Success,
                         Source.Server, SIPInvestmentRecomendation.UpdatedByUserName, SIPInvestmentRecomendation.SchemeName, SIPInvestmentRecomendation.MachineName);
            }
            catch (Exception ex)
            {
                StackTrace st = new StackTrace();
                StackFrame sf = st.GetFrame(0);
                MethodBase currentMethodName = sf.GetMethod();
                LogDebug(currentMethodName.Name, ex);
                throw ex;
            }
        }

        public void Delete(SIPTypeInvestmentRecomendation SIPInvestmentRecomendation)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY, SIPInvestmentRecomendation.Pid));

                DataBase.DBService.ExecuteCommand(string.Format(DELETE_QUERY,
                  SIPInvestmentRecomendation.Pid, SIPInvestmentRecomendation.SchemeId, SIPInvestmentRecomendation.Amount));

                Activity.ActivitiesService.Add(ActivityType.DeleteInvestmentRecommendation, EntryStatus.Success,
                         Source.Server, SIPInvestmentRecomendation.UpdatedByUserName, SIPInvestmentRecomendation.SchemeName,
                         SIPInvestmentRecomendation.MachineName);
            }
            catch (Exception ex)
            {
                StackTrace st = new StackTrace();
                StackFrame sf = st.GetFrame(0);
                MethodBase currentMethodName = sf.GetMethod();
                LogDebug(currentMethodName.Name, ex);
                throw ex;
            }
        }

        private SIPTypeInvestmentRecomendation convertToSIPInvestmentRecomendationObject(DataRow dr)
        {
            SIPTypeInvestmentRecomendation SIPInvestmentRecomendation = new SIPTypeInvestmentRecomendation();
            SIPInvestmentRecomendation.Pid = dr.Field<int>("PId");
            SIPInvestmentRecomendation.SchemeId = dr.Field<int>("SchemeId");
            SIPInvestmentRecomendation.SchemeName = dr.Field<string>("SchemeName");
            SIPInvestmentRecomendation.Amount = double.Parse(dr["Amount"].ToString());
            SIPInvestmentRecomendation.Category = dr.Field<string>("Category");
            SIPInvestmentRecomendation.ChequeInFavourOff = dr.Field<string>("ChequeInfavourOff");
            SIPInvestmentRecomendation.FirstHolder = dr.Field<string>("FirstHolder");
            SIPInvestmentRecomendation.SecondHolder = dr.Field<string>("SecondHolder");
            SIPInvestmentRecomendation.Type = dr.Field<string>("Type");
            return SIPInvestmentRecomendation;
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
