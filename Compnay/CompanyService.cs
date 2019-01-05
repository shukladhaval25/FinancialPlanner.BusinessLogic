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
        private const string INSERT = "INSERT INTO COMPANY VALUES ({0},'{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}',{12},'{13}',{14})";
        private const string DELETE_BY_ID = "DELETE FROM COMPANY WHERE ID = {0}";
        private const string UPDATE = "UPDATE COMPANY SET OTID = {0}, NAME = '{1}',Address ='{2}'," +
            "Contactno ='{3}', Email ='{4}', Website ='{5}', RegistrationNo ='{6}',GST ='{7}'," +
            "BANKACNO ='{8}', BANKNAME ='{9}', PANNO ='{10}', UPDATEDON ='{11}',UPDATEDBY ={12} WHERE ID ={13}";

        public IList<Company> Get()
        {
            try
            {
                Logger.LogInfo("Get: Company process start");
                IList<Company> lstCompany = new List<Company>();

                DataTable dtAppConfig =  DataBase.DBService.ExecuteCommand(string.Format(SELECT));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    lstCompany.Add(convertToCompansObject(dr));                    
                }
                Logger.LogInfo("Get: Company process completed.");
                return lstCompany;
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
            comp.Otid = dr.Field<int>("Otid");
            comp.Name = dr.Field<string>("Name");
            comp.Address = dr.Field<string>("Address");
            comp.Contactno = dr.Field<string>("Contactno");
            comp.Email = dr.Field<string>("Email");
            comp.Website = dr.Field<string>("Website");
            comp.RegistrationNo = dr.Field<string>("RegistrationNo");
            comp.Gst = dr.Field<string>("GST");
            comp.Bank = dr.Field<string>("BankName");
            comp.Accountno = dr.Field<string>("BANKACNO");
            comp.Panno = dr.Field<string>("PANNO");

            return comp;
        }

        public void Add(Company company)
        {
            try
            {
                DataBase.DBService.ExecuteCommand(string.Format(INSERT,
                   company.Otid,
                   company.Name, company.Address,company.Contactno,
                   company.Email,company.Website,company.RegistrationNo,
                   company.Gst,company.Accountno,company.Bank,company.Panno,
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
        public void Delete(Company company)
        {
            try
            {
                DataBase.DBService.ExecuteCommand(string.Format(DELETE_BY_ID, company.Id));
                Activity.ActivitiesService.Add(ActivityType.DeleteArea, EntryStatus.Success,
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

        public void Update(Company company)
        {
            try
            {
                DataBase.DBService.ExecuteCommand(string.Format(UPDATE,
                    company.Otid, company.Name, company.Address,
                    company.Contactno, company.Email, company.Website,
                    company.RegistrationNo, company.Gst, company.Accountno,
                    company.Bank, company.Panno, company.UpdatedOn,
                    company.UpdatedBy, company.Id));
                
                Activity.ActivitiesService.Add(ActivityType.UpdateFamilyMember, EntryStatus.Success,
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
    }
}
