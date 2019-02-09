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

namespace FinancialPlanner.BusinessLogic
{
    public class ExpensesService
    {
        private const string GET_CLIENT_NAME_QUERY = "SELECT C.NAME FROM CLIENT C, PLANNER P  WHERE P.CLIENTID = C.ID AND P.ID = {0}";
        const string SELECT_ALL = "SELECT N1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM EXPENSES N1, USERS U WHERE N1.UPDATEDBY = U.ID AND N1.PID = {0}";
        const string SELECT_BYID = "SELECT N1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM EXPENSES N1, USERS U WHERE N1.UPDATEDBY = U.ID AND N1.ID = {0} AND N1.PID ={1}";
      
        const string INSERT_QUERY = "INSERT INTO EXPENSES VALUES (" +
            "{0},'{1}','{2}',{3},{4},'{5}',{6},'{7}',{8})";
        const string UPDATE_QUERY = "UPDATE Expenses SET ITEMCATEGORY = '{0}',ITEM ='{1}',OCCURANCETYPE = {2}, " +             
            "AMOUNT ={3},UPDATEDON = '{4}'," +
            "UPDATEDBY={5} WHERE ID ={6}";
        const string DELET_QUERY = "DELETE FROM Expenses WHERE ID ={0}";
        public IList<Expenses> GetAll(int plannerId)
        {
            try
            {
                Logger.LogInfo("Get: Expenses process start");
                IList<Expenses> lstExpenses = new List<Expenses>();

                DataTable dtAppConfig =  DataBase.DBService.ExecuteCommand(string.Format(SELECT_ALL,plannerId));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    Expenses Expenses = convertToExpensesObject(dr);
                    lstExpenses.Add(Expenses);
                }
                Logger.LogInfo("Get: Expenses process completed.");
                return lstExpenses;
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

        public Expenses GetById(int id, int plannerId)
        {
            try
            {
                Logger.LogInfo("Get: Expenses process start");
                Expenses Expenses = new Expenses();

                DataTable dtAppConfig =  DataBase.DBService.ExecuteCommand(string.Format(SELECT_BYID,id,plannerId));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    Expenses = convertToExpensesObject(dr);
                }
                Logger.LogInfo("Get: Expenses process completed.");
                return Expenses;
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

        public void Add(Expenses Expenses)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY,Expenses.Pid));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(INSERT_QUERY,
                      Expenses.Pid, Expenses.ItemCategory, Expenses.Item, 
                      (Expenses.OccuranceType.ToString().Equals("Monthly") ? 0 : 1),
                      Expenses.Amount, 
                      Expenses.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss"), Expenses.CreatedBy,
                      Expenses.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), Expenses.UpdatedBy), true);
              
                Activity.ActivitiesService.Add(ActivityType.CreateExpenses, EntryStatus.Success,
                         Source.Server, Expenses.UpdatedByUserName, clientName, Expenses.MachineName);
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
     
        public void Update(Expenses Expenses)
        {
            try
            {
                string clientName = 
                    DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY,Expenses.Pid));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(UPDATE_QUERY,
                   Expenses.ItemCategory,
                   Expenses.Item, (Expenses.OccuranceType.ToString().Equals("Monthly") ? 0 : 1),
                   Expenses.Amount, 
                   Expenses.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), Expenses.UpdatedBy, Expenses.Id), true);
              
                Activity.ActivitiesService.Add(ActivityType.UpdateExpenses, EntryStatus.Success,
                         Source.Server, Expenses.UpdatedByUserName, clientName, Expenses.MachineName);
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

        public void Delete(Expenses Expenses)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY,Expenses.Pid));
                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(DELET_QUERY, Expenses.Id), true);

                Activity.ActivitiesService.Add(ActivityType.DeleteExpenses, EntryStatus.Success,
                         Source.Server, Expenses.UpdatedByUserName, clientName, Expenses.MachineName);
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
        private Expenses convertToExpensesObject(DataRow dr)
        {
            Expenses Expenses = new Expenses();
            Expenses.Id = dr.Field<int>("ID");
            Expenses.Pid = dr.Field<int>("PID");
            Expenses.ItemCategory = dr.Field<string>("ItemCategory");
            Expenses.Item = dr.Field<string>("Item");
            Expenses.OccuranceType = (ExpenseType) dr.Field<int>("OccuranceType");
            Expenses.Amount = double.Parse(dr["Amount"].ToString());            
            Expenses.UpdatedOn = dr.Field<DateTime>("UpdatedOn");
            Expenses.UpdatedBy = dr.Field<int>("UpdatedBy");
            return Expenses;
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
