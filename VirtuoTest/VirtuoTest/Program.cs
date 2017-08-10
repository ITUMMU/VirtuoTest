using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace VirtuoTest
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Regex regex = new Regex("[ ]{2,}", RegexOptions.None);
                String tempFileName = System.IO.Path.GetTempFileName();
                String sqlServer = @"itum-pg\virtuo";
                Int32 sqlTableCount = 0;
                Int32 sqlLines = 0;

                System.IO.StreamWriter file = new System.IO.StreamWriter(tempFileName);

                Console.WriteLine(@"Nom de l'ordinateur: " + System.Environment.MachineName);
                SqlConnection connection = new SqlConnection(@"Server=" + sqlServer + @"; Database=virtuo; Integrated Security=SSPI; Trusted_Connection=yes;");
                connection.Open();
                Console.WriteLine(@"Connecté à " + sqlServer);

                Console.WriteLine("Obtention de la liste des tables à exporter");
                SqlCommand command = new SqlCommand(@"SELECT TABLE_NAME FROM virtuo.INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'", connection);
                SqlDataReader dataReader = command.ExecuteReader();
                List<String> tableNames = new List<String>();
                while (dataReader.Read())
                {
                    tableNames.Add(dataReader.GetValue(0).ToString());
                    sqlTableCount++;
                }

                dataReader.Close();

                Console.WriteLine(sqlTableCount + @" tables à exporter");

                Console.WriteLine(@"Exportation vers le fichier " + tempFileName);

                var watch = System.Diagnostics.Stopwatch.StartNew();

                foreach (String tableName in tableNames)
                {
                    file.WriteLine(@"Parsing table : " + tableName);
                    command = new SqlCommand(@"SELECT * FROM virtuo.dbo." + tableName, connection);
                    dataReader = command.ExecuteReader();
                    Object[] obj = new Object[dataReader.FieldCount];

                    for (int i = 0; i < dataReader.FieldCount; i++)
                        obj[i] = dataReader.GetName(i);

                    file.WriteLine(String.Join(", ", obj));

                    while (dataReader.Read())
                    {
                        dataReader.GetValues(obj);
                        file.WriteLine(regex.Replace(String.Join(", ", obj), " "));
                        sqlLines++;
                    }

                    dataReader.Close();
                    file.WriteLine();
                }

                watch.Stop();

                file.Close();

                Console.WriteLine(@"Durée de l'exportation " + watch.Elapsed + ", " + sqlLines + " Lignes de données écrites dans le fichier.");
                Console.WriteLine("Appuyez sur retour pour sortir.");
                command.Dispose();
                connection.Close();
                Console.ReadLine();
            }

            catch (Exception ex)
            {
                Console.WriteLine("Can not open connection ! ");
                Console.WriteLine(ex.ToString());
                Console.ReadLine();
            }
        }
    }
}
