using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FinancialPlanner.Common.Model.TaskManagement;
using FinancialPlanner.Common.Model.TaskManagement.MFTransactions;

namespace FinancialPlanner.BusinessLogic.TaskManagements
{
    public class SIPServiceImpl : ITransactionTypeService
    {
        private const string INSERT_SIP = "INSERT INTO SIP VALUES ({0},{1},'{2}','{3}','{4}','{5}','{6}'," +
            "'{7}','{8}',{9},'{10}',{11},'{12}',{13},'{14}','{15}','{16}','{17}','{18}')";
        SIP sip;
        public void SaveTransaction(TaskCard taskCard, int id)
        {
            sip = new FinancialPlanner.Common.JSONSerialization().DeserializeFromString<SIP>(taskCard.TaskTransactionType.ToString());
            sip.TaskId = id;
            DataBase.DBService.ExecuteCommandString(string.Format(INSERT_SIP,
                   sip.TaskId,
                   sip.CID,
                   sip.MemberName,
                   sip.SecondHolder,
                   sip.ThirdHolder,
                   sip.Nominee,
                   sip.Guardian,
                   sip.AMC,
                   sip.FolioNo,
                   sip.SchemeId,
                   sip.Option,
                   sip.Amount,
                   sip.AccounType,
                   sip.SIPDayOn,
                   sip.TransactionDate,
                   sip.SIPStartDate,
                   sip.SIPEndDate,
                   sip.ModeOfExecution,
                   sip.Remark), true);
        }
    }
}
