using FinancialPlanner.Common;
using FinancialPlanner.Common.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Reflection;

namespace FinancialPlanner.BusinessLogic.LifeInsurance
{
    public class LifeInsuranceService
    {
        const string GET_CLIENT_NAME_QUERY = "SELECT C.NAME FROM CLIENT C, PLANNER P  WHERE P.CLIENTID = C.ID AND P.ID = {0}";
        const string SELECT_ALL = "SELECT N1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM LIFEINSURANCE N1, USERS U WHERE N1.UPDATEDBY = U.ID AND N1.PID = {0}";
        const string SELECT_BY_ID = "SELECT N1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM LIFEINSURANCE N1, USERS U WHERE N1.UPDATEDBY = U.ID AND N1.ID ={0} AND  N1.PID = {1}";
        const string INSERT_LIFE_INSURANCE = "INSERT INTO LIFEINSURANCE VALUES (" +
            "{0},'{1}','{2}','{3}'," +
            "'{4}','{5}','{6}','{7}',{8}," +
            "{9},'{10}',{11},'{12}', " +
            "'{13}','{14}','{15}',{16}," +
            "'{17}','{18}','{19}','{20}'," +
            "{21},'{22}','{23}','{24}'," +
            "{25},{26},'{27}',{28}," +
            "'{29}',{30},'{31}','{32}'," +
            "'{33}','{34}',{35},'{36}',{37},'{38}',{39})";



        const string UPDATE_LIFE_INSURANCE = "UPDATE LIFEINSURANCE SET " +
            "[Applicant] = '{0}', [Branch] ='{1}', [DateOfIssue] ='{2}',[MaturityDate] = '{3}', " +
            "[Company] = '{4}',[PolicyName] = '{5}',[PolicyNo] ='{6}',[Premium] = {7}," +
            "[Terms] ='{8}', [PremiumPayTerm] = '{9}' ,[SumAssured] ={10},[Status] = '{11}'," +
            "[ModeOfPayment] = '{12}',[Moneyback] = '{13}',[NextPremDate] = '{14}',[AccidentalDeathBenefit] = {15}," +
            "[Type] = '{16}', [Appointee] ='{17}',[Nominee] = '{18}', [Relation] ='{19}'," +
            "[LoanTaken] = {20},[LoanDate] ='{21}',[BalanceUnit] = '{22}', [AsOnDate] = '{23}'," +
            "[CurrentValue] = {24},[ExpectedMaturityValue] = '{25}', [Ridder1] = '{26}', " +
            "[Ridder1Amount] = {27}, [Ridder2] = '{28}', [Ridder2Amount] = {29}, [Remarks] = '{30}', " +
            "[AttachmentPath] = '{31}', [UpdatedOn] = '{32}', [UpdatedBy] ={33},[Agent] ='{34}', [LastPremiumDate] ='{37}', " +
            "[SetReminder] = {38} " +
            "WHERE ID = {35} AND PID = {36}";

        const string SELECT_PREMIUM_DATE = "SELECT LifeInsurance.Applicant, Client.Name, LifeInsurance.Company, LifeInsurance.PolicyName, LifeInsurance.PolicyNo, CONVERT(varchar, LifeInsurance.NextPremDate, 103)  As NextPremDate,LifeInsurance.Premium FROM LifeInsurance INNER JOIN Planner ON LifeInsurance.PID = Planner.ID INNER JOIN Client ON Planner.ClientId = Client.ID  WHERE (LifeInsurance.NextPremDate BETWEEN '{0}' AND '{1}')";

        const string SELECT_MATURITY_DATE = "SELECT LifeInsurance.Applicant, Client.Name, LifeInsurance.Company, LifeInsurance.PolicyName, LifeInsurance.PolicyNo, CONVERT(varchar, LifeInsurance.MaturityDate, 103)  As NextPremDate,LifeInsurance.ExpectedMaturityValue As Premium FROM LifeInsurance INNER JOIN Planner ON LifeInsurance.PID = Planner.ID INNER JOIN Client ON Planner.ClientId = Client.ID WHERE (LifeInsurance.MaturityDate BETWEEN '{0}' AND '{1}' and LifeInsurance.SetReminder = 1)";

