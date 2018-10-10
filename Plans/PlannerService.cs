﻿using FinancialPlanner.Common.Model;
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
            "{0},'{1}','{2}','{3}','{4}','{5}',{6},'{7}',{8})";

        private const string SELECT_BY_CLIENTID = "SELECT P1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM PLANNER P1, USERS U WHERE P1.UPDATEDBY = U.ID and P1.CLIENTID = {0}";
        private const string SELECT_ID = "SELECT P1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM PLANNER P1, USERS U WHERE P1.UPDATEDBY = U.ID and P1.ID = {0}";

        private const string UPDATE_QUERY = "UPDATE USERS SET FIRSTNAME = '{0}'," +
                "LASTNAME = '{1}', PASSWORD = '{2}',UPDATEDON ='{3}',UPDATEDBY = {4} WHERE ID= {5}";
        private const string DELETE_QUERY = "DELETE FROM PLANNER WHERE ID = {0}";

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
                UpdatedByUserName = dr.Field<string>("UpdatedByUserName")
            };
            return planner;
        }
        public void Add(Planner planner)
        {
            try
            {
                DataBase.DBService.ExecuteCommand(string.Format(INSERT_QUERY,
                    planner.ClientId, planner.Name, planner.StartDate.ToString("yyyy-MM-dd"),
                    planner.EndDate.ToString("yyyy-MM-dd"), planner.IsActive,
                    planner.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss"), planner.CreatedBy,
                    planner.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), planner.UpdatedBy));

                Activity.ActivitiesService.Add(ActivityType.CreatePlan, EntryStatus.Success,
                         Source.Server, planner.UpdatedByUserName, planner.Name, planner.MachineName);
            }
            catch (Exception ex)
            {
                FinancialPlanner.Common.Logger.LogDebug(ex.Message);

            }
        }
    }
}
