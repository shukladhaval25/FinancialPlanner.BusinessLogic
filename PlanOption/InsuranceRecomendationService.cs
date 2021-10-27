using FinancialPlanner.Common;
using FinancialPlanner.Common.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Reflection;

namespace FinancialPlanner.BusinessLogic.PlanOption
{
    public class InsuranceRecomendationService
    {
        private const string GET_CLIENT_NAME_QUERY = "SELECT NAME FROM CLIENT WHERE ID = {0}";
        private const string GET_SPOUSE_NAME_QUERY = "SELECT NAME FROM ClientSpouse WHERE Id = {0}";

        private const string GET_INSURANCE_COMPANY_NAME_QUERY = "SELECT NAME FROM INSURANCECOMPANY WHERE Id = {0}";

        private const string SELECT_INSURANCE_REC_DETAILS_BY = "SELECT * FROM InsuranceRecDetail WHERE InsuranceRecMasterId = {0}";
        private const string INSERT_INSURANCE_REC_DETAIL = "INSERT INTO INSURANCERECDETAIL VALUES ({0},'{1}','{2}','{3}',{4})";
        private const string UPDATE_INSURANCE_REC_DETAIL = "UPDATE INSURANCERECDETAIL SET InsuranceCompanyName = '{0}', Term = '{1}',SumAssured ='{2}', Premium = {3} WHERE  InsuranceRecMasterId = {4})";
        private const string DELETE_INSURANCE_REC_DETAIL_BY_NAME_ID = "DELETE FROM INSURANCERECDETAIL WHERE InsuranceCompanyName = '{0}' AND InsuranceRecMasterId ={1}";
        private const string DELETE_INSURANCE_REC_DETAIL_ID = "DELETE FROM INSURANCERECDETAIL WHERE InsuranceRecMasterId ={0}";


        private const string SELECT_INSURANCE_REC_MASTER_BY_INFORAMTION = "SELECT MAX(ID) FROM INSURANCERECMASTER";

        private const string SELECT_INSURANCE_REC_BY_PLAN_ID = "select * from InsuranceRecMaster where PID = {0}";
        private const string INSERT_INSURANCE_REC = "INSERT INTO INSURANCERECMASTER VALUES ({0},{1},{2},'{3}','{4}')";
        private const string UPDATE_INSURANCE_REC = "UPDATE INSURANCERECMASTER SET CID = {0},SPOUSEID ={1},SUMASSURED = '{2}',DESCRIPTION = '{3}' WHERE ID = {4}";
        private const string DELETE_INSURANCE_REC_BY_ID = "DELETE FROM INSURANCERECMASTER WHERE ID = {0}";

        public List<InsuranceRecomendationTransaction> GetInsuranceRecomendationByProjectId(int projectId)
        {
            List<InsuranceRecomendationTransaction> insuranceRecomendations = new List<InsuranceRecomendationTransaction>();
            try
            {
                Logger.LogInfo("Get: Insurance Recomendation information process start");

                DataTable dtfeesInvoice = DataBase.DBService.ExecuteCommand(string.Format(SELECT_INSURANCE_REC_BY_PLAN_ID, projectId));

                foreach (DataRow dr in dtfeesInvoice.Rows)
                {
                    DataTable dtFeesInvoiceDetails = DataBase.DBService.ExecuteCommand(string.Format(SELECT_INSURANCE_REC_DETAILS_BY, dr["Id"].ToString()));
                    InsuranceRecomendationTransaction insuranceRecomendation = convertToInsuranceRecomendationTransactionObject(dr, dtFeesInvoiceDetails);
                    insuranceRecomendations.Add(insuranceRecomendation);
                }
                Logger.LogInfo("Get: Insurance Recomendation details information process completed.");
                return insuranceRecomendations;
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

        public void Add(InsuranceRecomendationTransaction insuranceRecomendationTransaction)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY, 0));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(INSERT_INSURANCE_REC,
                    insuranceRecomendationTransaction.PId,
                    insuranceRecomendationTransaction.CId == null? 0: insuranceRecomendationTransaction.CId,
                    insuranceRecomendationTransaction.SpouseId == null ? 0 : insuranceRecomendationTransaction.SpouseId,
                    insuranceRecomendationTransaction.SumAssured,
                    insuranceRecomendationTransaction.Description), true);

                int insturanceRecMasterId = getInsuraceRecMasterId(insuranceRecomendationTransaction);

