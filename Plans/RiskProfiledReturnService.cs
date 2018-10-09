using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FinancialPlanner.Common.Model;
using FinancialPlanner.Common;
using System.Data;
using System.Diagnostics;
using System.Reflection;

namespace FinancialPlanner.BusinessLogic
{
    public class RiskProfiledReturnService
    {
        const string DUMMY_SELECT = "SELECT N1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM RISKPROFILEDMASTER N1, USERS U WHERE N1.UPDATEDBY = U.ID AND  N1.ID = 0";
        const string SELECT_ALL = "SELECT N1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM RISKPROFILEDMASTER N1, USERS U WHERE N1.UPDATEDBY = U.ID";
        const string SELECT_ALL_DETAILS = "SELECT [YEARREMAINING],R1.[FOREINGINVESTMENTRATIO],R1.[EQUITYINVESTMENTRATIO] " +
                ",R1.[DEBTINVESTMENTRATION],R1.[FOREINGINVESTEMENTREATURN] " +
                ",R1.[EQUITYINVESTEMENTREATURN],R1.[DEBTINVESTEMENTREATURN],[AVERAGEINVESTEMENTRETURN]," +
                " R1.RISKPROFILEID " +
                " FROM [RISKPROFILEDRETURNDETAILS]R1, RISKPROFILEDMASTER R2 WHERE " +
                " R2.ID = R1.RISKPROFILEID AND R1.RISKPROFILEID = {0}";
        const string SELECT_ID = "SELECT ID FROM RISKPROFILEDMASTER WHERE NAME ='{0}' AND DESCRIPTION ='{1}'";
        const string INSERT_QUERY_MASTER = "INSERT INTO RISKPROFILEDMASTER VALUES (" +
            "'{0}',{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},'{12}','{13}',{14},'{15}',{16})";
        const string INSERT_QUERY_DETAIL = "INSERT INTO RISKPROFILEDRETURNDETAILS VALUES ({0},{1},{2},{3},{4},{5},{6},{7},{8})";

        const string UPDATE_MASTER = "UPDATE RISKPROFILEDMASTER SET NAME = '{0}'," +
            "THRESHOLDYEAR ={1},MAXYEAR = {2},"+
            "PreThreshHoldYearForeingInvestementRatio = {3}, PreThreshHoldYearEqutyInvestementRatio = {4}, PreThreshHoldYearDebtInvestmentRatio = {5}," +
            "PostThreshHoldYearForeingInvestementRatio = {6}, PostThreshHoldYearEqutyInvestementRatio = {7}, PostThreshHoldYearDebtInvestmentRatio = {8}," +
            "ForeingInvestementReaturn ={9},EquityInvestementReturn ={10},DebtInvestementReturn ={11}," +
            "DESCRIPTION = '{12}', UPDATEDON = '{13}',UPDATEDBY = {14} WHERE ID ={15}";
        const string UPDATE_DETAIL = "UPDATE RISKPROFILEDRETURNDETAILS SET YearRemaining = {0} ," +
            "FOREINGINVESTMENTRATIO = {1}, EQUITYINVESTMENTRATIO = {2}, DEBTINVESTMENTRATION = {3} ," +
            "FOREINGINVESTEMENTREATURN = {4},EQUITYINVESTEMENTREATURN ={5},DEBTINVESTEMENTREATURN ={6}," +
            "AVERAGEINVESTEMENTRETURN = {7} WHERE RISKPROFILEID ={8} AND YearRemaining = {0}";

        const string DELETE_MASTER = "DELETE FROM RISKPROFILEDMASTER WHERE ID ={0}";
        const string DELETE_DETAIL = "DELETE FROM RISKPROFILEDRETURNDETAILS WHERE RISKPROFILEID = {0}";

