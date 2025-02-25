using Microsoft.Data.SqlClient;
using System.Data;

namespace LineWatch
{
    internal class DataAccess
    {
        static string connectionString= "Server=tcp:TOGPROD;Database=emc_prod;Trusted_Connection=True;TrustServerCertificate=True;";

        // Чтение строки подключения из файла
        static string ReadConnectionStringFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Файл с соединением не найден: {filePath}");
            }
            return File.ReadAllText(filePath);
        }

        public static void InitDB()
        {
            // Указание пути к файлу с соединением
            string connectionStringFilePath = @".\conf\db.txt"; // Путь к файлу с соединением
            connectionString = ReadConnectionStringFromFile(connectionStringFilePath); // Чтение строки подключения из файла
        }

        /// <summary>
        /// Получить список станций
        /// </summary>
        /// <returns>List of PLC</returns>
        public static List<PLC> GetPLCList()
        {
            using SqlConnection conn = new(connectionString);
            conn.Open();
            List<PLC> plcList = [];
            // название процедуры
            string sqlExpression = "GetPLCList";
            SqlCommand command = new(sqlExpression, conn)
            {
                // указываем, что команда представляет хранимую процедуру
                CommandType = CommandType.StoredProcedure
            };
            // Выполняем запрос и получаем данные
            using (SqlDataReader reader = command.ExecuteReader())
            {
                // Читаем каждую строку из результата
                while (reader.Read())
                {
                    // Извлекаем значения из каждой строки
                    string name = reader.GetString(reader.GetOrdinal("Name"));
                    string ip = reader.GetString(reader.GetOrdinal("IP")).TrimEnd();
                    string printer = reader.IsDBNull(reader.GetOrdinal("Printer")) ? "" : reader.GetString(reader.GetOrdinal("Printer")).TrimEnd(); // Обработка NULL
                    bool label = reader.GetBoolean(reader.GetOrdinal("Print_label"));
                    // Создаем новый объект PLC и добавляем его в список
                    plcList.Add(new PLC(name, ip, printer, label));
                }
            }
            return plcList;
        }

        /// <summary>
        /// Добавление собранного ящика
        /// </summary>
        /// <param name="Date"></param>
        /// <param name="Time"></param>
        /// <param name="Label"></param>
        /// <param name="Name"></param>
        /// <param name="Material"></param>
        /// <param name="Amount"></param>
        /// <returns></returns>
        public static async Task AddBoxAsync(string Date, string Time, string Label, string Name, string Material, int Amount)
        {
            try
            {
                using SqlConnection conn = new(connectionString);
                await conn.OpenAsync();
                // название процедуры
                string sqlExpression = "AddBox";
                SqlCommand command = new(sqlExpression, conn)
                {
                    // указываем, что команда представляет хранимую процедуру
                    CommandType = CommandType.StoredProcedure
                };
                // параметры
                SqlParameter dateParam = new() { ParameterName = "@Date", Value = Date };
                command.Parameters.Add(dateParam);
                SqlParameter timeParam = new() { ParameterName = "@Time", Value = Time };
                command.Parameters.Add(timeParam);
                SqlParameter labelParam = new() { ParameterName = "@labelNumber", Value = Label };
                command.Parameters.Add(labelParam);
                SqlParameter nameParam = new() { ParameterName = "@Name", Value = Name };
                command.Parameters.Add(nameParam);
                SqlParameter materialParam = new() { ParameterName = "@Material", Value = Material };
                command.Parameters.Add(materialParam);
                SqlParameter amountParam = new() { ParameterName = "@Amount", Value = Amount };
                command.Parameters.Add(amountParam);

                // выполняем процедуру
                await command.ExecuteNonQueryAsync();
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Ошибка SQL при добавлении ящика: {ex.Message}");
            }
        }
        /// <summary>
        /// Обновляет состояние станции в БД
        /// </summary>
        /// <param name="PLC">Имя линии</param>
        /// <param name="isOnline">true если подключено</param>
        /// <returns></returns>
        public static async Task UpdateIsOnline(string PLC, bool isOnline)
        {
            try
            {
                using SqlConnection conn = new(connectionString);
                await conn.OpenAsync();
                // название процедуры
                string sqlExpression = "UpdateIsOnline";
                SqlCommand command = new(sqlExpression, conn)
                {
                    // указываем, что команда представляет хранимую процедуру
                    CommandType = CommandType.StoredProcedure
                };
                // параметры
                SqlParameter nameParam = new() { ParameterName = "@Name", Value = PLC };
                command.Parameters.Add(nameParam);
                SqlParameter isonlineParam = new() { ParameterName = "@IsOnline", Value = isOnline };
                command.Parameters.Add(isonlineParam);
                // выполняем процедуру
                await command.ExecuteNonQueryAsync();
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error: {ex.Message}");
            }
        }

        public static async Task AddPartAsync(string json)
        {
            try
            {
                using SqlConnection conn = new(connectionString);
                await conn.OpenAsync();
                string sqlExpression = "AddPart";
                SqlCommand command = new(sqlExpression, conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.AddWithValue("@json", json);
                await command.ExecuteNonQueryAsync();
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Ошибка SQL при добавлении детали: {ex.Message}");
            }
        }
    }
}