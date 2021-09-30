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

namespace FinancialPlanner.BusinessLogic.Plans
{
    public class ImageBankService
    {
        const string SELECT_ALL = "SELECT * FROM IMAGEBANK";
        const string SELECT_COUNT = "SELECT COUNT(*) FROM IMAGEBANK";

        const string INSERT_QUERY = "INSERT INTO ASSUMPTIONMASTER VALUES (" +
            "{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},'{10}',{11},'{12}',{13},{14},{15})";

        const string UPDATE_QUERY = "UPDATE ASSUMPTIONMASTER SET " +
            "RetirementAge = {0}," +
            "LifeExpectancy = {1}," +
            "PreRetirementInflactionRate ={2}," +
            "PostRetirementInflactionRate ={3}," +
            "EquityReturnRate = {4}," +
            "DebtReturnRate = {5}," +
            "OtherReturnRate = {6}," +
            "IncomeRise = {7}," +
            "OngoingExpRise ={8}," +
            "NonFinancialRateOfReturn = {9}," +
            "UPDATEDON = '{10}'," +
            "UPDATEDBY={11},PostRetirementInvestmentReturnRate ={13}," +
            "InsuranceReturnRate={14} WHERE ID ={12}";

        public ImageBank GetAll()
        {
            try
            {
                Logger.LogInfo("Get: Image Bank master process start");
                ImageBank imageBank = new ImageBank();

                DataTable dtAppConfig = DataBase.DBService.ExecuteCommand(SELECT_ALL);
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    imageBank = convertToImageBankObject(dr);
                }
                Logger.LogInfo("Get: Image Bank master process completed.");
                return imageBank;
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
                       assumptionMaster.Id,
                       assumptionMaster.PostRetirementInvestmentReturnRate,
                       assumptionMaster.InsuranceReturnRate), true);
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
                      assumptionMaster.Id,
                      assumptionMaster.PostRetirementInvestmentReturnRate,
                      assumptionMaster.InsuranceReturnRate), true);
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

        private ImageBank convertToImageBankObject(DataRow dr)
        {
            ImageBank imageBank = new ImageBank();
            imageBank.Id = dr.Field<int>("ID");
            imageBank.PropertyName = dr["PropertyName"].ToString();
            imageBank.Category = dr["Category"].ToString();
            imageBank.ImageData = "";
            imageBank.ImagePath = dr["ImagePath"].ToString();
            return imageBank;
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
