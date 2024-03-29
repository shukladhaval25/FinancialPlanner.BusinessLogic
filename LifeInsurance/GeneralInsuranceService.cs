﻿using FinancialPlanner.Common;
using FinancialPlanner.Common.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Reflection;

namespace FinancialPlanner.BusinessLogic.LifeInsurance
{
    public class GeneralInsuranceService
    {
        const string GET_CLIENT_NAME_QUERY = "SELECT C.NAME FROM CLIENT C, PLANNER P  WHERE P.CLIENTID = C.ID AND P.ID = {0}";
        const string SELECT_ALL = "SELECT N1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM GENERALINSURANCE N1, USERS U WHERE N1.UPDATEDBY = U.ID AND N1.PID = {0}";
        const string SELECT_BY_ID = "SELECT N1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM GENERALINSURANCE N1, USERS U WHERE N1.UPDATEDBY = U.ID AND N1.ID ={0} AND  N1.PID = {1}";
        const string INSERT_LIFE_INSURANCE = "INSERT INTO GENERALINSURANCE VALUES (" +
            "{0},'{1}','{2}',{3}," +
            "'{4}','{5}','{6}','{7}','{8}'," +
            "{9},{10},{11},'{12}', " +
            "'{13}','{14}',{15},'{16}',{17},{18})";
        const string UPDATE_LIFE_INSURANCE = "UPDATE GENERALINSURANCE SET " +
            "[Applicant] = '{0}', [ISSUEDATE] ='{1}', [TERMSINYEARS] ={2},[RenewalDate] = '{3}', " +
            "[PolicyNo] ='{4}',[Company] ='{5}',[Policy] ='{6}',[Type] ='{7}'," +
            "[SumAssured]= {8},[Bonus] = {9}, [Premium] = {10}," +
            "[Remark] = '{11}', " +
            "[AttachmentPath] = '{12}', [UpdatedOn] = '{13}', [UpdatedBy] ={14}, [SetReminder] = {15}" +
            "WHERE ID = {16} AND PID = {17}";

        const string DELETE_LIFE_INSURNACE = "DELETE FROM GENERALINSURANCE WHERE ID = {0} AND PID ={1}";

        const string SELECT_RENEWAL_REMINDER = "SELECT Client.Name, GeneralInsurance.Applicant,     GeneralInsurance.Company, GeneralInsurance.Policy, GeneralInsurance.RenewalDate, GeneralInsurance.Premium, GeneralInsurance.PolicyNo FROM Planner INNER JOIN Client ON Planner.ClientId = Client.ID INNER JOIN GeneralInsurance ON Planner.ID = GeneralInsurance.PID AND Planner.ID = GeneralInsurance.PID AND Planner.ID = GeneralInsurance.PID WHERE (GeneralInsurance.RenewalDate BETWEEN '{0}' AND '{1}' AND GeneralInsurance.SetReminder = 1)";

