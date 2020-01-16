using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using FinancialPlanner.Common;
using FinancialPlanner.Common.Model;

namespace FinancialPlanner.BusinessLogic.PlanOption
{
    public class STPInvestmentRecomendationService
    {
        private const string GET_CLIENT_NAME_QUERY = "SELECT C.NAME FROM CLIENT C, PLANNER P  WHERE P.CLIENTID = C.ID AND P.ID = {0}";
       
        const string SELECT_ALL = "SELECT STPInvestmentRecomendation.*, Scheme.Name AS FromSchemeName FROM Scheme INNER JOIN " +
                         "STPInvestmentRecomendation ON Scheme.Id = STPInvestmentRecomendation.FromSchemeId AND " +
                         "(Scheme.Id = STPInvestmentRecomendation.ToSchemeId OR " +
                         "scheme.Id = STPInvestmentRecomendation.FromSchemeId)INNER JOIN " +
                         "Users ON STPInvestmentRecomendation.UpdatedBy = Users.ID " +
                         "WHERE(STPInvestmentRecomendation.PId = {0})";

        const string INSERT_QUERY = "INSERT INTO[dbo].[STPInvestmentRecomendation]" +
            "([PId],[FromSchemeId],[ToSchemeId],[Amount],[Duration],[Frequency],[CreatedOn]" +
            ",[CreatedBy],[UpdatedOn],[UpdatedBy]) VALUES (" +
            "{0},{1},{2},{3},{4},'{5}','{6}',{7},'{8}',{9})";

        const string DELETE_QUERY = "DELETE FROM STPInvestmentRecomendation WHERE PID = {0} AND ToSchemeId = {1} AND AMOUNT ={2}";

        public IList<STPTypeInvestmentRecomendation> GetAll(int plannerId)
        {
            IList<STPTypeInvestmentRecomendation> lumsumInvestmentRecomendations = new List<STPTypeInvestmentRecomendation>();
            try
            {
                Logger.LogInfo("Get: STP investment process start");
                DataTable dtAppConfig = DataBase.DBService.ExecuteCommand(string.Format(SELECT_ALL, plannerId));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    STPTypeInvestmentRecomendation lumsumInvestmentRecomendation = convertToSTPInvestmentRecomendationObject(dr);
                    lumsumInvestmentRecomendations.Add(lumsumInvestmentRecomendation);
                }
                Logger.LogInfo("Get: STP investment process completed.");
            }
            catch (Exception ex)
            {
                StackTrace st = new StackTrace();
                StackFrame sf = st.GetFrame(0);
                MethodBase currentMethodName = sf.GetMethod();
                LogDebug(currentMethodName.Name, ex);
            }
            return lumsumInvestmentRecomendations;
        }

        public void Add(STPTypeInvestmentRecomendation lumsumInvestmentRecomendation)
        {
            try
            {
                //string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY, lumsumInvestmentRecomendation.Pid));

                DataBase.DBService.ExecuteCommand(string.Format(INSERT_QUERY,
                   lumsumInvestmentRecomendation.Pid,
                   lumsumInvestmentRecomendation.FromSchemeId,
                   lumsumInvestmentRecomendation.SchemeId,
                   lumsumInvestmentRecomendation.Amount,                   
                   lumsumInvestmentRecomendation.Duration,
                   lumsumInvestmentRecomendation.Frequency,
                   lumsumInvestmentRecomendation.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss"),
                   lumsumInvestmentRecomendation.CreatedBy,
                   lumsumInvestmentRecomendation.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"),
                   lumsumInvestmentRecomendation.UpdatedBy));

                Activity.ActivitiesService.Add(ActivityType.CreateInvestmentRecommendation, EntryStatus.Success,
                         Source.Server, lumsumInvestmentRecomendation.UpdatedByUserName, lumsumInvestmentRecomendation.SchemeName, lumsumInvestmentRecomendation.MachineName);
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

        public void Delete(STPTypeInvestmentRecomendation lumsumInvestmentRecomendation)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY, lumsumInvestmentRecomendation.Pid));

                DataBase.DBService.ExecuteCommand(string.Format(DELETE_QUERY,
                  lumsumInvestmentRecomendation.Pid, lumsumInvestmentRecomendation.SchemeId, lumsumInvestmentRecomendation.Amount));

                Activity.ActivitiesService.Add(ActivityType.DeleteInvestmentRecommendation, EntryStatus.Success,
                         Source.Server, lumsumInvestmentRecomendation.UpdatedByUserName, lumsumInvestmentRecomendation.SchemeName,
                         lumsumInvestmentRecomendation.MachineName);
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

        private STPTypeInvestmentRecomendation convertToSTPInvestmentRecomendationObject(DataRow dr)
        {
            STPTypeInvestmentRecomendation stpTypeInvestmentRecommendation = new STPTypeInvestmentRecomendation();
            stpTypeInvestmentRecommendation.Pid = dr.Field<int>("PId");
            stpTypeInvestmentRecommendation.FromSchemeId = dr.Field<int>("FromSchemeId");
            stpTypeInvestmentRecommendation.FromSchemeName = dr.Field<string>("FromSchemeName");
            stpTypeInvestmentRecommendation.SchemeId = dr.Field<int>("ToSchemeId");
            stpTypeInvestmentRecommendation.SchemeName = getSchemeName(stpTypeInvestmentRecommendation.SchemeId);
            stpTypeInvestmentRecommendation.Amount = double.Parse(dr["Amount"].ToString());
            stpTypeInvestmentRecommendation.Duration = dr.Field<int>("Duration");
            stpTypeInvestmentRecommendation.Frequency = dr.Field<string>("Frequency");
            return stpTypeInvestmentRecommendation;
        }

        private string getSchemeName(int schemeId)
        {
            string schemeName = DataBase.DBService.ExecuteCommandScalar("Select Name from scheme where id =" + schemeId);
            return schemeName;
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
