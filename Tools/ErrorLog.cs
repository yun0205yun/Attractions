using Dapper;
using System;
using System.Data.SqlClient;
 

namespace Attractions.Tools
{
     
    public class ErrorLog
    {
        static readonly string _connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=Web;Integrated Security=True;";
        static public void LogError(string errorMessage)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    // 插入錯誤信息到 ErrorLog 表
                    string insertQuery = @"
                    INSERT INTO ErrorLog (ErrorMessage, LogTime)
                    VALUES (@ErrorMessage, @LogTime)";

                    connection.Execute(insertQuery, new
                    {
                        ErrorMessage = errorMessage,
                        LogTime = DateTime.Now,
                    });
                }
            }
            catch (Exception ex)
            {
                // 處理寫入錯誤日誌時可能發生的例外
                Console.WriteLine($"Error in LogError: {ex.Message}");
            }
        }
    }
}