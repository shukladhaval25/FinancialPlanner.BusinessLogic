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
    public class ExistingInsuranceService
    {
        private const string SELECT_ALL = "select ExistingSumAssuredAmount from ExistingInsurance where PID = {0}";
        private const string SELECT_COUNT = "select count(*) from ExistingInsurance where PID = {0}";
        private const string ADD_QUERY = "INSERT INTO EXISTINGINSURANCE VALUES ({0},{1})";
        private const string UPDATE_QUERY = "UPDATE EXISTINGINSURANCE SET ExistingSumAssuredAmount = {0} where pid = {1}";

        public ExistingInsurance GetExistingSumAssured(int plannerId)
        {
            try
            {
                Logger.LogInfo("Get: Existing insurance process start");
                ExistingInsurance existingInsurance = new ExistingInsurance();

                string result = DataBase.DBService.ExecuteCommandScalar(string.Format(SELECT_ALL, plannerId));
                if (!string.IsNullOrEmpty(result))
                {
                    existingInsurance.PID = plannerId;
                    existingInsurance.ExistingSumAssuredAmount = double.Parse(result);
                }
                Logger.LogInfo("Get: Existing insurance process completed.");
                return existingInsurance;
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

        public void Updatedata(ExistingInsurance existingInsurance)
        {
            try
            {
                Logger.LogInfo("Update: Existing insurance process start");

                string result = DataBase.DBService.ExecuteCommandScalar(string.Format(SELECT_COUNT, existingInsurance.PID));
                if (result.Equals("0"))
                {
                    //Insert record
                   DataBase.DBService.ExecuteCommand(string.Format(ADD_QUERY,
                    existingInsurance.PID,existingInsurance.ExistingSumAssuredAmount));
                }
                else //Update record
                {
                    DataBase.DBService.ExecuteCommand(string.Format(UPDATE_QUERY,
                    existingInsurance.ExistingSumAssuredAmount, existingInsurance.PID));
                }
                Logger.LogInfo("Update: Existing insurance process completed.");
            }
            catch (Exception ex)
            {
                StackTrace st = new StackTrace();
                StackFrame sf = st.GetFrame(0);
                MethodBase currentMethodName = sf.GetMethod();
                LogDebug(currentMethodName.Name, ex);
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
