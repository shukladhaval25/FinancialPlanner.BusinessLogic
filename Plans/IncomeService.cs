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
    public class IncomeService
    {
        private const string GET_CLIENT_NAME_QUERY = "SELECT C.NAME FROM CLIENT C, PLANNER P  WHERE P.CLIENTID = C.ID AND P.ID = {0}";
        const string SELECT_ALL = "SELECT N1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM INCOME N1, USERS U WHERE N1.UPDATEDBY = U.ID AND N1.PID = {0}";
        const string SELECT_BYID = "SELECT N1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM INCOME N1, USERS U WHERE N1.UPDATEDBY = U.ID AND N1.ID = {0} AND N1.PID ={1}";
        const string SELECT_INCOMEID = "SELECT ID  FROM INCOME WHERE PID ={0} AND SOURCE ='{1}' AND INCOMEBY ='{2}' AND AMOUNT ={3} AND " +
            "STARTYEAR ='{4}' AND ENDYEAR ='{5}' AND EXPECTGROWTHINPERCENTAGE ={6}";

        const string SELECT_SALARY_DETAIL = "SELECT N1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM SALARYDETAIL N1, USERS U WHERE N1.UPDATEDBY = U.ID AND N1.INCOMEID = {0} AND N1.PID ={1}";
        const string INSERT_SALARY_DETAIL = "INSERT INTO SALARYDETAIL VALUES " +
            "({0},{1},{2},{3},{4},{5},{6},{7},{8},'{9}',{10},{11},'{12}','{13}',{14},'{15}',{16})";

        const string UPDATE_SALARY_DETAIL = "UPDATE SALARYDETAIL SET CTC ={0}, "+
            "EmployeePFContribution ={1}, EmployerPFContribution ={2},Superannuation ={3}, " +
            "OtherDeduction ={4},NetTakeHome ={5},NextIncrementMonthYear ='{6}'," +
            "ExpectedGrowthInPercentage = {7},BonusAmt ={8}, BonusMonthYear = '{9}', "+
            "UPDATEDON = '{10}', UPDATEDBY={11},REIMBURSEMENT ={12} WHERE ID ={13}";
        const string DELET_SALARY_DETAIL_QUERY = "DELETE FROM SALARYDETAIL WHERE INCOMEID = {0}";


        const string INSERT_QUERY = "INSERT INTO INCOME VALUES ({0},'{1}','{2}',{3},{4},'{5}','{6}','{7}','{8}',{9},'{10}',{11},{12})";
        const string UPDATE_QUERY = "UPDATE INCOME SET SOURCE ='{0}',INCOMEBY = '{1}',AMOUNT ={2},EXPECTGROWTHINPERCENTAGE ={3}," +
            "STARTYEAR ='{4}',ENDYEAR ='{5}',DESCRIPTION = '{6}', UPDATEDON = '{7}'," +
            "UPDATEDBY={8},IncomeTax = {9} WHERE ID ={10}";
        const string DELET_QUERY = "DELETE FROM INCOME WHERE ID ={0}";
        public IList<Income> GetAll(int plannerId)
        {
            try
            {
                Logger.LogInfo("Get: Income process start");
                IList<Income> lstIncome = new List<Income>();

                DataTable dtAppConfig =  DataBase.DBService.ExecuteCommand(string.Format(SELECT_ALL,plannerId));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    Income Income = convertToIncomeObject(dr);
                    Income.SalaryDetail = getSalaryDetails(Income.Id, Income.Pid);
                    lstIncome.Add(Income);
                }
                Logger.LogInfo("Get: Income process completed.");
                return lstIncome;
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

        public Income GetById(int id, int plannerId)
        {
            try
            {
                Logger.LogInfo("Get: Income process start");
                Income Income = new Income();

                DataTable dtAppConfig =  DataBase.DBService.ExecuteCommand(string.Format(SELECT_BYID,id,plannerId));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    Income = convertToIncomeObject(dr);
                    Income.SalaryDetail = getSalaryDetails(Income.Id, Income.Pid);
                }
                Logger.LogInfo("Get: Income process completed.");
                return Income;
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

        public void Add(Income income)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY,income.Pid));

                DataBase.DBService.BeginTransaction();
                if (income.Source.Equals("salary", StringComparison.OrdinalIgnoreCase))
                {
                    DataBase.DBService.ExecuteCommandString(string.Format(INSERT_QUERY,
                       income.Pid, income.Source, income.IncomeBy,
                       income.Amount, income.ExpectGrowthInPercentage,
                       income.StartYear, income.EndYear,
                       income.Description,
                       income.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss"), income.CreatedBy,
                       income.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), income.UpdatedBy,
                       income.IncomeTax), true);

                    if (income.SalaryDetail != null)
                    {
                        income.Id = getIncomeId(income);
                        insertSalaryDetails(income);
                    }

                }
                else
                {
                    DataBase.DBService.ExecuteCommandString(string.Format(INSERT_QUERY,
                      income.Pid, income.Source, income.IncomeBy,
                      income.Amount, income.ExpectGrowthInPercentage,
                      income.StartYear, income.EndYear,
                      income.Description,
                      income.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss"), income.CreatedBy,
                      income.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), income.UpdatedBy), true);
                }

                Activity.ActivitiesService.Add(ActivityType.CreateIncome, EntryStatus.Success,
                         Source.Server, income.UpdatedByUserName, clientName, income.MachineName);
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

        private int getIncomeId(Income income)
        {
            string id =  DataBase.DBService.ExecuteCommandScalar(string.Format(SELECT_INCOMEID,
                   income.Pid, income.Source, income.IncomeBy,
                   income.Amount,
                   income.StartYear, income.EndYear,
                   income.ExpectGrowthInPercentage));
            return int.Parse(id);
        }
        private void insertSalaryDetails(Income income)
        {
            DataBase.DBService.ExecuteCommandString(string.Format(INSERT_SALARY_DETAIL,
                     income.Id, income.Pid, income.SalaryDetail.Ctc,
                     income.SalaryDetail.Reimbursement, income.SalaryDetail.EmployeePFContribution,
                     income.SalaryDetail.EmployerPFContribution, income.SalaryDetail.Superannuation,
                     income.SalaryDetail.OtherDeduction, income.SalaryDetail.NetTakeHome,
                     income.SalaryDetail.NextIncrementMonthYear, income.SalaryDetail.ExpectedGrowthInPercentage,
                     income.SalaryDetail.BonusAmt, income.SalaryDetail.BonusMonthYear,
                     income.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss"), income.CreatedBy,
                     income.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), income.UpdatedBy), true);
        }

        public void Update(Income income)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY,income.Pid));
                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(UPDATE_QUERY,
                   income.Source, income.IncomeBy,
                   income.Amount, income.ExpectGrowthInPercentage,
                   income.StartYear, income.EndYear,
                   income.Description,
                   income.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"),
                   income.UpdatedBy, income.IncomeTax, income.Id), true);

                if (income.SalaryDetail != null)
                {
                    if (income.SalaryDetail.Id == 0)
                    {
                        insertSalaryDetails(income);
                    }
                    else
                    {
                        updateSalaryDeatils(income);
                    }
                }

                Activity.ActivitiesService.Add(ActivityType.UpdateIncome, EntryStatus.Success,
                         Source.Server, income.UpdatedByUserName, clientName, income.MachineName);
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

        private void updateSalaryDeatils(Income income)
        {
            DataBase.DBService.ExecuteCommandString(string.Format(UPDATE_SALARY_DETAIL,
                    income.SalaryDetail.Ctc, income.SalaryDetail.EmployeePFContribution,
                    income.SalaryDetail.EmployerPFContribution, income.SalaryDetail.Superannuation,
                    income.SalaryDetail.OtherDeduction, income.SalaryDetail.NetTakeHome,
                    income.SalaryDetail.NextIncrementMonthYear, income.SalaryDetail.ExpectedGrowthInPercentage,
                    income.SalaryDetail.BonusAmt, income.SalaryDetail.BonusMonthYear,
                    income.SalaryDetail.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"),
                    income.SalaryDetail.UpdatedBy, income.SalaryDetail.Reimbursement,
                    income.SalaryDetail.Id), true);
        }

        public void Delete(Income Income)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY,Income.Pid));
                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(DELET_SALARY_DETAIL_QUERY, Income.Id), true);
                DataBase.DBService.ExecuteCommandString(string.Format(DELET_QUERY, Income.Id), true);

                Activity.ActivitiesService.Add(ActivityType.DeleteIncome, EntryStatus.Success,
                         Source.Server, Income.UpdatedByUserName, clientName, Income.MachineName);
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
        private Income convertToIncomeObject(DataRow dr)
        {
            Income income = new Income();
            income.Id = dr.Field<int>("ID");
            income.Pid = dr.Field<int>("PID");
            income.Source = dr.Field<string>("Source");
            income.IncomeBy = dr.Field<string>("IncomeBy");
            income.Amount = double.Parse(dr["Amount"].ToString());
            income.ExpectGrowthInPercentage = dr.Field<decimal>("ExpectGrowthInPercentage");
            income.StartYear = dr.Field<string>("StartYear");
            income.EndYear = dr.Field<string>("EndYear");
            income.Description = dr.Field<string>("Description");
            income.UpdatedOn = dr.Field<DateTime>("UpdatedOn");
            income.UpdatedBy = dr.Field<int>("UpdatedBy");
            income.IncomeTax = float.Parse(dr["IncomeTax"].ToString());
            return income;
        }

        private void LogDebug(string methodName, Exception ex)
        {
            DebuggerLogInfo debuggerInfo = new DebuggerLogInfo();
            debuggerInfo.ClassName = this.GetType().Name;
            debuggerInfo.Method = methodName;
            debuggerInfo.ExceptionInfo = ex;
            Logger.LogDebug(debuggerInfo);
        }

        private SalaryDetail getSalaryDetails(int incomeId, int plannerId)
        {
            try
            {
                SalaryDetail salaryDetail = new SalaryDetail();

                DataTable dtAppConfig =  DataBase.DBService.ExecuteCommand(string.Format(SELECT_SALARY_DETAIL,incomeId,plannerId));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    salaryDetail = convertToSalaryObject(dr);
                }
                return salaryDetail;
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

        private SalaryDetail convertToSalaryObject(DataRow dr)
        {
            SalaryDetail salaryDetail = new SalaryDetail();
            salaryDetail.Id = dr.Field<int>("ID");
            salaryDetail.Pid = dr.Field<int>("PID");
            salaryDetail.IncomeId = dr.Field<int>("IncomeId");
            salaryDetail.Ctc = double.Parse(dr["Ctc"].ToString());
            salaryDetail.Reimbursement = double.Parse(dr["Reimbursement"].ToString());
            salaryDetail.EmployeePFContribution = double.Parse(dr["EmployeePFContribution"].ToString());
            salaryDetail.EmployerPFContribution = double.Parse(dr["EmployerPFContribution"].ToString());
            salaryDetail.Superannuation = double.Parse(dr["Superannuation"].ToString());
            salaryDetail.OtherDeduction = double.Parse(dr["OtherDeduction"].ToString());
            salaryDetail.NetTakeHome = double.Parse(dr["NetTakeHome"].ToString());
            salaryDetail.NextIncrementMonthYear = dr.Field<string>("NextIncrementMonthYear");
            salaryDetail.ExpectedGrowthInPercentage = dr.Field<decimal>("ExpectedGrowthInPercentage");
            salaryDetail.BonusAmt = double.Parse(dr["BonusAmt"].ToString());
            salaryDetail.BonusMonthYear = dr.Field<string>("BonusMonthYear");
            return salaryDetail;
        }
    }
}
