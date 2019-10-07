using FinancialPlanner.Common;
using FinancialPlanner.Common.Model;
using FinancialPlanner.Common.Model.Masters;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FinancialPlanner.BusinessLogic.ApplicationMaster
{
    public class BankService
    {
        private const string GET_CLIENT_NAME_QUERY = "SELECT NAME FROM CLIENT WHERE ID = {0}";
        private const string SELECT_ALL = "SELECT C1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM Bank C1, USERS U WHERE C1.UPDATEDBY = U.ID";
        private const string SELECT_BANK_BY_ID = "SELECT C1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM Bank C1, USERS U WHERE C1.UPDATEDBY = U.ID AND C1.ID = {0}";

        private const string INSERT_QUERY = "INSERT INTO Bank VALUES ('{0}','{1}','{2}','{3}','{4}',{5},'{6}'," + 
            "'{7}','{8}',{9},'{10}',{11})";
        private const string UPDATE_QUERY = "UPDATE Bank SET NAME ='{0}',Branch = '{1}', "+
            "[Address] = '{2}', [City] = '{3}',[State] ='{4}', [Pincode] = {5}, IFSC ='{6}', MICR ='{7}'," +
            "[UpdatedOn] ='{8}', [UpdatedBy] = {9} WHERE ID = {10}";

        private const string DELETE_BY_ID = "DELETE FROM Bank WHERE ID ='{0}'";

        public IList<Bank> Get()
        {
            try
            {
                Logger.LogInfo("Get: Bank process start");
                IList<Bank> lstBank = new List<Bank>();

                DataTable dtAppConfig = DataBase.DBService.ExecuteCommand(string.Format(SELECT_ALL));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    Bank Bank = convertToBankObject(dr);
                    lstBank.Add(Bank);
                }
                Logger.LogInfo("Get: Bank process completed.");
                return lstBank;
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

        public Bank Get(int id)
        {
            try
            {
                Logger.LogInfo("Get: Bank process start");
                Bank bank = new Bank();

                DataTable dtAppConfig = DataBase.DBService.ExecuteCommand(string.Format(SELECT_BANK_BY_ID,id));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    bank = convertToBankObject(dr);
                }
                Logger.LogInfo("Get: Bank process completed.");
                return bank;
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

        public void Add(Bank bank)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY, 0));

                DataBase.DBService.ExecuteCommand(string.Format(INSERT_QUERY,
                   bank.Name,
                   bank.Branch,
                   bank.Address,
                   bank.City,
                   bank.State,
                   bank.Pincode,
                   bank.IFSC,
                   bank.MICR,
                   bank.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss"), bank.CreatedBy,
                   bank.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), bank.UpdatedBy));

                //Activity.ActivitiesService.Add(ActivityType.CreateBank, EntryStatus.Success,
                //         Source.Server, bank.UpdatedByUserName, bank.Name, bank.MachineName);
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
        
        public void Update(Bank bank)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY, 0));

                DataBase.DBService.ExecuteCommand(string.Format(UPDATE_QUERY,
                   bank.Name,
                   bank.Branch,
                   bank.Address,
                   bank.City,
                   bank.State,
                   bank.Pincode,
                   bank.IFSC,
                   bank.MICR,
                   bank.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), bank.UpdatedBy,
                   bank.Id));

                //Activity.ActivitiesService.Add(ActivityType.UpdateBank, EntryStatus.Success,
                //         Source.Server, bank.UpdatedByUserName, bank.Name, bank.MachineName);
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

        public void Delete(Bank Bank)
        {
            try
            {
                DataBase.DBService.ExecuteCommand(string.Format(DELETE_BY_ID, Bank.Id));
                //Activity.ActivitiesService.Add(ActivityType.DeleteBank, EntryStatus.Success,
                //         Source.Server, Bank.UpdatedByUserName, Bank.Name, Bank.MachineName);
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

        private Bank convertToBankObject(DataRow dr)
        {
            Bank bank = new Bank();
            bank.Id = dr.Field<int>("ID");
            bank.Name = dr.Field<string>("Name");
            bank.Branch = dr.Field<string>("Branch");
            bank.Address = dr.Field<string>("Address");
            bank.City = dr.Field<string>("City");
            bank.State = dr.Field<string>("State");
            //bank.Pincode = dr.Field<int>("Pincode");
            bank.IFSC = dr.Field<string>("IFSC");
            bank.MICR = dr.Field<string>("MICR");
            bank.UpdatedBy = dr.Field<int>("UpdatedBy");
            bank.UpdatedOn = dr.Field<DateTime>("UpdatedOn");
            bank.UpdatedByUserName = dr.Field<string>("UpdatedByUserName");
            bank.CreatedBy = dr.Field<int>("CreatedBy");
            bank.CreatedOn = dr.Field<DateTime>("CreatedOn");
            bank.UpdatedByUserName = dr.Field<string>("UpdatedByUserName");
            return bank;
        }
    }
}
