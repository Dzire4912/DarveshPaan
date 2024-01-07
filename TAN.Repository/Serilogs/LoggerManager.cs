using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAN.Repository.Serilogs
{
    [ExcludeFromCodeCoverage]
    public static class LoggerManager
    {
        public static void InitializeLogger(string ConnectionString, string TableName = null, Collection<DataColumn> SqlColumn = null)
        {
            if (string.IsNullOrEmpty(TableName))
            {
                TableName = "Logs";
            }         
           
            #region Additional Column Writer for SQL 
            var columnOptions = new Serilog.Sinks.MSSqlServer.ColumnOptions
            {
                AdditionalDataColumns = new Collection<DataColumn>
                {
                    //To Add More Columns
                }
            };
            if (SqlColumn != null)
            {
                foreach (var item in SqlColumn)
                {
                    columnOptions.AdditionalDataColumns.Add(item);
                }
            }

            #endregion          

            #region Configure Database sinks
            
                    Log.Logger = new LoggerConfiguration()
                   .MinimumLevel.Information().Enrich.FromLogContext()
                   .WriteTo.MSSqlServer(ConnectionString, TableName, columnOptions: columnOptions, autoCreateSqlTable: true)
                   .CreateLogger();                
                        
            #endregion
        }
    }
}
