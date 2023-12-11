using Attractions.Models;
using System;
using System.Linq;
using Attractions.Tools;
using Attractions.Models.Information;
using System.Collections.Generic;


namespace Attractions
{
    public class Service
    {
        private readonly Repository _repository;

        public Service()
        {
            _repository = new Repository();
        }

       

        public PagedMessagesResult<InformationDataModel> GetPagedMessages(int page, int pageSize, string sortBy = "CreatedAt", string sortOrder = "desc")
        {
           
            var messages = _repository.GetPagedMessages(page, pageSize);

            switch (sortBy)
            {
                case "CreatedAt":
                    messages.Messages = sortOrder == "asc" ? messages.Messages.OrderBy(a => a.CreatedAt).ToList() : messages.Messages.OrderByDescending(a => a.CreatedAt).ToList();
                    break;
                case "EditAt":
                    messages.Messages = sortOrder == "asc" ? messages.Messages.OrderBy(a => a.EditAt).ToList() : messages.Messages.OrderByDescending(a => a.EditAt).ToList();
                    break;
                    
            }

            return messages;
        }


        // 新增的搜尋景點方法的實現
        public PagedMessagesResult<InformationDataModel> SearchAttractions(string searchText, int page, int pageSize, List<string> selectedAreas, List<string> selectedCities)
        {
            try
            {
                // 請在這裡實現搜尋邏輯，可以參考 Repository 中的相應方法
                var result = _repository.SearchAttractions(searchText, page, pageSize, selectedAreas, selectedCities);
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SearchAttractions Error: {ex.Message}");
                throw;
            }
        }

        public InformationDataModel GetAttractionByTitle(string title)
        {
            var result = _repository.GetAttractionByTitle(title);
            return result;
        }

        // 新增景點
        public void AddMessage(CreateModel message)
        {
            _repository.Create(message);
        }

        // 得到景點編號
        public InformationDataModel GetAttractionID(int AttractionID)
        {
            try
            {
                var message = _repository.GetAttractionID(AttractionID);

                if (message == null)
                {
                    Console.WriteLine($"ContentId 為 {AttractionID} 的消息未找到");
                    ErrorLog.LogError($"ContentId 為 {AttractionID} 的消息未找到");
                    message = new InformationDataModel();
                }

                return message;
            }
            catch
            {
                return null;
            }
        }

        // 刪除
        public bool DeleteAttraction(int AttractionID)
        {
            try
            {
                _repository.DeleteAttraction(AttractionID);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DeleteAttraction Error: {ex.Message}");
                return false;
            }
        }

        // 編輯
        public bool EditPage(InformationDataModel attraction)
        {
            try
            {
                _repository.EditPage(attraction);
                return true; 
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EditPage Error: {ex.Message}");
                throw;
            }
        }
    }
}
