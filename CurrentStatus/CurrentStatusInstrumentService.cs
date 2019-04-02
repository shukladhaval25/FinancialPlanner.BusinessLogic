using FinancialPlanner.Common.Model.CurrentStatus;
using System;
using System.Collections.Generic;
using System.Data;

namespace FinancialPlanner.BusinessLogic.CurrentStatus
{
    public class CurrentStatusInstrumentService
    {
        #region "Instrument Mapped With Goal"
        #region "Equity Part"
        readonly string GET_SHARES_VALUE = "SELECT 'SHARES' AS NAME,(CURRENTVALUE) AS SHARESVALUE," +
            "INVESTMENTRETURNRATE AS ROI,GOALID FROM [SHARES] WHERE PID =  {0} AND GOALID = {1}";

        readonly string GET_EQUITY_VALUE = "SELECT 'MF' AS NAME, SUM(((NAV * UNITS) * EQUITYRATIO /100)) AS EQUITYMFSHARES," +
           "SUM(((NAV * UNITS) * DEBTRATIO /100)) AS DEBTMFSHARES," +
           "SUM((NAV * UNITS)) AS TOTALVALUE, INVESTMENTRETURNRATE AS ROI " +
            " FROM [MUTUALFUND] WHERE PID ={0} AND GOALID = {1} ";

        readonly string GET_EQUITY_NPS_VALUE = "SELECT 'NPS' AS NAME, ((NAV * UNITS) * EQUITYRATIO /100) AS EQUITYNPSSHARES," +
            "((NAV * UNITS) * DEBTRATIO /100) AS DEBTNPSSHARES," +
            "(NAV * UNITS) AS TOTALVALUE,INVESTMENTRETURNRATE AS ROI FROM [NPS] WHERE PID ={0} AND GOALID = {1} ";
        #endregion

        #region "Debt part"
        readonly string GET_RD_VALUE = "SELECT 'RD' AS NAME, (BALANCE) AS BALANCE, IntRate AS ROI,GOALID FROM [RecurringDeposit] " +
            "WHERE PID ={0} AND GOALID = {1} ";

        readonly string GET_SA_VALUE = "SELECT 'SA' AS NAME, (BALANCE) AS BALANCE, IntRate AS ROI,GOALID FROM [SavingAccount] " +
            "where PID ={0} AND GOALID = {1}";

        readonly string GET_FD_VALUE = "SELECT 'FD' AS NAME, (BALANCE) AS BALANCE, IntRate AS ROI,GOALID FROM [FixedDeposit] WHERE PID ={0} AND GOALID = {1}";

        readonly string GET_PPF_VALUE = "SELECT 'PPF' AS NAME, (CURRENTVALUE) AS PPFVALUE, INVESTMENTRETURNRATE AS ROI, GOALID FROM [PPF] WHERE PID = {0} AND GOALID = {1}";

        readonly string GET_EPF_VALUE = "SELECT 'EPF' AS NAME, (AMOUNT) AS EPFVALUE, INVESTMENTRETURNRATE AS ROI, GOALID FROM [EPF] WHERE PID = {0} AND GOALID = {1}";

        readonly string GET_SS_VALUE = "SELECT 'SS' AS NAME, (CURRENTVALUE) AS SSVALUE,INVESTMENTRETURNRATE AS ROI,GOALID FROM [SukanyaSamrudhi] WHERE PID = {0} AND GOALID = {1}";

        readonly string GET_NSC_VALUE = "SELECT 'NSC' AS NAME, (CURRENTVALUE) AS NSCVALUE, INVESTMENTRETURNRATE AS ROI,GOALID FROM [NSC] WHERE PID = {0} AND GOALID = {1}";

        readonly string GET_SCSS_VALUE = "SELECT 'SCSS' AS NAME, (CURRENTVALUE) AS SCSSVALUE,INVESTMENTRETURNRATE AS ROI,GOALID FROM [SCSS] WHERE PID = {0} AND GOALID = {1}";

        readonly string GET_BONDS_VALUE = "SELECT 'BONDS' AS NAME,(CURRENTVALUE) AS BONDSVALUE,INVESTMENTRETURNRATE AS ROI,GOALID FROM [BONDS] WHERE PID = {0} AND GOALID = {1}";

        #endregion
        #endregion

        #region "GetAll"
        #region "Equity Part"
        readonly string GET_ALL_SHARES_VALUE = "SELECT 'SHARES' AS NAME,(CURRENTVALUE) AS SHARESVALUE," +
            "INVESTMENTRETURNRATE AS ROI,GOALID FROM [SHARES] WHERE PID =  {0}";

