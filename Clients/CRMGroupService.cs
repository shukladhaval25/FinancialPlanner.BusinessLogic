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
    public class CRMGroupService
    {
        private const string GET_CLIENT_NAME_QUERY = "SELECT NAME FROM CLIENT WHERE ID = {0}";
        private const string SELECT_ALL = "SELECT C1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM CRMGroups C1, USERS U WHERE C1.UPDATEDBY = U.ID";

        private const string INSERT_QUERY = "INSERT INTO CRMGroups VALUES ('{0}','{1}',{2},'{3}',{4})";

        private const string DELETE_BY_ID = "DELETE FROM CRMGroups WHERE NAME ='{0}'";

        public IList<CRMGroup> Get()
        {
            try
            {
                Logger.LogInfo("Get: CRM group process start");
                IList<CRMGroup> lstCRMGroup = new List<CRMGroup>();

                DataTable dtAppConfig =  DataBase.DBService.ExecuteCommand(string.Format(SELECT_ALL));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    CRMGroup CRMGroup = convertToCRMGroupObject(dr);
                    lstCRMGroup.Add(CRMGroup);
                }
                Logger.LogInfo("Get: CRM group process completed.");
                return lstCRMGroup;
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

        public void Add(CRMGroup CRMGroup)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY,0));

                DataBase.DBService.ExecuteCommand(string.Format(INSERT_QUERY,
                   CRMGroup.Name,
                   CRMGroup.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss"), CRMGroup.CreatedBy,
                   CRMGroup.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), CRMGroup.UpdatedBy));

                Activity.ActivitiesService.Add(ActivityType.CreateCRMGroup, EntryStatus.Success,
                         Source.Server, CRMGroup.UpdatedByUserName, CRMGroup.Name, CRMGroup.MachineName);
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

        public void Delete(CRMGroup CRMGroup)
        {
            try
            {
                DataBase.DBService.ExecuteCommand(string.Format(DELETE_BY_ID, CRMGroup.Name));
                Activity.ActivitiesService.Add(ActivityType.DeleteCRMGroup, EntryStatus.Success,
                         Source.Server, CRMGroup.UpdatedByUserName, CRMGroup.Name, CRMGroup.MachineName);
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

        private CRMGroup convertToCRMGroupObject(DataRow dr)
        {
            CRMGroup CRMGroup = new CRMGroup ();
            CRMGroup.Name = dr.Field<string>("Name");
            return CRMGroup;
        }
    }
}
