
using LumenWorks.Framework.IO.Csv;
using System.Data;
using System.Diagnostics.CodeAnalysis;

namespace TANWeb.Areas.Inventory.Services
{
    [ExcludeFromCodeCoverage]
    public class CsvReaderService
    {
        public CsvReaderService()
        {
            Console.WriteLine("Inside CsvReaderService ctor");
        }

        public DataTable LocalFileCsvReader()
        {
            DataTable csvTable = new();
            using (var csvReader = new CsvReader(new StreamReader(File.OpenRead(@"C:\Users\ASeebeck\Downloads\bhc_paydata_20220426_140500.csv")), true))
            {
                csvTable.Load(csvReader);
            }

            return csvTable;
        }

        public DataTable CsvStreamToDataTable(Stream file)
        {
            Console.WriteLine("Inside CsvStreamToDataTable");
            try
            {
                DataTable csvTable = new DataTable();
                using (var csvReader = new CsvReader(new StreamReader(file), true))
                {
                    csvTable.Load(csvReader);
                }

                Console.WriteLine("Successfully created datatable from file stream");

                return csvTable;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CsvStreamToDataTable threw an error: {ex.Message}");
                return null;
            }
        }
    }
}