        readonly string GET_ALL_EQUITY_VALUE = "SELECT 'MF' AS NAME, SUM(((NAV * UNITS) * EQUITYRATIO /100)) AS EQUITYMFSHARES," +
           "SUM(((NAV * UNITS) * DEBTRATIO /100)) AS DEBTMFSHARES," +
           "SUM((NAV * UNITS)) AS TOTALVALUE, INVESTMENTRETURNRATE AS ROI, GOALID " +
            " FROM [MUTUALFUND] WHERE PID ={0}";

        readonly string GET_ALL_EQUITY_NPS_VALUE = "SELECT 'NPS' AS NAME, ((NAV * UNITS) * EQUITYRATIO /100) AS EQUITYNPSSHARES," +
            "((NAV * UNITS) * DEBTRATIO /100) AS DEBTNPSSHARES," +
            "(NAV * UNITS) AS TOTALVALUE,INVESTMENTRETURNRATE AS ROI, GOALID FROM [NPS] WHERE PID ={0}";
        #endregion

        #region "Debt part"
        readonly string GET_ALL_RD_VALUE = "SELECT 'RD' AS NAME, (BALANCE) AS BALANCE, IntRate AS ROI,GOALID FROM [RecurringDeposit] " +
            "WHERE PID ={0}";

        readonly string GET_ALL_SA_VALUE = "SELECT 'SA' AS NAME, (BALANCE) AS BALANCE, IntRate AS ROI,GOALID FROM [SavingAccount] " +
            "where PID ={0}";

        readonly string GET_ALL_FD_VALUE = "SELECT 'FD' AS NAME, (BALANCE) AS BALANCE, IntRate AS ROI,GOALID FROM [FixedDeposit] WHERE PID ={0}";

        readonly string GET_ALL_PPF_VALUE = "SELECT 'PPF' AS NAME, (CURRENTVALUE) AS PPFVALUE, INVESTMENTRETURNRATE AS ROI,GOALID FROM [PPF] WHERE PID = {0}";

        readonly string GET_ALL_EPF_VALUE = "SELECT 'EPF' AS NAME, (AMOUNT) AS EPFVALUE, INVESTMENTRETURNRATE AS ROI,GOALID FROM [EPF] WHERE PID = {0}";

        readonly string GET_ALL_SS_VALUE = "SELECT 'SS' AS NAME, (CURRENTVALUE) AS SSVALUE,INVESTMENTRETURNRATE AS ROI,GOALID FROM [SukanyaSamrudhi] WHERE PID = {0}";

        readonly string GET_ALL_NSC_VALUE = "SELECT 'NSC' AS NAME, (CURRENTVALUE) AS NSCVALUE, INVESTMENTRETURNRATE AS ROI,GOALID FROM [NSC] WHERE PID = {0}";

        readonly string GET_ALL_SCSS_VALUE = "SELECT 'SCSS' AS NAME, (CURRENTVALUE) AS SCSSVALUE,INVESTMENTRETURNRATE AS ROI,GOALID FROM [SCSS] WHERE PID = {0}";

        readonly string GET_ALL_BONDS_VALUE = "SELECT 'BONDS' AS NAME,(CURRENTVALUE) AS BONDSVALUE,INVESTMENTRETURNRATE AS ROI,GOALID FROM [BONDS] WHERE PID = {0}";

        #endregion
        #endregion

        public IList<CurrentStatusInstrument> GetMappedInstrumentWithGoal(int plannerId, int goalId)
        {
            IList<CurrentStatusInstrument> instrument = new List<CurrentStatusInstrument>();
            IList<CurrentStatusInstrument> currentStatusInstruments = GetAllCurrentStatusAmount(plannerId);
            if (currentStatusInstruments != null)
            {                             
                foreach(CurrentStatusInstrument element in currentStatusInstruments)
                {
                    if (element.GoalId == goalId)
                        instrument.Add(element);
                }
            }
            return instrument;

        }

        public IList<CurrentStatusInstrument> GetAllCurrentStatusAmount(int plannerId)
        {

            //CurrentStatusCalculation csCal = new CurrentStatusCalculation();
            IList<CurrentStatusInstrument> csCal = new List<CurrentStatusInstrument>();            
            #region "Equity"
            addSharesToCurrentStatusInstrument(plannerId, csCal);

            addMFToCurrentStatusInstrument(plannerId, csCal);

            addNPSToCurrentStatusInstrument(plannerId, csCal);
            #endregion

            #region "DEBT"
            addRDToCurrentStatusInstrument(plannerId, csCal);
            addFDToCurrentStatusInstrument(plannerId, csCal);
            addSavingAccountToCurrentStatusInstrument(plannerId, csCal);
            addPPFToCurrentStatusInstrument(plannerId, csCal);
            addEPFToCurrentStatusInstrument(plannerId, csCal);
            addSSToCurrentStatusInstrument(plannerId, csCal);
            addSCSSToCurrentSatusInstrument(plannerId, csCal);
            addNSCToCurrentStatusInstrument(plannerId, csCal);
            addBondsToCurrentStatusInstrument(plannerId, csCal);
            #endregion

            return csCal;
        }

