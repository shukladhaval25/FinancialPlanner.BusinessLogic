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
    public class SchemeCategoryService
    {
        private const string SELECT_ALL =  "SELECT C1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM SchemeCategory C1, USERS U WHERE C1.UPDATEDBY = U.ID";
        private const string INSERT_QUERY = "INSERT INTO SchemeCategory VALUES ('{0}','{1}',{2},'{3}',{4})";

        private const string DELETE_BY_ID = "DELETE FROM SchemeCategory WHERE NAME ='{0}'";
        public IList<SchemeCategory> Get()
        {
            try
            {
                Logger.LogInfo("Get: Scheme category process start");
                IList<SchemeCategory> schemeCategories = new List<SchemeCategory>();

                DataTable dtAppConfig = DataBase.DBService.ExecuteCommand(string.Format(SELECT_ALL));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    SchemeCategory schemeCategory = convertToSchemeCategory(dr);
                    schemeCategories.Add(schemeCategory);
                }
                Logger.LogInfo("Get: Scheme category process completed.");
                return schemeCategories;
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

        private SchemeCategory convertToSchemeCategory(DataRow dr)
        {
            SchemeCategory schemeCategory = new SchemeCategory();

            schemeCategory.Id = dr.Field<int>("ID");
            schemeCategory.Name = dr.Field<string>("Name");

            schemeCategory.UpdatedBy = dr.Field<int>("UpdatedBy");
            schemeCategory.UpdatedOn = dr.Field<DateTime>("UpdatedOn");
            schemeCategory.UpdatedByUserName = dr.Field<string>("UpdatedByUserName");
            schemeCategory.CreatedBy = dr.Field<int>("CreatedBy");
            schemeCategory.CreatedOn = dr.Field<DateTime>("CreatedOn");
            schemeCategory.UpdatedByUserName = dr.Field<string>("UpdatedByUserName");
            return schemeCategory;
        }

        public void Delete(SchemeCategory schemeCategory)
        {
            try
            {
                DataBase.DBService.ExecuteCommand(string.Format(DELETE_BY_ID, schemeCategory.Name));
                Activity.ActivitiesService.Add(ActivityType.DeleteSchemeCategory, EntryStatus.Success,
                         Source.Server, schemeCategory.UpdatedByUserName, schemeCategory.Name, schemeCategory.MachineName);
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

        public void Add(SchemeCategory schemeCategory)
        {
            try
            {
                //string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY, 0));

                DataBase.DBService.ExecuteCommand(string.Format(INSERT_QUERY,
                   schemeCategory.Name,
                   schemeCategory.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss"), schemeCategory.CreatedBy,
                   schemeCategory.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), schemeCategory.UpdatedBy));

                Activity.ActivitiesService.Add(ActivityType.CreateSchemeCategory, EntryStatus.Success,
                         Source.Server, schemeCategory.UpdatedByUserName, schemeCategory.Name, schemeCategory.MachineName);
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
    }
}
