using Dapper;
using Attractions.Models;
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using Attractions.Models.Loggin;
using Attractions.Tools;
using Attractions.Models.Information;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace Attractions
{
    public class Repository
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        // 檢查登入是否成功
        public LogginDataModel IsLoginSuccessful(LogginViewModel model)
        {
            try
            {
                // 使用 SqlConnection 建立與資料庫的連接
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    // 準備 SQL 查詢語句，查詢是否有符合使用者名稱的資料
                    var query = "SELECT * FROM Users WHERE Username = @Username";
                    // 使用 Dapper 執行查詢，並將結果映射到 UserDataModel 物件
                    //QuerySingleOrDefaultDapper 套件從資料庫中查詢符合條件的單一資料行（一個使用者的資料），並將其映射到 UserDataModel 物件。
                    var dbData = connection.QuerySingleOrDefault<UserDataModel>(query, new { model.Username });

                    // 驗證輸入的密碼
                    if (dbData?.Password != null && VerifyPassword(model.Password, dbData.Password))
                    {
                        // 如果密碼驗證成功，回傳成功登入的相關資訊
                        return new LogginDataModel
                        {
                            Password = dbData.Password,
                            IsLoginSuccessful = true,
                            UserId = dbData.UserId,
                            Username = model.Username,

                        };
                    }
                    // 如果密碼驗證成功，回傳成功登入的相關資訊
                    return new LogginDataModel
                    {
                        IsLoginSuccessful = false,
                    };
                }
            }
            catch (Exception ex)
            {
                // 處理例外狀況，並回傳登入失敗的相關資訊
                Console.WriteLine($"登入失敗: {ex.Message}");

                // 記錄錯誤日誌
                ErrorLog.LogError($"Login failed. Exception: {ex.Message}");

                return new LogginDataModel
                {
                    IsLoginSuccessful = false,
                };
            }
        }

        // 密碼驗證邏輯
        private bool VerifyPassword(string enteredPassword, string storedPassword)
        {
            return enteredPassword == storedPassword;
        }

        // 處理使用者註冊
        public string RegisterViewModel(string username, string password)
        {
            try
            {
                // 使用 SqlConnection 連接資料庫
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    // 檢查使用者名稱是否已存在
                    var checkIfExistsSql = "SELECT COUNT(*) FROM Users WHERE Username = @Username";
                    var existingUserCount = connection.ExecuteScalar<int>(checkIfExistsSql, new { Username = username });
                    // 如果使用者名稱已存在，返回已登記的訊息
                    if (existingUserCount > 0)
                    {
                        return "已登記";
                    }

                    // 將使用者資料插入資料庫
                    var insertUserSql = "INSERT INTO Users (Username, Password, Email) VALUES (@Username, @Password, @Email)";
                    connection.Execute(insertUserSql, new { Username = username, Password = password, Email = "沒改資料庫(db is not null)" });



                    return "註冊成功";
                }
            }
            catch (Exception ex)
            {
                // 捕捉並記錄註冊失敗的例外
                Console.WriteLine($"註冊失敗: {ex.Message}");

                // 記錄錯誤日誌
                ErrorLog.LogError($"Registration failed. Exception: {ex.Message}");

                return "註冊失敗";
            }

        }

        //以上是登入註冊




        //出現全部訊息
        //分頁利用offset和fetch子句
        public PagedMessagesResult<InformationDataModel> GetPagedMessages(int? page, int pageSize, string sortBy, string sortOrder)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    // SQL 查詢語句，檢索 Attraction 表的相關信息
                    string query = @"
                            SELECT　AttractionID, AttractioDesc, Attraction.CreateUserID, CategoryName, CityName, AttractionTitle, Attraction.CreatedAt, Attraction.EditAt, COUNT(*) OVER () as TotalMessages
                            FROM Attraction
                            JOIN City ON Attraction.CityID = City.CityID
                            JOIN Category ON City.CateID = Category.CateID
                            JOIN Users ON Attraction.CreateUserID = Users.UserId
                            WHERE Attraction.Status = 1  
                            ORDER BY 
                                Attraction.CreatedAt DESC, Attraction.EditAt DESC 
                            OFFSET @Offset ROWS
                            FETCH NEXT @PageSize ROWS ONLY;";

                    // 計算 OFFSET
                    int offset = ((page ?? 1) < 1 ? 0 : (page ?? 1) - 1) * pageSize;

                    // 使用 Dapper 執行 SQL 查詢
                    var messages = connection.Query<InformationDataModel>(
                        query,
                        new { Offset = offset, PageSize = pageSize, SortBy = sortBy, SortOrder = sortOrder }
                    ).ToList();

                    // 計算總消息數
                    int totalMessages = messages?.FirstOrDefault()?.TotalMessages ?? 0;

                    // 返回 PagedMessagesResult 對象，其中包含分頁信息和檢索到的消息列表
                    return new PagedMessagesResult<InformationDataModel>
                    {
                        CurrentPage = page ?? 1,
                        PageSize = pageSize,
                        TotalMessages = totalMessages,
                        Messages = messages
                    };
                }
            }
            catch (Exception ex)
            {
                // 處理異常，並返回包含錯誤信息的 PagedMessagesResult 對象
                Console.WriteLine($"Error in GetPagedMessages: {ex.Message}");
                ErrorLog.LogError($"Error in GetPagedMessages: {ex.Message}");

                return new PagedMessagesResult<InformationDataModel>
                {
                    ErrorMessage = ex.Message,
                    ExceptionStackTrace = ex.StackTrace
                };
            }
        }




        // 模糊搜尋
        public PagedMessagesResult<InformationDataModel> SearchAttractions(string searchText, int page, int pageSize, List<string> selectedAreas, List<string> selectedCities, string sortBy, string sortOrder)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    // 建立 StringBuilder 來構建動態 SQL 查詢
                    var queryBuilder = new StringBuilder();
                    queryBuilder.Append(@"
                        SELECT COUNT(*) 
                        FROM Attraction
                        JOIN City ON Attraction.CityID = City.CityID 
                        JOIN Category ON City.CateID = Category.CateID
                        JOIN Users ON Attraction.CreateUserID = Users.UserId
                        WHERE Attraction.Status = 1
                        AND (AttractionTitle LIKE @SearchText OR AttractioDesc LIKE @SearchText)");
                    // 如果選擇了區域或城市，加入對應的查詢條件
                    if ((selectedAreas != null && selectedAreas.Count > 0) || (selectedCities != null && selectedCities.Count > 0))
                    {
                        queryBuilder.Append(" AND (Category.CategoryName = @SelectedAreas OR City.CityName =  @SelectedCities)");
                    }

                    // 使用 Dapper 的 QueryMultiple 一次執行多條 SQL 語句
                    using (var multi = connection.QueryMultiple(queryBuilder.ToString(), new
                    {
                        SearchText = $"%{searchText}%",
                        SelectedAreas = selectedAreas,
                        SelectedCities = selectedCities
                    }))
                    {
                        // 取得總行數
                        var count = multi.Read<int>().Single();
                        //清空 queryBuilder
                        queryBuilder.Clear();
                        // 建立查詢景點的 SQL  
                        queryBuilder.Append(@"
                            SELECT AttractionID, AttractioDesc, Attraction.CreateUserID, CategoryName, CityName, AttractionTitle, Attraction.CreatedAt, Attraction.EditAt
                            FROM Attraction
                            JOIN City ON Attraction.CityID = City.CityID 
                            JOIN Category ON City.CateID = Category.CateID
                            JOIN Users ON Attraction.CreateUserID = Users.UserId
                            WHERE Attraction.Status = 1
                            AND (AttractionTitle LIKE @SearchText OR AttractioDesc LIKE @SearchText)");

                        if ((selectedAreas != null && selectedAreas.Count > 0) || (selectedCities != null && selectedCities.Count > 0))
                        {
                            queryBuilder.Append(" AND (Category.CategoryName = @SelectedAreas OR City.CityName = @SelectedCities)");
                        }

                        queryBuilder.Append("ORDER BY Attraction.AttractionID DESC  OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;");

                        int offset = ((page < 1) ? 0 : page - 1) * pageSize;

                        //執行查景點的 SQL 語句
                        var parameters = new
                        {
                            SearchText = $"%{searchText}%",
                            SelectedAreas = selectedAreas,
                            SelectedCities = selectedCities,
                            Offset = offset,
                            PageSize = pageSize,
                            SortBy = sortBy,
                            SortOrder = sortOrder
                        };
                        //使用 Dapper 執行 SQL 查詢
                        var messages = connection.Query<InformationDataModel>(queryBuilder.ToString(), parameters).ToList();
                        //返回的結果是一個包含分頁信息和檢索到的消息列表的 PagedMessagesResult<InformationDataModel> 對象
                        return new PagedMessagesResult<InformationDataModel>
                        {
                            CurrentPage = page,
                            PageSize = pageSize,
                            TotalMessages = count,
                            Messages = messages
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SearchAttractions 出错: {ex.Message}");
                ErrorLog.LogError($"SearchAttractions 出错: {ex.Message}");
                return new PagedMessagesResult<InformationDataModel>
                {
                    ErrorMessage = ex.Message,
                    ExceptionStackTrace = ex.StackTrace
                };
            }
        }

        //景點內容(標題點進去)
        public InformationDataModel GetAttractionByTitle(string title)
        {
            // 使用 SqlConnection 連接資料庫
            using (var connection = new SqlConnection(_connectionString))
            {
                // 開啟資料庫連接
                connection.Open();

                // 構建 SQL 查詢語句，這個查詢涉及 Attraction、City、Category 和 Users 表格的 JOIN 操作
                string query =
                    @"SELECT AttractionID, CityName, AttractionTitle, AttractioDesc, Attraction.EditAt, EditUserID, Users.UserName as LastEditorName 
                        FROM Attraction
                        JOIN City ON Attraction.CityID = City.CityID
                        JOIN Category ON City.CateID = Category.CateID
                        LEFT JOIN Users ON Attraction.EditUserID = Users.UserId
                        WHERE Attraction.AttractionTitle = @AttractionTitle";

                // 使用 Dapper 的 QueryFirstOrDefault 方法執行 SQL 查詢，並將結果映射到 InformationDataModel 類型
                var attraction = connection.QueryFirstOrDefault<InformationDataModel>(query, new { AttractionTitle = title });

                // 返回查詢結果
                return attraction;
            }
        }

        //新增景點
        public void Create(CreateModel message)
        {
            try
            {
                // 確保 CategoryName 和 CityName 有有效值
                if (string.IsNullOrEmpty(message.CategoryName) || string.IsNullOrEmpty(message.CityName))
                {
                    Console.WriteLine("CategoryName or CityName is invalid.");
                    return;
                }
                // 獲取 CategoryName 對應的 CateID(北基宜 = 1)
                int cateID = GetCateIDByCategoryName(message.CategoryName);

                // 獲取 CityID(台北=1)
                int cityID = GetCityIDByCityName(message.CityName);
              
                    // 插入到DB
                    string insertquery = @"
                    INSERT INTO Attraction (AttractionTitle, AttractioDesc, CreateUserID, CityID) 
                    VALUES (@AttractionTitle, @AttractionDesc, @CreateUserID, @CityID);
                    INSERT INTO Content (UserId, ContentText) VALUES (@UserId, @ContentText);";
                // 使用 ADO.NET 進行資料庫操作
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    // 執行 SQL 插入語句，使用 Dapper 提供的 Execute 方法
                    int rowsaffected = connection.Execute(insertquery, new
                    {
                        message.UserId,
                        message.CreateUserID,
                        cateID, // 使用獲得的 CateID
                        message.CategoryName,
                        cityID, // 使用獲得的 CityID
                        message.CityName,
                        message.AttractionTitle,
                        message.AttractionDesc,
                        message.ContentText
                    });

                    // 檢查是否有新增到首頁
                    if (rowsaffected > 0)
                    {
                        Console.WriteLine("Data added successfully.");
                    }
                    else
                    {
                        Console.WriteLine("Failed to add data.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        // 要拿到CateID
        private int GetCateIDByCategoryName(string categoryName)
        {
            int cateID = 0;  // 預設值為0，表示未找到對應的 CateID

            try
            {
                // 使用 ADO.NET 進行資料庫操作
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();// 打開資料庫連接

                    // SQL 查詢語句，根據 CategoryName 查找對應的 CateID
                    string query = "SELECT CateID FROM Category WHERE CategoryName = @CategoryName";

                    // 使用 SqlCommand 設定參數和執行查詢
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        //SQL 查詢添加一個參數，該參數的名稱是 @CategoryName，其值由變數 categoryName 提供。
                        command.Parameters.AddWithValue("@CategoryName", categoryName);

                        // 使用 SqlDataReader 讀取查詢結果
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            // 如果有找到結果，讀取第一列的 CateID
                            if (reader.Read())
                            {
                                cateID = reader.GetInt32(0);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // 處理例外情況，輸出錯誤訊息
                Console.WriteLine($"Error in GetCateIDByCategoryName: {ex.Message}");
            }

            // 返回取得的 CateID
            return cateID;
        }


        //  要拿到CityID
        private int GetCityIDByCityName(string cityName)
        {
            int cityID = 0;  // 初始化cityID 為0

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();  // 打開資料庫連接

                    // 定義 SQL 查詢，根據城市名稱選擇對應的CityID
                    string query = "SELECT CityID FROM City WHERE CityName = @CityName";
                    // 使用 SqlCommand 設定參數和執行查詢
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        // 為 SQL 查詢添加一個參數，參數名稱是 @CityName，其值由方法的參數 cityName 提供
                        command.Parameters.AddWithValue("@CityName", cityName);

                        // 使用 SQLDataReader 讀取執行查詢的結果
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            // 如果有資料行可以讀取
                            if (reader.Read())
                            {
                                // 從讀取的資料行中取得城市ID的值，並賦值給 cityID 變數
                                cityID = reader.GetInt32(0);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // 如果發生錯誤，印出錯誤訊息到控制台
                Console.WriteLine($"Error in GetCityIDByCityName: {ex.Message}");
            }

            // 返回取得的城市ID值
            return cityID;
        }



        // 獲取景點信息(可以帶到編輯頁)
        public InformationDataModel GetAttractionID(int AttractionID)
        {
            try
            {
                // 使用 SqlConnection 創建一個資料庫連接
                using (var connection = new SqlConnection(_connectionString))
                {
                    // 打開資料庫連接
                    connection.Open();

                    // 定義 SQL 查詢語句，使用 JOIN 連接 Attraction、City、Category 和 Users 表
                    string query =
                        @"SELECT AttractionID, CityName, CategoryName, AttractionTitle, AttractioDesc, Attraction.EditAt, EditUserID 
                            FROM Attraction 
                            JOIN City ON Attraction.CityID = City.CityID 
                            JOIN Category ON City.CateID = Category.CateID
                            JOIN Users ON Attraction.CreateUserID = Users.UserId
                            WHERE AttractionID = @AttractionID";

                    // 使用 Dapper 的 QueryFirstOrDefault 方法執行 SQL 查詢，並將結果映射為 InformationDataModel
                    var attraction = connection.QueryFirstOrDefault<InformationDataModel>(query, new { AttractionID });

                    // 返回從資料庫中獲取的景點信息
                    return attraction;
                }
            }
            catch (Exception ex)
            {
                // 如果發生異常，記錄錯誤信息至錯誤日誌，並返回 null
                ErrorLog.LogError($"Error in GetAttractionID method: {ex.Message}");
                return null;
            }
        }

        // 刪除 Attractions. Status 設置 0 (保留刪除紀錄)
        public bool DeleteAttraction(int AttractionID)
        {
            try
            {
                // 使用 SqlConnection 建立與資料庫的連接
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open(); // 開啟資料庫連接

                    // 定義 SQL 查詢語句，將指定 AttractionID 的景點的 Status 設置為 0
                    string query = "UPDATE Attraction SET Status = 0 WHERE AttractionID = @AttractionID";

                    // 使用 Dapper 的 Execute 方法執行 SQL 更新語句
                    int rowsAffected = connection.Execute(query, new { AttractionID });

                    // 檢查是否有資料被更新，如果 rowsAffected 大於 0，表示更新成功
                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                // 如果發生異常，捕獲並輸出錯誤訊息至控制台
                Console.WriteLine($"DeleteAttraction Error: {ex.Message}");

                // 返回 false 以指示刪除失敗
                return false;
            }
        }

        //最後編輯人
        private int GetCurrentUserId()
        {
            int userId = (int)HttpContext.Current.Session["UserId"];
            return userId;
        }
        //編輯更新景色
        public bool EditPage(InformationDataModel attraction)
        {
            try
            {
                // 獲取當前使用者的ID
                int currentUserId = GetCurrentUserId();

                // 使用 SqlConnection 連接到資料庫
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    // 定義 SQL 查詢語句，更新 Attraction 表中的資料
                    string query =
                        @" 
                          UPDATE Attraction 
                            SET AttractionTitle = @AttractionTitle, 
                                AttractioDesc = @AttractioDesc,
                                EditUserID = @EditUserID,
                                EditAt = GETDATE()
                            WHERE AttractionID = @AttractionID";

                    // 執行 SQL 查詢，更新資料
                    int rowsAffected = connection.Execute(query, new
                    {
                        attraction.AttractionID,
                        attraction.AttractionTitle,
                        attraction.AttractioDesc,
                        EditUserID = currentUserId,
                        attraction.EditAt
                    });

                    // 檢查是否有資料被成功更新
                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                // 如果發生異常，將錯誤信息輸出到控制台，然後重新拋出異常以通知調用方發生了錯誤
                Console.WriteLine($"EditPage Error: {ex.Message}");
                throw;
            }
        }


    }
}