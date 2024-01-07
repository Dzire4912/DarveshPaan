using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAN.Repository.Serilogs
{
    [ExcludeFromCodeCoverage]
    public class DeleteOldRecords
    {
        public static void Delete(string connectionString, string TableName,DateTime Retention, string? schena = null)
        {
            #region Delete old Records 
            var cutoff = Retention;
            
                    using (var connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        using (var command = new SqlCommand($"DELETE FROM {TableName} WHERE Timestamp < @Cutoff and Level !='Error'", connection))
                        {
                            command.Parameters.AddWithValue("Cutoff", cutoff);
                            command.ExecuteNonQuery();
                        }
                        connection.Close();
                    }              
            
            #endregion
        }
        
    }
}
