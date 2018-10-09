using FinancialPlanner.Common;
using FinancialPlanner.Common.Model;
using FinancialPlanner.Common.Model.RiskProfile;
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
    public class RecommendedSchemesService
    {
        const string SELECT_ALL = "SELECT N1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM RECOMMENDEDSCHEMES N1, USERS U WHERE N1.UPDATEDBY = U.ID AND N1.INVESTMENTSEGMENTID = {0}";
        const string INSERT_QUERY = "INSERT INTO RECOMMENDEDSCHEMES VALUES ({0},'{1}','{2}',{3},'{4}',{5})";
        const string UPDATE_QUERY = "UPDATE RECOMMENDEDSCHEMES SET SCHEMENAME ='{0}', UPDATEDON ='{1}', UPDATEDBY = {2} WHERE ID = {3}";
        const string DELETE_QUERY = "DELETE RECOMMENDEDSCHEMES WHERE ID = {0}";
        private readonly string GET_RISK_PROFILE_NAME_QUERY = "SELECT [NAME] FROM [RISKPROFILEDMASTER] WHERE ID = {0}";

        public IList<RecommendedSchemes> GetAll(int investmentsegmentid)
        {
            try
            {
                Logger.LogInfo("Get: Recommended scheme process start");
                IList<RecommendedSchemes> RecommededSchemess = new List<RecommendedSchemes>();

                DataTable dtAppConfig =  DataBase.DBService.ExecuteCommand(string.Format(SELECT_ALL,investmentsegmentid));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    RecommendedSchemes riskPrfile = convertToRecommededSchemesObject(dr);
                    RecommededSchemess.Add(riskPrfile);
                }
                Logger.LogInfo("Get: Recommended scheme process completed.");
                return RecommededSchemess;
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

        public void Add(RecommendedSchemes recommededSchemes)
        {
            try
            {
                string riskProfileName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_RISK_PROFILE_NAME_QUERY,recommededSchemes.InvestmentSegmentID));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(INSERT_QUERY,
                      recommededSchemes.InvestmentSegmentID, recommededSchemes.SchemeName,
                      recommededSchemes.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss"), recommededSchemes.CreatedBy,
                      recommededSchemes.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), recommededSchemes.UpdatedBy), true);

                Activity.ActivitiesService.Add(ActivityType.CreatedSchemes, EntryStatus.Success,
                         Source.Server, recommededSchemes.UpdatedByUserName, riskProfileName, recommededSchemes.MachineName);
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

        public void Update(RecommendedSchemes recommededSchemes)
        {
            try
            {
                string riskProfileName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_RISK_PROFILE_NAME_QUERY,recommededSchemes.InvestmentSegmentID));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(UPDATE_QUERY,
                      recommededSchemes.SchemeName,                     
                      recommededSchemes.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"),
                      recommededSchemes.UpdatedBy,
                      recommededSchemes.Id), true);

                Activity.ActivitiesService.Add(ActivityType.UpdateSchemes, EntryStatus.Success,
                         Source.Server, recommededSchemes.UpdatedByUserName, riskProfileName, recommededSchemes.MachineName);
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

        public void Delete(RecommendedSchemes RecommededSchemes)
        {
            try
            {
                string riskProfileName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_RISK_PROFILE_NAME_QUERY,RecommededSchemes.InvestmentSegmentID));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(DELETE_QUERY,
                      RecommededSchemes.Id), true);

                Activity.ActivitiesService.Add(ActivityType.DeleteSchemes, EntryStatus.Success,
                         Source.Server, RecommededSchemes.UpdatedByUserName, riskProfileName, RecommededSchemes.MachineName);
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

        private void LogDebug(string methodName, Exception ex)
        {
            DebuggerLogInfo debuggerInfo = new DebuggerLogInfo();
            debuggerInfo.ClassName = this.GetType().Name;
            debuggerInfo.Method = methodName;
            debuggerInfo.ExceptionInfo = ex;
            Logger.LogDebug(debuggerInfo);
        }


        private RecommendedSchemes convertToRecommededSchemesObject(DataRow dr)
        {
            RecommendedSchemes invSeg = new RecommendedSchemes();
            invSeg.Id = dr.Field<int>("ID");
            invSeg.InvestmentSegmentID = dr.Field<int>("InvestmentSegmentID");
            invSeg.SchemeName = dr.Field<string>("SchemeName");
            return invSeg;
        }
    }
}
