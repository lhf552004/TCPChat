using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Data;
using System.Drawing;

namespace ChatWeb
{
    public partial class Register : System.Web.UI.Page
    {
       
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.ContentType = "text/HTML";
        }
        /// <summary>
        /// try to connect to SQL server
        /// </summary>
        private void connectToSQLServer()
        {
            if (sqlConn == null)
            {
                sqlConn = new SqlConnection(connectString);
            }
            if (sqlConn.State == ConnectionState.Closed)
            {
                sqlConn.Open();
            }
        }
        SqlConnection sqlConn;
        string connectString = System.Configuration.ConfigurationManager.ConnectionStrings["sqlConnectionString"].ConnectionString;
        private bool UserChecked()
        {
            bool IsExisted = false;
            string queryString = "";
            connectToSQLServer();
            SqlCommand thiscommand = sqlConn.CreateCommand();
            queryString = "select ID, Name from [User] where Ident = '" + IdentText.Text.Trim() + "'";
            thiscommand.CommandText = queryString;
            SqlDataReader rd = thiscommand.ExecuteReader();
            while (rd.Read())
            {
                IsExisted = true;
                break;
            }
            rd.Close();
            return IsExisted;
        }
        private bool checkPassword()
        {
            bool passwordIsValid = false;
            if (!string.IsNullOrEmpty(NewPasswordText.Text) && NewPasswordText.Text == ConfirmPasswordText.Text)
            {
                passwordIsValid = true;
            }
            return passwordIsValid;
        }
        protected void IdentText_TextChanged(object sender, EventArgs e)
        {
            if (UserChecked() == true)
            {
                ErrorInfoLabel.Text = "Ident is existed.";
                ErrorInfoLabel.ForeColor = Color.Red;
            }
            else
            {
                ErrorInfoLabel.Text = "Ident can be used.";
                ErrorInfoLabel.ForeColor = Color.Green;
            }
        }

        protected void RegisterButton_Click(object sender, EventArgs e)
        {
            string ident = IdentText.Text.Trim();
            string name = NameText.Text.Trim();
            string gender = GenderDropdwon.SelectedValue;
            string password = "";
            //Pre-check
            if (UserChecked())
            {
                Response.Write("<script>alert('Ident is not valid!')</script>");
                return;
            }
            if (string.IsNullOrEmpty(ident) || string.IsNullOrEmpty(name))
            {
                Response.Write("<script>alert('Ident or name should not be empty!')</script>");
                return;
            }
            if (!checkPassword())
            {
                Response.Write("<script>alert('Password is empty or not same!')</script>");
                return;
            }
            else
            {
                password = NewPasswordText.Text.Trim();
            }
            //--------------
            try
            {
                string insertString = "INSERT INTO [dbo].[User]([Ident],[Name],[Password],[Gender]) VALUES( '" +
               ident + " ', '" +
               name + "', '" +
               password + "', " +
               int.Parse(gender) + " )";
                connectToSQLServer();
                SqlCommand thiscommand = sqlConn.CreateCommand();
                thiscommand.CommandText = insertString;
                thiscommand.ExecuteNonQuery();
                afterSuccess();
            }
            catch (Exception ex)
            {
                Response.Write("<script>alert('"  + ex.Message + "!')</script>");
            }

        }

        private void afterSuccess()
        {
            IdentText.Text = "";
            NameText.Text = "";
            GenderDropdwon.Text = "";
            NewPasswordText.Text = ConfirmPasswordText.Text = "";
            ResultLabel.Text = "Register is successfull.";
        }
    }
}