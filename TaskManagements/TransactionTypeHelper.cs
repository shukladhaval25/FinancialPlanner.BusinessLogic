using FinancialPlanner.Common.Model.TaskManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialPlanner.BusinessLogic.TaskManagements
{
    internal class TransactionTypeHelper
    {
        ITransactionTypeService transactionTypeService;
        TaskCard taskCard;
        int id;
        public TransactionTypeHelper (TaskCard taskCard,int id)
        {
            this.taskCard = taskCard;
            this.id = id;

            switch (taskCard.TransactionType)
            {
                case "Fresh Purchase":
                case "Additional Purchase":
                    transactionTypeService = new FreshPurchaseTransactionServiceImpl();
                    break;
                case "SIP Fresh":
                case "SIP Old":
                    transactionTypeService = new SIPServiceImpl();
                    break;
                case "Switch":
                    transactionTypeService = new SwitchTransaactionServiceImpl();
                    break;
                case "STP":
                    transactionTypeService = new STPTransactionServiceImpl();
                    break;
                case "SWP":
                    transactionTypeService = new SWPTransactionServiceImpl();
                    break;
                case "STP Cancellation":
                    transactionTypeService = new STPCancellationCancellationTransactionServiceImpl();
                    break;
                case "SIP Cancellation":
                    transactionTypeService = new SIPCancellationTransactionServiceImpl();
                    break;
                default:
                    transactionTypeService = null;
                    break;
            }
        }
        public void SaveTransaction()
        {
            transactionTypeService.SaveTransaction(taskCard, id);
        }
        public void UpdateTransaction()
        {
            transactionTypeService.UpdateTransaction(taskCard);
        }
        public object GetTransaction()
        {
            return (transactionTypeService == null) ? null : transactionTypeService.GetTransaction(id);
        }
    }
}
