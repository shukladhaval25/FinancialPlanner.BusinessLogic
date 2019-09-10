using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FinancialPlanner.Common;
using FinancialPlanner.Common.Model.TaskManagement;
using FinancialPlanner.Common.Model.TaskManagement.MFTransactions;

namespace FinancialPlanner.BusinessLogic.TaskManagements
{
    class FreshPurchaseTransactionServiceImpl : ITransactionTypeService
    {
        private const string INSERT_FRESHPURCHASE = "INSERT INTO FRESHPURCHASE VALUES ({0},{1},{2},'{3}','{4}','{5}','{6}'," +
            "'{7}','{8}',{9},'{10}',{11},'{12}',{13},'{14}','{15}','{16}')";
        private const string SELECT_BY_ID = "SELECT * FROM FRESHPURCHASE WHERE TASKID ={0}";
        FreshPurchase freshPurchase;

        public object GetTransaction(int id)
        {
            try
            {
                Logger.LogInfo("Get: Fresh purchase transaction process start");
                FreshPurchase freshPurchase = new FreshPurchase();

                DataTable dtAppConfig = DataBase.DBService.ExecuteCommand(string.Format(SELECT_BY_ID, id));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    freshPurchase = converToFreshPruchase(dr);
                }
                Logger.LogInfo("Get: Fresh purchase transaction process completed.");
                return freshPurchase;
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

        private void LogDebug(string methodName, Exception ex)
        {
            DebuggerLogInfo debuggerInfo = new DebuggerLogInfo();
            debuggerInfo.ClassName = this.GetType().Name;
            debuggerInfo.Method = methodName;
            debuggerInfo.ExceptionInfo = ex;
            Logger.LogDebug(debuggerInfo);
        }

        private FreshPurchase  converToFreshPruchase(DataRow dr)
        {
            FreshPurchase freshPurchase = new FreshPurchase();
            freshPurchase.Id =  dr.Field<int>("ID");
            freshPurchase.TaskId = dr.Field<int>("TaskId");
            freshPurchase.Arn = dr.Field<int>("ARN");
            freshPurchase.Cid = dr.Field<int>("CID");
            freshPurchase.MemberName = dr.Field<string>("MemberName");
            freshPurchase.SecondHolder = dr.Field<string>("SecondHolder");
            freshPurchase.ThirdHolder = dr.Field<string>("ThirdHolder");
            freshPurchase.Nominee = dr.Field<string>("Nominee");
            freshPurchase.Guardian = dr.Field<string>("Guardian");
            freshPurchase.ModeOfHolding = dr.Field<string>("ModeOfHolding");
            freshPurchase.Amc = dr.Field<int>("AMC");
            freshPurchase.FolioNumber = dr.Field<string>("FolioNumber");
            freshPurchase.Scheme = dr.Field<int>("SchemeId");
            freshPurchase.Options = dr.Field<string>("Option");
            freshPurchase.Amount = dr.Field<long>("Amount");
            freshPurchase.TransactionDate = dr.Field<DateTime>("TransactionDate");
            freshPurchase.ModeOfExecution = dr.Field<string>("ModeOfExecution");
            freshPurchase.Remark = dr.Field<string>("Remark");
            return freshPurchase;
        }

        public void SaveTransaction(TaskCard taskCard, int id)
        {
            freshPurchase = new FinancialPlanner.Common.JSONSerialization().DeserializeFromString<FreshPurchase>(taskCard.TaskTransactionType.ToString());
            DataBase.DBService.ExecuteCommandString(string.Format(INSERT_FRESHPURCHASE,
                   id,
                   freshPurchase.Arn,
                   freshPurchase.Cid,
                   freshPurchase.MemberName,
                   freshPurchase.SecondHolder,
                   freshPurchase.ThirdHolder,
                   freshPurchase.Nominee,
                   freshPurchase.Guardian,
                   freshPurchase.ModeOfHolding,
                   freshPurchase.Amc,
                   freshPurchase.FolioNumber,
                   freshPurchase.Scheme,
                   freshPurchase.Options,
                   freshPurchase.Amount,
                   freshPurchase.TransactionDate,
                   freshPurchase.ModeOfExecution,
                   freshPurchase.Remark), true);
        }
    }
}
