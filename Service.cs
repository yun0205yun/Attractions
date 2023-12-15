using Attractions.Models.Information;
using Attractions.Models;
using Attractions.Tools;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Data.SqlClient;
using System.Globalization;

namespace Attractions
{
    public class Service
    {
        private readonly Repository _repository;

        // Service 類別的建構函式
        public Service()
        {
            // 初始化 Repository 物件，用於訪問資料庫
            _repository = new Repository();
        }

        // 獲取分頁消息的方法
        public PagedMessagesResult<InformationDataModel> GetPagedMessages(int page, int pageSize, string sortBy, string sortOrder)
        {
            try
            {
                // 調用 Repository 的 GetPagedMessages 方法，獲取分頁消息
                var messages = _repository.GetPagedMessages(page, pageSize, sortBy, sortOrder);

                // 根據排序條件進行排序
                SortMessages(messages, sortBy, sortOrder);

                // 返回分頁消息結果
                return messages;
            }
            catch (Exception ex)
            {
                // 如果發生異常，輸出錯誤信息至控制台，並返回一個空的 PagedMessagesResult
                Console.WriteLine($"GetPagedMessages Error: {ex.Message}");
                return new PagedMessagesResult<InformationDataModel>();
            }
        }

        // 根據排序條件對消息進行排序的私有方法
        private void SortMessages(PagedMessagesResult<InformationDataModel> messages, string sortBy, string sortOrder)
        {
            switch (sortBy)
            {
                case "CreatedAt":
                    // 如果排序條件為 "CreatedAt"
                    // 根據 CreatedAt 屬性升序或降序排序消息列表
                    messages.Messages = sortOrder == "asc" ? messages.Messages.OrderBy(m => m.CreatedAt).ToList() : messages.Messages.OrderByDescending(m => m.CreatedAt).ToList();
                    break;
                case "EditAt":
                    // 如果排序條件為 "EditAt"
                    // 根據 EditAt 屬性升序或降序排序消息列表
                    messages.Messages = sortOrder == "asc" ? messages.Messages.OrderBy(a => a.EditAt).ToList() : messages.Messages.OrderByDescending(a => a.EditAt).ToList();
                    break;
            }
        }


        // 新增的搜尋景點方法的實現
        public PagedMessagesResult<InformationDataModel> SearchAttractions(string searchText, int page, int pageSize, List<string> selectedAreas, List<string> selectedCities, string sortBy, string sortOrder)
        {
            try
            {
                // 調用 Repository 的 SearchAttractions 方法，實現搜尋邏輯
                var result = _repository.SearchAttractions(searchText, page, pageSize, selectedAreas, selectedCities, sortBy, sortOrder);
                SortMessages(result, sortBy, sortOrder);
                // 返回搜尋結果
                return result;
            }
            catch (Exception ex)
            {
                // 如果發生異常，輸出錯誤信息至控制台，然後重新拋出異常
                Console.WriteLine($"SearchAttractions Error: {ex.Message}");
                throw;
            }
        }

        // 根據標題獲取景點的方法
        public InformationDataModel GetAttractionByTitle(string title)
        {
            // 調用 Repository 的 GetAttractionByTitle 方法，根據標題獲取景點信息
            var result = _repository.GetAttractionByTitle(title);

            // 返回景點信息
            return result;
        }

        // 新增景點的方法
        public void AddMessage(CreateModel message)
        {
            // 調用 Repository 的 Create 方法，新增景點
            _repository.Create(message);
        }

        // 根據景點編號獲取景點信息的方法
        public InformationDataModel GetAttractionID(int AttractionID)
        {
            try
            {
                // 調用 Repository 的 GetAttractionID 方法，獲取指定 AttractionID 的景點信息
                var message = _repository.GetAttractionID(AttractionID);

                // 檢查是否找到了相應的消息
                if (message == null)
                {
                    // 如果未找到，輸出錯誤信息至控制台和錯誤日誌
                    Console.WriteLine($"ContentId 為 {AttractionID} 的消息未找到");
                    ErrorLog.LogError($"ContentId 為 {AttractionID} 的消息未找到");

                    // 將 message 設置為一個新的 InformationDataModel 對象，避免返回 null
                    message = new InformationDataModel();
                }

                // 返回找到的或新建的消息對象
                return message;
            }
            catch
            {
                // 如果發生異常，返回 null
                return null;
            }
        }

        // 刪除景點的方法
        public bool DeleteAttraction(int AttractionID)
        {
            try
            {
                // 調用 Repository 物件的 DeleteAttraction 方法進行刪除操作
                _repository.DeleteAttraction(AttractionID);

                // 如果成功執行到這一行，表示刪除操作成功，返回 true
                return true;
            }
            catch (Exception ex)
            {
                // 如果發生異常，捕獲並輸出錯誤訊息至控制台
                Console.WriteLine($"DeleteAttraction Error: {ex.Message}");

                // 返回 false 表示刪除操作失敗
                return false;
            }
        }

        // 編輯景點信息的方法
        public bool EditPage(InformationDataModel attraction)
        {
            try
            {
                // 調用 Repository 的 EditPage 方法，將傳入的 InformationDataModel 對象用於編輯景點信息
                _repository.EditPage(attraction);

                // 如果編輯操作成功，返回 true
                return true;
            }
            catch (Exception ex)
            {
                // 如果發生異常，記錄錯誤信息至控制台，然後重新拋出異常
                Console.WriteLine($"EditPage Error: {ex.Message}");
                throw;
            }
        }
    }
}