        public IList<RiskProfiledReturnMaster> GetAll()
        {
            try
            {
                Logger.LogInfo("Get: RiskProfiledReturn process start");
                IList<RiskProfiledReturnMaster> lstRiskProfile = new List<RiskProfiledReturnMaster>();

                DataTable dtAppConfig =  DataBase.DBService.ExecuteCommand(SELECT_ALL);
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    RiskProfiledReturnMaster riskPrfile = convertToRiskProfileObject(dr);
                    lstRiskProfile.Add(riskPrfile);
                }
                Logger.LogInfo("Get: RiskProfiledReturn process completed.");
                return lstRiskProfile;
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

        public IList<RiskProfiledReturn> GetAllDetails(int id)
        {
            try
            {
                Logger.LogInfo("Get: RiskProfiledReturn process start");
                IList<RiskProfiledReturn> lstRiskProfile = new List<RiskProfiledReturn>();

                DataTable dtAppConfig =  DataBase.DBService.ExecuteCommand(string.Format(SELECT_ALL_DETAILS,id));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    RiskProfiledReturn riskPrfile = convertToRiskProfileDetailsObject(dr);
                    lstRiskProfile.Add(riskPrfile);
                }
                Logger.LogInfo("Get: RiskProfiledReturn process completed.");
                return lstRiskProfile;
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

        public void Add(RiskProfiledReturnMaster riskProfileReturnMaster)
        {
            try
            {
                string value =  DataBase.DBService.ExecuteCommandScalar(DUMMY_SELECT);
                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(INSERT_QUERY_MASTER,
                    riskProfileReturnMaster.Name,
                    riskProfileReturnMaster.ThresholdYear, 
                    riskProfileReturnMaster.MaxYear,
                    riskProfileReturnMaster.PreForeingInvestmentRatio,
                    riskProfileReturnMaster.PreEquityInvestmentRatio,
                    riskProfileReturnMaster.PreDebtInvestmentRatio,
                    riskProfileReturnMaster.PostForeingInvestmentRatio,
                    riskProfileReturnMaster.PostEquityInvestmentRatio,
                    riskProfileReturnMaster.PostDebtInvestmentRatio,
                    riskProfileReturnMaster.ForeingInvestmentReturn,
                    riskProfileReturnMaster.EquityInvestmentReturn,
                    riskProfileReturnMaster.DebtInvestmentReturn,
                    riskProfileReturnMaster.Description,
                    riskProfileReturnMaster.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss"), riskProfileReturnMaster.CreatedBy,
                    riskProfileReturnMaster.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), riskProfileReturnMaster.UpdatedBy), true);

                int riskprofileId = getRiskProfileId(riskProfileReturnMaster.Name, riskProfileReturnMaster.Description);

                if (riskProfileReturnMaster.RiskProfileReturn.Count > 0)
                {
                    foreach (RiskProfiledReturn rpr in riskProfileReturnMaster.RiskProfileReturn)
                    {
                        DataBase.DBService.ExecuteCommandString(string.Format(INSERT_QUERY_DETAIL,
                                riskprofileId, rpr.YearRemaining,
                                rpr.ForeingInvestmentRatio,
                                rpr.EquityInvestementRatio, rpr.DebtInvestementRatio,
                                rpr.ForeingInvestementReaturn, rpr.EquityInvestementReturn,
                                rpr.DebtInvestementReturn, rpr.AverageInvestemetReturn));
                    }
                }

                Activity.ActivitiesService.Add(ActivityType.CreateRiskProfiled, EntryStatus.Success,
                         Source.Server, riskProfileReturnMaster.UpdatedByUserName, riskProfileReturnMaster.Name, riskProfileReturnMaster.MachineName);
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

        private int getRiskProfileId(string name, string description)
        {
            string id =  DataBase.DBService.ExecuteCommandScalar(string.Format(SELECT_ID,
                  name,description));
            return int.Parse(id);
        }

        public void Update(RiskProfiledReturnMaster riskProfileReturnMaster)
        {
            try
            {
                string value =  DataBase.DBService.ExecuteCommandScalar(DUMMY_SELECT);
                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(UPDATE_MASTER,
                   riskProfileReturnMaster.Name,
                   riskProfileReturnMaster.ThresholdYear,
                   riskProfileReturnMaster.MaxYear,
                   riskProfileReturnMaster.PreForeingInvestmentRatio,
                   riskProfileReturnMaster.PreEquityInvestmentRatio,
                   riskProfileReturnMaster.PreDebtInvestmentRatio,
                   riskProfileReturnMaster.PostForeingInvestmentRatio,
                   riskProfileReturnMaster.PostEquityInvestmentRatio,
                   riskProfileReturnMaster.PostDebtInvestmentRatio,
                   riskProfileReturnMaster.ForeingInvestmentReturn,
                   riskProfileReturnMaster.EquityInvestmentReturn,
                   riskProfileReturnMaster.DebtInvestmentReturn,
                   riskProfileReturnMaster.Description,
                   riskProfileReturnMaster.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"),
                   riskProfileReturnMaster.UpdatedBy, riskProfileReturnMaster.Id), true);

                if (riskProfileReturnMaster.RiskProfileReturn.Count > 0)
                {
                    foreach (RiskProfiledReturn rpr in riskProfileReturnMaster.RiskProfileReturn)
                    {
                        DataBase.DBService.ExecuteCommandString(string.Format(UPDATE_DETAIL,
                            rpr.YearRemaining, rpr.ForeingInvestmentRatio,
                            rpr.EquityInvestementRatio, rpr.DebtInvestementRatio,
                            rpr.ForeingInvestementReaturn, rpr.EquityInvestementReturn,
                            rpr.DebtInvestementReturn, rpr.AverageInvestemetReturn,
                            rpr.RiskProfileId), true);
                    }
                }
                Activity.ActivitiesService.Add(ActivityType.UpdateRiskProfiled, EntryStatus.Success,
                         Source.Server, riskProfileReturnMaster.UpdatedByUserName, riskProfileReturnMaster.Name, riskProfileReturnMaster.MachineName);
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

        public void Delete(RiskProfiledReturnMaster riskProfileReturnMaster)
        {
            try
            {
                DataBase.DBService.BeginTransaction();


                if (riskProfileReturnMaster.RiskProfileReturn != null)
                {
                    DataBase.DBService.ExecuteCommandString(string.Format(DELETE_DETAIL, riskProfileReturnMaster.Id), true);
                }

                DataBase.DBService.ExecuteCommandString(string.Format(DELETE_MASTER,
                  riskProfileReturnMaster.Id), true);

                Activity.ActivitiesService.Add(ActivityType.DeleteRiskProfiled, EntryStatus.Success,
                         Source.Server, riskProfileReturnMaster.UpdatedByUserName, riskProfileReturnMaster.Name, riskProfileReturnMaster.MachineName);
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

        private RiskProfiledReturn convertToRiskProfileDetailsObject(DataRow dr)
        {
            RiskProfiledReturn riskProfile = new RiskProfiledReturn();
            //riskProfile.Id = dr.Field<int>("ID");
            riskProfile.RiskProfileId = dr.Field<int>("RiskProfileID");
            riskProfile.YearRemaining = dr.Field<int>("YearRemaining");
            riskProfile.ForeingInvestmentRatio = dr.Field<decimal>("ForeingInvestmentRatio");
            riskProfile.EquityInvestementRatio = dr.Field<decimal>("EquityInvestmentRatio");
            riskProfile.DebtInvestementRatio = dr.Field<decimal>("DebtInvestmentRation");

            riskProfile.ForeingInvestementReaturn = dr.Field<decimal>("ForeingInvestementReaturn");
            riskProfile.EquityInvestementReturn = dr.Field<decimal>("EquityInvestementReaturn");
            riskProfile.DebtInvestementReturn = dr.Field<decimal>("DebtInvestementReaturn");
            return riskProfile;
        }

        private void LogDebug(string methodName, Exception ex)
        {
            DebuggerLogInfo debuggerInfo = new DebuggerLogInfo();
            debuggerInfo.ClassName = this.GetType().Name;
            debuggerInfo.Method = methodName;
            debuggerInfo.ExceptionInfo = ex;
            Logger.LogDebug(debuggerInfo);
        }

        private RiskProfiledReturnMaster convertToRiskProfileObject(DataRow dr)
        {
            RiskProfiledReturnMaster riskPrfile = new RiskProfiledReturnMaster();
            riskPrfile.Id = dr.Field<int>("ID");
            riskPrfile.Name = dr.Field<string>("Name");
            riskPrfile.ThresholdYear = dr.Field<int>("ThresholdYear");
            riskPrfile.MaxYear = dr.Field<int>("MaxYear");
            riskPrfile.PreForeingInvestmentRatio = float.Parse(dr["PreThreshHoldYearForeingInvestementRatio"].ToString());
            riskPrfile.PreEquityInvestmentRatio = float.Parse(dr["PreThreshHoldYearEqutyInvestementRatio"].ToString());
            riskPrfile.PreDebtInvestmentRatio = float.Parse(dr["PreThreshHoldYearDebtInvestmentRatio"].ToString());
            riskPrfile.PostForeingInvestmentRatio = float.Parse(dr["PostThreshHoldYearForeingInvestementRatio"].ToString());
            riskPrfile.PostEquityInvestmentRatio = float.Parse(dr["PostThreshHoldYearEqutyInvestementRatio"].ToString());
            riskPrfile.PostDebtInvestmentRatio = float.Parse(dr["PostThreshHoldYearDebtInvestmentRatio"].ToString());
            riskPrfile.ForeingInvestmentReturn = float.Parse(dr["ForeingInvestementReaturn"].ToString());
            riskPrfile.EquityInvestmentReturn = float.Parse(dr["EquityInvestementReturn"].ToString());
            riskPrfile.DebtInvestmentReturn = float.Parse(dr["DebtInvestementReturn"].ToString());
            riskPrfile.Description = dr.Field<string>("Description");
            return riskPrfile;
        }
    }
}