        public IList<LicPremiumReminder> GetByPremiumdate(DateTime fromDate, DateTime toDate)
        {
            try
            {
                Logger.LogInfo("Get: Life insurance premium date process start");
                IList<LicPremiumReminder> lstLifeInsurance = new List<LicPremiumReminder>();

                DataTable dtAppConfig = DataBase.DBService.ExecuteCommand(string.Format(SELECT_PREMIUM_DATE, fromDate.ToString("yyyy-MM-dd"), toDate.ToString("yyyy-MM-dd")));

                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    LicPremiumReminder lifeInsurance = convertToLicPremiumReminder(dr);
                    lstLifeInsurance.Add(lifeInsurance);
                }
                Logger.LogInfo("Get: Life insurance premium process completed.");
                return lstLifeInsurance;
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

        public IList<LicPremiumReminder> GetLICPolicyMaturity(DateTime fromDate, DateTime toDate)
        {
            try
            {
                Logger.LogInfo("Get: Life insurance premium date process start");
                IList<LicPremiumReminder> lstLifeInsurance = new List<LicPremiumReminder>();

                DataTable dtAppConfig = DataBase.DBService.ExecuteCommand(string.Format(SELECT_MATURITY_DATE, fromDate.ToString("yyyy-MM-dd"), toDate.ToString("yyyy-MM-dd")));

                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    LicPremiumReminder lifeInsurance = convertToLicPremiumReminder(dr);
                    lstLifeInsurance.Add(lifeInsurance);
                }
                Logger.LogInfo("Get: Life insurance premium process completed.");
                return lstLifeInsurance;
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

        private LicPremiumReminder convertToLicPremiumReminder(DataRow dr)
        {
            LicPremiumReminder licPremiumReminder = new LicPremiumReminder();
            licPremiumReminder.Applicant = dr.Field<string>("Applicant");
            licPremiumReminder.ClientName = dr.Field<string>("Name");
            licPremiumReminder.Company = dr.Field<string>("Company");
            licPremiumReminder.PolicyName = dr.Field<string>("PolicyName");
            licPremiumReminder.PolicyNo = dr.Field<string>("PolicyNo");
            licPremiumReminder.PremiumDate = DateTime.Parse(dr.Field<string>("NextPremDate"));
            licPremiumReminder.PremiumAmount = Double.Parse(dr["Premium"].ToString()); //Double.Parse(dr["Balance"].ToString());

            return licPremiumReminder;
        }

        const string DELETE_LIFE_INSURNACE = "DELETE FROM LIFEINSURANCE WHERE ID = {0} AND PID {1}";

        public IList<Common.Model.CurrentStatus.LifeInsurance> GetAll(int plannerId)
        {
            try
            {
                Logger.LogInfo("Get: Life insurance process start");
                IList<Common.Model.CurrentStatus.LifeInsurance> lstLifeInsurance =
                    new List<Common.Model.CurrentStatus.LifeInsurance>();

                DataTable dtAppConfig = DataBase.DBService.ExecuteCommand(string.Format(SELECT_ALL, plannerId));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    Common.Model.CurrentStatus.LifeInsurance lifeInsurance = convertToLifeInsuranceObject(dr);
                    lstLifeInsurance.Add(lifeInsurance);
                }
                Logger.LogInfo("Get: Life insurance process completed.");
                return lstLifeInsurance;
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

        public Common.Model.CurrentStatus.LifeInsurance GetById(int id, int plannerId)
        {
            try
            {
                Logger.LogInfo("Get: Life insurance by id process start");
                Common.Model.CurrentStatus.LifeInsurance lifeInsurance =
                    new Common.Model.CurrentStatus.LifeInsurance();

                DataTable dtAppConfig = DataBase.DBService.ExecuteCommand(string.Format(SELECT_BY_ID, id, plannerId));
                foreach (DataRow dr in dtAppConfig.Rows)
                {
                    lifeInsurance = convertToLifeInsuranceObject(dr);
                }
                Logger.LogInfo("Get: Life insurance by id process completed.");
                return lifeInsurance;
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

        public void Add(Common.Model.CurrentStatus.LifeInsurance lifeInsurance)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY, lifeInsurance.Pid));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(INSERT_LIFE_INSURANCE,
                      lifeInsurance.Pid, lifeInsurance.Applicant, lifeInsurance.Branch, lifeInsurance.DateOfIssue.ToString("yyyy-MM-dd hh:mm:ss"),
                      lifeInsurance.MaturityDate.ToString("yyyy-MM-dd"), lifeInsurance.Company, lifeInsurance.PolicyName, lifeInsurance.PolicyNo, lifeInsurance.Premium,
                      lifeInsurance.Terms, lifeInsurance.PremiumPayTerm, lifeInsurance.SumAssured, lifeInsurance.Status,
                      lifeInsurance.ModeOfPayment, lifeInsurance.Moneyback,
                      (lifeInsurance.NextPremDate == null) ? null : lifeInsurance.NextPremDate.Value.ToString("yyyy-MM-dd hh:mm:ss"), lifeInsurance.AccidentalDeathBenefit,
                      lifeInsurance.Type, lifeInsurance.Appointee, lifeInsurance.Nominee, lifeInsurance.Relation,
                      lifeInsurance.LoanTaken,
                      (lifeInsurance.LoanDate == null) ? null : lifeInsurance.LoanDate.Value.ToString("yyyy-MM-dd hh:mm:ss"),
                      lifeInsurance.BalanceUnit,
                      (lifeInsurance.AsOnDate == null) ? null : lifeInsurance.AsOnDate.Value.ToString("yyyy-MM-dd hh:mm:ss"),
                      lifeInsurance.CurrentValue, lifeInsurance.ExpectedMaturityValue, lifeInsurance.Rider1, lifeInsurance.Rider1Amount,
                      lifeInsurance.Rider2, lifeInsurance.Rider2Amount, lifeInsurance.Remarks, lifeInsurance.AttachmentPath,
                      lifeInsurance.Agent,
                      lifeInsurance.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss"), lifeInsurance.CreatedBy,
                      lifeInsurance.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), lifeInsurance.UpdatedBy,
                      lifeInsurance.LastPremiumDate,
                      (lifeInsurance.SetReminder == true) ? 1 : 0), true);

                Activity.ActivitiesService.Add(ActivityType.CreateLifeInsurance, EntryStatus.Success,
                         Source.Server, lifeInsurance.UpdatedByUserName, clientName, lifeInsurance.MachineName);
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

        public void Update(Common.Model.CurrentStatus.LifeInsurance lifeInsurance)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY, lifeInsurance.Pid));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(UPDATE_LIFE_INSURANCE,
                      lifeInsurance.Applicant,
                      lifeInsurance.Branch, lifeInsurance.DateOfIssue.ToString("yyyy-MM-dd hh:mm:ss"), lifeInsurance.MaturityDate.ToString("yyyy-MM-dd hh:mm:ss"),
                      lifeInsurance.Company, lifeInsurance.PolicyName, lifeInsurance.PolicyNo, lifeInsurance.Premium,
                      lifeInsurance.Terms, lifeInsurance.PremiumPayTerm, lifeInsurance.SumAssured, lifeInsurance.Status,
                      lifeInsurance.ModeOfPayment, lifeInsurance.Moneyback,
                      (lifeInsurance.NextPremDate == null) ? null : lifeInsurance.NextPremDate.Value.ToString("yyyy-MM-dd hh:mm:ss"),
                      lifeInsurance.AccidentalDeathBenefit,
                      lifeInsurance.Type, lifeInsurance.Appointee, lifeInsurance.Nominee, lifeInsurance.Relation,
                      lifeInsurance.LoanTaken,
                      (lifeInsurance.LoanDate == null) ? null : lifeInsurance.LoanDate.Value.ToString("yyyy-MM-dd hh:mm:ss"),
                      lifeInsurance.BalanceUnit,
                      (lifeInsurance.AsOnDate == null) ? null : lifeInsurance.AsOnDate.Value.ToString("yyyy-MM-dd hh:mm:ss"),
                      lifeInsurance.CurrentValue, lifeInsurance.ExpectedMaturityValue, lifeInsurance.Rider1,
                      lifeInsurance.Rider1Amount, lifeInsurance.Rider2, lifeInsurance.Rider2Amount, lifeInsurance.Remarks,
                      lifeInsurance.AttachmentPath,
                      lifeInsurance.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), lifeInsurance.UpdatedBy, lifeInsurance.Agent, lifeInsurance.Id, lifeInsurance.Pid,
                      lifeInsurance.LastPremiumDate,
                       (lifeInsurance.SetReminder == true) ? 1 : 0), true);

                Activity.ActivitiesService.Add(ActivityType.UpdateLifeInsurance, EntryStatus.Success,
                         Source.Server, lifeInsurance.UpdatedByUserName, clientName, lifeInsurance.MachineName);
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

        public void Delete(Common.Model.CurrentStatus.LifeInsurance lifeInsurance)
        {
            try
            {
                string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY, lifeInsurance.Pid));

                DataBase.DBService.BeginTransaction();
                DataBase.DBService.ExecuteCommandString(string.Format(DELETE_LIFE_INSURNACE,
                     lifeInsurance.Id, lifeInsurance.Pid), true);

                Activity.ActivitiesService.Add(ActivityType.DeleteLifeInsurance, EntryStatus.Success,
                         Source.Server, lifeInsurance.UpdatedByUserName, clientName, lifeInsurance.MachineName);
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

        private Common.Model.CurrentStatus.LifeInsurance convertToLifeInsuranceObject(DataRow dr)
        {
            Common.Model.CurrentStatus.LifeInsurance lifeInsurance = new Common.Model.CurrentStatus.LifeInsurance();
            lifeInsurance.Id = dr.Field<int>("ID");
            lifeInsurance.Pid = dr.Field<int>("PID");
            lifeInsurance.Applicant = dr.Field<string>("Applicant");
            lifeInsurance.Branch = dr.Field<string>("Branch");
            lifeInsurance.Agent = dr.Field<string>("Agent");
            lifeInsurance.DateOfIssue = dr.Field<DateTime>("DateOfIssue");
            lifeInsurance.MaturityDate = dr.Field<DateTime>("MaturityDate");
            lifeInsurance.Company = dr.Field<string>("Company");
            lifeInsurance.PolicyName = dr.Field<string>("PolicyName");
            lifeInsurance.PolicyNo = dr.Field<string>("PolicyNo");
            lifeInsurance.Premium = double.Parse(dr["Premium"].ToString());
            lifeInsurance.Terms = dr.Field<int>("Terms");
            lifeInsurance.PremiumPayTerm = dr.Field<string>("PremiumPayTerm");
            lifeInsurance.SumAssured = double.Parse(dr["SumAssured"].ToString());
            lifeInsurance.Status = dr.Field<string>("Status");
            lifeInsurance.ModeOfPayment = dr.Field<string>("ModeOfPayment");
            lifeInsurance.Moneyback = dr.Field<string>("Moneyback");
            lifeInsurance.NextPremDate = dr.Field<DateTime?>("NextPremDate");
            lifeInsurance.AccidentalDeathBenefit = double.Parse(dr["AccidentalDeathBenefit"].ToString());
            lifeInsurance.Type = dr.Field<string>("Type");
            lifeInsurance.Appointee = dr.Field<string>("Appointee");
            lifeInsurance.Nominee = dr.Field<string>("Nominee");
            lifeInsurance.Relation = dr.Field<string>("Relation");
            lifeInsurance.LoanTaken = double.Parse(dr["LoanTaken"].ToString());
            lifeInsurance.LoanDate = dr.Field<DateTime?>("LoanDate");
            lifeInsurance.BalanceUnit = dr.Field<string>("BalanceUnit");
            lifeInsurance.AsOnDate = dr.Field<DateTime?>("AsOnDate");
            lifeInsurance.CurrentValue = double.Parse(dr["CurrentValue"].ToString());
            lifeInsurance.ExpectedMaturityValue = double.Parse(dr["ExpectedMaturityValue"].ToString());
            lifeInsurance.Rider1 = dr.Field<string>("Ridder1");
            lifeInsurance.Rider1Amount = double.Parse(dr["Ridder1Amount"].ToString());
            lifeInsurance.Rider2 = dr.Field<string>("Ridder2");
            lifeInsurance.Rider2Amount = double.Parse(dr["Ridder2Amount"].ToString());
            lifeInsurance.Remarks = dr.Field<string>("Remarks");
            lifeInsurance.UpdatedBy = dr.Field<int>("UpdatedBy");
            lifeInsurance.UpdatedOn = dr.Field<DateTime>("UpdatedOn");
            lifeInsurance.UpdatedByUserName = dr.Field<string>("UpdatedByUserName");
            lifeInsurance.LastPremiumDate = dr.Field<DateTime?>("LastPremiumDate");
            lifeInsurance.SetReminder = (dr["SetReminder"] == DBNull.Value) ? false  : bool.Parse(dr["SetReminder"].ToString());
            return lifeInsurance;
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
