using FinancialPlanner.BusinessLogic.ApplicationMaster;
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

namespace FinancialPlanner.BusinessLogic.Clients
{
    public class FamilyMemberBankService
    {
        private const string SELECT_ALL_BY_ACCOUNTHOLDER_ID = "SELECT FMB.*,U.USERNAME AS UPDATEDBYUSERNAME FROM FAMILYMEMBERBANK FMB," +
            "USERS U WHERE FMB.UPDATEDBY = U.ID and FMB.AccountHolderId = {0}";

        private const string SELECT_ALL_BY_ACCOUNTHOLDER_ID_AND_ID = "SELECT FMB.*,U.USERNAME AS UPDATEDBYUSERNAME FROM FAMILYMEMBERBANK FMB," +
            "USERS U WHERE FMB.UPDATEDBY = U.ID and FMB.AccountHolderId = {0} AND FMB.ID = {1}";

        private const string INSERT_FAMILYMEMBER_BANK = "INSERT INTO FAMILYMEMBERBANK VALUES ({0},{1},'{2}','{3}','{4}',{5},'{6}',{7})";

        private const string UPDATE_FAMILYMEMBER_BANK = "UPDATE FAMILYMEMBERBANK SET BankId ={0}, ACCOUNTNO = '{1}', ACCOUNTTYPE ='{2}', "+
            "UPDATEDON ='{3}',UPDATEDBY ={4} WHERE ID ={5}";

        private const string DELETE_FAMILYMEMBER_BANK = "DELETE FROM FAMILYMEMBERBANK WHERE ID ={0}";
   
        public IList<FamilyMemberBank> Get(int accountHolderId)
        {
            try
            {
                Logger.LogInfo("Get: Family member information process start");
                IList<FamilyMemberBank> lstFamilyMember = new List<FamilyMemberBank>();

                DataTable dtAppConfig = DataBase.DBService.ExecuteCommand(string.Format(SELECT_ALL_BY_ACCOUNTHOLDER_ID, accountHolderId));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    FamilyMemberBank familyMemberBank = convertToFamilyMemberBankObject(dr);
                    lstFamilyMember.Add(familyMemberBank);
                }
                Logger.LogInfo("Get: Family member information process completed.");
                return lstFamilyMember;
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

        public IList<FamilyMemberBank> Get(int accountHolderId,int id)
        {
            try
            {
                Logger.LogInfo("Get: Family member information process start");
                IList<FamilyMemberBank> lstFamilyMember = new List<FamilyMemberBank>();

                DataTable dtAppConfig = DataBase.DBService.ExecuteCommand(string.Format(SELECT_ALL_BY_ACCOUNTHOLDER_ID_AND_ID,
                    accountHolderId,id));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    FamilyMemberBank familyMemberBank = convertToFamilyMemberBankObject(dr);
                    lstFamilyMember.Add(familyMemberBank);
                }
                Logger.LogInfo("Get: Family member information process completed.");
                return lstFamilyMember;
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

        public void Add(FamilyMemberBank familyMemberBank)
        {
            try
            {
                //string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY, familyMember.Cid));
                DataBase.DBService.ExecuteCommand(string.Format(INSERT_FAMILYMEMBER_BANK,
                   familyMemberBank.AccountHolderId, familyMemberBank.BankId, familyMemberBank.AccountNo,
                   familyMemberBank.AccountType, 
                   familyMemberBank.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss"), familyMemberBank.CreatedBy,
                   familyMemberBank.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), familyMemberBank.UpdatedBy));

                Activity.ActivitiesService.Add(ActivityType.CreateFamilyMember, EntryStatus.Success,
                         Source.Server, familyMemberBank.UpdatedByUserName, familyMemberBank.Bank.Name, familyMemberBank.MachineName);
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

        public void Delete(FamilyMemberBank familyMemberBank)
        {
            try
            {
                DataBase.DBService.ExecuteCommand(string.Format(DELETE_FAMILYMEMBER_BANK, familyMemberBank.Id));
                Activity.ActivitiesService.Add(ActivityType.DeleteFamilyMember, EntryStatus.Success,
                         Source.Server, familyMemberBank.UpdatedByUserName, familyMemberBank.Bank.Name, familyMemberBank.MachineName);
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

        public void Update(FamilyMemberBank familyMemberBank)
        {
            try
            {
                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(UPDATE_FAMILYMEMBER_BANK,
                   familyMemberBank.BankId, familyMemberBank.AccountNo,
                   familyMemberBank.AccountType,
                   familyMemberBank.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"),
                   familyMemberBank.UpdatedBy, familyMemberBank.Id),true);

                Activity.ActivitiesService.Add(ActivityType.UpdateFamilyMember, EntryStatus.Success,
                         Source.Server, familyMemberBank.UpdatedByUserName, familyMemberBank.Bank.Name, familyMemberBank.MachineName);
                DataBase.DBService.CommitTransaction();
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

        private FamilyMemberBank convertToFamilyMemberBankObject(DataRow dr)
        {
            FamilyMemberBank familymemberBank = new FamilyMemberBank();
            familymemberBank.Id = dr.Field<int>("ID");
            familymemberBank.AccountHolderId = dr.Field<int>("AccountHolderId");
            familymemberBank.BankId = dr.Field<int>("BankId");
            familymemberBank.AccountNo = dr.Field<string>("AccountNo");
            familymemberBank.AccountType = dr.Field<string>("AccountType");
            familymemberBank.Bank = new BankService().Get(familymemberBank.BankId);
            return familymemberBank;
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
