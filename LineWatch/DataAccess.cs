﻿using Microsoft.Data.SqlClient;

namespace LineWatch
{
    internal class DataAccess
    {
        static string connectionString = "Server=.\\SQLEXPRESS;Database=emc_prod;Trusted_Connection=True;TrustServerCertificate=true";
        public static SqlConnection connection = new SqlConnection(connectionString);
        static SqlCommand command = new SqlCommand();

        public static void InitDB()
        {

        }

        public static void Execute(string SQLcommand)
        {
            try
            {
                connection.Open();
                command.CommandText = SQLcommand;
                command.Connection = connection;
                command.ExecuteNonQuery();
                connection.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(DateTime.Now.ToString()+"Ошибка записи в БД: " +e.Message);
            }
        }
    }
}
