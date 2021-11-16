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

namespace FinancialPlanner.BusinessLogic.Plans
{
    public class PersonalAccidentInsuranceService
    {

        private const string GET_CLIENT_NAME_QUERY = "SELECT C.NAME FROM CLIENT C, PLANNER P  WHERE P.CLIENTID = C.ID AND P.ID = {0}";

        private const string INSERT_QUERY = "INSERT INTO[dbo].[PersonalAccidentInsurance] " +
          "([PID],[InsuranceCompanyName],[SumAssured] ,[Premium],[Name]) VALUES " +
          "({0},'{1}','{2}',{3},'{4}')";

        private const string UPDATE_QUERY = "UPDATE[dbo].[PersonalAccidentInsurance] " +
            "SET [InsuranceCompanyName] = '{0}', [SumAssured] = '{1}', [Premium] = {2},[Name] ='{3}' WHERE [ID] = {4}";

        private const string DELETE_QUERY = "DELETE from PersonalAccidentInsurance WHERE ID = {0}";

        private const string SELECT_BY_PROJECT_ID = "SELECT * FROM PersonalAccidentInsurance WHERE PID ={0}";


        public IList<PersonalAccidentInsurance> GetAll(int plannerId)
        {
            try
            {
                Logger.LogInfo("Get: Personal accident insurance process start");
                IList<PersonalAccidentInsurance> personalAccidentInsurances = new List<PersonalAccidentInsurance>();

                DataTable dtAppConfig = DataBase.DBService.ExecuteCommand(string.Format(SELECT_BY_PROJECT_ID, plannerId));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    PersonalAccidentInsurance personalAccidentInsurance = convertToPersonalAccidentalInsuranceObject(dr);
                    personalAccidentInsurances.Add(personalAccidentInsurance);
                }
                Logger.LogInfo("Get: Personal accident insurance process completed.");
                return personalAccidentInsurances;
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

        public void Add(PersonalAccidentInsurance personalAccidentInsurance)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY,0));

                DataBase.DBService.ExecuteCommand(string.Format(INSERT_QUERY,
                   personalAccidentInsurance.PId,
                   personalAccidentInsurance.InsuranceCompanyName,
                   personalAccidentInsurance.SumAssured,
                   personalAccidentInsurance.Premium,
                   personalAccidentInsurance.Name));

                //Activity.ActivitiesService.Add(ActivityType.CreateLoan, EntryStatus.Success,
                //         Source.Server, personalAccidentInsurance.UpdatedByUserName, clientName, personalAccidentInsurance.MachineName);
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

        public void Update(PersonalAccidentInsurance personalAccidentInsurance)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY, 0));

                DataBase.DBService.ExecuteCommand(string.Format(UPDATE_QUERY,
                   personalAccidentInsurance.InsuranceCompanyName, 
                   personalAccidentInsurance.SumAssured,
                   personalAccidentInsurance.Premium,
                   personalAccidentInsurance.Name,
                   personalAccidentInsurance.Id));
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

                DataBase.DBService.ExecuteCommand(string.Format(DELETE_QUERY,
                  Id));
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

        private PersonalAccidentInsurance convertToPersonalAccidentalInsuranceObject(DataRow dr)
        {
            PersonalAccidentInsurance personalAccidentInsurance = new PersonalAccidentInsurance();
            personalAccidentInsurance.Id = dr.Field<int>("ID");
            personalAccidentInsurance.PId = dr.Field<int>("PID");
            personalAccidentInsurance.Name = dr.Field<string>("Name");
            personalAccidentInsurance.InsuranceCompanyName = dr.Field<string>("InsuranceCompanyName");
            personalAccidentInsurance.SumAssured = dr.Field<string>("SumAssured");
            personalAccidentInsurance.Premium = double.Parse(dr["Premium"].ToString());
            return personalAccidentInsurance;
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
