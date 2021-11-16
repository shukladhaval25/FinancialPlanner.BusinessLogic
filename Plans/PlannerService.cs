using FinancialPlanner.Common.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialPlanner.BusinessLogic.Plans
{
    public class PlannerService
    {
        private const string INSERT_QUERY = "INSERT INTO PLANNER VALUES (" +
            "{0},'{1}','{2}','{3}','{4}','{5}',{6},'{7}',{8},{9},{10},'{11}','{12}','{13}','{14}','{15}',{16},{17},'{18}')";

        private const string SELECT_BY_CLIENTID = "SELECT P1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM PLANNER P1, USERS U WHERE P1.UPDATEDBY = U.ID and P1.CLIENTID = {0} AND P1.ISDELETED = 'FALSE'";
        private const string SELECT_ID = "SELECT P1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM PLANNER P1, USERS U WHERE P1.UPDATEDBY = U.ID and P1.ID = {0} AND P1.ISDELETED = 'FALSE'";

        private const string UPDATE_QUERY = "UPDATE PLANNER SET " +
            "[Name] = '{0}', [StartDate] ='{1}', [EndDate] ='{2}',[IsActive]='{3}',[CreatedOn] ='{4}'," +
            "[CreatedBy] = {5}, [UpdatedOn] ='{6}', [UpdatedBy] = {7}, [PlannerStartMonth] = {8}," +
            "[AccountManagedBy] = {9}, [Description] = '{10}',[ReviewFrequency] ='{11}',[Recommendation] ='{13}', [CurrencySymbol]='{14}',EquityRatio ={15}, DebtRatio = {16},FaceType='{17}' WHERE ID = {12}";
    
        private const string DELETE_QUERY = "UPDATE PLANNER SET ISDELETED = 'TRUE' WHERE ID = {0}";

        public IList<Planner> GetByClientId(int id)
        {
            IList<Planner> lstPlanner = new List<Planner>();

            DataTable dtAppConfig =  DataBase.DBService.ExecuteCommand(string.Format(SELECT_BY_CLIENTID,id));
            foreach (DataRow dr in dtAppConfig.Rows)
            {
                Planner planner = convertToPlannerObject(dr);
                lstPlanner.Add(planner);
            }
            return lstPlanner;
        }
        public Planner GetByPlannerId(int Id)
        {
            Planner planner = new Planner();

            DataTable dtAppConfig =  DataBase.DBService.ExecuteCommand(string.Format(SELECT_ID,Id));
            foreach (DataRow dr in dtAppConfig.Rows)
            {
                planner = convertToPlannerObject(dr);
            }
            return planner;
        }

        private Planner convertToPlannerObject(DataRow dr)
        {
            Planner planner = new Planner()
            {
                ID = dr.Field<int>("ID"),
                ClientId = dr.Field<int>("ClientID"),
                Name = dr.Field<string>("Name"),
                StartDate = dr.Field<DateTime>("StartDate"),
                UpdatedByUserName = dr.Field<string>("UpdatedByUserName"),
                PlannerStartMonth = dr.Field<int>("PlannerStartMonth"),
                AccountManagedBy = dr.Field<int>("AccountManagedBy"),
                Description = dr.Field<string>("Description"),
                IsDeleted = dr.Field<bool>("IsDeleted"),
                ReviewFrequency = dr.Field<string>("ReviewFrequency"),
                Recommendation = dr["Recommendation"] == DBNull.Value ? "" : dr["Recommendation"].ToString(),
                CurrencySymbol = dr["CurrencySymbol"] == DBNull.Value ? "" : dr["CurrencySymbol"].ToString(),
                EquityRatio = dr["EquityRatio"] == DBNull.Value ? 0 : float.Parse(dr["EquityRatio"].ToString()),
                DebtRatio = dr["DebtRatio"] == DBNull.Value ? 0 : float.Parse(dr["DebtRatio"].ToString()),
                FaceType = dr["FaceType"].ToString()
            };
            return planner;
        }
        public void Add(Planner planner)
        {
            try
            {
                FinancialPlanner.Common.Logger.LogInfo(string.Format(INSERT_QUERY,
                    planner.ClientId, planner.Name, planner.StartDate.ToString("yyyy-MM-dd"),
                    planner.EndDate.ToString("yyyy-MM-dd"), planner.IsActive,
                    planner.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss"), planner.CreatedBy,
                    planner.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), planner.UpdatedBy,
                    planner.PlannerStartMonth, planner.AccountManagedBy, planner.Description,
                    planner.IsDeleted, planner.ReviewFrequency,planner.Recommendation,
                    planner.CurrencySymbol,planner.EquityRatio,planner.DebtRatio,
                    planner.FaceType));

                DataBase.DBService.ExecuteCommand(string.Format(INSERT_QUERY,
                    planner.ClientId, planner.Name, planner.StartDate.ToString("yyyy-MM-dd"),
                    planner.EndDate.ToString("yyyy-MM-dd"), planner.IsActive,
                    planner.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss"), planner.CreatedBy,
                    planner.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), planner.UpdatedBy,
                    planner.PlannerStartMonth,planner.AccountManagedBy,planner.Description,
                    planner.IsDeleted,planner.ReviewFrequency,planner.Recommendation,
                    planner.CurrencySymbol,planner.EquityRatio, planner.DebtRatio,
                    planner.FaceType));

                Activity.ActivitiesService.Add(ActivityType.CreatePlan, EntryStatus.Success,
                         Source.Server, planner.UpdatedByUserName, planner.Name, planner.MachineName);
            }
            catch (Exception ex)
            {
                FinancialPlanner.Common.Logger.LogDebug(ex.Message);
            }
        }

        public void Update(Planner planner)
        {
            try
            {
                DataBase.DBService.ExecuteCommand(string.Format(UPDATE_QUERY,
                    planner.Name, planner.StartDate.ToString("yyyy-MM-dd"),
                    planner.EndDate.ToString("yyyy-MM-dd"), planner.IsActive,
                    planner.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss"), planner.CreatedBy,
                    planner.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), planner.UpdatedBy,
                    planner.PlannerStartMonth, planner.AccountManagedBy, planner.Description,
                    planner.ReviewFrequency,planner.ID,planner.Recommendation,planner.CurrencySymbol,
                    planner.EquityRatio,planner.DebtRatio,planner.FaceType));

                Activity.ActivitiesService.Add(ActivityType.UpdatePlan, EntryStatus.Success,
                         Source.Server, planner.UpdatedByUserName, planner.Name, planner.MachineName);
            }
            catch (Exception ex)
            {
                FinancialPlanner.Common.Logger.LogDebug(ex.Message);
            }
        }

        public void Delete(Planner planner)
        {
            try
            {
                DataBase.DBService.ExecuteCommand(string.Format(DELETE_QUERY, planner.ID));

                Activity.ActivitiesService.Add(ActivityType.DeletePlan, EntryStatus.Success,
                         Source.Server, planner.UpdatedByUserName, planner.Name, planner.MachineName);
            }
            catch (Exception ex)
            {
                FinancialPlanner.Common.Logger.LogDebug(ex.Message);
            }
        }
    }
}
