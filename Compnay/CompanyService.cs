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

namespace FinancialPlanner.BusinessLogic.Compnay
{
    public class CompanyService
    {
        private const string SELECT = "SELECT C1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM COMPANY C1, USERS U WHERE C1.UPDATEDBY = U.ID";
        private const string INSERT = "INSERT INTO COMPANY VALUES ('{0}','{1}','{2}','{3}','{4}','{5}','{6}',{7},'{8}',{9})";

        public Company Get()
        {
            try
            {
                Logger.LogInfo("Get: Company process start");
                Company comp = new Company();

                DataTable dtAppConfig =  DataBase.DBService.ExecuteCommand(string.Format(SELECT));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    comp = convertToCompansObject(dr);                    
                }
                Logger.LogInfo("Get: Company process completed.");
                return comp;
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

        private Company convertToCompansObject(DataRow dr)
        {
            Company comp = new Company();
            comp.Name = dr.Field<string>("Name");
            comp.Address = dr.Field<string>("Address");
            comp.Contactno = dr.Field<string>("Contactno");
            comp.Email = dr.Field<string>("Email");
            comp.Website = dr.Field<string>("Website");
            comp.RegistrationNo = dr.Field<string>("RegistrationNo");

            return comp;
        }

        public void Add(Company company)
        {
            try
            {
                DataBase.DBService.ExecuteCommand(string.Format(INSERT,
                   company.Name, company.Address,company.Contactno,
                   company.Email,company.Website,company.RegistrationNo,
                   company.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss"), company.CreatedBy,
                   company.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), company.UpdatedBy));

                Activity.ActivitiesService.Add(ActivityType.UpdateCompany, EntryStatus.Success,
                         Source.Server, company.UpdatedByUserName, company.Name, company.MachineName);
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
    }
}
