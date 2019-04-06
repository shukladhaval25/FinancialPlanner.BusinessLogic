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
    public class AssumptionMasterService
    {
        const string SELECT_ALL = "SELECT N1.* FROM ASSUMPTIONMASTER N1";
        const string SELECT_COUNT = "SELECT COUNT(*) FROM ASSUMPTIONMASTER";

        const string INSERT_QUERY = "INSERT INTO ASSUMPTIONMASTER VALUES (" +
            "{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},'{10}',{11},'{12}',{13})";

        const string UPDATE_QUERY = "UPDATE ASSUMPTIONMASTER SET " +
            "RetirementAge = {0}," +
            "LifeExpectancy = {1}," +           
            "PreRetirementInflactionRate ={3}," +
            "PostRetirementInflactionRate ={4}," +
            "EquityReturnRate = {5}," +
            "DebtReturnRate = {6}," +
            "OtherReturnRate = {7}," +
            "IncomeRaise = {8}," +
            "OngoingExpRise ={9}," +
            "NonFinancialRateOfReturn = {10}," +
            "UPDATEDON = '{10}'," +
            "UPDATEDBY={11} WHERE ID ={12}";

        public AssumptionMaster GetAll()
        {
            try
            {
                Logger.LogInfo("Get: Assumption master process start");
                AssumptionMaster assumptionMaster = new AssumptionMaster();

                DataTable dtAppConfig = DataBase.DBService.ExecuteCommand(SELECT_ALL);
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    assumptionMaster = convertToAssumptionMasterObject(dr);
                }
                Logger.LogInfo("Get: Assumption master process completed.");
                return assumptionMaster;
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

        public void Update(AssumptionMaster assumptionMaster)
        {
            try
            {
                string recordCount = DataBase.DBService.ExecuteCommandScalar(SELECT_COUNT);
                DataBase.DBService.BeginTransaction();
                if (recordCount != "0")
                {
                    DataBase.DBService.ExecuteCommandString(string.Format(UPDATE_QUERY,
                       assumptionMaster.RetirementAge,
                       assumptionMaster.LifeExpectancy,
                       assumptionMaster.PreRetirementInflactionRate,
                       assumptionMaster.PostRetirementInflactionRate,
                       assumptionMaster.EquityReturnRate,
                       assumptionMaster.DebtReturnRate,
                       assumptionMaster.OtherReturnRate,
                       assumptionMaster.IncomeRaiseRatio,
                       assumptionMaster.OngoingExpRise,
                       assumptionMaster.NonFinancialRateOfReturn,
                       assumptionMaster.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"),
                       assumptionMaster.UpdatedBy,                    
                       assumptionMaster.Id), true);
                }
                else
                {
                    DataBase.DBService.ExecuteCommandString(string.Format(INSERT_QUERY,                      
                      assumptionMaster.RetirementAge,
                      assumptionMaster.LifeExpectancy,
                      assumptionMaster.PreRetirementInflactionRate,
                      assumptionMaster.PostRetirementInflactionRate,
                      assumptionMaster.EquityReturnRate,
                      assumptionMaster.DebtReturnRate,
                      assumptionMaster.OtherReturnRate,
                      assumptionMaster.IncomeRaiseRatio,
                      assumptionMaster.OngoingExpRise,
                      assumptionMaster.NonFinancialRateOfReturn,
                      assumptionMaster.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss"),
                      assumptionMaster.CreatedBy,
                      assumptionMaster.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"),
                      assumptionMaster.UpdatedBy,
                      assumptionMaster.Id), true);
                }
                Activity.ActivitiesService.Add(ActivityType.UpdatePlannerAssumption, EntryStatus.Success,
                            Source.Server, assumptionMaster.UpdatedByUserName, "", assumptionMaster.MachineName);
                DataBase.DBService.CommitTransaction();
            }
            catch (Exception ex)
            {
                DataBase.DBService.RollbackTransaction();
                StackTrace st = new StackTrace();
                StackFrame sf = st.GetFrame(0);
                MethodBase currentMethodName = sf.GetMethod();
                LogDebug(currentMethodName.Name, ex);
                throw ex;
            }
        }

        private AssumptionMaster convertToAssumptionMasterObject(DataRow dr)
        {
            AssumptionMaster assumptionMaster = new AssumptionMaster();
            assumptionMaster.Id = dr.Field<int>("ID");
            assumptionMaster.RetirementAge = dr.Field<int>("RetirementAge");            
            assumptionMaster.LifeExpectancy = dr.Field<int>("LifeExpectancy");
            assumptionMaster.PreRetirementInflactionRate = dr.Field<decimal>("PreRetirementInflactionRate");
            assumptionMaster.PostRetirementInflactionRate = dr.Field<decimal>("PostRetirementInflactionRate");
            assumptionMaster.EquityReturnRate = dr.Field<decimal>("EquityReturnRate");
            assumptionMaster.DebtReturnRate = dr.Field<decimal>("DebtReturnRate");
            assumptionMaster.OtherReturnRate = dr.Field<decimal>("OtherReturnRate");
            assumptionMaster.IncomeRaiseRatio = dr.Field<decimal>("IncomeRise");
            assumptionMaster.OngoingExpRise = dr.Field<decimal>("OngoingExpRise");
            assumptionMaster.NonFinancialRateOfReturn = dr.Field<decimal>("NonFinancialRateOfReturn");
            assumptionMaster.UpdatedOn = dr.Field<DateTime>("UpdatedOn");
            assumptionMaster.UpdatedBy = dr.Field<int>("UpdatedBy");           
            return assumptionMaster;
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
