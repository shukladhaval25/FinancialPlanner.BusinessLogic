using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FinancialPlanner.Common.Model.TaskManagement;
using FinancialPlanner.Common.Model.TaskManagement.MFTransactions;

namespace FinancialPlanner.BusinessLogic.TaskManagements
{
    class FreshPurchaseTransactionServiceImpl : ITransactionTypeService
    {
        private const string INSERT_FRESHPURCHASE = "INSERT INTO FRESHPURCHASE VALUES ({0},{1},{2},'{3}','{4}','{5}','{6}'," + 
            "'{7}','{8}','{9}','{10}',{11},'{12}',{13},'{14}','{15}','{16}')";
        FreshPurchase freshPurchase;
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
