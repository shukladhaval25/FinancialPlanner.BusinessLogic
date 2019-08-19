using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FinancialPlanner.Common.Model.TaskManagement;
using FinancialPlanner.Common.Model.TaskManagement.MFTransactions;

namespace FinancialPlanner.BusinessLogic.TaskManagements
{
    internal class AdditionalPurchaseServiceImpl : ITransactionTypeService
    {
        AdditionalPurchase additionalPurchase;
        public void SaveTransaction(TaskCard taskCard, int id)
        {
            additionalPurchase = new FinancialPlanner.Common.JSONSerialization().DeserializeFromString<AdditionalPurchase>(taskCard.TaskTransactionType.ToString());
        }
    }
}