        public IList<Common.Model.CurrentStatus.GeneralInsurance> GetAll(int plannerId)
        {
            try
            {
                Logger.LogInfo("Get: General insurance process start");
                IList<Common.Model.CurrentStatus.GeneralInsurance> lstGeneralInsurance =
                    new List<Common.Model.CurrentStatus.GeneralInsurance>();

                DataTable dtAppConfig = DataBase.DBService.ExecuteCommand(string.Format(SELECT_ALL, plannerId));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    Common.Model.CurrentStatus.GeneralInsurance GeneralInsurance = convertToGeneralInsuranceObject(dr);
                    lstGeneralInsurance.Add(GeneralInsurance);
                }
                Logger.LogInfo("Get: General insurance process completed.");
                return lstGeneralInsurance;
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

        public IList<GeneralInsuranceRenewalReminder> GetRenewalReminder(DateTime fromDate, DateTime toDate)
        {
            try
            {
                Logger.LogInfo("Get: General insurance premium reminder process start");
                IList<GeneralInsuranceRenewalReminder> lstGeneralInsurance =
                    new List<GeneralInsuranceRenewalReminder>();

                DataTable dtAppConfig = DataBase.DBService.ExecuteCommand(string.Format(SELECT_RENEWAL_REMINDER, fromDate.ToString("yyyy-MM-dd"), toDate.ToString("yyyy-MM-dd")));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    GeneralInsuranceRenewalReminder GeneralInsurance = convertToGeneralInsurancePremiumReminderObject(dr);
                    lstGeneralInsurance.Add(GeneralInsurance);
                }
                Logger.LogInfo("Get: General insurance premium reminder process completed.");
                return lstGeneralInsurance;
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

        private GeneralInsuranceRenewalReminder convertToGeneralInsurancePremiumReminderObject(DataRow dr)
        {
            GeneralInsuranceRenewalReminder generalInsurancePremiumReminder = new GeneralInsuranceRenewalReminder();
            generalInsurancePremiumReminder.Applicant = dr.Field<string>("Applicant");
            generalInsurancePremiumReminder.ClientName = dr.Field<string>("Name");
            generalInsurancePremiumReminder.Company = dr.Field<string>("Company");
            generalInsurancePremiumReminder.PolicyName = dr.Field<string>("Policy");
            generalInsurancePremiumReminder.PolicyNo = dr.Field<string>("PolicyNo");
            DateTime renewalDate;

            if (DateTime.TryParse(dr["RenewalDate"].ToString(), out renewalDate))
                generalInsurancePremiumReminder.RenewalDate = renewalDate;

            generalInsurancePremiumReminder.PremiumAmount = Double.Parse(dr["Premium"].ToString()); //Double.Parse(dr["Balance"].ToString());

            return generalInsurancePremiumReminder;
        }

        public Common.Model.CurrentStatus.GeneralInsurance GetById(int id, int plannerId)
        {
            try
            {
                Logger.LogInfo("Get: General insurance by id process start");
                Common.Model.CurrentStatus.GeneralInsurance GeneralInsurance =
                    new Common.Model.CurrentStatus.GeneralInsurance();

                DataTable dtAppConfig = DataBase.DBService.ExecuteCommand(string.Format(SELECT_BY_ID, id, plannerId));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    GeneralInsurance = convertToGeneralInsuranceObject(dr);
                }
                Logger.LogInfo("Get: General insurance by id process completed.");
                return GeneralInsurance;
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

        public void Add(Common.Model.CurrentStatus.GeneralInsurance GeneralInsurance)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY, GeneralInsurance.Pid));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(INSERT_LIFE_INSURANCE,
                      GeneralInsurance.Pid, GeneralInsurance.Applicant,
                      (GeneralInsurance.IssueDate != null) ? GeneralInsurance.IssueDate.Value.ToString("yyyy-MM-dd hh:mm:ss") : null,
                      GeneralInsurance.TermsInYears,
                      (GeneralInsurance.RenewalDate != null) ? GeneralInsurance.RenewalDate.Value.ToString("yyyy-MM-dd hh:mm:ss") : null,
                      GeneralInsurance.PolicyNo, GeneralInsurance.Company, GeneralInsurance.Policy,
                      GeneralInsurance.Type, GeneralInsurance.SumAssured, GeneralInsurance.Bonus,
                      GeneralInsurance.Premium,
                      GeneralInsurance.Remark, GeneralInsurance.AttachmentPath,
                      GeneralInsurance.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss"), GeneralInsurance.CreatedBy,
                      GeneralInsurance.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), GeneralInsurance.UpdatedBy,
                        (GeneralInsurance.SetReminder == true) ? 1 : 0), true);

                Activity.ActivitiesService.Add(ActivityType.CreateGeneralInsurance, EntryStatus.Success,
                         Source.Server, GeneralInsurance.UpdatedByUserName, clientName, GeneralInsurance.MachineName);
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