                foreach (InsuranceRecomendationDetail insuranceRecomendationDetail in insuranceRecomendationTransaction.InsuranceRecomendationDetails)
                {
                    DataBase.DBService.ExecuteCommandString(string.Format(INSERT_INSURANCE_REC_DETAIL,
                    insturanceRecMasterId,
                    insuranceRecomendationDetail.InsuranceCompanyName,
                    insuranceRecomendationDetail.Term,
                    insuranceRecomendationDetail.SumAssured,
                    insuranceRecomendationDetail.Premium), true);
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

        public void Update(InsuranceRecomendationTransaction transaction)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY, 0));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(UPDATE_INSURANCE_REC,
                     (transaction.CId == null) ? 0 : transaction.CId,
                    transaction.SpouseId == null ? 0 : transaction.SpouseId,
                    transaction.SumAssured,
                    transaction.Description,
                    transaction.Id), true);



                foreach (InsuranceRecomendationDetail insuranceRecomendationDetail in transaction.InsuranceRecomendationDetails)
                {

                    DataBase.DBService.ExecuteCommandString(string.Format(DELETE_INSURANCE_REC_DETAIL_BY_NAME_ID, insuranceRecomendationDetail.InsuranceCompanyName, transaction.Id),true);

                    DataBase.DBService.ExecuteCommandString(string.Format(INSERT_INSURANCE_REC_DETAIL,
                    transaction.Id,
                    insuranceRecomendationDetail.InsuranceCompanyName,
                    insuranceRecomendationDetail.Term,
                    insuranceRecomendationDetail.SumAssured,
                    insuranceRecomendationDetail.Premium), true);
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

        private int getInsuraceRecMasterId(InsuranceRecomendationTransaction insuranceRecomendationTransaction)
        {
            return int.Parse(DataBase.DBService.ExecuteCommandScalar(SELECT_INSURANCE_REC_MASTER_BY_INFORAMTION));
        }

        private InsuranceRecomendationTransaction convertToInsuranceRecomendationTransactionObject(DataRow dr, DataTable dtFeesInvoiceDetails)
        {
            InsuranceRecomendationTransaction insuranceRecomendation = new InsuranceRecomendationTransaction();
            insuranceRecomendation.Id = int.Parse(dr["Id"].ToString());
            insuranceRecomendation.PId = int.Parse(dr["PId"].ToString());
            if (dr["CId"] == DBNull.Value )
            {
                insuranceRecomendation.CId = null;
            }
            else if (dr["CId"].ToString() != "0")
            {
                insuranceRecomendation.CId = int.Parse(dr["CId"].ToString());
                insuranceRecomendation.Name = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY, insuranceRecomendation.CId));
            }
            else
            {
                insuranceRecomendation.CId = int.Parse(dr["CId"].ToString());
            }

            if (dr["SpouseId"] == DBNull.Value)
            {
                insuranceRecomendation.SpouseId = null;
            }
            else if (int.Parse(dr["SpouseId"].ToString()) > 0)
            {
                insuranceRecomendation.SpouseId = int.Parse(dr["SpouseId"].ToString());
                insuranceRecomendation.Name = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_SPOUSE_NAME_QUERY, insuranceRecomendation.SpouseId));
            }
            else
            {
                insuranceRecomendation.SpouseId = int.Parse(dr["SpouseId"].ToString());
            }


            insuranceRecomendation.SumAssured = dr["SumAssured"] == DBNull.Value ? "" : dr["SumAssured"].ToString();
            insuranceRecomendation.Description = dr["Description"] == DBNull.Value ? "" : dr["Description"].ToString();

            insuranceRecomendation.InsuranceRecomendationDetails = new List<InsuranceRecomendationDetail>();
            foreach (DataRow dataRow in dtFeesInvoiceDetails.Rows)
            {
                InsuranceRecomendationDetail insuranceRecomendationDetail = new InsuranceRecomendationDetail();
                insuranceRecomendationDetail.InsRecMasterId = int.Parse(dataRow["InsuranceRecMasterId"].ToString());
                //insuranceRecomendationDetail.InsuranceCompayId = int.Parse(dataRow["InsuranceCompanyId"].ToString());
                insuranceRecomendationDetail.InsuranceCompanyName = dataRow.Field<string>("InsuranceCompanyName");

                insuranceRecomendationDetail.Term = dataRow.Field<string>("Term");
                insuranceRecomendationDetail.SumAssured = dataRow.Field<string>("SumAssured");

                insuranceRecomendationDetail.Premium = dataRow["Premium"] == DBNull.Value ? 0 : double.Parse(dataRow["Premium"].ToString());

                insuranceRecomendation.InsuranceRecomendationDetails.Add(insuranceRecomendationDetail);
            }

            return insuranceRecomendation;
        }

        private void LogDebug(string methodName, Exception ex)
        {
            DebuggerLogInfo debuggerInfo = new DebuggerLogInfo();
            debuggerInfo.ClassName = this.GetType().Name;
            debuggerInfo.Method = methodName;
            debuggerInfo.ExceptionInfo = ex;
            Logger.LogDebug(debuggerInfo);
        }

        public void DeleteInsuranceRecDetail(string insuranceCompanyName, int id)
        {
            try
            {
                DataBase.DBService.ExecuteCommand(string.Format(DELETE_INSURANCE_REC_DETAIL_BY_NAME_ID, insuranceCompanyName, id));
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

        public void DeleteInsuranceRecomendation(int Id)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY, 0));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(DELETE_INSURANCE_REC_BY_ID,
                    Id), true);
                DataBase.DBService.ExecuteCommandString(string.Format(DELETE_INSURANCE_REC_DETAIL_ID,
                    Id), true);
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
    }
}