        private void addBondsToCurrentStatusInstrument(int plannerId, IList<CurrentStatusInstrument> csCal)
        {
            DataTable dtInstrument = new DataTable();
            dtInstrument = DataBase.DBService.ExecuteCommand(string.Format(GET_ALL_BONDS_VALUE, plannerId));
            if (dtInstrument != null && dtInstrument.Rows.Count > 0)
            {
                setInstrument(csCal, dtInstrument);
            }            
        }

        private void addNSCToCurrentStatusInstrument(int plannerId, IList<CurrentStatusInstrument> csCal)
        {
            DataTable dtInstrument = new DataTable();
            dtInstrument = DataBase.DBService.ExecuteCommand(string.Format(GET_ALL_NSC_VALUE, plannerId));
            if (dtInstrument != null && dtInstrument.Rows.Count > 0)
            {
                setInstrument(csCal, dtInstrument);
            }
        }

        private void addSCSSToCurrentSatusInstrument(int plannerId, IList<CurrentStatusInstrument> csCal)
        {
            DataTable dtInstrument = new DataTable();
            dtInstrument = DataBase.DBService.ExecuteCommand(string.Format(GET_ALL_SCSS_VALUE, plannerId));
            if (dtInstrument != null && dtInstrument.Rows.Count > 0)
            {
                setInstrument(csCal, dtInstrument);
            }            
        }

        private void addSSToCurrentStatusInstrument(int plannerId, IList<CurrentStatusInstrument> csCal)
        {
            DataTable dtInstrument = new DataTable();
            dtInstrument = DataBase.DBService.ExecuteCommand(string.Format(GET_ALL_SS_VALUE, plannerId));
            if (dtInstrument != null && dtInstrument.Rows.Count > 0)
            {
                setInstrument(csCal, dtInstrument);
            }           
        }

        private void addEPFToCurrentStatusInstrument(int plannerId, IList<CurrentStatusInstrument> csCal)
        {
            DataTable dtInstrument = new DataTable();
            dtInstrument = DataBase.DBService.ExecuteCommand(string.Format(GET_ALL_EPF_VALUE, plannerId));
            if (dtInstrument != null && dtInstrument.Rows.Count > 0)
            {
                setInstrument(csCal, dtInstrument);
            }           
        }

        private void addPPFToCurrentStatusInstrument(int plannerId, IList<CurrentStatusInstrument> csCal)
        {
            DataTable dtInstrument = new DataTable();
            dtInstrument = DataBase.DBService.ExecuteCommand(string.Format(GET_ALL_PPF_VALUE, plannerId));
            if (dtInstrument != null && dtInstrument.Rows.Count > 0)
            {
                setInstrument(csCal, dtInstrument);
            }
        }

        private void addSavingAccountToCurrentStatusInstrument(int plannerId, IList<CurrentStatusInstrument> csCal)
        {
            DataTable dtInstrument = new DataTable();
            dtInstrument = DataBase.DBService.ExecuteCommand(string.Format(GET_ALL_SA_VALUE, plannerId));
            if (dtInstrument != null && dtInstrument.Rows.Count > 0)
            {
                setInstrument(csCal, dtInstrument);
            }
        }

        private void addFDToCurrentStatusInstrument(int plannerId, IList<CurrentStatusInstrument> csCal)
        {
            DataTable dtInstrument = new DataTable();
            dtInstrument = DataBase.DBService.ExecuteCommand(string.Format(GET_ALL_FD_VALUE, plannerId));
            if (dtInstrument != null && dtInstrument.Rows.Count > 0)
            {
                setInstrument(csCal, dtInstrument);
            }
        }

        private void addRDToCurrentStatusInstrument(int plannerId, IList<CurrentStatusInstrument> csCal)
        {
            DataTable dtInstrument = new DataTable();
            dtInstrument = DataBase.DBService.ExecuteCommand(string.Format(GET_ALL_RD_VALUE, plannerId));
            if (dtInstrument != null && dtInstrument.Rows.Count > 0)
            {
                setInstrument(csCal, dtInstrument);
            }
        }

