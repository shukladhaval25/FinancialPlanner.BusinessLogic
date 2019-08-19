using FinancialPlanner.Common.Model.TaskManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialPlanner.BusinessLogic.TaskManagements
{
    internal interface ITransactionTypeService
    {
        void SaveTransaction(TaskCard taskCard, int id);
    }
}
