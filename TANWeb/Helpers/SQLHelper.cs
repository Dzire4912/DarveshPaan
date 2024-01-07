using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;

namespace TANWeb.Helpers
{
    [ExcludeFromCodeCoverage]
    public class SQLHelper
    {
        private readonly IConfiguration _configuration;
        string strConString = "";
        SqlConnection con;
        SqlCommand cmd;
        DataTable dt;
        SqlDataReader dr;
        public SQLHelper()
        {
           // strConString = _configuration.GetValue<string>("ConnectionStrings:Defaultcon");
        }

        public DataTable Get_DataTable_SP(String procedureName, SqlParameter[] parameters)
        {

            try
            {
                con = new SqlConnection(strConString);
                cmd = new SqlCommand();
                dt = new DataTable();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = procedureName;
                cmd.CommandTimeout = 600000;
                if (parameters != null)
                {
                    cmd.Parameters.AddRange(parameters);
                }
                cmd.Connection = con;
                if (con.State == ConnectionState.Closed)
                {
                    con.Close();
                    con.Open();
                }
                dr = cmd.ExecuteReader();
                dt.Load(dr);

            }
            catch (Exception ex)
            { 
                throw; 
            }
            finally
            {
                dispose();
            }
            return dt;

        }

        public void dispose()
        {
            con.Close();
            cmd.Dispose();
            dr.Close();

        }
    }
}
