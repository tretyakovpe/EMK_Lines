using Microsoft.Data.SqlClient;
using System.Data;
using System.IO;

namespace LineWatch
{
    internal class DataAccess
    {
        static string db_name = "emc_prod";
        static string connectionString;

        // Чтение строки подключения из файла
        static string ReadConnectionStringFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Файл с соединением не найден: {filePath}");
            }

            return File.ReadAllText(filePath);
        }

        static SqlConnection connection;

        public static void InitDB()
        {
            // Указание пути к файлу с соединением
            string connectionStringFilePath = @".\conf\db.txt"; // Путь к файлу с соединением
            connectionString = ReadConnectionStringFromFile(connectionStringFilePath); // Чтение строки подключения из файла
            connection = new SqlConnection(connectionString);

            if (connection.State == ConnectionState.Closed)
            {
                connection.Open();
            }

            if (TestDatabase(connectionString, db_name).Result == false)
            {
                CreateDatabase();
            }

            connection.ChangeDatabase(db_name);
        }

        private static void CreateDatabase()
        {
            string script = File.ReadAllText(@".\data\database.sql");
            Execute(script);
        }

        private static async Task<bool> TestDatabase(string connectionString, string databaseName)
        {
            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand("SELECT db_id(@databaseName)", connection))
            {
                command.Parameters.Add(new SqlParameter("databaseName", databaseName));
                connection.Open();
                return (await command.ExecuteScalarAsync() != DBNull.Value);
            }
        }

        // добавление собранного ящика
        public static async Task AddBoxAsync(string Date, string Time, string Label, string Name, string Material, int Amount)
        {
            // название процедуры
            string sqlExpression = "AddBox";
            SqlCommand command = new SqlCommand(sqlExpression, connection);
            // указываем, что команда представляет хранимую процедуру
            command.CommandType = CommandType.StoredProcedure;
            // параметры
            SqlParameter dateParam = new SqlParameter { ParameterName = "@Date", Value = Date };
            command.Parameters.Add(dateParam);
            SqlParameter timeParam = new SqlParameter { ParameterName = "@Time", Value = Time };
            command.Parameters.Add(timeParam);
            SqlParameter labelParam = new SqlParameter { ParameterName = "@labelNumber", Value = Label };
            command.Parameters.Add(labelParam);
            SqlParameter nameParam = new SqlParameter { ParameterName = "@Name", Value = Name };
            command.Parameters.Add(nameParam);
            SqlParameter materialParam = new SqlParameter { ParameterName = "@Material", Value = Material };
            command.Parameters.Add(materialParam);
            SqlParameter amountParam = new SqlParameter { ParameterName = "@Amount", Value = Amount };
            command.Parameters.Add(amountParam);

            // выполняем процедуру
            var id = await command.ExecuteScalarAsync();
            // если нам не надо возвращать id
            //var id = await [command.ExecuteNonQueryAsync()](command.ExecuteNonQueryAsync());
            //Console.WriteLine(Date](//Console.WriteLine(Date) + "\t" + Time + "\t" + Label + "\t" + Name + "\t" + Material + "\t" + [Amount.ToString()](Amount.ToString()) + $" Id: {id}");
        }
        /// <summary>
        /// Обновляет состояние станции в БД
        /// </summary>
        /// <param name="PLC">Имя линии</param>
        /// <param name="isOnline">true если подключено</param>
        /// <returns></returns>
        public static async Task UpdateIsOnline(string PLC, bool isOnline)
        {
            // название процедуры
            string sqlExpression = "UpdateIsOnline";
            SqlCommand command = new SqlCommand(sqlExpression, connection);
            // указываем, что команда представляет хранимую процедуру
            command.CommandType = CommandType.StoredProcedure;
            // параметры
            SqlParameter nameParam = new SqlParameter { ParameterName = "@Name", Value = PLC };
            command.Parameters.Add(nameParam);
            SqlParameter isonlineParam = new SqlParameter { ParameterName = "@IsOnline", Value = isOnline };
            command.Parameters.Add(isonlineParam);
            // выполняем процедуру
            var id = await command.ExecuteScalarAsync();
        }

        public static async Task AddPartAsync(string json)
        {
            // название процедуры
            string sqlExpression = "AddPart";
            SqlCommand command = new SqlCommand(sqlExpression, connection);
            // указываем, что команда представляет хранимую процедуру
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@json", json);
            var id = await command.ExecuteNonQueryAsync();
        }

        public static int Execute(string SQLcommand)
        {
            int result = 0;
            try
            {
                SqlCommand cmd = new SqlCommand(SQLcommand);
                cmd.Connection = connection;
                result = cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Console.WriteLine(DateTime.Now.ToString() + " Ошибка записи в БД: " + e.Message);
            }
            return result;
        }
    }
}