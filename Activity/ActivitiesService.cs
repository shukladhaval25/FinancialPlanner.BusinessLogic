using FinancialPlanner.Common.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FinancialPlanner.BusinessLogic.Activity
{
    public static class ActivitiesService
    {
        private const string INSERT_QUERY = "INSERT INTO ACTIVITIES VALUES  " +
            "('{0}','{1}','{2}','{3}','{4}','{5}','{6}')";
        private const string SELECT_ALL = "SELECT * FROM ACTIVITIES ORDER BY ACTIVITYAT DESC";
        private const string SELECT_ALL_BY_USERID = "SELECT * FROM ACTIVITIES WHERE USERNAME = '{0}' ORDER BY ACTIVITYAT DESC";
        private const string SELECT_MAX_ID = "SELECT MAX(ID) FROM ACTIVITIES";

        private const string SERVERLLOGIN ="Logged in successfully.";
        private const string LOGIN_FAIL ="Logged in fail";
       
        private const string CLIENTLOGIN ="User logged in from the client successfully.";
        private const string LOGOUT ="Logged out successfully.";

        private const string CREATEUSER ="User ''{0}'' added successfully.";
        private const string DELETEUSER ="User ''{0}'' deleted successfully.";
        private const string UPDATEUSER ="User ''{0}'' updated successfully.";

        private const string ADD_PROSPECTCLIENT_CONVERSATION = "Conversation added for ''{0}'' successfully.";
        private const string UPDATE_PROSPECTCLIENT_CONVERSATION = "Conversation updated for ''{0}'' successfully.";
        private const string DELETE_PROSPECTCLIENT_CONVERSATION = "Conversation deleted for ''{0}'' successfully.";

        private const string ADD_PROSPECT_CLIENT = "Prospect client ''{0}'' added successfully.";
        private const string UPDATE_PROSPECT_CLIENT = "Prospect client ''{0}'' updated successfully.";
        private const string DELETE_PROSPECT_CLIENT = "Prospect client ''{0}'' deleted successfully.";

        private const string ADD_EMAILARTICLE = "Email article ''{0}'' added successfully.";
        private const string UPDATE_EMAILARTICLE = "Email article ''{0}'' updated successfully.";
        private const string DELETE_EMAILARTICLE = "Email article ''{0}'' deleted successfully.";

        private const string ADD_CLIENT ="Client ''{0}'' added successfully.";
        private const string UPDATE_CLIENT ="Client ''{0}'' updated successfully.";
        private const string DELETE_CLIENT ="Client ''{0}'' deleted successfully.";

        private const string UPDATE_CLIENT_SPOUSE ="Client spouse ''{0}'' updated successfully.";
        private const string DELETE_CLIENT_SPOUSE ="Client spouse ''{0}'' deleted successfully.";

        private const string UPDATE_CLIENT_CONTACT = "Client contact ''{0}'' updated successfully.";
        private const string DELETE_CLIENT_CONTACT = "Client contact ''{0}'' deleted successfully.";

        private const string UPDATE_EMPLOYMENT ="Client employment ''{0}'' updated successfully.";
        private const string DELETE_EMPLOYMENT ="Client employment ''{0}'' deleted successfully.";

        private const string ADD_PLAN ="Plan ''{0}'' added successfully.";
        private const string UPDATE_PLAN ="Plan ''{0}'' updated successfully.";
        private const string DELETE_PLAN ="Plan ''{0}'' deleted successfully.";

        private const string ADD_EMAILSCHEDULE ="Email schedule ''{0}'' added successfully.";
        private const string UPDATE_EMAILSCHEDULE ="Email schedule ''{0}'' updated successfully.";
        private const string DELETE_EMAILSCHEDULE ="Email schedule ''{0}'' deleted successfully.";

        private const string ADD_GOAL ="Goal for  ''{0}'' added successfully.";
        private const string UPDATE_GOAL ="Goal for ''{0}'' updated successfully.";
        private const string DELETE_GOAL ="Goal for ''{0}'' deleted successfully.";

        private const string UPDATE_PLANNER_ASSUMPTION = "Planner assumption updated for ''{0}'' successfully.";
        private const string UPDATE_ASSUMPTION_MASTER = "Assumption master updated successfully.";

        private const string ADD_NON_FINANCIAL_ASSET = "Non financial asset added for ''{0}'' successfully.";
        private const string UPDATE_NON_FINANCIAL_ASSET = "Non financial asset updated for ''{0}'' successfully.";
        private const string DELETE_NON_FINANCIAL_ASSET = "Non financial asset deleted for ''{0}'' successfully.";

        private const string ADD_LOAN = "Loan added for ''{0}'' successfully.";
        private const string UPDATE_LOAN = "Loan updated for ''{0}'' successfully.";
        private const string DELETE_LOAN= "Loan deleted for ''{0}'' successfully.";

        private const string ADD_INCOME = "Income added for ''{0}'' successfully.";
        private const string UPDATE_INCOME = "Income updated for ''{0}'' successfully.";
        private const string DELETE_INCOME= "Income deleted for ''{0}'' successfully.";

        private const string ADD_RISK_PROFILED = "Risk profiled added ''{0}'' successfully.";
        private const string UPDATE_RISK_PROFILED = "Risk profiled updated ''{0}'' successfully.";
        private const string DELETE_RISK_PROFILED = "Risk profiled deleted ''{0}'' successfully.";

        private const string ADD_LIFEINSURANCE ="Life insurance added ''{0}'' successfully.";
        private const string UPDATE_LIFEINSURANCE = "Life insurance updated ''{0}'' successfully.";
        private const string DELETE_LIFEINSURANCE = "Life insurance deleted ''{0}'' successfully.";

        private const string ADD_GENERAL_INSURANCE ="General insurance added ''{0}'' successfully.";
        private const string UPDATE_GENERAL_INSURANCE = "General insurance updated ''{0}'' successfully.";
        private const string DELETE_GENERAL_INSURANCE = "General insurance deleted ''{0}'' successfully.";


        private const string ADD_EXPENSES  = "Expense added for ''{0}'' successfully.";
        private const string UPDATE_EXPENSES = "Expense updated for ''{0}'' successfully.";
        private const string DELETE_EXPENSE = "Expense deleted for ''{0}'' successfully.";

        private const string ADD_FAMILYMEMBER ="Family member for ''{0}'' added successfully.";
        private const string UPDATE_FAMILYMEMBER ="Family member for  ''{0}'' updated successfully.";
        private const string DELETE_FAMILYMEMBER ="Family member for  ''{0}'' deleted successfully.";

        private const string UPDATE_SYSTEM_SETTING = "System setting ''{0}'' updated successfully.";

        private static List<KeyValuePair<ActivityType, string>> _lstActivityDescription = new List<KeyValuePair<ActivityType, string>>();
        private static readonly string ADD_CASHFLOW = "Cash flow added successfully.";
        private static readonly string UPDATE_CASHFLOW = "Cash flow updated successfully.";
        private static readonly string DELETE_CASHFLOW = "Cash flow deleted successfully.";

        private static readonly string ADD_PLANNER_OPTION = "Planner option ''{0}'' added successfully.";
        private static readonly string UPDATE_PLANNER_OPTION = "Planner option ''{0}'' edited successfully.";
        private static readonly string DELETE_PLANNER_OPTION = "Planner option ''{0}'' deleted successfully.";

        private static readonly string ADD_MUTUALFUND = "Mutual fund ''{0}'' added successfully.";
        private static readonly string UPDATE_MUTUALFUND = "Mutual fund ''{0}'' edited successfully.";
        private static readonly string DELETE_MUTUALFUND = "Mutual fund ''{0}'' deleted successfully.";

        private static readonly string ADD_NPS = "NPS ''{0}'' added successfully.";
        private static readonly string UPDATE_NPS = "NPS ''{0}'' edited successfully.";
        private static readonly string DELETE_NPS = "NPS ''{0}'' deleted successfully.";

        private static readonly string ADD_SHARES = "Shares ''{0}'' added successfully.";
        private static readonly string UPDATE_SHARES = "Shares ''{0}'' edited successfully.";
        private static readonly string DELETE_SHARES = "Shares ''{0}'' deleted successfully.";

        private static readonly string ADD_INVESTMENT_SEGMENT = "Investment Segment ''{0}'' added successfully.";
        private static readonly string UPDATE_INVESTMENT_SEGMENT = "Investment Segment ''{0}'' edited successfully.";
        private static readonly string DELETE_INVESTMENT_SEGMENT = "Investment Segment ''{0}'' deleted successfully.";

        private static readonly string DELETE_SCHEMES = "Recommended scheme ''{0}'' deleted successfully.";
        private static readonly string ADD_SCHEMES = "Recommended scheme ''{0}'' added successfully.";
        private static readonly string UPDATE_SCHEMES = "Recommended scheme ''{0}'' added successfully.";

        public static readonly string ADD_BONDS ="Bond ''{0}'' added successfully.";
        private static readonly string UPDATE_BONDS = "Bond ''{0}'' updated successfully.";
        private static readonly string DELETE_BONDS = "Bond ''{0}'' deleted successfully.";

        private static readonly string ADD_SAVING_ACCOUNT = "Saving Account ''{0}'' added successfully.";
        private static readonly string UPDATE_SAVING_ACCOUNT = "Saving Account ''{0}'' updated successfully.";
        private static readonly string DELETE_SAVING_ACCOUNT = "Saving Account ''{0}'' deleted successfully.";

        private static readonly string ADD_FIXED_DEPOSIT = "Fixed deposit account ''{0}'' added successfully.";
        private static readonly string UPDATE_FIXED_DEPOSIT = "Fixed deposit account ''{0}'' updated successfully.";
        private static readonly string DELETE_FIXED_DEPOSIT = "Fixed deposit account ''{0}'' deleted successfully.";

        private static readonly string ADD_RECURRING_DEPOSIT = "Recurring deposit account ''{0}'' added successfully.";
        private static readonly string UPDATE_RECURRING_DEPOSIT = "Recurring deposit account ''{0}'' updated successfully.";
        private static readonly string DELETE_RECURRING_DEPOSIT = "Recurring deposit account ''{0}'' deleted successfully.";

        private static readonly string ADD_PPF ="PPF account ''{0}'' added successfully.";
        private static readonly string UPDATE_PPF = "PPF account ''{0}'' edited successfully.";
        private static readonly string DELETE_PPF = "PPF account ''{0}'' deleted successfully.";

        private static readonly string ADD_EPF ="EPF account ''{0}'' added successfully.";
        private static readonly string UPDATE_EPF = "EPF account ''{0}'' edited successfully.";
        private static readonly string DELETE_EPF = "EPF account ''{0}'' deleted successfully.";

        private static readonly string ADD_SUKANYASAMRUDHI = "Sukanya Samrudhi ''{0}'' added successfully.";
        private static readonly string UPDATE_SUKANYASAMRUDHI= "Sukanya Samrudhi ''{0}'' updated successfully.";
        private static readonly string DELETE_SUKANYASAMRUDHI = "Sukanya Samrudhi ''{0}'' deleted successfully.";

        private static readonly string ADD_SCSS = "SCSS ''{0}'' added successfully.";
        private static readonly string UPDATE_SCSS = "SCSS ''{0}'' updated successfully.";
        private static readonly string DELETE_SCSS = "SCSS ''{0}'' deleted successfully.";

        private static readonly string ADD_NSC ="NSC ''{0}'' added successfully.";
        private static readonly string UPDATE_NSC = "NSC ''{0}'' updated successfully.";
        private static readonly string DELETE_NSC = "NSC ''{0}'' deleted successfully.";

        private static readonly string ADD_ULIP = "ULIP ''{0}'' added successfully.";
        private static readonly string UPDATE_ULIP = "ULIP ''{0}'' updated successfully.";
        private static readonly string DELETE_ULIP = "ULIP ''{0}'' deleted successfully.";

        private static readonly string ADD_MF_TRANSACTION ="{0} added successfully.";
        private static readonly string UPDATE_MF_TRANSACTION ="{0} updated successfully.";
        private static readonly string DELETE_MF_TRANSACTION = "{0} deleted successfully.";

        private static string ADD_CURRENTSTATUSTOGOAL="{0} fund allocation added successfully.";
        private static string UPDATE_CURRENTSTATUSTOGOAL ="{0} fund allocation updated successfully.";
        private static string DELETE_CURRENTSTATUSTOGOAL = "{0} fund allocation deleted successfully.";

        private static readonly string ADD_BANK_ACCOUNT = "{0} Bank Account added successfully.";
        private static readonly string UPDATE_BANK_ACCOUNT = "{0} Bank Account updated successfully.";
        private static readonly string DELETE_BANK_ACCOUNT = "{0} Bank Account deleted successfully";

        private static readonly string ADD_DOCUMENT = "{0} document added successfully.";
        private static readonly string DELETE_DOCUMENT ="{0} document deleted successfully.";

        private static readonly string ADD_FESTIVALS = "{0} festival added successfully";
        private static readonly string DELETE_FESTIVALS ="{0} festival deleted successfully";

        private static readonly string ADD_CRM_GROUP = "{0} CRM group added successfully";
        private static readonly string DELETE_CRM_GROUP ="{0} CRM group deleted successfully";
        private static readonly string UPDATE_COMPANY ="Company {0} information updated successfully";

        private static readonly string ADD_AREA = "{0} area added successfully";
        private static readonly string DELETE_AREA = "{0} area deleted successfully";

        private static readonly string ADD_ORGANISATIONTYPE ="{0} organisation type added successfully";
        private static readonly string UPDATE_ORGANISATIONTYPE ="{0} organisation type updated successfully";
        private static readonly string DELETE_ORGANISATIONTYPE ="{0} organisation type deleted successfully";

        public static readonly string ADD_PROCESS_ACTION = "{0} process action added successfully";
        public static readonly string UPDATE_PROCESS_ACTION = "{0} process action updated successfully";
        public static readonly string DELETE_PROCESS_ACTION = "{0} process action added successfully";

        private static readonly string ADD_CLIENT_RATING = "{0} client rating added successfully";
        private static readonly string DELETE_CLIENT_RATING = "{0} clcient rating deleted successfully";

        public static readonly string ADD_TASKPROJECT_ACTION = "{0} task project added sucessfully";
        public static readonly string UPDATE_TASKPROJECT_ACTION = "{0} task project updated sucessfully";
        public static readonly string DELETE_TASKPROJECT_ACTION = "{0} task project deleted sucessfully";

        private static readonly string ADD_OTHERS = "OTHERS account ''{0}'' added successfully.";
        private static readonly string UPDATE_OTHERS = "OTHERS account ''{0}'' edited successfully.";
        private static readonly string DELETE_OTHERS = "OTHERS account ''{0}'' deleted successfully.";

        private static readonly string ADD_ARN_ACTION ="ARN  ''{0}'' added sucessfully";
        private static readonly string UPDATE_ARN_ACTION = "ARN  ''{0}'' edited sucessfully";
        private static readonly string DELETE_ARN_ACTION = "ARN  ''{0}'' deleted sucessfully";

        public static IList<Activities> Get(string userName)
        {
            IList<Activities> lstActivities = new List<Activities>();

            DataTable dtAppConfig =  DataBase.DBService.ExecuteCommand(string.Format(SELECT_ALL_BY_USERID,userName));
            foreach (DataRow dr in dtAppConfig.Rows)
            {
                Activities activities = new Activities();
                activities.Id = dr.Field<int>("ID");
                activities.EventDescription = dr.Field<string>("EventDescription");
                activities.ActivityTypeValue = (ActivityType)Enum.Parse(typeof(ActivityType), dr.Field<string>("ActivityType")); //(ActivityType) int.Parse();
                activities.HostName = dr.Field<string>("HostName");
                activities.UserName = dr.Field<string>("UserName");
                activities.ActivityAt = dr.Field<DateTime>("ActivityAt");
                activities.EntryType = (EntryStatus)Enum.Parse(typeof(EntryStatus), dr.Field<string>("Status"));
                activities.SourceType = (Source)Enum.Parse(typeof(Source), dr.Field<string>("Source"));

                lstActivities.Add(activities);
            }
            return lstActivities;
        }

        public static IList<Activities> Get()
        {
            IList<Activities> lstActivities = new List<Activities>();

            DataTable dtAppConfig =  DataBase.DBService.ExecuteCommand(SELECT_ALL);
            foreach (DataRow dr in dtAppConfig.Rows)
            {
                Activities activities = new Activities();
                activities.Id = dr.Field<int>("ID");
                activities.EventDescription = dr.Field<string>("EventDescription");
                activities.ActivityTypeValue = (ActivityType)Enum.Parse(typeof(ActivityType), dr.Field<string>("ActivityType")); //(ActivityType) int.Parse();
                activities.HostName = dr.Field<string>("HostName");
                activities.UserName = dr.Field<string>("UserName");
                activities.ActivityAt = dr.Field<DateTime>("ActivityAt");
                activities.EntryType = (EntryStatus)Enum.Parse(typeof(EntryStatus), dr.Field<string>("Status"));
                activities.SourceType = (Source)Enum.Parse(typeof(Source), dr.Field<string>("Source"));

                lstActivities.Add(activities);
            }
            return lstActivities;
        }
        public static void Add(ActivityType activityType,
            EntryStatus entryStatus, Source sourceType, string username, string paramValue, string hostname)
        {
            initializeListActivityDescription();

            Activities activity = new Activities();
            activity.EventDescription = string.Format(getDescription(activityType), paramValue);
            activity.ActivityTypeValue = activityType;
            activity.HostName = hostname;
            activity.UserName = username;
            activity.EntryType = entryStatus;
            activity.SourceType = sourceType;
            DataBase.DBService.ExecuteCommandString(string.Format(INSERT_QUERY,
               activity.EventDescription, activity.ActivityTypeValue,
               activity.HostName, activity.UserName, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
               activity.EntryType, activity.SourceType),true);
        }

        private static void initializeListActivityDescription()
        {
            if (_lstActivityDescription.Count == 0)
            {
                //Login/Logout
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.ServerLogin, SERVERLLOGIN));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.ClientLogin, CLIENTLOGIN));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.LoginFail, LOGIN_FAIL));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.Logout, LOGOUT));

                //User
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.CreateUser, CREATEUSER));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.UpdateUser, UPDATEUSER));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.DeleteUser, DELETEUSER));

                //Prospect client
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.CreateProspectClient, ADD_PROSPECT_CLIENT));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.UpdateProspectClient, UPDATE_PROSPECT_CLIENT));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.DeleteProspectClient, DELETE_PROSPECT_CLIENT));

                //Prospect client conversation
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.CreateProspectClientConversation, ADD_PROSPECTCLIENT_CONVERSATION));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.UpdateProspectClientConversation, UPDATE_PROSPECTCLIENT_CONVERSATION));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.DeleteProspectClientConversation, DELETE_PROSPECTCLIENT_CONVERSATION));

                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.UpdateSystemSetting, UPDATE_SYSTEM_SETTING));

                //Email Article
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.CreateEmailArticle, ADD_EMAILARTICLE));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.UpdateEmailArticle, UPDATE_EMAILARTICLE));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.DeleteEmailArticle, DELETE_EMAILARTICLE));

                //Client
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.CreateClient, ADD_CLIENT));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.UpdateClient, UPDATE_CLIENT));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.DeleteClient, DELETE_CLIENT));

                //Family Member
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.CreateFamilyMember, ADD_FAMILYMEMBER));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.UpdateFamilyMember, UPDATE_FAMILYMEMBER));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.DeleteFamilyMember, DELETE_FAMILYMEMBER));

                //Non Financial Asset
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.CreateNonFinancialAsset, ADD_NON_FINANCIAL_ASSET));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.UpdateNonFinancialAsset, UPDATE_NON_FINANCIAL_ASSET));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.DeleteNonFinancialAsset, DELETE_NON_FINANCIAL_ASSET));

                //Loan
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.CreateLoan, ADD_LOAN));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.UpdateLoan, UPDATE_LOAN));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.DeleteLoan, DELETE_LOAN));

                //Income
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.CreateIncome, ADD_INCOME));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.UpdateIncome, UPDATE_INCOME));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.DeleteIncome, DELETE_INCOME));

                //Expense
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.CreateExpenses, ADD_EXPENSES));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.UpdateExpenses, UPDATE_EXPENSES));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.DeleteExpenses, DELETE_EXPENSE));

                //ClientSpouse
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.UpdateClientSpouse, UPDATE_CLIENT));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.DeleteClientSpouse, DELETE_CLIENT));

                //ClientContact
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.UpdateClientContact, UPDATE_CLIENT_CONTACT));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.DeleteClientContact, DELETE_CLIENT_CONTACT));

                //Employment
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.UpdateEmployment, UPDATE_EMPLOYMENT));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.DeleteEmployment, DELETE_EMPLOYMENT));

                //Plan
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.CreatePlan, ADD_PLAN));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.UpdatePlan, UPDATE_PLAN));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.DeletePlan, DELETE_PLAN));

                //Planner Option
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.CreatePlannerOption, ADD_PLANNER_OPTION));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.UpdatePlannerOption, UPDATE_PLANNER_OPTION));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.DeletePlannerOption, DELETE_PLANNER_OPTION));

                //Email Schedule
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.CreateEmailSchedule, ADD_EMAILSCHEDULE));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.UpdateEmailSchedule, UPDATE_EMAILSCHEDULE));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.DeleteEmailSchedule, DELETE_EMAILSCHEDULE));


                //Goals
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.CreateGoals, ADD_GOAL));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.UpdateGoals, UPDATE_GOAL));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.DeleteGoals, DELETE_GOAL));

                //Risk Profiled
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.CreateRiskProfiled, ADD_RISK_PROFILED));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.UpdateRiskProfiled, UPDATE_GOAL));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.DeleteRiskProfiled, DELETE_GOAL));

                //Life Insurance
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.CreateLifeInsurance, ADD_LIFEINSURANCE));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.UpdateLifeInsurance, UPDATE_LIFEINSURANCE));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.DeleteLifeInsurance, DELETE_LIFEINSURANCE));

                //General Insurance
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.CreateGeneralInsurance, ADD_GENERAL_INSURANCE));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.UpdateGeneralInsurance, UPDATE_GENERAL_INSURANCE));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.DeleteGeneralInsurance, DELETE_GENERAL_INSURANCE));

                //Mutual Fund
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.CreateMutualFund, ADD_MUTUALFUND));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.UpdateMutualFund, UPDATE_MUTUALFUND));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.DeleteMutualFund, DELETE_MUTUALFUND));

                //Mutual Fund Transaction
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.CreateMFTransactions, ADD_MF_TRANSACTION));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.UpdateMFTransactions, UPDATE_MF_TRANSACTION));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.DeleteMFTransactions, DELETE_MF_TRANSACTION));

                //ULIP
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.CreateULIP, ADD_ULIP));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.UpdateULIP, UPDATE_ULIP));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.DeleteULIP, DELETE_ULIP));

                //NPS
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.CreateNPS, ADD_NPS));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.UpdateNPS, UPDATE_NPS));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.DeleteNPS, DELETE_NPS));

                //Cash Flow
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.CreateCashFlow, ADD_CASHFLOW));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.UpdateCashFlow, UPDATE_CASHFLOW));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.DeleteCashFlow, DELETE_CASHFLOW));

                //Shares
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.CreateShares, ADD_SHARES));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.UpdateShares, UPDATE_SHARES));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.DeleteShares, DELETE_SHARES));

                //Investment Segment
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.CreateInvestmentSegement, ADD_INVESTMENT_SEGMENT));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.UpdateInvestmentSegement, UPDATE_INVESTMENT_SEGMENT));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.DeleteInvestmentSegement, DELETE_INVESTMENT_SEGMENT));

                //Recommended Scheme
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.CreatedSchemes, ADD_SCHEMES));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.UpdateSchemes, UPDATE_SCHEMES));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.DeleteSchemes, DELETE_SCHEMES));

                //Bonds
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.CreateBonds, ADD_BONDS));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.UpdateBonds, UPDATE_BONDS));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.DeleteBonds, DELETE_BONDS));

                //Saving Account
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.CreateSavingAccount, ADD_SAVING_ACCOUNT));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.UpdateSavingAccount, UPDATE_SAVING_ACCOUNT));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.DeleteSavingAccount, DELETE_SAVING_ACCOUNT));

                //Fixed Deposit
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.CreateFixedDeposit, ADD_FIXED_DEPOSIT));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.UpdateFixedDeposit, UPDATE_FIXED_DEPOSIT));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.DeleteFixedDeposit, DELETE_FIXED_DEPOSIT));

                //Recurring Deposit
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.CreateRecurringDeposit, ADD_RECURRING_DEPOSIT));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.UpdateRecurringDeposit, UPDATE_RECURRING_DEPOSIT));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.DeleteRecurringDeposit, DELETE_RECURRING_DEPOSIT));

                //PPF
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.CreatePPF, ADD_PPF));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.UpdatePPF, UPDATE_PPF));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.DeletePPF, DELETE_PPF));

                // EPF
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.CreateEPF, ADD_EPF));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.UpdateEPF, UPDATE_EPF));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.DeleteEPF, DELETE_EPF));

                // Others
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.CreateOthers, ADD_OTHERS));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.UpdateOthers, UPDATE_OTHERS));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.DeleteOthers, DELETE_OTHERS));

                //Sukanya Samrudhi
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.CreateSukanyaSamrudhi, ADD_SUKANYASAMRUDHI));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.UpdateSukanyaSamrudhi, UPDATE_SUKANYASAMRUDHI));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.DeleteSukanyaSamrudhi, DELETE_SUKANYASAMRUDHI));

                //SCSS
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.CreateSCSS, ADD_SCSS));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.UpdateSCSS, UPDATE_SCSS));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.DeleteSCSS, DELETE_SCSS));

                // NSC
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.CreateNSC, ADD_NSC));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.UpdateNSC, UPDATE_NSC));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.DeleteNSC, DELETE_NSC));

                // Current Status To Goal
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.CreateCurrentStatusToGoal, ADD_CURRENTSTATUSTOGOAL));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.UpdateCurrentStatusToGoal, UPDATE_CURRENTSTATUSTOGOAL));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.DeleteCurrentStatusToGoal, DELETE_CURRENTSTATUSTOGOAL));

                // Bank Account
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.CreateBankAccount, ADD_BANK_ACCOUNT));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.UpdateBankAccount, UPDATE_BANK_ACCOUNT));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.DeleteBankAccount, DELETE_BANK_ACCOUNT));

                // Organisation Type
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.CreateOrganisationType, ADD_ORGANISATIONTYPE));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.UpdateOrganisationType, UPDATE_ORGANISATIONTYPE));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.DeleteOrganisationType, DELETE_ORGANISATIONTYPE));

                //Document
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.CreateDocument, ADD_DOCUMENT));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.DeleteDocument, DELETE_DOCUMENT));

                //Festivals
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.CreateFestivals, ADD_FESTIVALS));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.DeleteFestivals, DELETE_FESTIVALS));

                //CRM Group
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.CreateCRMGroup, ADD_CRM_GROUP));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.DeleteCRMGroup, DELETE_CRM_GROUP));

                //Area
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.CreateArea, ADD_AREA));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.DeleteArea, DELETE_AREA));

                //Client Rating
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.CreateClientRating, ADD_CLIENT_RATING));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.DeleteClientRating, DELETE_CLIENT_RATING));

                //Company
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.UpdateCompany, UPDATE_COMPANY));

                //PlannerAssumption
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.UpdatePlannerAssumption, UPDATE_PLANNER_ASSUMPTION));

                //Assumpitn Master
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.UpdateAssumptionMaster, UPDATE_ASSUMPTION_MASTER));

                //Process Actions
                //Loan
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.CreateProcessAction, ADD_PROCESS_ACTION));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.UpdateProcessAction, UPDATE_PROCESS_ACTION));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.DeleteProcessAction, DELETE_PROCESS_ACTION));

                //Task Project
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.CreateTaskProject, ADD_TASKPROJECT_ACTION));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.UpdateTaskProject, UPDATE_TASKPROJECT_ACTION));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.DeleteTaskProject, DELETE_TASKPROJECT_ACTION));

                //Task Project
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.CreateARN, ADD_ARN_ACTION));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.UpdateARN, UPDATE_ARN_ACTION));
                _lstActivityDescription.Add(new KeyValuePair<ActivityType, string>(ActivityType.DeleteARN, DELETE_ARN_ACTION));
            }
        }

        private static string getDescription(ActivityType activityType)
        {
            string desc = string.Empty;
            desc = _lstActivityDescription.First(i => i.Key == activityType).Value;
            return desc;
        }

        private static string GetDisplayName(ActivityType enumValue)
        {
            return enumValue.GetType()
                   .GetMember(enumValue.ToString())
                   .First()
                   .GetCustomAttribute<DisplayAttribute>()
                   .GetName();
        }
    }
}

