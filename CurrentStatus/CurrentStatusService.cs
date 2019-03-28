using FinancialPlanner.Common.Model.CurrentStatus;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialPlanner.BusinessLogic.CurrentStatus
{
    public class CurrentStatusService
    {
        #region "Equity Part"
        readonly string GET_SHARES_VALUE  = "SELECT SUM(CURRENTVALUE) AS SHARESVALUE FROM [SHARES] WHERE " +
                        "PID =  {0} AND GOALID = {1}";

        readonly string GET_EQUITY_VALUE = "SELECT SUM(((NAV * UNITS) * EQUITYRATIO /100)) AS EQUITYMFSHARES," +
            "SUM(((NAV * UNITS) * DEBTRATIO /100)) AS DEBTMFSHARES," +
            "SUM((NAV * UNITS)) AS TOTALVALUE FROM [MUTUALFUND] WHERE PID ={0} AND GOALID = {1} ";

        readonly string GET_EQUITY_NPS_VALUE = "SELECT ((NAV * UNITS) * EQUITYRATIO /100) AS EQUITYNPSSHARES," +
            "((NAV * UNITS) * DEBTRATIO /100) AS DEBTNPSSHARES," +
            "(NAV * UNITS) AS TOTALVALUE FROM [NPS] WHERE PID ={0} AND GOALID = {1} ";
        #endregion

        #region "Debt part"
        readonly string GET_RD_VALUE = "SELECT SUM(BALANCE) FROM [RecurringDeposit] " +
            "WHERE PID ={0} AND GOALID = {1} ";

        readonly string GET_SA_VALUE = "SELECT SUM(BALANCE) FROM [SavingAccount] " +
            "where PID ={0} AND GOALID = {1}";

        readonly string GET_FD_VALUE = "SELECT SUM(BALANCE) FROM [FixedDeposit] WHERE PID ={0} AND GOALID = {1}";

        readonly string GET_PPF_VALUE = "SELECT SUM(CURRENTVALUE) AS PPFVALUE FROM [PPF] WHERE PID = {0} AND GOALID = {1}";

        readonly string GET_EPF_VALUE = "SELECT SUM(AMOUNT) AS EPFVALUE FROM [EPF] WHERE PID = {0} AND GOALID = {1}";

        readonly string GET_SS_VALUE = "SELECT SUM(CURRENTVALUE) AS SSVALUE FROM [SukanyaSamrudhi] WHERE PID = {0} AND GOALID = {1}";

        readonly string GET_NSC_VALUE = "SELECT SUM(CURRENTVALUE) AS NSCVALUE FROM [NSC] WHERE PID = {0} AND GOALID = {1}";

        readonly string GET_SCSS_VALUE = "SELECT SUM(CURRENTVALUE) AS SCSSVALUE FROM [SCSS] WHERE PID = {0} AND GOALID = {1}";

        readonly string GET_BONDS_VALUE = "SELECT SUM(CURRENTVALUE) AS BONDSVALUE FROM [BONDS] WHERE PID = {0} AND GOALID = {1}";

        #endregion

        #region GetlAll_Query

        #region "Equity Part"
        readonly string GET_ALL_SHARES_VALUE  = "SELECT SUM(CURRENTVALUE) AS SHARESVALUE FROM [SHARES] WHERE " +
                        "PID =  {0}";

        readonly string GET_ALL_EQUITY_VALUE = "SELECT SUM(((NAV * UNITS) * EQUITYRATIO /100)) AS EQUITYMFSHARES," +
            "SUM(((NAV * UNITS) * DEBTRATIO /100)) AS DEBTMFSHARES," +
            "SUM((NAV * UNITS)) AS TOTALVALUE FROM [MUTUALFUND] WHERE PID ={0} ";

        readonly string GET_ALL_EQUITY_NPS_VALUE = "SELECT ((NAV * UNITS) * EQUITYRATIO /100) AS EQUITYNPSSHARES," +
            "((NAV * UNITS) * DEBTRATIO /100) AS DEBTNPSSHARES," +
            "(NAV * UNITS) AS TOTALVALUE FROM [NPS] WHERE PID ={0} ";
        #endregion

        #region "Debt part"
        readonly string GET_ALL_RD_VALUE = "SELECT SUM(BALANCE) FROM [RecurringDeposit] " +
            "WHERE PID ={0} ";

        readonly string GET_ALL_SA_VALUE = "SELECT SUM(BALANCE) FROM [SavingAccount] " +
            "where PID ={0}";

        readonly string GET_ALL_FD_VALUE = "SELECT SUM(BALANCE) FROM [FixedDeposit] WHERE PID ={0}";

        readonly string GET_ALL_PPF_VALUE = "SELECT SUM(CURRENTVALUE) AS PPFVALUE FROM [PPF] WHERE PID = {0}";

        readonly string GET_ALL_EPF_VALUE = "SELECT SUM(AMOUNT) AS EPFVALUE FROM [EPF] WHERE PID = {0}";

        readonly string GET_ALL_SS_VALUE = "SELECT SUM(CURRENTVALUE) AS SSVALUE FROM [SukanyaSamrudhi] WHERE PID = {0} ";

        readonly string GET_ALL_NSC_VALUE = "SELECT SUM(CURRENTVALUE) AS NSCVALUE FROM [NSC] WHERE PID = {0}";

        readonly string GET_ALL_SCSS_VALUE = "SELECT SUM(CURRENTVALUE) AS SCSSVALUE FROM [SCSS] WHERE PID = {0}";

        readonly string GET_ALL_BONDS_VALUE = "SELECT SUM(CURRENTVALUE) AS BONDSVALUE FROM [BONDS] WHERE PID = {0}";

        #endregion
        #endregion

        public CurrentStatusCalculation Get(int plannerId, int goalId = 0)
        {
            CurrentStatusCalculation csCal = new CurrentStatusCalculation();

            #region "Shares"
            //Shares
            double sharesValue = 0;
            string returnvalue = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_SHARES_VALUE,plannerId,goalId));
            double.TryParse(returnvalue, out sharesValue);
            csCal.ShresValue = sharesValue;
            #endregion

            #region "MF"
            //MF
            double equityMFValue = 0;
            double debtMFValue = 0;
            DataTable dtMF =  DataBase.DBService.ExecuteCommand(string.Format(GET_EQUITY_VALUE,plannerId,goalId));
            if (dtMF != null && dtMF.Rows.Count > 0 )
            {
                double.TryParse(dtMF.Rows[0]["EQUITYMFSHARES"].ToString(),out equityMFValue);
                double.TryParse(dtMF.Rows[0]["DEBTMFSHARES"].ToString(), out debtMFValue);
            }
            csCal.EquityMFvalue = equityMFValue;
            csCal.DebtMFValue = debtMFValue;
            #endregion

            #region "NPS"
            //NPS
            double equityNPSValue = 0;
            double debtNPSValue = 0;
            DataTable dtNPS =  DataBase.DBService.ExecuteCommand(string.Format(GET_EQUITY_NPS_VALUE,plannerId,goalId));
            if (dtNPS != null && dtNPS.Rows.Count > 0 )
            {
                double.TryParse(dtNPS.Rows[0]["EQUITYNPSSHARES"].ToString(), out equityNPSValue);
                double.TryParse(dtNPS.Rows[0]["DEBTNPSSHARES"].ToString(), out debtNPSValue);
            }
            csCal.NpsEquityValue = equityNPSValue;
            csCal.NpsDebtValue = debtNPSValue;
            #endregion

            #region "DEBT"

            #region "RD"
            //RD
            double rdValue = 0;
            returnvalue = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_RD_VALUE, plannerId, goalId));
            double.TryParse(returnvalue, out rdValue);
            csCal.RdValue = rdValue;
            #endregion

            #region "FD"
            //FD
            double fdValue = 0;
            returnvalue = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_FD_VALUE, plannerId, goalId));
            double.TryParse(returnvalue, out fdValue);
            csCal.FdValue = fdValue;
            #endregion

            #region "Saving Account"
            //SA
            double saValue = 0;
            returnvalue = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_SA_VALUE, plannerId, goalId));
            double.TryParse(returnvalue, out saValue);
            csCal.SaValue = saValue;
            #endregion

            #region "PPF"
            //PPF
            double ppfValue = 0;
            returnvalue = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_PPF_VALUE, plannerId, goalId));
            double.TryParse(returnvalue, out ppfValue);
            csCal.PPFValue  = ppfValue;
            #endregion

            #region "EPF"
            //EPF
            double epfValue = 0;
            returnvalue = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_EPF_VALUE, plannerId, goalId));
            double.TryParse(returnvalue, out epfValue);
            csCal.EPFValue = epfValue;
            #endregion

            #region "SS"
            //Sukanya Samrudhi
            double ssValue = 0;
            returnvalue = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_SS_VALUE, plannerId, goalId));
            double.TryParse(returnvalue, out ssValue);
            csCal.SSValue = ssValue;
            #endregion

            #region "SCSS"
            //SCSS
            double scssValue = 0;
            returnvalue = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_SCSS_VALUE, plannerId, goalId));
            double.TryParse(returnvalue, out scssValue);
            csCal.SCSSValue = scssValue;
            #endregion

            #region "NSC"
            //NSC
            double nscValue = 0;
            returnvalue = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_NSC_VALUE, plannerId, goalId));
            double.TryParse(returnvalue, out nscValue);
            csCal.NscValue = nscValue;
            #endregion

            #region "Bonds"
            double bondsValue = 0;
            returnvalue = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_BONDS_VALUE, plannerId, goalId));
            double.TryParse(returnvalue, out bondsValue);
            csCal.BondsValue = bondsValue;
            #endregion

            #endregion

            return csCal;
        }

        public CurrentStatusCalculation GetAllCurrentStatusAmount (int plannerId)
        {

            CurrentStatusCalculation csCal = new CurrentStatusCalculation();

            #region "Shares"
            //Shares
            double sharesValue = 0;
            string returnvalue = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_ALL_SHARES_VALUE,plannerId));
            double.TryParse(returnvalue, out sharesValue);
            csCal.ShresValue = sharesValue;
            #endregion

            #region "MF"
            //MF
            double equityMFValue = 0;
            double debtMFValue = 0;
            DataTable dtMF =  DataBase.DBService.ExecuteCommand(string.Format(GET_ALL_EQUITY_VALUE,plannerId));
            if (dtMF != null && dtMF.Rows.Count > 0)
            {
                double.TryParse(dtMF.Rows[0]["EQUITYMFSHARES"].ToString(), out equityMFValue);
                double.TryParse(dtMF.Rows[0]["DEBTMFSHARES"].ToString(), out debtMFValue);
            }
            csCal.EquityMFvalue = equityMFValue;
            csCal.DebtMFValue = debtMFValue;
            #endregion

            #region "NPS"
            //NPS
            double equityNPSValue = 0;
            double debtNPSValue = 0;
            DataTable dtNPS =  DataBase.DBService.ExecuteCommand(string.Format(GET_ALL_EQUITY_NPS_VALUE,plannerId));
            if (dtNPS != null && dtNPS.Rows.Count > 0)
            {
                double.TryParse(dtNPS.Rows[0]["EQUITYNPSSHARES"].ToString(), out equityNPSValue);
                double.TryParse(dtNPS.Rows[0]["DEBTNPSSHARES"].ToString(), out debtNPSValue);
            }
            csCal.NpsEquityValue = equityNPSValue;
            csCal.NpsDebtValue = debtNPSValue;
            #endregion

            #region "DEBT"

            #region "RD"
            //RD
            double rdValue = 0;
            returnvalue = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_ALL_RD_VALUE, plannerId));
            double.TryParse(returnvalue, out rdValue);
            csCal.RdValue = rdValue;
            #endregion

            #region "FD"
            //FD
            double fdValue = 0;
            returnvalue = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_ALL_FD_VALUE, plannerId));
            double.TryParse(returnvalue, out fdValue);
            csCal.FdValue = fdValue;
            #endregion

            #region "Saving Account"
            //SA
            double saValue = 0;
            returnvalue = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_ALL_SA_VALUE, plannerId));
            double.TryParse(returnvalue, out saValue);
            csCal.SaValue = saValue;
            #endregion

            #region "PPF"
            //PPF
            double ppfValue = 0;
            returnvalue = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_ALL_PPF_VALUE, plannerId));
            double.TryParse(returnvalue, out ppfValue);
            csCal.PPFValue = ppfValue;
            #endregion

            #region "EPF"
            //EPF
            double epfValue = 0;
            returnvalue = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_ALL_EPF_VALUE, plannerId));
            double.TryParse(returnvalue, out epfValue);
            csCal.EPFValue = epfValue;
            #endregion

            #region "SS"
            //Sukanya Samrudhi
            double ssValue = 0;
            returnvalue = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_ALL_SS_VALUE, plannerId));
            double.TryParse(returnvalue, out ssValue);
            csCal.SSValue = ssValue;
            #endregion

            #region "SCSS"
            //SCSS
            double scssValue = 0;
            returnvalue = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_ALL_SCSS_VALUE, plannerId));
            double.TryParse(returnvalue, out scssValue);
            csCal.SCSSValue = scssValue;
            #endregion

            #region "NSC"
            //NSC
            double nscValue = 0;
            returnvalue = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_ALL_NSC_VALUE, plannerId));
            double.TryParse(returnvalue, out nscValue);
            csCal.NscValue = nscValue;
            #endregion

            #region "Bonds"
            double bondsValue = 0;
            returnvalue = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_ALL_BONDS_VALUE, plannerId));
            double.TryParse(returnvalue, out bondsValue);
            csCal.BondsValue = bondsValue;
            #endregion

            #endregion

            return csCal;
        }
    }
}
