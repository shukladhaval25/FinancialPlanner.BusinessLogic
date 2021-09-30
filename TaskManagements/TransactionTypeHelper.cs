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
                case "Redemption":
                    transactionTypeService = new FreshPurchaseTransactionServiceImpl();
                    break;
                case "New SIP":
                case "SIP Old":
                    transactionTypeService = new SIPServiceImpl();
                    break;
                case "Switch":
                    transactionTypeService = new SwitchTransaactionServiceImpl();
                    break;
                case "STP":
                    transactionTypeService = new STPTransactionServiceImpl();
                    break;
                case "SWP Pause":
                case "SWP":
                    transactionTypeService = new SWPTransactionServiceImpl();
                    break;
                case "STP Pause":
                case "STP Cancel":
                    transactionTypeService = new STPCancellationCancellationTransactionServiceImpl();
                    break;
                case "SIP Pause":
                case "SIP Cancel":
                    transactionTypeService = new SIPCancellationTransactionServiceImpl();
                    break;
                case "Bank Change Request":
                    transactionTypeService = new BankChangeRequestService();
                    break;
                case "Contact Update":
                    transactionTypeService = new ContactUpdateServiceImpl();
                    break;
                case "PAN Card Update":
                    transactionTypeService = new PANCardUpdateServiceImpl();
                    break;
                case "Address Change":
                    transactionTypeService = new AddressChangeServiceImpl();
                    break;
                case "Transmission After Death":
                    transactionTypeService = new TransmissionAfterDeathServiceImpl();
                    break;
                case "Signature Change":
                    transactionTypeService = new SignatureChangeServiceImpl();
                    break;
                case "SIP Bank Change":
                    transactionTypeService = new SIPBankChangeServiceImpl();
                    break;
                case "Minor To Major":
                    transactionTypeService = new MinorToMajorServiceImpl();
                    break;
                case "Change of Name":
                    transactionTypeService = new ChangeOfNameServiceImpl();
                    break;
                case "Nomination":
                    transactionTypeService = new NominationServiceImpl();
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
