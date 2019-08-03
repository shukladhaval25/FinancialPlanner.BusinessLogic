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
    public class OthersService
    {
        private readonly string SELECT_ID = "SELECT N1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM Others N1, USERS U WHERE N1.UPDATEDBY = U.ID AND N1.ID = {0}";

        private readonly string SELECT_ALL = "SELECT N1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM Others N1, USERS U WHERE N1.UPDATEDBY = U.ID AND N1.PID = {0}";

        const string INSERT_Others= "INSERT INTO Others VALUES (" +
            "{0},'{1}','{2}','{3}',{4},{5},{6},'{7}',{8},'{9}',{10},'{11}')";

        const string UPDATE_Others = "UPDATE Others SET " +
            "[INVESTERNAME] = '{0}'," +
            "[ACCOUNTNO] = '{1}'," +
            "[PARTICULAR] ='{2}',AMOUNT ={3}, INVESTMENTRETURNRATE = {4}, GoalId ={5}, " +
            "[UpdatedOn] = '{6}', [UpdatedBy] ={7},[TransactionType] = '{8}' " +
            "WHERE ID = {9} ";

        const string DELETE_Others = "DELETE FROM Others WHERE ID = {0}";


        public IList<Others> GetAll(int plannerId)
        {
            try
            {
                Logger.LogInfo("Get: Others process start");
                IList<Others> lstOthersOption = new List<Others>();

                DataTable dtAppConfig =  DataBase.DBService.ExecuteCommand(string.Format(SELECT_ALL,plannerId));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    Others mf = convertToOthers(dr);
                    lstOthersOption.Add(mf);
                }
                Logger.LogInfo("Get: Others fund process completed.");
                return lstOthersOption;
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


        public Others Get(int id)
        {
            try
            {
                Logger.LogInfo("Get: Others by id process start");
                Others Others = new Others();

                DataTable dtAppConfig =  DataBase.DBService.ExecuteCommand(string.Format(SELECT_ID,id));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    Others = convertToOthers(dr);
                }
                Logger.LogInfo("Get: Others by id process completed");
                return Others;
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

        public void Add(Others Others)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(SELECT_ID,Others.Id));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(INSERT_Others,
                      Others.Pid, Others.InvesterName, Others.AccountNo,
                      Others.Particular,
                      Others.Amount,                    
                      Others.GoalId,
                      Others.InvestmentReturnRate,
                      Others.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss"), Others.CreatedBy,
                      Others.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), Others.UpdatedBy,
                      Others.TransactionType), true);

                Activity.ActivitiesService.Add(ActivityType.CreateOthers, EntryStatus.Success,
                         Source.Server, Others.UpdatedByUserName, Others.AccountNo, Others.MachineName);
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

        public void Update(Others Others)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(SELECT_ID,Others.Id));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(UPDATE_Others,
                      Others.InvesterName,
                      Others.AccountNo,
                      Others.Particular,                     
                      Others.Amount,
                      Others.InvestmentReturnRate,
                      (Others.GoalId == null) ? null : Others.GoalId.Value.ToString(),
                      Others.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"),
                      Others.UpdatedBy,
                      Others.TransactionType,
                      Others.Id), true);

                Activity.ActivitiesService.Add(ActivityType.UpdateOthers, EntryStatus.Success,
                         Source.Server, Others.UpdatedByUserName, Others.AccountNo, Others.MachineName);
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

        public void Delete(Others Others)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(SELECT_ID,Others.Id));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(DELETE_Others,
                      Others.Id), true);

                Activity.ActivitiesService.Add(ActivityType.DeleteOthers, EntryStatus.Success,
                         Source.Server, Others.UpdatedByUserName, Others.AccountNo, Others.MachineName);
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
        private Others convertToOthers(DataRow dr)
        {
            Others Others = new Others();
            Others.Id = dr.Field<int>("ID");
            Others.Pid = dr.Field<int>("PID");
            Others.InvesterName = dr.Field<string>("InvesterName");
            Others.AccountNo = dr.Field<string>("AccountNo");
            Others.Particular = dr.Field<string>("Particular");
            Others.Amount = Double.Parse(dr["Amount"].ToString());
            Others.InvestmentReturnRate = float.Parse(dr["INVESTMENTRETURNRATE"].ToString());
            Others.GoalId = dr.Field<int>("GoalId");
            Others.UpdatedBy = dr.Field<int>("UpdatedBy");
            Others.UpdatedOn = dr.Field<DateTime>("UpdatedOn");
            Others.UpdatedByUserName = dr.Field<string>("UpdatedByUserName");
            Others.UpdatedBy = dr.Field<int>("UpdatedBy");
            Others.UpdatedOn = dr.Field<DateTime>("UpdatedOn");
            Others.UpdatedByUserName = dr.Field<string>("UpdatedByUserName");
            Others.TransactionType = dr.Field<string>("TransactionType");
            return Others;
        }
    }
}
