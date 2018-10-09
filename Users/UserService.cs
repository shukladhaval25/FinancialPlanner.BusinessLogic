using FinancialPlanner.Common.DataEncrypterDecrypter;
using FinancialPlanner.Common.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialPlanner.BusinessLogic.Users
{
    public class UserService
    {
        private const string INSERT_QUERY = "INSERT INTO USERS VALUES ({0},'{1}','{2}','{3}','{4}','{5}',{6},'{7}',{8})";
        private const string SELECT_ALL = "SELECT U1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM USERS U1, USERS U WHERE U1.UPDATEDBY = U.ID";
        private const string SELECT_ID = "SELECT U1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM USERS U1, USERS U WHERE U1.UPDATEDBY = U.ID and U1.ID = {0}";

        private const string SELECT_BY_NAME = "SELECT U1.*,U.USERNAME AS UPDATEDBYUSERNAME FROM USERS U1, USERS U WHERE U1.UPDATEDBY = U.ID and U1.USERNAME = '{0}'";
        private const string SELECT_MAX_ID = "SELECT MAX(ID) FROM USERS";
        private const string UPDATE_QUERY = "UPDATE USERS SET FIRSTNAME = '{0}'," +
                "LASTNAME = '{1}', PASSWORD = '{2}',UPDATEDON ='{3}',UPDATEDBY = {4} WHERE ID= {5}";
        private const string DELETE_QUERY = "DELETE FROM USERS WHERE ID = {0}";

        public IList<User> Get()
        {
            IList<User> lstUsers = new List<User>();
            
            DataTable dtAppConfig =  DataBase.DBService.ExecuteCommand(SELECT_ALL);
            foreach (DataRow dr in dtAppConfig.Rows)
            {
                User user = convertToUserObject(dr);
                lstUsers.Add(user);
            }
            return lstUsers;
        }

        private static User convertToUserObject(DataRow dr)
        {
            User user = new User();
            user.Id = dr.Field<int>("ID");
            user.UserName = dr.Field<string>("UserName");
            user.FirstName = dr.Field<string>("FirstName");
            user.LastName = dr.Field<string>("LastName");
            user.Password = dr.Field<string>("Password");
            user.CreatedOn = dr.Field<DateTime>("CreatedOn");
            user.CreatedBy = dr.Field<int>("CreatedBy");
            user.UpdatedOn = dr.Field<DateTime>("UpdatedOn");
            user.UpdatedBy = dr.Field<int>("UpdatedBy");
            user.UpdatedByUserName = dr.Field<string>("UpdatedByUserName");
            return user;
        }

        public void SetAdminAccount(User user)
        {
            try
            {
                string value = DataBase.DBService.ExecuteCommandScalar(SELECT_MAX_ID);
                value = (string.IsNullOrEmpty(value)) ? "0" : value;
                int maxValue = int.Parse(value) + 1;
                DataBase.DBService.ExecuteCommand(string.Format(INSERT_QUERY,
                    maxValue, user.UserName, user.FirstName, user.LastName, user.Password,
                    user.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss"), maxValue, user.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), maxValue));
                Activity.ActivitiesService.Add(ActivityType.CreateUser, EntryStatus.Success,
                         Source.Server, user.UpdatedByUserName, user.UserName, user.MachineName);
            }
            catch (Exception ex)
            {
                FinancialPlanner.Common.Logger.LogDebug(ex.Message);

            }
        }

        public User Get(int id)
        {
            User user = new User();
            DataTable dtAppConfig =  DataBase.DBService.ExecuteCommand(string.Format(SELECT_ID,id));
            foreach (DataRow dr in dtAppConfig.Rows)
            {
                user = convertToUserObject(dr);
            }
            return user;
        }

        public User GetUserByName(string username)
        {
            User user = new User();
            DataTable dtAppConfig =  DataBase.DBService.ExecuteCommand(string.Format(SELECT_BY_NAME,username));
            foreach (DataRow dr in dtAppConfig.Rows)
            {
                user = convertToUserObject(dr);
            }
            return user;
        }
        
        public void Add(User user)
        {
            try
            {
                string value = DataBase.DBService.ExecuteCommandScalar(SELECT_MAX_ID);
                value = (string.IsNullOrEmpty(value)) ? "0" : value;
                int maxValue = int.Parse(value) + 1;
                DataBase.DBService.ExecuteCommand(string.Format(INSERT_QUERY,
                    maxValue, user.UserName, user.FirstName, user.LastName, user.Password,
                    user.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss"), user.CreatedBy, user.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), user.UpdatedBy));
                Activity.ActivitiesService.Add(ActivityType.CreateUser, EntryStatus.Success,
                         Source.Server, user.UpdatedByUserName, user.UserName, user.MachineName);
            }
            catch (Exception ex)
            {               
                FinancialPlanner.Common.Logger.LogDebug(ex.Message);

            }
        }

        public void Update(User user)
        {
            DataBase.DBService.ExecuteCommandString(string.Format(UPDATE_QUERY,
                user.FirstName, user.LastName,user.Password, user.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"), user.UpdatedBy, user.Id));
            Activity.ActivitiesService.Add(ActivityType.UpdateUser, EntryStatus.Success,
                        Source.Server, user.UpdatedByUserName, user.UserName, user.MachineName);
        }

        public void Delete(User user)
        {
            DataBase.DBService.ExecuteCommandString(string.Format(DELETE_QUERY, user.Id));
            Activity.ActivitiesService.Add(ActivityType.DeleteUser, EntryStatus.Success,
                        Source.Server, user.UpdatedByUserName, user.UserName, user.MachineName);
        }
    }   
}
