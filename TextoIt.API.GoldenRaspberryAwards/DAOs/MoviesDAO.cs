using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using System.Data;
using System.Reflection;
using TextoIt.API.GoldenRaspberryAwards.Models;

namespace TextoIt.API.GoldenRaspberryAwards.Repository
{
    public class MoviesDAO
    {
        private SqliteConnection _connection;
        public MoviesDAO(string dbFilePath)
        {
            _connection = new SqliteConnection($"Data Source={dbFilePath}");
        }
        public int Create(MoviesModel movie)
        {
            try
            {
                using (_connection)
                {
                    _connection.Open();
                    using (SqliteCommand command = _connection.CreateCommand())
                    {
                        command.CommandText = $"INSERT INTO MoviesGoldenRaspberryAwards (year, title, studio, producers, winner) VALUES ({movie.year}, '{movie.title}', '{movie.studio}', '{movie.producers}', '{movie.winner}')";
                        command.ExecuteNonQuery();
                    }
                    _connection.Close();
                }
                return StatusCodes.Status201Created;
            }
            catch (SqliteException e)
            {
                if (e.SqliteErrorCode == 19) return StatusCodes.Status409Conflict;
                return StatusCodes.Status500InternalServerError;
            }
            catch (Exception)
            {
                return StatusCodes.Status500InternalServerError;
            }
        }

        public List<MoviesModel> Read(string selectQuery)
        {
            try
            {
                List<MoviesModel>? movieList = new List<MoviesModel>();

                using (_connection)
                {
                    _connection.Open();
                    using (SqliteCommand command = _connection.CreateCommand())
                    {
                        command.CommandText = selectQuery;
                        SqliteDataReader reader = command.ExecuteReader();

                        while (reader.Read())
                        {

                            MoviesModel movie = new MoviesModel(reader.GetInt32("year"), reader.GetString("title"), reader.GetString("studio"), reader.GetString("producers"), reader.GetString("winner"));
                            movieList.Add(movie);
                        }
                        reader.Close();
                    }
                    _connection.Close();
                    return movieList;
                }
            }
            catch (Exception)
            {

                return new List<MoviesModel>(); ;
            }
        }

        public int Update(MoviesModel movie)
        {
            try
            {
                string updateQuery = ConstructUpdateQuery(movie);

                using (_connection)
                {
                    int updatedRow = 0;
                    _connection.Open();
                    using (SqliteCommand command = _connection.CreateCommand())
                    {
                        command.CommandText = updateQuery;
                        updatedRow = command.ExecuteNonQuery();
                    }
                    _connection.Close();

                    if (updatedRow == 0) return StatusCodes.Status404NotFound;
                }
                return StatusCodes.Status204NoContent;
            }
            catch (Exception)
            {
                return StatusCodes.Status500InternalServerError;
            }
        }

        public int Delete(int year, string title)
        {
            try
            {
                string deleteQuery = $"DELETE FROM MoviesGoldenRaspberryAwards WHERE year={year} AND title='{title}'";

                using (_connection)
                {
                    int affectedRows = 0;
                    _connection.Open();
                    using (SqliteCommand command = _connection.CreateCommand())
                    {
                        command.CommandText = deleteQuery;
                        affectedRows = command.ExecuteNonQuery();
                    }
                    _connection.Close();

                    if (affectedRows == 0) return StatusCodes.Status404NotFound;
                }
                return StatusCodes.Status204NoContent;
            }
            catch (Exception)
            {
                return StatusCodes.Status500InternalServerError;
            }
        }

        private string ConstructUpdateQuery(MoviesModel movie)
        {
            var sqlQuery = "UPDATE MoviesGoldenRaspberryAwards SET";

            IEnumerable<PropertyInfo>? propertieMovieInfo = movie.GetType().GetProperties().Where(p => !p.GetGetMethod().GetParameters().Any());

            foreach (var property in propertieMovieInfo)
            {
                var propertyValue = property.GetValue(movie);

                if (property != null && property.Name != "year" && property.Name != "title" && propertyValue != null)
                {
                    Type propertyType = propertyValue.GetType();
                    string singleQuoteWhenPropertyIsString = propertyType.Name == "String" ? "'" : "";
                    sqlQuery += $" {property.Name}={singleQuoteWhenPropertyIsString + propertyValue + singleQuoteWhenPropertyIsString},";
                }
            }

            sqlQuery = sqlQuery.Substring(0, sqlQuery.Length - 1);
            sqlQuery += $" WHERE year={movie.year} AND title='{movie.title}'";

            return sqlQuery;
        }
    }
}
