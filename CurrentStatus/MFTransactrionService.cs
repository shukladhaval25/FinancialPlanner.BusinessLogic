using FinancialPlanner.Common;
using FinancialPlanner.Common.Model;
using FinancialPlanner.Common.Model.CurrentStatus;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FinancialPlanner.BusinessLogic.CurrentStatus
{
    public class MFTransactionsService
    {
        private readonly string SELECT_ID = "SELECT N1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM MFTransactions N1, USERS U WHERE N1.UPDATEDBY = U.ID AND N1.ID = {0}";

        private readonly string SELECT_ALL = "SELECT N1.*,U.USERNAME AS UPDATEDBYUSERNAME, FROM MFTransactions N1, USERS U WHERE N1.UPDATEDBY = U.ID AND N1.MFID = {0}";

        const string INSERT_MFTransactions= "INSERT INTO MFTransactions VALUES (" +
            "{0},{1},{2},{3},'{4}','{5}','{6}',{7},'{8}',{9})";

        const string UPDATE_MFTransactions = "UPDATE MFTransactions SET " +
            "[NAV] ={0}, UNITS = {1},CurrentValue = {2}," +
            "[UpdatedOn] = '{3}', [UpdatedBy] ={4},TransactionType = '{5}',"+
            " TransactionDate = '{6}',TotalValue ={7} WHERE ID = {8} ";

        const string DELETE_MFTransactions = "DELETE FROM MFTransactions WHERE ID = {0}";


        public IList<MFTransactions> GetAll(int plannerId)
        {
            try
            {
                Logger.LogInfo("Get: Mutual fund transaction process start");
                IList<MFTransactions> lstMFTransactionsOption = new List<MFTransactions>();

                DataTable dtAppConfig =  DataBase.DBService.ExecuteCommand(string.Format(SELECT_ALL,plannerId));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    MFTransactions mf = convertToMFTransactions(dr);
                    lstMFTransactionsOption.Add(mf);
                }
                Logger.LogInfo("Get: Mutual fund transaction process completed.");
                return lstMFTransactionsOption;
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



        public MFTransactions Get(int id)
        {               
            try
            {
                Logger.LogInfo("Get: Mutual fund by id process start");
                MFTransactions MFTransactions = new MFTransactions();

                DataTable dtAppConfig =  DataBase.DBService.ExecuteCommand(string.Format(SELECT_ID,id));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    MFTransactions = convertToMFTransactions(dr);
                }
                Logger.LogInfo("Get: Mutual fund by id process completed");
                return MFTransactions;
            }
            catch (Exception ex)
            {
                StackTrace st = new StackTrace ();
                StackFrame sf = st.GetFrame (0);
                MethodBase currentMethodName = sf.GetMethod();
                LogDebug(currentMethodName.Name, ex);
                return null;
            }
        }

        public void Add(MFTransactions MFTransactions)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(SELECT_ID,MFTransactions.MFId));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(INSERT_MFTransactions,
                      MFTransactions.MFId, 
                      MFTransactions.Nav, MFTransactions.Units,MFTransactions.CurrentValue,
                      MFTransactions.TransactionType,MFTransactions.TransactionDate,
                      MFTransactions.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss"), MFTransactions.CreatedBy,
                      MFTransactions.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), MFTransactions.UpdatedBy), true);

                Activity.ActivitiesService.Add(ActivityType.CreateMFTransactions, EntryStatus.Success,
                         Source.Server, MFTransactions.UpdatedByUserName, "MFTransactions", MFTransactions.MachineName);
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

        public void Update(MFTransactions MFTransactions)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(SELECT_ID,MFTransactions.Id));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(UPDATE_MFTransactions,                  
                      MFTransactions.Nav,
                      MFTransactions.Units,
                      MFTransactions.CurrentValue,
                      MFTransactions.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"),
                      MFTransactions.UpdatedBy,
                      MFTransactions.TransactionType,MFTransactions.TransactionDate,
                      MFTransactions.CurrentValue,
                      MFTransactions.Id), true);

                Activity.ActivitiesService.Add(ActivityType.UpdateMFTransactions, EntryStatus.Success,
                         Source.Server, MFTransactions.UpdatedByUserName, "MFTransactions", MFTransactions.MachineName);
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

        public void Delete(MFTransactions MFTransactions)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(SELECT_ID,MFTransactions.Id));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(DELETE_MFTransactions,
                      MFTransactions.Id), true);

                Activity.ActivitiesService.Add(ActivityType.DeleteMFTransactions, EntryStatus.Success,
                         Source.Server, MFTransactions.UpdatedByUserName, "MFTransactions", MFTransactions.MachineName);
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
        private MFTransactions convertToMFTransactions(DataRow dr)
        {
            MFTransactions MFTransactions = new MFTransactions();
            MFTransactions.Id = dr.Field<int>("ID");
            MFTransactions.MFId = dr.Field<int>("MFId");
            MFTransactions.TransactionType = dr.Field<string>("TransactionType");
            MFTransactions.TransactionDate = dr.Field<DateTime>("TransactionDate");
            MFTransactions.Nav = float.Parse(dr["NAV"].ToString());            
            MFTransactions.Units = dr.Field<int>("units");
            MFTransactions.UpdatedBy = dr.Field<int>("UpdatedBy");
            MFTransactions.UpdatedOn = dr.Field<DateTime>("UpdatedOn");
            //MFTransactions.UpdatedByUserName = dr.Field<string>("UpdatedByUserName");
            return MFTransactions;
        }
    }
}
