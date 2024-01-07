using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace TANWeb.Helpers
{
    public class Utils
    {
        [ExcludeFromCodeCoverage]
        public string GenerateOTP()
        {
            Random random = new Random();
            string value = Convert.ToString(random.Next(100001, 999999));
            return value;
        }

        public string CreateDummyPassword()
        { 
                Random ran = new Random();

                string Characters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
                string Numbers = "1234567890";
                string sc = "!@#$%^&*";
                string random = "";

                for (int i = 0; i < 10; i++)
                {
                    int a = ran.Next(Characters.Length);
                    random = random + Characters.ElementAt(a);
                }
                for (int i = 0; i < 1; i++)
                {
                    int a = ran.Next(Numbers.Length);
                    random = random + Numbers.ElementAt(a);
                }
                for (int j = 0; j < 2; j++)
                {
                    int sz = ran.Next(sc.Length);
                    random = random + sc.ElementAt(sz);
                }
                return random; 
        }
        [ExcludeFromCodeCoverage]
        public List<string> ConvertDataTableToList(DataTable dataTable)
        {
            List<string> list = new List<string>();

            foreach (DataRow row in dataTable.Rows)
            {
                foreach (DataColumn column in dataTable.Columns)
                {
                    string? value = row[column].ToString();
                    list.Add(value);
                }
            }

            return list;
        }

    }
}
