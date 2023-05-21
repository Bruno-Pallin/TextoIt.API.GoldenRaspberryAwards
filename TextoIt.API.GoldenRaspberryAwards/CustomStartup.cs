using Microsoft.Data.Sqlite;
using Microsoft.VisualBasic.FileIO;
using TextoIt.API.GoldenRaspberryAwards.Models;

namespace TextoIt.API.GoldenRaspberryAwards
{
    public class CustomStartup
    {
        private string _dbFilePath;
        public CustomStartup()
        {
            string dbFilePath = Path.Combine(Path.GetTempPath(), "MoviesDB.db");
            _dbFilePath = dbFilePath;
            File.Delete(_dbFilePath);
        }

        public void StartUp()
        {
            List<MoviesModel> rows = ReadCSVFile();
            CreateDB(rows, _dbFilePath);
        }

        public List<MoviesModel> ReadCSVFile()
        {
            using (var parser = new TextFieldParser("./CSVFile/MovieList.csv"))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(";");

                List<MoviesModel> records = new List<MoviesModel>();

                if (parser.ReadFields() != null)
                {
                    while (!parser.EndOfData)
                    {
                        var fields = parser.ReadFields();
                        if (fields != null)
                        {
                            var objeto = new MoviesModel
                            {
                                year = fields[0],
                                title = fields[1],
                                studio = fields[2],
                                producers = fields[3],
                                winner = fields[4]
                            };
                            records.Add(objeto);
                        }
                    }
                }
                return records;
            }
        }

        public void CreateDB(List<MoviesModel> rows, string dbFilePath)
        {
            SQLitePCL.Batteries.Init();
            using (var connection = new SqliteConnection($"Data Source={dbFilePath}"))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "CREATE TABLE IF NOT EXISTS MoviesGoldenRaspberryAwards (year INTEGER , title TEXT, studio TEXT, producers TEXT, winner BOOLEAN, PRIMARY KEY (year, title))";

                    command.ExecuteNonQuery();
                }

                foreach (var row in rows)
                {
                    try
                    {
                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = "INSERT INTO MoviesGoldenRaspberryAwards (year, title, studio, producers, winner) VALUES (@year, @title, @studio, @producers, @winner)";

                            command.Parameters.AddWithValue("@year", row.year);
                            command.Parameters.AddWithValue("@title", row.title);
                            command.Parameters.AddWithValue("@studio", row.studio);
                            command.Parameters.AddWithValue("@producers", row.producers);
                            bool winner = row.winner == "yes" ? true : false;
                            command.Parameters.AddWithValue("@winner", winner);

                            command.ExecuteNonQuery();
                        }
                    }
                    catch (Exception e)
                    {
                        if (!e.Message.Contains("SQLite Error 19")) throw;
                    }
                }
                connection.Close();
            }
        }
    }
}
