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
    public class ScoreEntryService
    {
        private const string SELECT_ALL = "SELECT  * FROM ScoreEntry order by EntryDate Desc";
        private const string SELECT_BY_DATE = "SELECT  * FROM ScoreEntry where EntryDate = '{0}' order by EntryDate Desc";
        private const string INSERT_QUERY = "INSERT INTO ScoreEntry VALUES ('{0}','{1}',{2})";
        private const string UPDATE_QUERY = "UPDATE ScoreEntry SET Title ='{0}',Value = {1} WHERE EntryDate = '{2}' AND TITLE = '{0}'";

        private const string DELETE_BY_ID = "DELETE FROM ScoreEntry WHERE EntryDate ='{0}'";
        private readonly string GET_CLIENT_NAME_QUERY = "SELECT NAME FROM CLIENT WHERE ID = {0}";

        public IList<ScoreEntry> Get()
        {
            try
            {
                Logger.LogInfo("Get: ScoreEntry process start");
                IList<ScoreEntry> lstScoreEntry = new List<ScoreEntry>();

                DataTable dtAppConfig = DataBase.DBService.ExecuteCommand(string.Format(SELECT_ALL));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    ScoreEntry ScoreEntry = convertToScoreEntryObject(dr);
                    lstScoreEntry.Add(ScoreEntry);
                }
                Logger.LogInfo("Get: ScoreEntry process completed.");
                return lstScoreEntry;
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

        public IList<ScoreEntry> Get(DateTime entryDate)
        {
            try
            {
                Logger.LogInfo("Get: ScoreEntry process start");
                IList<ScoreEntry> lstScoreEntry = new List<ScoreEntry>();

                DataTable dtAppConfig = DataBase.DBService.ExecuteCommand(string.Format(SELECT_BY_DATE,entryDate));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    ScoreEntry ScoreEntry = convertToScoreEntryObject(dr);
                    lstScoreEntry.Add(ScoreEntry);
                }
                Logger.LogInfo("Get: ScoreEntry process completed.");
                return lstScoreEntry;
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

        public void Add(List<ScoreEntry> scores)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY, 0));

                DataBase.DBService.BeginTransaction();
                foreach (ScoreEntry score in scores)
                {
                    DataBase.DBService.ExecuteCommandString(string.Format(INSERT_QUERY,
                            score.EntryDate,
                            score.Title,
                            score.Value),true);
                }
                DataBase.DBService.CommitTransaction();

                //Activity.ActivitiesService.Add(ActivityType.CreateScoreEntry, EntryStatus.Success,
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

        public void Update(List<ScoreEntry> ScoreEntries)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY, 0));


                DataBase.DBService.BeginTransaction();
                foreach (ScoreEntry score in ScoreEntries)
                {
                    DataBase.DBService.ExecuteCommandString(string.Format(UPDATE_QUERY,
                            score.Title,
                            score.Value,
                            score.EntryDate), true);
                }
                DataBase.DBService.CommitTransaction();

                //Activity.ActivitiesService.Add(ActivityType.UpdateScoreEntry, EntryStatus.Success,
                //         Source.Server, ScoreEntry.UpdatedByUserName, ScoreEntry.Name, ScoreEntry.MachineName);
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

        public void Delete(ScoreEntry ScoreEntry)
        {
            try
            {
                DataBase.DBService.ExecuteCommand(string.Format(DELETE_BY_ID, ScoreEntry.EntryDate));
                //Activity.ActivitiesService.Add(ActivityType.DeleteScoreEntry, EntryStatus.Success,
                //         Source.Server, ScoreEntry.UpdatedByUserName, ScoreEntry.Name, ScoreEntry.MachineName);
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

        private ScoreEntry convertToScoreEntryObject(DataRow dr)
        {
            ScoreEntry score = new ScoreEntry();
            score.EntryDate = dr.Field<DateTime>("EntryDate");
            score.Title = dr.Field<string>("Title");
            score.Value  = float.Parse(dr["Value"].ToString());
            return score;
        }
    }
}
