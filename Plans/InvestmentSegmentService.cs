using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FinancialPlanner.Common.Model.RiskProfile;
using FinancialPlanner.Common;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using FinancialPlanner.Common.Model;

namespace FinancialPlanner.BusinessLogic.Plans
{
    public class InvestmentSegmentService
    {
        const string SELECT_ALL = "SELECT N1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM INVESTMENTSEGMENT N1, USERS U WHERE N1.UPDATEDBY = U.ID AND N1.RISKPROFIILEID = {0}";
        const string INSERT_QUERY = "INSERT INTO INVESTMENTSEGMENT VALUES ({0},'{1}','{2}',{3},'{4}',{5},'{6}',{7})";
        const string UPDATE_QUERY = "UPDATE INVESTMENTSEGMENT SET INVESTMENTTYPE ='{0}', SEGMENTNAME ='{1}', " +
            "SEGMENTRATIO = {2}, UPDATEDON ='{3}', UPDATEDBY = {4} WHERE ID = {5}";
        const string DELETE_QUERY = "DELETE INVESTMENTSEGMENT WHERE ID = {0}";
        private readonly string GET_RISK_PROFILE_NAME_QUERY = "SELECT [NAME] FROM [RISKPROFILEDMASTER] WHERE ID = {0}";

        public IList<InvestmentSegment> GetAll(int riskProfileId)
        {
            try
            {
                Logger.LogInfo("Get: Investment segment process start");
                IList<InvestmentSegment> investmentSegments = new List<InvestmentSegment>();

                DataTable dtAppConfig =  DataBase.DBService.ExecuteCommand(string.Format(SELECT_ALL,riskProfileId));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    InvestmentSegment riskPrfile = convertToInvestmentSegmentObject(dr);
                    investmentSegments.Add(riskPrfile);
                }
                Logger.LogInfo("Get: Investment segment process completed.");
                return investmentSegments;
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

        public void Add(InvestmentSegment investmentSegment)
        {
            try
            {
                string riskProfileName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_RISK_PROFILE_NAME_QUERY,investmentSegment.RiskProfileId));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(INSERT_QUERY,
                      investmentSegment.RiskProfileId,investmentSegment.InvestmentType,
                      investmentSegment.SegmentName,investmentSegment.SegmentRatio,
                      investmentSegment.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss"), investmentSegment.CreatedBy,
                      investmentSegment.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), investmentSegment.UpdatedBy), true);

                Activity.ActivitiesService.Add(ActivityType.CreateInvestmentSegement, EntryStatus.Success,
                         Source.Server, investmentSegment.UpdatedByUserName, investmentSegment.SegmentName, investmentSegment.MachineName);
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

        public void Update(InvestmentSegment investmentSegment)
        {
            try
            {
                string riskProfileName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_RISK_PROFILE_NAME_QUERY,investmentSegment.RiskProfileId));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(UPDATE_QUERY,
                      investmentSegment.InvestmentType,
                      investmentSegment.SegmentName, investmentSegment.SegmentRatio,                      
                      investmentSegment.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), 
                      investmentSegment.UpdatedBy,
                      investmentSegment.Id), true);

                Activity.ActivitiesService.Add(ActivityType.UpdateInvestmentSegement, EntryStatus.Success,
                         Source.Server, investmentSegment.UpdatedByUserName, investmentSegment.SegmentName, investmentSegment.MachineName);
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

        public void Delete(InvestmentSegment investmentSegment)
        {
            try
            {
                string riskProfileName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_RISK_PROFILE_NAME_QUERY,investmentSegment.RiskProfileId));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(DELETE_QUERY,                     
                      investmentSegment.Id), true);

                Activity.ActivitiesService.Add(ActivityType.DeleteInvestmentSegement, EntryStatus.Success,
                         Source.Server, investmentSegment.UpdatedByUserName, investmentSegment.SegmentName, investmentSegment.MachineName);
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


        private InvestmentSegment convertToInvestmentSegmentObject(DataRow dr)
        {
            InvestmentSegment invSeg = new InvestmentSegment();
            invSeg.Id = dr.Field<int>("ID");
            invSeg.RiskProfileId = dr.Field<int>("RISKPROFIILEID");
            invSeg.InvestmentType = dr.Field<string>("InvestmentType");
            invSeg.SegmentName = dr.Field<string>("SegmentName");
            invSeg.SegmentRatio = float.Parse(dr["SegmentRatio"].ToString());
            return invSeg;
        }
    }
}
