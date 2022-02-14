using FinancialPlanner.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Reflection;

namespace FinancialPlanner.BusinessLogic.Clients
{
    public class FeesInvoiceService
    {
        private const string GET_CLIENT_NAME_QUERY = "SELECT NAME FROM CLIENT WHERE ID = {0}";

        private const string GET_MAX_INVOICEID = "select Max(InvoiceId)  from FeesInvoice where InvoiceId like '{0}%'";



        private const string SELECT_INVOICE_DETAILS_BY = "SELECT * FROM FeesInvoiceDetails WHERE INVOICEID = '{0}'";
        private readonly string DELETE_FEESINVOICEDETAILS_BY_ID = "DELETE FROM FEESINVOICEDETAILS WHERE ID = {0}";
        private readonly string DELETE_FEESINVOICEDETAILS_BY_INVOICEID = "DELETE FROM FEESINVOICEDETAILS WHERE INVOICEID = '{0}'";
        private const string INSERT_FEES_INVOICE_DETAILS = "INSERT INTO FEESINVOICEDETAILS VALUES ('{0}','{1}',{2})";
        private const string UPDATE_FEES_INVOICE_DETAILS = "UPDATE FEESINVOICEDETAILS SET InvoiceId ='{0}', Particulars ='{1}', Amount = {2} where id = {3}";


        private const string SELECT_INVOICE_BY_CLIENT_ID = "select * from FeesInvoice where CId = {0}";
        private readonly string INSERT_FEES_INVOICE = "INSERT INTO FEESINVOICE VALUES ('{0}','{1}',{2})";   
        private const string UPDATE_FEES_INVOICE = "UPDATE FEESINVOICE SET INVOICEDATE = '{0}' WHERE INVOICEID = '{1}'";
        private const string DELETE_FEES_INVOICE = "DELETE FROM FEESINVOICE WHERE INVOICEID = '{0}'";

        public string GetMaxInvoiceId(string year)
        {
            string maxId = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_MAX_INVOICEID, year));
            if (string.IsNullOrEmpty(maxId))
            {
                return string.Format("{0}/1", year);
            }
            else
            {
                int number = int.Parse(maxId.Substring(maxId.LastIndexOf("/") + 1));
                return string.Format("{0}/{1}", year,number  + 1);
            }
        }
       

        public List<FeesInvoiceTransacation> GetFeesInvoice(int clientId)
        {
            List<FeesInvoiceTransacation> mOMTransactions = new List<FeesInvoiceTransacation>();
            try
            {
                Logger.LogInfo("Get: Fees Invoice information process start");

                DataTable dtfeesInvoice = DataBase.DBService.ExecuteCommand(string.Format(SELECT_INVOICE_BY_CLIENT_ID, clientId));
               
                foreach (DataRow dr in dtfeesInvoice.Rows)
                {
                    DataTable dtFeesInvoiceDetails = DataBase.DBService.ExecuteCommand(string.Format(SELECT_INVOICE_DETAILS_BY, dr["InvoiceId"].ToString()));
                    FeesInvoiceTransacation mOMTransaction = convertToFeesInvoiceTransactionObject(dr, dtFeesInvoiceDetails);
                    mOMTransactions.Add(mOMTransaction);
                }
                Logger.LogInfo("Get: Fees invoice details information process completed.");
                return mOMTransactions;
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

        public void Add(FeesInvoiceTransacation feesInvoiceTransacation)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY, feesInvoiceTransacation.CId));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(INSERT_FEES_INVOICE,
                    feesInvoiceTransacation.InvoiceNo,
                    feesInvoiceTransacation.InvoiceDate.ToString("yyyy-MM-dd"),
                    feesInvoiceTransacation.CId), true);


                    foreach (FeesInvoiceDetail point in feesInvoiceTransacation.feesInvoiceDetails)
                    {
                        DataBase.DBService.ExecuteCommandString(string.Format(INSERT_FEES_INVOICE_DETAILS,
                        point.InvoiceNo,
                        point.Particulars,
                        point.Amount), true);
                    }
                
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

        public void Delete(string invoiceId)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY, 0));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(DELETE_FEES_INVOICE,
                    invoiceId), true);
                DataBase.DBService.ExecuteCommandString(string.Format(DELETE_FEESINVOICEDETAILS_BY_INVOICEID,
                    invoiceId), true);
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

        public void Update(FeesInvoiceTransacation feesInvoiceTransacation)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY,feesInvoiceTransacation.CId));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(UPDATE_FEES_INVOICE,
                    feesInvoiceTransacation.InvoiceDate.ToString("yyyy-MM-dd"),
                    feesInvoiceTransacation.CId,
                    feesInvoiceTransacation.InvoiceNo), true);



                foreach (FeesInvoiceDetail point in feesInvoiceTransacation.feesInvoiceDetails)
                {
                    if (point.Id == 0)
                    {
                        DataBase.DBService.ExecuteCommandString(string.Format(INSERT_FEES_INVOICE_DETAILS,
                        point.InvoiceNo,
                        point.Particulars,
                        point.Amount), true);
                    }
                    else
                    {
                        DataBase.DBService.ExecuteCommandString(string.Format(UPDATE_FEES_INVOICE_DETAILS,
                        point.InvoiceNo,
                        point.Particulars,
                        point.Amount,
                        point.Id), true);
                    }
                }

                DataBase.DBService.CommitTransaction();
                //return mid;

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

        public void DeleteFeesInvoiceDetails(int id)
        {
            try
            {
                DataBase.DBService.ExecuteCommand(string.Format(DELETE_FEESINVOICEDETAILS_BY_ID, id));
                //Activity.ActivitiesService.Add(ActivityType.DeleteMOMPoint, EntryStatus.Success,
                //         Source.Server, company.UpdatedByUserName, company.Name, company.MachineName);
            }
            catch (Exception ex)
            {
                StackTrace st = new StackTrace();
                StackFrame sf = st.GetFrame(0);
                MethodBase currentMethodName = sf.GetMethod();
                LogDebug(currentMethodName.Name, ex);
                throw ex;
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

        private FeesInvoiceTransacation convertToFeesInvoiceTransactionObject(DataRow dr, DataTable dtFeesInvoiceDetails)
        {
            FeesInvoiceTransacation feesInvoiceTransacation = new FeesInvoiceTransacation();
            feesInvoiceTransacation.InvoiceNo = dr["InvoiceId"].ToString();
            feesInvoiceTransacation.InvoiceDate = DateTime.Parse(dr["InvoiceDate"].ToString());
            feesInvoiceTransacation.CId = int.Parse(dr["CId"].ToString());

            feesInvoiceTransacation.feesInvoiceDetails = new List<FeesInvoiceDetail>();
            foreach (DataRow dataRow in dtFeesInvoiceDetails.Rows)
            {
                FeesInvoiceDetail  feesInvoiceDetail = new FeesInvoiceDetail();
                feesInvoiceDetail.Id = int.Parse(dataRow["Id"].ToString());
                feesInvoiceDetail.InvoiceNo = dataRow.Field<string>("InvoiceId");
                feesInvoiceDetail.Particulars  = dataRow.Field<string>("Particulars");
                feesInvoiceDetail.Amount = double.Parse(dataRow["Amount"].ToString());
                feesInvoiceTransacation.feesInvoiceDetails.Add(feesInvoiceDetail);
            }

            return feesInvoiceTransacation;
        }
    }
}
