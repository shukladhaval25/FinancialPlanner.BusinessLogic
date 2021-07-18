using FinancialPlanner.Common;
using FinancialPlanner.Common.Model.ScoreCalculation;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FinancialPlanner.BusinessLogic.ScoreRangeCalculation
{
    public class ScoreRangeService
    {
        private const string GET_CLIENT_NAME_QUERY = "SELECT NAME FROM CLIENT WHERE ID = {0}";
        private const string SELECT_ALL = "SELECT  * FROM ScoreRange where RiskProfileId = {0}";
        private const string INSERT_QUERY = "INSERT INTO ScoreRange VALUES ({0},{1},{2},{3},{4},{5})";
        private const string UPDATE_QUERY = "UPDATE ScoreRange SET FromRange = {0},ToRange ={1},Equity ={2},Debt ={3},Gold = {4}  WHERE ID = {5} And RiskProfileId = {6}";

        private const string DELETE_BY_ID = "DELETE FROM ScoreRange WHERE Id ={0}";

        public IList<ScoreRange> Get(int riskProfileId)
        {
            try
            {
                Logger.LogInfo("Get: ScoreRange process start");
                IList<ScoreRange> lstScoreRange = new List<ScoreRange>();

                DataTable dtAppConfig = DataBase.DBService.ExecuteCommand(string.Format(SELECT_ALL, riskProfileId));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    ScoreRange ScoreRange = convertToScoreRangeObject(dr);
                    lstScoreRange.Add(ScoreRange);
                }
                Logger.LogInfo("Get: ScoreRange process completed.");
                return lstScoreRange;
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

        public void Add(List<ScoreRange> scores)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY, 0));

                DataBase.DBService.BeginTransaction();
                foreach (ScoreRange score in scores)
                {
                    if (score.Id == 0)
                    {
                        DataBase.DBService.ExecuteCommandString(string.Format(INSERT_QUERY,
                                score.RiskProfileId,
                                score.FromRange,
                                score.ToRange,
                                score.Equity,
                                score.Debt,
                                score.Gold), true);
                    }
                    else
                    {
                        DataBase.DBService.ExecuteCommandString(string.Format(UPDATE_QUERY,
                                score.FromRange,
                                score.ToRange,
                                score.Equity,
                                score.Debt,
                                score.Gold,
                                score.Id,
                                score.RiskProfileId), true);
                    }
                }

                DataBase.DBService.CommitTransaction();

                //Activity.ActivitiesService.Add(ActivityType.CreateScoreRange, EntryStatus.Success,
                //         Source.Server, score.UpdatedByUserName, score.Name, score.MachineName);
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

        public void Update(ScoreRange score)
        {
            try
            {
                //string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY, 0));

                DataBase.DBService.ExecuteCommandString(string.Format(UPDATE_QUERY,
                              score.FromRange,
                              score.ToRange,
                              score.Equity,
                              score.Debt,
                              score.Gold,
                              score.Id,
                              score.RiskProfileId));

                //Activity.ActivitiesService.Add(ActivityType.UpdateScoreRange, EntryStatus.Success,
                //         Source.Server, ScoreRange.UpdatedByUserName, ScoreRange.Name, ScoreRange.MachineName);
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

        public void Delete(ScoreRange ScoreRange)
        {
            try
            {
                DataBase.DBService.ExecuteCommand(string.Format(DELETE_BY_ID, ScoreRange.Id));
                //Activity.ActivitiesService.Add(ActivityType.DeleteScoreRange, EntryStatus.Success,
                //         Source.Server, ScoreRange.UpdatedByUserName, ScoreRange.Name, ScoreRange.MachineName);
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

        private void LogDebug(string methodName, Exception ex)
        {
            DebuggerLogInfo debuggerInfo = new DebuggerLogInfo();
            debuggerInfo.ClassName = this.GetType().Name;
            debuggerInfo.Method = methodName;
            debuggerInfo.ExceptionInfo = ex;
            Logger.LogDebug(debuggerInfo);
        }

        private ScoreRange convertToScoreRangeObject(DataRow dr)
        {
            ScoreRange score = new ScoreRange();
            score.Id = dr.Field<int>("ID");
            score.RiskProfileId = dr.Field<int>("RiskProfileId");
            score.FromRange = float.Parse(dr["FromRange"].ToString());
            score.ToRange = float.Parse(dr["ToRange"].ToString());
            score.Equity = float.Parse(dr["Equity"].ToString());
            score.Debt = float.Parse(dr["Debt"].ToString());
            score.Gold = float.Parse(dr["Gold"].ToString());
            return score;
        }
    }
}
