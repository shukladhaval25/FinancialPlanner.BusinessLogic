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

namespace FinancialPlanner.BusinessLogic.Plans
{
    public class LoanService
    {
        private const string GET_CLIENT_NAME_QUERY = "SELECT C.NAME FROM CLIENT C, PLANNER P  WHERE P.CLIENTID = C.ID AND P.ID = {0}";
        const string SELECT_ALL = "SELECT N1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM LOAN N1, USERS U WHERE N1.UPDATEDBY = U.ID AND N1.PID = {0}";
        const string SELECT_BYID = "SELECT N1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM LOAN N1, USERS U WHERE N1.UPDATEDBY = U.ID AND N1.ID = {0} AND N1.PID ={1}";

        const string INSERT_QUERY = "INSERT INTO LOAN VALUES ({0},'{1}',{2},{3},{4},{5},{6},'{7}','{8}',{9},'{10}',{11})";
        const string UPDATE_QUERY = "UPDATE LOAN SET TYPEOFLOAN ='{0}',OUTSTANDINGAMT = {1},EMIS ={2},INTERESTRATE ={3}," + 
            "TERMLEFTINMONTHS ={4},NOEMISPAYABLEUNTILYEAR ={5},DESCRIPTION = '{6}', UPDATEDON = '{7}'," +
            "UPDATEDBY={8} WHERE ID ={9}";
        const string DELET_QUERY = "DELETE FROM LOAN WHERE ID ={0}";
        public IList<Loan> GetAll(int plannerId)
        {            
            try
            {
                Logger.LogInfo("Get: Loan process start");
                IList<Loan> lstLoan = new List<Loan>();

                DataTable dtAppConfig =  DataBase.DBService.ExecuteCommand(string.Format(SELECT_ALL,plannerId));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    Loan loan = convertToLoanObject(dr);
                    lstLoan.Add(loan);
                }
                Logger.LogInfo("Get: Loan process completed.");
                return lstLoan;
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

        public Loan GetById(int id,int plannerId)
        {
            try
            {
                Logger.LogInfo("Get: Loan process start");
                Loan loan = new Loan();

                DataTable dtAppConfig =  DataBase.DBService.ExecuteCommand(string.Format(SELECT_BYID,id,plannerId));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    loan = convertToLoanObject(dr);
                }
                Logger.LogInfo("Get: Loan process completed.");
                return loan;
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

        public void Add(Loan loan)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY,loan.Pid));

                DataBase.DBService.ExecuteCommand(string.Format(INSERT_QUERY,
                   loan.Pid, loan.TypeOfLoan, loan.OutstandingAmt,
                   loan.Emis, loan.InterestRate,
                   loan.TermLeftInMonths, loan.NoEmisPayableUntilYear,                   
                   loan.Description,
                   loan.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss"), loan.CreatedBy,
                   loan.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), loan.UpdatedBy));

                Activity.ActivitiesService.Add(ActivityType.CreateLoan, EntryStatus.Success,
                         Source.Server, loan.UpdatedByUserName, clientName, loan.MachineName);
            }
            catch (Exception ex)
            {
                StackTrace st = new StackTrace ();
                StackFrame sf = st.GetFrame (0);
                MethodBase  currentMethodName = sf.GetMethod();
                LogDebug(currentMethodName.Name, ex);
                throw ex;
            }
        }
        public void Update(Loan loan)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY,loan.Pid));

                DataBase.DBService.ExecuteCommand(string.Format(UPDATE_QUERY,
                   loan.TypeOfLoan, loan.OutstandingAmt,
                   loan.Emis, loan.InterestRate,
                   loan.TermLeftInMonths, loan.NoEmisPayableUntilYear,
                   loan.Description,
                   loan.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), loan.UpdatedBy,loan.Id));

                Activity.ActivitiesService.Add(ActivityType.UpdateLoan, EntryStatus.Success,
                         Source.Server, loan.UpdatedByUserName, clientName, loan.MachineName);
            }
            catch (Exception ex)
            {
                StackTrace st = new StackTrace ();
                StackFrame sf = st.GetFrame (0);
                MethodBase  currentMethodName = sf.GetMethod();
                LogDebug(currentMethodName.Name, ex);
                throw ex;
            }
        }
        public void Delete(Loan loan)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY,loan.Pid));

                DataBase.DBService.ExecuteCommand(string.Format(DELET_QUERY,
                  loan.Id));

                Activity.ActivitiesService.Add(ActivityType.DeleteLoan, EntryStatus.Success,
                         Source.Server, loan.UpdatedByUserName, clientName, loan.MachineName);
            }
            catch (Exception ex)
            {
                StackTrace st = new StackTrace ();
                StackFrame sf = st.GetFrame (0);
                MethodBase  currentMethodName = sf.GetMethod();
                LogDebug(currentMethodName.Name, ex);
                throw ex;
            }
        }
        private Loan convertToLoanObject(DataRow dr)
        {
            Loan loan = new Loan();
            loan.Id = dr.Field<int>("ID");
            loan.Pid = dr.Field<int>("PID");
            loan.TypeOfLoan = dr.Field<string>("TypeOfLoan");
            loan.OutstandingAmt = double.Parse(dr["OutstandingAmt"].ToString());
            loan.Emis = dr.Field<int>("Emis");
            loan.InterestRate =  dr.Field<decimal>("InterestRate");
            loan.TermLeftInMonths = dr.Field<int>("TermLeftInMonths");
            loan.NoEmisPayableUntilYear = dr.Field<int>("NoEMISPayableUntilYear");            
            loan.Description = dr.Field<string>("Description");
            return loan;
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
