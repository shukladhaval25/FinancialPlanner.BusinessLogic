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

namespace FinancialPlanner.BusinessLogic.ScoreCalculation
{
    public class ScoreService
    {
        private const string GET_CLIENT_NAME_QUERY = "SELECT NAME FROM CLIENT WHERE ID = {0}";
        private const string SELECT_ALL = "SELECT  * FROM Score";
        private const string INSERT_QUERY = "INSERT INTO Score VALUES ('{0}',{1},{2},{3})";
        private const string UPDATE_QUERY = "UPDATE Score SET Title ='{0}',MaxValue = {1},MinValue = {2},Weightage ={3}  WHERE ID = {4}";

        private const string DELETE_BY_ID = "DELETE FROM Score WHERE ID ='{0}'";

        public IList<Score> Get()
        {
            try
            {
                Logger.LogInfo("Get: Score process start");
                IList<Score> lstScore = new List<Score>();

                DataTable dtAppConfig = DataBase.DBService.ExecuteCommand(string.Format(SELECT_ALL));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    Score Score = convertToScoreObject(dr);
                    lstScore.Add(Score);
                }
                Logger.LogInfo("Get: Score process completed.");
                return lstScore;
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

        public void Add(List<Score> scores)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY, 0));

                DataBase.DBService.BeginTransaction();
                foreach (Score score in scores)
                {
                    if (score.Id == 0)
                    {
                        DataBase.DBService.ExecuteCommandString(string.Format(INSERT_QUERY,
                                score.Title,
                                score.MaxValue,
                                score.MinValue,
                                score.Weightage),true);
                    }
                    else
                    {
                        DataBase.DBService.ExecuteCommandString(string.Format(UPDATE_QUERY,
                                score.Title,
                                score.MaxValue,
                                score.MinValue,
                                score.Weightage,
                                score.Id),true);
                    }
                }

                DataBase.DBService.CommitTransaction();

                //Activity.ActivitiesService.Add(ActivityType.CreateScore, EntryStatus.Success,
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

        public void Update(Score Score)
        {
            try
            {
                //string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY, 0));

                DataBase.DBService.ExecuteCommand(string.Format(UPDATE_QUERY,
                   Score.Title,
                   Score.MaxValue,
                   Score.MinValue,
                   Score.Weightage,
                   Score.Id));

                //Activity.ActivitiesService.Add(ActivityType.UpdateScore, EntryStatus.Success,
                //         Source.Server, Score.UpdatedByUserName, Score.Name, Score.MachineName);
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

        public void Delete(Score Score)
        {
            try
            {
                DataBase.DBService.ExecuteCommand(string.Format(DELETE_BY_ID, Score.Id));
                //Activity.ActivitiesService.Add(ActivityType.DeleteScore, EntryStatus.Success,
                //         Source.Server, Score.UpdatedByUserName, Score.Name, Score.MachineName);
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

        private Score convertToScoreObject(DataRow dr)
        {
            Score score = new Score();
            score.Id = dr.Field<int>("ID");
            score.Title = dr.Field<string>("Title");
            score.MaxValue  = float.Parse( dr["MaxValue"].ToString());
            score.MinValue  = float.Parse(dr["MinValue"].ToString());
            score.Weightage  = float.Parse(dr["Weightage"].ToString()); 
            return score;
        }
    }
}
