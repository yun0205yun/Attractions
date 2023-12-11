using Dapper;
using Attractions.Models;
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using Attractions.Models.Loggin;
using Attractions.Tools;
using Attractions.Models.Information;
using System.Web.Configuration;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Text;
using System.Web.Services.Description;
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
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    var query = "SELECT * FROM Users WHERE Username = @Username";
                    var dbData = connection.QuerySingleOrDefault<UserDataModel>(query, new { model.Username });//QuerySingleOrDefaultDapper 套件從資料庫中查詢符合條件的單一資料行（一個使用者的資料），並將其映射到 UserDataModel 物件。

                    // 驗證輸入的密碼
                    if (dbData?.Password != null && VerifyPassword(model.Password, dbData.Password))
                    {
                        return new LogginDataModel
                        {
                            Password = dbData.Password,
                            IsLoginSuccessful = true,
                            UserId = dbData.UserId,
                            Username = model.Username,

                        };
                    }

                    return new LogginDataModel
                    {
                        IsLoginSuccessful = false,
                    };
                }
            }
            catch (Exception ex)
            {
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
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    // 檢查使用者名稱是否已存在
                    var checkIfExistsSql = "SELECT COUNT(*) FROM Users WHERE Username = @Username";
                    var existingUserCount = connection.ExecuteScalar<int>(checkIfExistsSql, new { Username = username });

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
                Console.WriteLine($"註冊失敗: {ex.Message}");

                // 記錄錯誤日誌
                ErrorLog.LogError($"Registration failed. Exception: {ex.Message}");

                return "註冊失敗";
            }

        }


        //出現全部訊息
        //分頁利用offset和fetch子句
        public PagedMessagesResult<InformationDataModel> GetPagedMessages(int? page, int pageSize)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    string query = @"
                        SELECT　AttractionID, AttractioDesc, Attraction.CreateUserID, CategoryName, CityName, AttractionTitle, Attraction.CreatedAt, Attraction.EditAt, COUNT(*) OVER () as TotalMessages
                        FROM Attraction
                        JOIN City ON Attraction.CityID = City.CityID
                        JOIN Category ON City.CateID = Category.CateID
                        JOIN Users ON Attraction.CreateUserID = Users.UserId
                        WHERE Attraction.Status = 1 -- Only search for data with Status 1
                        ORDER BY Attraction.AttractionID DESC
                        OFFSET @Offset ROWS
                        FETCH NEXT @PageSize ROWS ONLY;";

                    // Calculate offset
                    int offset = ((page ?? 1) < 1 ? 0 : (page ?? 1) - 1) * pageSize;

                    var messages = connection.Query<InformationDataModel>(query, new { Offset = offset, PageSize = pageSize }).ToList();

                    // Calculate total messages
                    int totalMessages = messages?.FirstOrDefault()?.TotalMessages ?? 0;

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
                Console.WriteLine($"Error in GetPagedMessages: {ex.Message}");
                ErrorLog.LogError($"Error in GetPagedMessages: {ex.Message}");

                return new PagedMessagesResult<InformationDataModel>
                {
                    ErrorMessage = ex.Message,
                    ExceptionStackTrace = ex.StackTrace
                };
            }
        }

        // 搜尋
        public PagedMessagesResult<InformationDataModel> SearchAttractions(string searchText, int page, int pageSize, List<string> selectedAreas, List<string> selectedCities)
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

                        // 建立查詢景點的 SQL 語句
                        queryBuilder.Clear();
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

                        queryBuilder.Append(" ORDER BY Attraction.AttractionId DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;");

                        int offset = ((page < 1) ? 0 : page - 1) * pageSize;

                        //執行查景點的 SQL 語句
                        var parameters = new
                        {
                            SearchText = $"%{searchText}%",
                            SelectedAreas = selectedAreas,
                            SelectedCities = selectedCities,
                            Offset = offset,
                            PageSize = pageSize
                        };

                        var messages = connection.Query<InformationDataModel>(queryBuilder.ToString(), parameters).ToList();

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
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                
                string query =
                    @"SELECT AttractionID,CityName, AttractionTitle, AttractioDesc, Attraction.EditAt, EditUserID, Users.UserName as LastEditorName 
                        FROM Attraction
                        JOIN City ON Attraction.CityID = City.CityID
                        JOIN Category ON City.CateID = Category.CateID
                        LEFT JOIN Users ON　Attraction.EditUserID=Users.UserId
                        WHERE Attraction.AttractionTitle = @AttractionTitle";

                var attraction = connection.QueryFirstOrDefault<InformationDataModel>(query, new { AttractionTitle = title });

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

                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    
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

        //  要拿到CateID
        private int GetCateIDByCategoryName(string categoryName)
        {
            int cateID = 0;  

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    string query = "SELECT CateID FROM Category WHERE CategoryName = @CategoryName";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@CategoryName", categoryName);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
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
                Console.WriteLine($"Error in GetCateIDByCategoryName: {ex.Message}");
            }

            return cateID;
        }

        //  要拿到CityID
        private int GetCityIDByCityName(string cityName)
        {
            int cityID = 0;  

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    string query = "SELECT CityID FROM City WHERE CityName = @CityName";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@CityName", cityName);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                cityID = reader.GetInt32(0);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetCityIDByCityName: {ex.Message}");
            }

            return cityID;
        }
         

        // 獲取景點信息(可以帶到編輯頁)
        public InformationDataModel GetAttractionID(int AttractionID)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    string query =
                        @"SELECT AttractionID,CityName,CategoryName,AttractionTitle, AttractioDesc, Attraction.EditAt, EditUserID 
                            FROM Attraction 
                            JOIN City ON Attraction.CityID = City.CityID 
                            JOIN Category ON City.CateID = Category.CateID
                            JOIN Users on　Attraction.CreateUserID=Users.UserId
                            WHERE AttractionID =@AttractionID";
                    var attraction = connection.QueryFirstOrDefault<InformationDataModel>(query, new { AttractionID });

                    return attraction;
                }
            }
            catch (Exception ex)
            {
                ErrorLog.LogError($"Error in GetAttractionID method: {ex.Message}");
                return null;
            }
        }
        // 刪除 Attractions. Status 設置 0 (保留刪除紀錄)
        public bool DeleteAttraction(int AttractionID)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    string query = "UPDATE Attraction SET Status = 0 WHERE AttractionID = @AttractionID";
                    int rowsAffected = connection.Execute(query, new { AttractionID });

                    return rowsAffected > 0; // 檢查是否有資料被更新
                }
            }
            catch (Exception ex)
            {
                // 記錄錯誤訊息
                Console.WriteLine($"DeleteAttraction Error: {ex.Message}");
                return false; // 返回 false 以指示刪除失敗
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
                int currentUserId = GetCurrentUserId();
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    string query =
                    @" 
                      UPDATE Attraction 
                        SET AttractionTitle = @AttractionTitle, 
                            AttractioDesc = @AttractioDesc,
                            EditUserID = @EditUserID,
                            EditAt=GETDATE()
                        WHERE AttractionID = @AttractionID";
                    int rowsAffected = connection.Execute(query
                        , new { attraction.AttractionID,
                                attraction.AttractionTitle,
                                attraction.AttractioDesc,
                                EditUserID = currentUserId,
                                attraction.EditAt
                        });;

                    return rowsAffected > 0; // 檢查是否有資料被更新
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EditPage Error: {ex.Message}");
                throw; // 將錯誤繼續拋出以顯示在瀏覽器的控制台中
            }
        }

    }
}