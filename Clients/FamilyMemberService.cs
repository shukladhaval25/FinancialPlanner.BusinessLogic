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
    public class FamilyMemberService
    {
        private const string GET_CLIENT_NAME_QUERY = "SELECT NAME FROM CLIENT WHERE ID = {0}";
        private const string SELECT_ALL_BY_CLIENT_ID = "SELECT C1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM CLIENTFAMILYMEMBER C1, USERS U WHERE C1.UPDATEDBY = U.ID and C1.CID = {0}";
        private const string SELECT_ALL_BY_CLIENT_ID_AND_ID = "SELECT C1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM CLIENTFAMILYMEMBER C1, USERS U WHERE C1.UPDATEDBY = U.ID and C1.ID = {0} AND C1.CID = {1}";

        private const string INSERT_QUERY = "INSERT INTO CLIENTFAMILYMEMBER VALUES " + 
            "({0},'{1}','{2}','{3}','{4}','{5}','{6}','{7}',{8},'{9}',{10},'{11}','{12}','{13}')";
        private const string UPDATE_QUERY = "UPDATE CLIENTFAMILYMEMBER SET NAME ='{0}',RELATIONSHIP ='{1}',DOB ='{2}'," +
            "ISDEPENDENT ='{3}',CHILDRENCLASS ='{4}',Description='{5}',UPDATEDON ='{6}', " + 
            "UPDATEDBY ={7}, PAN ='{10}',AADHAR ='{11}',OCCUPATION='{12}' WHERE CID ={8} AND ID ={9}";

        private const string DELETE_BY_ID = "DELETE FROM CLIENTFAMILYMEMBER WHERE ID ={0}";

        public IList<FamilyMember> Get(int clientId)
        {
            try
            {
                Logger.LogInfo("Get: Family member information process start");
                IList<FamilyMember> lstFamilyMember = new List<FamilyMember>();

                DataTable dtAppConfig =  DataBase.DBService.ExecuteCommand(string.Format(SELECT_ALL_BY_CLIENT_ID,clientId));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    FamilyMember familyMember = convertToFamilyMemberObject(dr);
                    lstFamilyMember.Add(familyMember);
                }
                Logger.LogInfo("Get: Family member information process completed.");
                return lstFamilyMember;
            }
            catch(Exception ex)
            {
                StackTrace st = new StackTrace ();
                StackFrame sf = st.GetFrame (0);
                MethodBase  currentMethodName = sf.GetMethod();
                LogDebug(currentMethodName.Name, ex);
                return null;
            }
        }

        public FamilyMember Get(int id, int clientId)
        {
            try
            {
                Logger.LogInfo("Get: Family member information process start");
                FamilyMember familyMember = new FamilyMember();

                DataTable dtAppConfig =  DataBase.DBService.ExecuteCommand(string.Format(SELECT_ALL_BY_CLIENT_ID_AND_ID,id,clientId));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    familyMember = convertToFamilyMemberObject(dr);
                }
                Logger.LogInfo("Get: Family member information process completed.");
                return familyMember;
            }
            catch(Exception ex)
            {
                StackTrace st = new StackTrace ();
                StackFrame sf = st.GetFrame (0);
                MethodBase  currentMethodName = sf.GetMethod();
                LogDebug(currentMethodName.Name, ex);
                return null;
            }
        }

        public void Add(FamilyMember familyMember)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY,familyMember.Cid));

                DataBase.DBService.ExecuteCommand(string.Format(INSERT_QUERY,
                   familyMember.Cid,familyMember.Name, familyMember.Relationship,
                   familyMember.DOB.ToString("yyyy-MM-dd"), familyMember.IsDependent, familyMember.ChildrenClass,
                   familyMember.Description,
                   familyMember.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss"), familyMember.CreatedBy,
                   familyMember.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), familyMember.UpdatedBy,
                   familyMember.Pancard,familyMember.AadharCard,familyMember.Occupation));

                Activity.ActivitiesService.Add(ActivityType.CreateFamilyMember, EntryStatus.Success,
                         Source.Server, familyMember.UpdatedByUserName, clientName, familyMember.MachineName);
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
        public void Update(FamilyMember familyMember)
        {
            try
            {
                DataBase.DBService.ExecuteCommand(string.Format(UPDATE_QUERY,
                   familyMember.Name, familyMember.Relationship,
                   familyMember.DOB.ToString("yyyy-MM-dd"), familyMember.IsDependent,
                   familyMember.ChildrenClass, familyMember.Description,
                   familyMember.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), 
                   familyMember.UpdatedBy,familyMember.Cid,familyMember.Id,
                   familyMember.Pancard,familyMember.AadharCard,
                   familyMember.Occupation));

                Activity.ActivitiesService.Add(ActivityType.UpdateFamilyMember, EntryStatus.Success,
                         Source.Server, familyMember.UpdatedByUserName, familyMember.Name, familyMember.MachineName);
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

        public void Delete(FamilyMember familyMember)
        {
            try
            {
                DataBase.DBService.ExecuteCommand(string.Format(DELETE_BY_ID,familyMember.Id));
                Activity.ActivitiesService.Add(ActivityType.DeleteFamilyMember, EntryStatus.Success,
                         Source.Server, familyMember.UpdatedByUserName, familyMember.Name, familyMember.MachineName);
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

        private void LogDebug(string methodName, Exception ex)
        {
            DebuggerLogInfo debuggerInfo = new DebuggerLogInfo();
            debuggerInfo.ClassName = this.GetType().Name;
            debuggerInfo.Method = methodName;
            debuggerInfo.ExceptionInfo = ex;
            Logger.LogDebug(debuggerInfo);
        }

        private FamilyMember convertToFamilyMemberObject(DataRow dr)
        {
            FamilyMember familymember = new FamilyMember ();
            familymember.Id = dr.Field<int>("ID");
            familymember.Cid = dr.Field<int>("CID");
            familymember.Name = dr.Field<string>("Name");
            familymember.Relationship = dr.Field<string>("Relationship");
            familymember.DOB = dr.Field<DateTime>("DOB");
            familymember.IsDependent = dr.Field<bool>("IsDependent");
            familymember.ChildrenClass = dr.Field<string>("ChildrenClass");
            familymember.Description = dr.Field<string>("Description");
            familymember.Pancard = dr.Field<string>("PAN");
            familymember.AadharCard = dr.Field<string>("AADHAR");
            familymember.Occupation = dr.Field<string>("Occupation");

            return familymember;
        }
    }
}
