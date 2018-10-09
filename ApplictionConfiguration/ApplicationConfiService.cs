using FinancialPlanner.Common.Model;
using System.Collections.Generic;
using System.Data;
using System;

namespace FinancialPlanner.BusinessLogic.ApplictionConfiguration
{
    public class ApplicationConfiService
    {
        private const string INSERT_QUERY = "INSERT INTO ApplicationConfiguration VALUES ('{0}','{1}','{2}','{3}',{4},'{5}',{6})";
        private const string SELECT_ALL = "SELECT AC.*,U.USERNAME AS UPDATEDBYUSERNAME FROM APPLICATIONCONFIGURATION AC, USERS U WHERE AC.UPDATEDBY = U.ID";
        private const string SELECT_ID = "SELECT AC.*,U.USERNAME AS UPDATEDBYUSERNAME FROM APPLICATIONCONFIGURATION AC, USERS U WHERE AC.UPDATEDBY = U.ID and AC.ID = {0}";
        private const string SELECT_MAX_ID = "SELECT MAX(ID) FROM APPLICATIONCONFIGURATION";
        private const string UPDATE_QUERY = "UPDATE APPLICATIONCONFIGURATION SET SETTINGKEY = '{0}'," +
                "VALUE = '{1}', UPDATEDON = '{2}',UPDATEDBY = {3} WHERE ID= {4}";
        private const string DELETE_QUERY = "DELETE FROM APPLICATIONCONFIGURATION WHERE ID = {0}";
        private const string DELETE_QUERY_BY_CATEGORY ="DELETE FROM APPLICATIONCONFIGURATION WHERE CATEGORY ='{0}'";
        public IList<FinancialPlanner.Common.Model.ApplicationConfiguration> Get()
        {
            IList<FinancialPlanner.Common.Model.ApplicationConfiguration> lstAppConfig = new List<FinancialPlanner.Common.Model.ApplicationConfiguration>();


            DataTable dtAppConfig =  DataBase.DBService.ExecuteCommand(SELECT_ALL);
            foreach (DataRow dr in dtAppConfig.Rows)
            {
                FinancialPlanner.Common.Model.ApplicationConfiguration appConfig = new FinancialPlanner.Common.Model.ApplicationConfiguration();
                appConfig.Id = dr.Field<int>("ID");
                appConfig.Category = dr.Field<string>("Category");
                appConfig.SettingName = dr.Field<string>("SettingKey");
                appConfig.SettingValue = dr.Field<string>("Value");
                appConfig.CreatedOn = dr.Field<DateTime>("CreatedOn");
                appConfig.CreatedBy = dr.Field<int>("CreatedBy");
                appConfig.UpdatedOn = dr.Field<DateTime>("UpdatedOn");
                appConfig.UpdatedBy = dr.Field<int>("UpdatedBy");
                appConfig.UpdatedByUserName = dr.Field<string>("UpdatedByUserName");

                lstAppConfig.Add(appConfig);
            }
            return lstAppConfig;
        }

        public FinancialPlanner.Common.Model.ApplicationConfiguration Get(int id)
        {
            FinancialPlanner.Common.Model.ApplicationConfiguration appConfig = new FinancialPlanner.Common.Model.ApplicationConfiguration();
            DataTable dtAppConfig =  DataBase.DBService.ExecuteCommand(string.Format(SELECT_ID,id));
            foreach (DataRow dr in dtAppConfig.Rows)
            {
                appConfig.Id = dr.Field<int>("ID");
                appConfig.Category = dr.Field<string>("Category");
                appConfig.SettingName = dr.Field<string>("SettingKey");
                appConfig.SettingValue = dr.Field<string>("Value");
                appConfig.CreatedOn = dr.Field<DateTime>("CreatedOn");
                appConfig.CreatedBy = dr.Field<int>("CreatedBy");
                appConfig.UpdatedOn = dr.Field<DateTime>("UpdatedOn");
                appConfig.UpdatedBy = dr.Field<int>("UpdatedBy");
                appConfig.UpdatedByUserName = dr.Field<string>("UpdatedByUserName");
            }
            return appConfig;
        }

        public void Add(ApplicationConfiguration appConfig)
        {
            DataBase.DBService.ExecuteCommand(string.Format(INSERT_QUERY,
                appConfig.Category, appConfig.SettingName, appConfig.SettingValue,
                appConfig.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss"), appConfig.CreatedBy,
                appConfig.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), appConfig.UpdatedBy));
            Activity.ActivitiesService.Add(ActivityType.UpdateSystemSetting, EntryStatus.Success,
                        Source.Server, appConfig.UpdatedByUserName, appConfig.SettingName, appConfig.MachineName);
        }

        public void Update(IList<ApplicationConfiguration> lstappConfig)
        {
            if (lstappConfig.Count > 0)
            {
                DataBase.DBService.ExecuteCommand(string.Format(DELETE_QUERY_BY_CATEGORY,
                    lstappConfig[0].Category));
                Add(lstappConfig);
            }
        }

        public void Add(IList<ApplicationConfiguration> lstappConfig)
        {
            try
            {
                if (lstappConfig.Count > 0)
                {
                    DataBase.DBService.BeginTransaction();
                    foreach (ApplicationConfiguration appConfig in lstappConfig)
                    {
                        DataBase.DBService.ExecuteCommandString(string.Format(INSERT_QUERY,
                        appConfig.Category, appConfig.SettingName, appConfig.SettingValue,
                        appConfig.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss"), appConfig.CreatedBy,
                        appConfig.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), appConfig.UpdatedBy), true);
                    }
                    DataBase.DBService.CommitTransaction();

                    Activity.ActivitiesService.Add(ActivityType.UpdateSystemSetting, EntryStatus.Success,
                           Source.Server, lstappConfig[0].UpdatedByUserName, lstappConfig[0].SettingName,
                           lstappConfig[0].MachineName);
                }
            }
            catch (Exception ex)
            {
                DataBase.DBService.RollbackTransaction();
                throw ex;
            }
        }

        public void Update(ApplicationConfiguration appConfig)
        {
            DataBase.DBService.ExecuteCommand(string.Format(UPDATE_QUERY,
                appConfig.SettingName, appConfig.SettingValue,
                appConfig.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), appConfig.UpdatedBy, appConfig.Id));

            Activity.ActivitiesService.Add(ActivityType.UpdateSystemSetting, EntryStatus.Success,
                       Source.Server, appConfig.UpdatedByUserName, appConfig.SettingName, appConfig.MachineName);
        }

        public void Delete(int id)
        {
            DataBase.DBService.ExecuteCommandString(string.Format(DELETE_QUERY, id));
        }
    }
}
