using FinancialPlanner.Common;
using FinancialPlanner.Common.Model.Masters;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Reflection;

namespace FinancialPlanner.BusinessLogic.ApplicationMaster
{
    public class InsuranceCompanyService
    {

        private const string GET_CLIENT_NAME_QUERY = "SELECT NAME FROM CLIENT WHERE ID = {0}";
        private const string SELECT_ALL = "SELECT * FROM INSURANCECOMPANY";

        private const string INSERT_QUERY = "INSERT INTO INSURANCECOMPANY VALUES ('{0}')";
        private const string UPDATE_QUERY = "UPDATE INSURANCECOMPANY SET NAME ='{0}' WHERE ID = {1}";

        private const string DELETE_BY_ID = "DELETE FROM INSURANCECOMPANY WHERE ID ='{0}'";

        public IList<InsuranceCompany> Get()
        {
            try
            {
                Logger.LogInfo("Get: Insurance Company process start");
                IList<InsuranceCompany> insuranceCompanies = new List<InsuranceCompany>();

                DataTable dtAppConfig = DataBase.DBService.ExecuteCommand(string.Format(SELECT_ALL));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    InsuranceCompany insuranceCompany = convertToInsuranceCompanyObject(dr);
                    insuranceCompanies.Add(insuranceCompany);
                }
                Logger.LogInfo("Get: Insurance Company process completed.");
                return insuranceCompanies;
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

        public void Add(InsuranceCompany insuranceCompany)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY, 0));
                
                DataBase.DBService.ExecuteCommand(string.Format(INSERT_QUERY,
                   insuranceCompany.Name));
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

        public void Update(InsuranceCompany insuranceCompany)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY, 0));

                DataBase.DBService.ExecuteCommand(string.Format(UPDATE_QUERY,
                   insuranceCompany.Name,
                   insuranceCompany.Id
                   ));
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

        public void Delete(InsuranceCompany insuranceCompany)
        {
            try
            {
                DataBase.DBService.ExecuteCommand(string.Format(DELETE_BY_ID, insuranceCompany.Id));
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

        private InsuranceCompany convertToInsuranceCompanyObject(DataRow dr)
        {
            InsuranceCompany insurance = new InsuranceCompany();
            insurance.Id = dr.Field<int>("ID");
            insurance.Name = dr.Field<string>("Name");
            return insurance;
        }
    }
}
