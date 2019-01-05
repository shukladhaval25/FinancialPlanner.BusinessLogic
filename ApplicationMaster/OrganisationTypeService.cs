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
    public class OrganisationTypeService
    {
        private const string GET_CLIENT_NAME_QUERY = "SELECT NAME FROM CLIENT WHERE ID = {0}";
        private const string SELECT_ALL = "SELECT C1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM OrganisationType C1, USERS U WHERE C1.UPDATEDBY = U.ID";

        private const string INSERT_QUERY = "INSERT INTO OrganisationType VALUES ('{0}','{1}',{2},'{3}',{4})";

        private const string UPDATE_QUERY = "UPDATE OrganisationType SET TYPE = '{0}',UPDATEDON = '{1}'," +
            "UPDATEDBY={2} WHERE ID ={3}";

        private const string DELETE_BY_ID = "DELETE FROM OrganisationType WHERE Id = {0}";

        public IList<OrganisationType> Get()
        {
            try
            {
                Logger.LogInfo("Get: Organisation type process start");
                IList<OrganisationType> lstOrganisationType = new List<OrganisationType>();

                DataTable dtAppConfig =  DataBase.DBService.ExecuteCommand(string.Format(SELECT_ALL));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    OrganisationType OrganisationType = convertToOrganisationTypeObject(dr);
                    lstOrganisationType.Add(OrganisationType);
                }
                Logger.LogInfo("Get: Organisation type process completed.");
                return lstOrganisationType;
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

        public void Add(OrganisationType OrganisationType)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY,0));

                DataBase.DBService.ExecuteCommand(string.Format(INSERT_QUERY,
                   OrganisationType.Type,
                   OrganisationType.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss"), OrganisationType.CreatedBy,
                   OrganisationType.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), OrganisationType.UpdatedBy));

                Activity.ActivitiesService.Add(ActivityType.CreateOrganisationType, EntryStatus.Success,
                         Source.Server, OrganisationType.UpdatedByUserName, OrganisationType.Type, OrganisationType.MachineName);
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

        public void Update(OrganisationType OrganisationType)
        {
            try
            {
                DataBase.DBService.ExecuteCommand(string.Format(UPDATE_QUERY,
                   OrganisationType.Type,
                   OrganisationType.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), OrganisationType.UpdatedBy,
                   OrganisationType.Id));

                Activity.ActivitiesService.Add(ActivityType.UpdateOrganisationType, EntryStatus.Success,
                         Source.Server, OrganisationType.UpdatedByUserName, OrganisationType.Type, OrganisationType.MachineName);
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

        public void Delete(OrganisationType OrganisationType)
        {
            try
            {
                DataBase.DBService.ExecuteCommand(string.Format(DELETE_BY_ID, OrganisationType.Id));
                Activity.ActivitiesService.Add(ActivityType.DeleteOrganisationType, EntryStatus.Success,
                         Source.Server, OrganisationType.UpdatedByUserName, OrganisationType.Type, OrganisationType.MachineName);
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

        private OrganisationType convertToOrganisationTypeObject(DataRow dr)
        {
            OrganisationType OrganisationType = new OrganisationType ();
            OrganisationType.Id = dr.Field<int>("ID");
            OrganisationType.Type = dr.Field<string>("Type");
            return OrganisationType;
        }
    }
}
