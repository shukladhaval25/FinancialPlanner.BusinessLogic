using FinancialPlanner.Common;
using FinancialPlanner.Common.Planning;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FinancialPlanner.BusinessLogic.Process
{
    public class DesignationHeirarchyService
    {
        private const string GET_CLIENT_NAME_QUERY = "SELECT C.NAME FROM CLIENT C, PLANNER P  WHERE P.CLIENTID = C.ID AND P.ID = {0}";

        const string SELECT_All = "SELECT [dbo].[DesignationHierarchy].[Id], " +
        "[dbo].[DesignationHierarchy].[Designation], " +
        "[dbo].[DesignationHierarchy].[ReportingToDesignationId], " +
        "R1.Designation as ReportingTo , " +
        "[dbo].[DesignationHierarchy].[Description] FROM " +
        "[dbo].[DesignationHierarchy] Full outer join[dbo].[DesignationHierarchy] as R1 ON " +
        "R1.Id  = [dbo].[DesignationHierarchy].ReportingToDesignationId " +
         "Where [dbo].[DesignationHierarchy].[Id] is not null";

        const string DELETE_QUERY = "DELETE FROM DesignationHierarchy WHERE ID = {0}";

        const string INSERT_QUERY = "INSERT INTO [dbo].[DesignationHierarchy] " +
            "([Designation],[ReportingToDesignationId],[Description]) VALUES " +
            "('{0}',{1},'{2}')";

        const string UPDATE_QUERY = "UPDATE [dbo].[DesignationHierarchy] " +
                    " SET [Designation] = '{0}'" +
                    ",[ReportingToDesignationId] = {1}" +
                    ",[Description] = '{2}' WHERE ID = {3}";

        public IList<DesignationHierarchy> GetAll()
        {

            try
            {
                Logger.LogInfo("Get: DesignationHierarchy steps process start");
                IList<DesignationHierarchy> processSteps = new List<DesignationHierarchy>();
                DataTable dtAppConfig = DataBase.DBService.ExecuteCommand(SELECT_All);
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    DesignationHierarchy processStep = convertToDesignationHeirarchy(dr);
                    processSteps.Add(processStep);
                }
                Logger.LogInfo("Get: DesignationHierarchy steps process completed.");
                return processSteps;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public void Add(DesignationHierarchy designationHeirarchy)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY, 0));
                int? id = (designationHeirarchy.ReportingToDesignationId == null) ? null : designationHeirarchy.ReportingToDesignationId;

                if ((designationHeirarchy.ReportingToDesignationId == null))
                {
                    DataBase.DBService.ExecuteCommand("INSERT INTO[dbo].[DesignationHierarchy] " +
                "([Designation],[ReportingToDesignationId],[Description]) VALUES ('" + designationHeirarchy.Designation + "'," + "Null" + ",'" + designationHeirarchy.Description + "')");
                }
                else
                {
                    DataBase.DBService.ExecuteCommand(string.Format(INSERT_QUERY,
                       designationHeirarchy.Designation, id, designationHeirarchy.Description));
                }
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
        public void Update(DesignationHierarchy designationHeirarchy)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY, 0));

                DataBase.DBService.ExecuteCommand(string.Format(UPDATE_QUERY,
                   designationHeirarchy.Designation, designationHeirarchy.ReportingToDesignationId,
                   designationHeirarchy.Description, designationHeirarchy.Id));

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
        public void Delete(int Id)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY, 0));

                DataBase.DBService.ExecuteCommand(string.Format(DELETE_QUERY, Id));

                //Activity.ActivitiesService.Add(ActivityType.DeleteLoan, EntryStatus.Success,
                //         Source.Server, designationHeirarchy.UpdatedByUserName, clientName, designationHeirarchy.MachineName);
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

        private DesignationHierarchy convertToDesignationHeirarchy(DataRow dr)
        {
            DesignationHierarchy designationHeirarchy = new DesignationHierarchy();
            designationHeirarchy.Id = dr.Field<int>("ID");
            designationHeirarchy.Designation = dr.Field<string>("Designation");
            if (dr["ReportingToDesignationId"] != DBNull.Value)
            {
                designationHeirarchy.ReportingToDesignationId = dr.Field<int>("ReportingToDesignationId");
            }
            else
            {
                designationHeirarchy.ReportingToDesignationId = null;
            }
            designationHeirarchy.ReportTo = dr.Field<string>("ReportingTo");
            designationHeirarchy.Description = dr.Field<string>("Description");
            return designationHeirarchy;
        }
    }
}
