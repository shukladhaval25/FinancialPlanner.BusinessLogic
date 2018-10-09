using FinancialPlanner.Common;
using FinancialPlanner.Common.Model;
using FinancialPlanner.Common.Model.PlanOptions;
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
    public class CashFlowService
    {
        private readonly string SELECT_ID = "SELECT N1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM CASHFLOW N1, USERS U WHERE N1.UPDATEDBY = U.ID AND N1.OID = {0}";

        const string INSERT_CASHFLOW= "INSERT INTO CASHFLOW VALUES (" +
            "{0},{1},'{2}',{3},'{4}',{5})";

        const string UPDATE_CASHFLOW = "UPDATE CASHFLOW SET " +
            "[INCOMETAX] = '{0}', [UpdatedOn] = '{1}', [UpdatedBy] ={2} " +
            "WHERE OID = {3}";

        const string DELETE_CASHFLOW = "DELETE FROM CASHFLOW WHERE OID = {0}";

        public CashFlow Get(int id)
        {
            try
            {
                Logger.LogInfo("Get: Cash flow by id process start");
                CashFlow cashFlow = new CashFlow();

                DataTable dtAppConfig =  DataBase.DBService.ExecuteCommand(string.Format(SELECT_ID,id));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    cashFlow = convertToCashFlow(dr);
                }
                Logger.LogInfo("Get: Cash flow by id process completed");
                return cashFlow;
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

        public void Add(CashFlow cashFlow)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(SELECT_ID,cashFlow.Id));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(INSERT_CASHFLOW,
                      cashFlow.Oid, cashFlow.IncomeTax,
                      cashFlow.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss"), cashFlow.CreatedBy,
                      cashFlow.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), cashFlow.UpdatedBy), true);

                Activity.ActivitiesService.Add(ActivityType.CreateCashFlow, EntryStatus.Success,
                         Source.Server, cashFlow.UpdatedByUserName, "CashFlow", cashFlow.MachineName);
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

        public void Update(CashFlow cashFlow)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(SELECT_ID,cashFlow.Id));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(UPDATE_CASHFLOW,
                      cashFlow.IncomeTax,
                      cashFlow.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), cashFlow.UpdatedBy,
                      cashFlow.Oid), true);

                Activity.ActivitiesService.Add(ActivityType.UpdateCashFlow, EntryStatus.Success,
                         Source.Server, cashFlow.UpdatedByUserName, "CashFlow", cashFlow.MachineName);
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

        public void Delete(CashFlow cashFlow)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(SELECT_ID,cashFlow.Id));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(DELETE_CASHFLOW,                      
                      cashFlow.Oid), true);

                Activity.ActivitiesService.Add(ActivityType.DeleteCashFlow, EntryStatus.Success,
                         Source.Server, cashFlow.UpdatedByUserName, "CashFlow", cashFlow.MachineName);
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
        private CashFlow convertToCashFlow(DataRow dr)
        {
            CashFlow cashFlow = new CashFlow();
            cashFlow.Id = dr.Field<int>("ID");
            cashFlow.Oid = dr.Field<int>("OID");
            cashFlow.IncomeTax = float.Parse(dr["IncomeTax"].ToString());
            cashFlow.UpdatedBy = dr.Field<int>("UpdatedBy");
            cashFlow.UpdatedOn = dr.Field<DateTime>("UpdatedOn");
            cashFlow.UpdatedByUserName = dr.Field<string>("UpdatedByUserName");
            return cashFlow;
        }
    }
}
