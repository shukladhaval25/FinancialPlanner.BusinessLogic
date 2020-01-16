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
    public class SwitchInvestmentRecommendationService
    {
        private const string GET_CLIENT_NAME_QUERY = "SELECT C.NAME FROM CLIENT C, PLANNER P  WHERE P.CLIENTID = C.ID AND P.ID = {0}";

        const string SELECT_ALL = "SELECT SwitchInvestmentRecommendation.Id, SwitchInvestmentRecommendation.PId, SwitchInvestmentRecommendation.ToSchemeId, SwitchInvestmentRecommendation.FromSchemeId, SwitchInvestmentRecommendation.Amount, Scheme.Name As FromScheme FROM SwitchInvestmentRecommendation INNER JOIN Scheme ON SwitchInvestmentRecommendation.FromSchemeId = Scheme.Id WHERE (SwitchInvestmentRecommendation.PId = {0})";

        const string INSERT_QUERY = "INSERT INTO[dbo].[SwitchInvestmentRecommendation]" +
            "([PId],[FromSchemeId],[ToSchemeId],[Amount],[CreatedOn]" +
            ",[CreatedBy],[UpdatedOn],[UpdatedBy]) VALUES (" +
            "{0},{1},{2},{3},'{4}',{5},'{6}',{7})";

        const string DELETE_QUERY = "DELETE FROM SwitchInvestmentRecommendation WHERE PID = {0} AND ToSchemeId = {1} AND AMOUNT ={2}";

        public IList<SwitchTypeInvestmentRecommendation> GetAll(int plannerId)
        {
            IList<SwitchTypeInvestmentRecommendation> lumsumInvestmentRecomendations = new List<SwitchTypeInvestmentRecommendation>();
            try
            {
                Logger.LogInfo("Get: STP investment process start");
                DataTable dtAppConfig = DataBase.DBService.ExecuteCommand(string.Format(SELECT_ALL, plannerId));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    SwitchTypeInvestmentRecommendation switchType = convertToSTPInvestmentRecomendationObject(dr);
                    lumsumInvestmentRecomendations.Add(switchType);
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

        public void Add(SwitchTypeInvestmentRecommendation switchTypeInvestment)
        {
            try
            {
                //string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY, lumsumInvestmentRecomendation.Pid));

                DataBase.DBService.ExecuteCommand(string.Format(INSERT_QUERY,
                   switchTypeInvestment.Pid,
                   switchTypeInvestment.FromSchemeId,
                   switchTypeInvestment.ToSchemeId,
                   switchTypeInvestment.Amount,
                   switchTypeInvestment.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss"),
                   switchTypeInvestment.CreatedBy,
                   switchTypeInvestment.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"),
                   switchTypeInvestment.UpdatedBy));

                Activity.ActivitiesService.Add(ActivityType.CreateInvestmentRecommendation, EntryStatus.Success,
                         Source.Server, switchTypeInvestment.UpdatedByUserName, switchTypeInvestment.ToSchemeName, switchTypeInvestment.MachineName);
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

        public void Delete(SwitchTypeInvestmentRecommendation switchTypeInvestment)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY, switchTypeInvestment.Pid));

                DataBase.DBService.ExecuteCommand(string.Format(DELETE_QUERY,
                  switchTypeInvestment.Pid, switchTypeInvestment.ToSchemeId, switchTypeInvestment.Amount));

                Activity.ActivitiesService.Add(ActivityType.DeleteInvestmentRecommendation, EntryStatus.Success,
                         Source.Server, switchTypeInvestment.UpdatedByUserName, switchTypeInvestment.ToSchemeName,
                         switchTypeInvestment.MachineName);
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

        private SwitchTypeInvestmentRecommendation convertToSTPInvestmentRecomendationObject(DataRow dr)
        {
            SwitchTypeInvestmentRecommendation switchTypeInvestmentRecommendation = new SwitchTypeInvestmentRecommendation();
            switchTypeInvestmentRecommendation.Pid = dr.Field<int>("PId");
            switchTypeInvestmentRecommendation.FromSchemeId = dr.Field<int>("FromSchemeId");
            switchTypeInvestmentRecommendation.FromSchemeName = dr.Field<string>("FromScheme");
            switchTypeInvestmentRecommendation.ToSchemeId = dr.Field<int>("ToSchemeId");
            switchTypeInvestmentRecommendation.ToSchemeName = getSchemeName(switchTypeInvestmentRecommendation.ToSchemeId);
            switchTypeInvestmentRecommendation.Amount = double.Parse(dr["Amount"].ToString());
            return switchTypeInvestmentRecommendation;
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
