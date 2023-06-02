using Microsoft.Data.Sqlite;
using Microsoft.VisualBasic.FileIO;
using TextoIt.API.GoldenRaspberryAwards.Models;
using TextoIt.API.GoldenRaspberryAwards.Repository;

namespace TextoIt.API.GoldenRaspberryAwards
{
    public class CustomStartup
    {
        public CustomStartup() { }

        public void StartUp()
        {
            string? dbFilePath = System.Environment.GetEnvironmentVariable("DBFilePath");
            string? csvFilePath = System.Environment.GetEnvironmentVariable("CSVFilePath");
            string? dbName = System.Environment.GetEnvironmentVariable("DBName");
            
            if (dbFilePath == null) throw new NullReferenceException("Please, configure the DBFilePath in launchSettings.json");
            if (csvFilePath == null) throw new NullReferenceException("Please, configure the CSVFilePath in launchSettings.json");
            if (dbName == null) throw new NullReferenceException("Please, configure the DBName in launchSettings.json");

            List<MoviesModel> rows = ReadCSVFile(csvFilePath);
            CreateDB(dbFilePath, dbName);
            InsertRowsInDB(rows, dbFilePath, dbName);
        }

        private List<MoviesModel> ReadCSVFile(string csvFilePath)
        {
            try
            {
                using (var parser = new TextFieldParser(csvFilePath))
                {
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(";");

                    List<MoviesModel> rows = new List<MoviesModel>();

                    if (parser.ReadFields() != null)
                    {
                        while (!parser.EndOfData)
                        {
                            var fields = parser.ReadFields();
                            if (fields != null)
                            {
                                int year = Convert.ToInt32(fields[0]);
                                string title = fields[1];
                                string studio = fields[2];
                                string producers = fields[3];
                                string winner = fields[4];
                                MoviesModel movie = new MoviesModel(year, title, studio, producers, winner);
                                rows.Add(movie);
                            }
                        }
                    }
                    return rows;
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        private void CreateDB(string dbFilePath, string dbName)
        {
            try
            {
                File.Delete(dbFilePath);

                SQLitePCL.Batteries.Init();
                using (var connection = new SqliteConnection($"Data Source={dbFilePath}"))
                {
                    connection.Open();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = string.Format("CREATE TABLE IF NOT EXISTS {0} (year INTEGER , title TEXT, studio TEXT, producers TEXT, winner TEXT, PRIMARY KEY (year, title))", dbName);

                        command.ExecuteNonQuery();
                    }
                    connection.Close();
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        private void InsertRowsInDB(List<MoviesModel> rows, string dbFilePath, string dbName)
        {
            MoviesDAO moviesDAO = new MoviesDAO(dbFilePath, dbName);
            foreach (var row in rows)
            {
                moviesDAO.Create(row);
            }
        }
    }
}