        private static void setInstrument(IList<CurrentStatusInstrument> csCal, DataTable dtInstrument)
        {
            foreach (DataRow dr in dtInstrument.Rows)
            {
                string name = dr[0].ToString();
                double value = (string.IsNullOrEmpty(dr[1].ToString()) ? 0 : double.Parse(dr[1].ToString()));
                if (value > 0)
                {
                    float roi = (string.IsNullOrEmpty(dr[2].ToString()) ? 0 : float.Parse(dr[2].ToString()));
                    int goalid = (dr[3] == DBNull.Value) ? 0 : int.Parse(dr[3].ToString());

                    CurrentStatusInstrument currentStatus = new CurrentStatusInstrument();
                    currentStatus.InstrumentName = name;
                    currentStatus.Amount = value;
                    currentStatus.Roi = roi;
                    currentStatus.GoalId = goalid;

                    csCal.Add(currentStatus);
                }
            }
        }

        private void addNPSToCurrentStatusInstrument(int plannerId, IList<CurrentStatusInstrument> csCal)
        {
            DataTable dtInstrument = new DataTable();
            dtInstrument = DataBase.DBService.ExecuteCommand(string.Format(GET_ALL_EQUITY_NPS_VALUE, plannerId));
            if (dtInstrument != null && dtInstrument.Rows.Count > 0)
            {
                foreach (DataRow dr in dtInstrument.Rows)
                {
                    double equityNPSValue = 0;
                    double debtNPSValue = 0;
                    double.TryParse(dr["EQUITYNPSSHARES"].ToString(), out equityNPSValue);
                    double.TryParse(dr["DEBTNPSSHARES"].ToString(), out debtNPSValue);

                    string name = "NPS_EQUITY";
                    double value = equityNPSValue;
                    float roi = (string.IsNullOrEmpty(dr[2].ToString()) ? 0 : float.Parse(dr[2].ToString()));
                    int goalid = (dr[3] == DBNull.Value) ? 0 : int.Parse(dr[3].ToString());

                    CurrentStatusInstrument currentStatus = new CurrentStatusInstrument();
                    currentStatus.InstrumentName = name;
                    currentStatus.Amount = value;
                    currentStatus.Roi = roi;
                    currentStatus.GoalId = goalid;
                    csCal.Add(currentStatus);

                    name = "NPS_DEBT";
                    value = debtNPSValue;
                    CurrentStatusInstrument currentStatus1 = new CurrentStatusInstrument();
                    currentStatus1.InstrumentName = name;
                    currentStatus1.Amount = value;
                    currentStatus1.Roi = roi;
                    currentStatus1.GoalId = goalid;
                    csCal.Add(currentStatus1);
                }
            }
        }

        private void addMFToCurrentStatusInstrument(int plannerId, IList<CurrentStatusInstrument> csCal)
        {
            DataTable dtInstrument = new DataTable();
            dtInstrument = DataBase.DBService.ExecuteCommand(string.Format(GET_ALL_EQUITY_VALUE, plannerId));
            if (dtInstrument != null && dtInstrument.Rows.Count > 0)
            {
                foreach (DataRow dr in dtInstrument.Rows)
                {
                    double equityMFValue = 0;
                    double debtMFValue = 0;
                    double.TryParse(dr["EQUITYMFSHARES"].ToString(), out equityMFValue);
                    double.TryParse(dr["DEBTMFSHARES"].ToString(), out debtMFValue);

                    string name = "MF_EQUITY";
                    double value = equityMFValue;
                    float roi = (string.IsNullOrEmpty(dr[2].ToString()) ? 0 : float.Parse(dr[2].ToString()));
                    int goalid = (dr[3] == DBNull.Value) ? 0 : int.Parse(dr[3].ToString());

                    CurrentStatusInstrument currentStatus = new CurrentStatusInstrument();
                    currentStatus.InstrumentName = name;
                    currentStatus.Amount = value;
                    currentStatus.Roi = roi;
                    currentStatus.GoalId = goalid;
                    csCal.Add(currentStatus);

                    name = "MF_DEBT";
                    value = debtMFValue;
                    CurrentStatusInstrument currentStatus1 = new CurrentStatusInstrument();
                    currentStatus1.InstrumentName = name;
                    currentStatus1.Amount = value;
                    currentStatus1.Roi = roi;
                    currentStatus1.GoalId = goalid;
                    csCal.Add(currentStatus1);                    
                }
            }
        }

        private void addSharesToCurrentStatusInstrument(int plannerId, IList<CurrentStatusInstrument> currentStatusInstrument)
        {
            DataTable dtInstrument = new DataTable();
            dtInstrument = DataBase.DBService.ExecuteCommand(string.Format(GET_ALL_SHARES_VALUE, plannerId));
            if (dtInstrument != null && dtInstrument.Rows.Count > 0)
            {
                setInstrument(currentStatusInstrument, dtInstrument);
            }
        }
    }
}
