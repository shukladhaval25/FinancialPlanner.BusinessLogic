using FinancialPlanner.Common.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialPlanner.BusinessLogic.PlanOption
{
    public class NetWorthService
    {
        private const string INSERT_QUERY = "INSERT INTO [dbo].[NetWorth] ([CID],[Year],[NetWorth],[CreatedOn],[CreatedBy],[UpdatedOn],[UpdatedBy]) VALUES ({0},{1},{2},'{3}',{4},'{5}',{6})";

        private const string UPDATE_QUERY = "UPDATE NETWORTH SET YEAR = {0},NETWORTH = {1} WHERE ID = {2}";

        private const string SELECT_ALL = "SELECT N1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM NETWORTH N1, USERS U WHERE N1.UPDATEDBY = U.ID AND N1.CID = {0} ORDER BY YEAR";

        private const string SELECT_ID = "SELECT N1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM NETWORTH N1, USERS U WHERE N1.UPDATEDBY = U.ID AND N1.ID = {0}";

        private const string DELETE_QUERY = "DELETE FROM NETWORTH WHERE ID = {0}";
        public IList<NetWorth> Get(int clientId)
        {
            IList<NetWorth> lstNetWorth = new List<NetWorth>();

            DataTable dtAppConfig = DataBase.DBService.ExecuteCommand(string.Format(SELECT_ALL,clientId));

            foreach (DataRow dr in dtAppConfig.Rows)
            {
                NetWorth  netWorth = convertToNetWorthObject(dr);
                lstNetWorth.Add(netWorth);
            }
            return lstNetWorth;
        }

        private static NetWorth  convertToNetWorthObject(DataRow dr)
        {
            //NetWorth netWorth = new NetWorth()
            //{
            //    Id = dr.Field<int>("Id"),
            //    CId = dr.Field<int>("CID"),
            //    Year = dr.Field<int>("Year"),
            //    Amount = double.Parse(dr["NetWorth"].ToString()),
            //    CreatedOn = dr.Field<DateTime>("CreatedOn"),
            //    CreatedBy = dr.Field<int>("CreatedBy"),
            //    UpdatedOn = dr.Field<DateTime>("UpdatedOn"),
            //    UpdatedBy = dr.Field<int>("UpdatedBy"),
            //    UpdatedByUserName = dr.Field<string>("UpdatedByUserName")
            //};
            NetWorth  netWorth = new NetWorth();
            netWorth.Id = dr.Field<int>("Id");
            netWorth.CId = dr.Field<int>("CID");
            netWorth.Year = dr.Field<int>("Year");
            netWorth.Amount = double.Parse(dr["Networth"].ToString());
            netWorth.CreatedOn = dr.Field<DateTime>("CreatedOn");
            netWorth.CreatedBy = dr.Field<int>("CreatedBy");
            netWorth.UpdatedOn = dr.Field<DateTime>("UpdatedOn");
            netWorth.UpdatedBy = dr.Field<int>("UpdatedBy");
            netWorth.UpdatedByUserName = dr.Field<string>("UpdatedByUserName");

            return netWorth;
        }

        public void Add(NetWorth  netWorth)
        {
            try
            {
                //string clientName = DataBase.DBService.ExecuteCommandScalar(string.Format(GET_CLIENT_NAME_QUERY, netWorth.CId));

                DataBase.DBService.ExecuteCommandString(string.Format(INSERT_QUERY,
                      netWorth.CId, netWorth.Year, netWorth.Amount, netWorth.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss"), netWorth.CreatedBy, netWorth.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), netWorth.UpdatedBy));
               // Activity.ActivitiesService.Add(ActivityType.CreateUser, EntryStatus.Success,
               //          Source.Server, user.UpdatedByUserName, user.UserName, user.MachineName);
            }
            catch (Exception ex)
            {
                FinancialPlanner.Common.Logger.LogDebug(ex.Message);
            }
        }

        public void Update(NetWorth  netWorth)
        {
            try
            {
                DataBase.DBService.ExecuteCommandString(string.Format(UPDATE_QUERY,
                    netWorth.Year, netWorth.Amount, netWorth.Id));
            }
            //Activity.ActivitiesService.Add(ActivityType.UpdateUser, EntryStatus.Success,
            //           Source.Server, user.UpdatedByUserName, user.UserName, user.MachineName);
            catch (Exception ex)
            {
                FinancialPlanner.Common.Logger.LogDebug(ex.Message);
            }
        }

        public void Delete(NetWorth netWorth)
        {
            try
            {
                DataBase.DBService.ExecuteCommandString(string.Format(DELETE_QUERY, netWorth.Id), true);
            }
            catch(Exception ex)
            {
                FinancialPlanner.Common.Logger.LogDebug(ex.Message);
            }
        }
    }
}