        public void Update(Common.Model.CurrentStatus.GeneralInsurance GeneralInsurance)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY, GeneralInsurance.Pid));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(UPDATE_LIFE_INSURANCE,
                      GeneralInsurance.Applicant,
                      (GeneralInsurance.IssueDate != null) ? GeneralInsurance.IssueDate.Value.ToString("yyyy-MM-dd hh:mm:ss") : null,
                      GeneralInsurance.TermsInYears,
                      (GeneralInsurance.RenewalDate != null) ? GeneralInsurance.RenewalDate.Value.ToString("yyyy-MM-dd hh:mm:ss") : null,
                      GeneralInsurance.PolicyNo, GeneralInsurance.Company, GeneralInsurance.Policy,
                      GeneralInsurance.Type,
                      GeneralInsurance.SumAssured, GeneralInsurance.Bonus, GeneralInsurance.Premium,
                      GeneralInsurance.Remark,
                      GeneralInsurance.AttachmentPath,
                      GeneralInsurance.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"),
                      GeneralInsurance.UpdatedBy,
                      (GeneralInsurance.SetReminder == true) ? 1 : 0,
                      GeneralInsurance.Id, GeneralInsurance.Pid), true);

                Activity.ActivitiesService.Add(ActivityType.UpdateGeneralInsurance, EntryStatus.Success,
                         Source.Server, GeneralInsurance.UpdatedByUserName, clientName, GeneralInsurance.MachineName);
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

        public void Delete(Common.Model.CurrentStatus.GeneralInsurance GeneralInsurance)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY, GeneralInsurance.Pid));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(DELETE_LIFE_INSURNACE,
                     GeneralInsurance.Id, GeneralInsurance.Pid), true);

                Activity.ActivitiesService.Add(ActivityType.DeleteGeneralInsurance, EntryStatus.Success,
                         Source.Server, GeneralInsurance.UpdatedByUserName, clientName, GeneralInsurance.MachineName);
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

        private Common.Model.CurrentStatus.GeneralInsurance convertToGeneralInsuranceObject(DataRow dr)
        {
            Common.Model.CurrentStatus.GeneralInsurance GeneralInsurance = new Common.Model.CurrentStatus.GeneralInsurance();
            GeneralInsurance.Id = dr.Field<int>("ID");
            GeneralInsurance.Pid = dr.Field<int>("PID");
            GeneralInsurance.Applicant = dr.Field<string>("Applicant");
            GeneralInsurance.IssueDate = dr.Field<DateTime>("IssueDate");
            GeneralInsurance.TermsInYears = dr.Field<int>("TermsInYears");
            GeneralInsurance.RenewalDate = dr.Field<DateTime>("RenewalDate");
            GeneralInsurance.PolicyNo = dr.Field<string>("PolicyNo");
            GeneralInsurance.Company = dr.Field<string>("Company");
            GeneralInsurance.Policy = dr.Field<string>("Policy");
            GeneralInsurance.Type = dr.Field<string>("Type");
            GeneralInsurance.SumAssured = double.Parse(dr["SumAssured"].ToString());
            GeneralInsurance.Bonus = double.Parse(dr["Bonus"].ToString());
            GeneralInsurance.Premium = double.Parse(dr["Premium"].ToString());
            GeneralInsurance.Remark = dr.Field<string>("Remark");
            GeneralInsurance.AttachmentPath = dr.Field<string>("AttachmentPath");
            GeneralInsurance.UpdatedBy = dr.Field<int>("UpdatedBy");
            GeneralInsurance.UpdatedOn = dr.Field<DateTime>("UpdatedOn");
            GeneralInsurance.UpdatedByUserName = dr.Field<string>("UpdatedByUserName");
            GeneralInsurance.SetReminder =(dr["SetReminder"] == DBNull.Value) ? false : bool.Parse(dr["SetReminder"].ToString());
            return GeneralInsurance;
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
