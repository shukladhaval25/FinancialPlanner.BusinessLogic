using FinancialPlanner.Common.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialPlanner.BusinessLogic
{
    public class EmailArticleService
    {

        private const string SELECT_COUNT_BY_GROUPID = "SELECT COUNT(ID) FROM EMAILARTICLE WHERE GROUPID = {0}";
        private const string SELECT_ALL = "SELECT EA.*,U.USERNAME AS UPDATEDBYUSERNAME, EG.NAME AS GROUPNAME FROM EMAILARTICLE EA," +
            "USERS U,EMAILGROUP EG WHERE EA.UPDATEDBY = U.ID AND EA.GROUPID = EG.ID";
        private const string INSERT_EMAILARTICLE_QUERY = "INSERT INTO EMAILARTICLE VALUES ('{0}','{1}','{2}','{3}','{4}',{5},'{6}',{7})";
        private const string UPDATE_EMAILARTICLE_QUERY = "UPDATE EMAILARTICLE SET [GROUPID] = {0},TITLE ='{1}',CONTENTPATH ='{2}',DESCRIPTION ='{3}'," +
            "UPDATEDON ='{4}',UPDATEDBY ={5} WHERE ID = {6}";
        private const string DELETE_EMAILARTICLE_QUERY = "DELETE FROM EMAILARTICLE WHERE ID ={0}";

        #region "Email Group"
        private const string SELECT_NAME_BY_ID = "SELECT NAME FROM EMAILGROUP WHERE ID = {0}";
        private const string SELECT_ID_BY_NAME = "SELECT ID FROM EMAILGROUP WHERE NAME ='{0}'";
        private const string INSERT_EMAILGROUP_QUERY = "INSERT INTO EMAILGROUP VALUES ('{0}')";
        private const string UPDATE_EMAILGROUP = "UPDATE EMAILGROUP SET NAME ='{0}' WHERE ID = {1}";
        private const string DELETE_EMAILGROUP ="DELETE FROM EMAILGROUP WHERE ID = {0}";
        #endregion

        public object Get()
        {
            IList<EmailArticle> lstEmailArticle = new List<EmailArticle>();

            DataTable dtEmailArticle =  DataBase.DBService.ExecuteCommand(SELECT_ALL);
            ///DataTable dtEmailArticle = getDummyArticleData();
            foreach (DataRow dr in dtEmailArticle.Rows)
            {
                EmailArticle emailArticle = new EmailArticle();
                emailArticle.ID = dr.Field<int>("ID");
                emailArticle.GroupId = dr.Field<int>("GroupId");
                emailArticle.GroupName = dr.Field<string>("GroupName");
                emailArticle.Title = dr.Field<string>("Title");
                emailArticle.ContentFilePath = dr.Field<string>("ContentPath");
                emailArticle.Description = dr.Field<string>("Description");
                emailArticle.CreatedOn = dr.Field<DateTime>("CreatedOn");
                emailArticle.CreatedBy = dr.Field<int>("CreatedBy");
                emailArticle.UpdatedOn = dr.Field<DateTime>("UpdatedOn");
                emailArticle.UpdatedBy = dr.Field<int>("UpdatedBy");
                emailArticle.UpdatedByUserName = dr.Field<string>("UpdatedByUserName");

                lstEmailArticle.Add(emailArticle);
            }
            return lstEmailArticle;
        }

        public void Update(EmailArticle emailArticle)
        {
            updateGroupDetail(emailArticle);
            DataBase.DBService.ExecuteCommand(
                string.Format(UPDATE_EMAILARTICLE_QUERY, emailArticle.GroupId,
                emailArticle.Title,
                emailArticle.ContentFilePath,
                emailArticle.Description,
                emailArticle.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"),
                emailArticle.UpdatedBy, emailArticle.ID));

            Activity.ActivitiesService.Add(ActivityType.UpdateEmailArticle, EntryStatus.Success,
                Source.Client, emailArticle.UpdatedByUserName, emailArticle.Title, emailArticle.MachineName);
        }

        public void Delete(EmailArticle emailArticle)
        {
            DataBase.DBService.ExecuteCommand(
                string.Format(DELETE_EMAILARTICLE_QUERY, emailArticle.ID));

            string count = DataBase.DBService.ExecuteCommandScalar(
                string.Format(SELECT_COUNT_BY_GROUPID, emailArticle.GroupId));

            if (int.Parse(count) == 0)
                DataBase.DBService.ExecuteCommand(
               string.Format(DELETE_EMAILGROUP, emailArticle.GroupId));

            Activity.ActivitiesService.Add(ActivityType.DeleteEmailArticle, EntryStatus.Success,
                Source.Client, emailArticle.UpdatedByUserName, emailArticle.Title, emailArticle.MachineName);
        }

        public void Add(EmailArticle emailArticle)
        {


            updateGroupDetail(emailArticle);

            DataBase.DBService.ExecuteCommand(
                string.Format(INSERT_EMAILARTICLE_QUERY, emailArticle.GroupId,
                emailArticle.Title,
                emailArticle.ContentFilePath,
                emailArticle.Description,
                emailArticle.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss"),
                emailArticle.CreatedBy,
                emailArticle.UpdatedOn.ToString("yyyy-MM-dd hh:mm:ss"),
                emailArticle.UpdatedBy));

            Activity.ActivitiesService.Add(ActivityType.CreateEmailArticle, EntryStatus.Success,
                Source.Client, emailArticle.UpdatedByUserName, emailArticle.Title, emailArticle.MachineName);

        }

        private void updateGroupDetail(EmailArticle emailArticle)
        {
            string groupId =  DataBase.DBService.ExecuteCommandScalar(string.Format(SELECT_ID_BY_NAME,emailArticle.GroupName));
            if (!string.IsNullOrEmpty(groupId))
                emailArticle.GroupId = int.Parse(groupId);
            else
            {
                DataBase.DBService.ExecuteCommand(
                 string.Format(INSERT_EMAILGROUP_QUERY, emailArticle.GroupName));
                groupId = DataBase.DBService.ExecuteCommandScalar(string.Format(SELECT_ID_BY_NAME, emailArticle.GroupName));
                emailArticle.GroupId = int.Parse(groupId);
            }
        }

        private DataTable getDummyArticleData()
        {
            DataTable dtTemp = new  DataTable();
            dtTemp.Columns.Add("ID", System.Type.GetType("System.String"));
            dtTemp.Columns.Add("Group");
            dtTemp.Columns.Add("Title");
            dtTemp.Columns.Add("ContentFilePath");
            dtTemp.Columns.Add("Description");
            dtTemp.Columns.Add("CreatedOn", System.Type.GetType("System.DateTime"));
            dtTemp.Columns.Add("CreatedBy", System.Type.GetType("System.String"));
            dtTemp.Columns.Add("UpdatedOn", System.Type.GetType("System.DateTime"));
            dtTemp.Columns.Add("UpdatedBy", System.Type.GetType("System.String"));
            dtTemp.Columns.Add("UpdatedByUserName");

            DataRow dr = dtTemp.NewRow();
            dr[0] = "1";
            dr[1] = "Weekly-Article";
            dr[2] = "First step for financial planning";
            dr[3] = @"E:\Article\First1";
            dr[4] = "Details about first step for financial planning.";
            dr[5] = "2018-07-23";
            dr[6] = "1";
            dr[7] = "2018-07-23";
            dr[8] = "1";
            dr[9] = "Admin";
            dtTemp.Rows.Add(dr);

            dr = dtTemp.NewRow();
            dr[0] = "2";
            dr[1] = "Weekly-Article";
            dr[2] = "Second step for financial planning";
            dr[3] = @"E:\Article\First2";
            dr[4] = "Details about second step for financial planning.";
            dr[5] = "2018-07-28";
            dr[6] = "1";
            dr[7] = "2018-07-28";
            dr[8] = "1";
            dr[9] = "Admin";
            dtTemp.Rows.Add(dr);

            dr = dtTemp.NewRow();
            dr[0] = "3";
            dr[1] = "Festival-Session";
            dr[2] = "Festivals are important for investment too.";
            dr[3] = @"E:\Article\FS1";
            dr[4] = "Details about second step for financial planning.";
            dr[5] = "2018-07-28";
            dr[6] = "1";
            dr[7] = "2018-07-28";
            dr[8] = "1";
            dr[9] = "Admin";
            dtTemp.Rows.Add(dr);

            return dtTemp;
        }
    }
}
