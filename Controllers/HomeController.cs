using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Attractions.Models;
using Attractions.Models.Information;
using Attractions.Tools;
using MvcPaging;

namespace Attractions.Controllers
{
    public class HomeController : Controller
    {
        private const int PageSize = 5;
        private readonly Service _service;

        // 這個建構函式使用了依賴注入的方式，接受一個 Service 類別的實例作為參數。
        public HomeController(Service service)
        {
            //如果 service 參數為 null，則將拋出一個 ArgumentNullException(空引數例外處理)。
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        // 空的無參數建構函式
        public HomeController()
        {
            _service = new Service();
            // 如果未提供 Service，則使用無參數建構函式初始化。這樣做的目的是為了確保即使未提供 Service，應用程式仍能運行，避免因缺少相依性而導致的執行時錯誤。
        }

        // 顯示全部景點的頁面
        public ActionResult AttractionInformation(string searchText, int? page, string sortBy, string sortOrder)
        {
            try
            {
                // 如果 page 為空，則使用 GetValueOrDefault() 方法將其設置為默認值。
                int correctedPage = page.GetValueOrDefault();

                // 將搜索文本存儲到 ViewBag 中，以在視圖中使用。
                ViewBag.searchText = searchText;
                ViewBag.sortBy = sortBy;
                ViewBag.sortOrder = sortOrder;
                // 使用排序參數調用 GetPagedMessagesResult 方法，獲取分頁的景點信息。
                var paginatedMessages = GetPagedMessagesResult(searchText, correctedPage, PageSize, null, null, sortBy, sortOrder);

                // 使用 ToPagedList 方法將分頁結果轉換為分頁對象，並將其存儲在 pages 變數中。
                var pages = paginatedMessages.Messages.ToPagedList(correctedPage, PageSize, paginatedMessages.TotalMessages);
                // 返回 View，將分頁對象作為參數傳遞。
                return View(pages);
            }
            catch (Exception ex)
            {
                // 如果發生異常，調用 HandleError 方法處理異常，同時返回包含空數據的分頁對象。
                HandleError(ex, "前端操作出錯");
                return View(new PagedList<InformationDataModel>(new List<InformationDataModel>(), 1, PageSize));
            }
        }

        // 用來換部分頁面
        public ActionResult AjaxPage(string searchText, int? page, List<string> selectedAreas, List<string> selectedCities, string sortBy, string sortOrder)
        {
            // 如果 page 為空，則將其設置為 1。
            int correctedPage = page ?? 1;

            // 將搜索文本存儲到 ViewBag 中，以在視圖中使用。
            ViewBag.searchText = searchText;
             
            try
            {
                // 使用 GetPagedMessagesResult 方法獲取分頁的景點信息。
                var paginatedMessages = GetPagedMessagesResult(searchText, correctedPage, PageSize, selectedAreas, selectedCities, sortBy, sortOrder);

                // 如果是 AJAX 請求，返回部分視圖；否則，返回完整視圖。
                if (Request.IsAjaxRequest())
                {
                    // 如果是 AJAX 請求，使用 PartialView 方法返回名為 "partialArea" 的部分視圖。
                    return PartialView("partialArea", paginatedMessages.Messages.ToPagedList(correctedPage, PageSize, paginatedMessages.TotalMessages + 1));
                }
                else
                {
                    // 如果不是 AJAX 請求，直接使用 View 方法返回完整視圖。
                    return View(paginatedMessages.Messages.ToPagedList(correctedPage, PageSize, paginatedMessages.TotalMessages + 1));
                }
            }
            catch (Exception ex)
            {
                // 如果發生異常，調用 HandleError 方法處理異常，同時返回包含空數據的部分視圖。
                HandleError(ex, "AjaxPage 方法出錯");
                // 記錄詳細的錯誤信息
                ModelState.AddModelError("", "無法顯示留言。");
                return PartialView("partialArea", new PagedList<InformationDataModel>(new List<InformationDataModel>(), correctedPage, PageSize));
            }
        }


        // 來判斷是輸入文字搜尋還是沒有輸入
        private PagedMessagesResult<InformationDataModel> GetPagedMessagesResult(string searchText, int correctedPage, int pageSize, List<string> selectedAreas, List<string> selectedCities, string sortBy, string sortOrder)
        {
            // 如果搜索文字為空或為 null，則執行未搜索的分頁查詢。
            if (string.IsNullOrEmpty(searchText))
            {
                // 調用 _service 的 GetPagedMessages 方法，獲取分頁的景點信息。
                return _service.GetPagedMessages(correctedPage, pageSize, sortBy, sortOrder);
            }
            else
            {
                // 如果有搜索文字，則執行帶有搜索條件的分頁查詢。
                // 調用 _service 的 SearchAttractions 方法，使用搜索條件獲取分頁的景點信息。
                return _service.SearchAttractions(searchText, correctedPage, pageSize, selectedAreas, selectedCities, sortBy, sortOrder);
            }
        }


        // 新增景點
        public ActionResult Create()
        {
            // 創建一個 CreateModel 對象，用於在視圖中顯示相關的表單。
            CreateModel model = new CreateModel();

            // 返回包含 CreateModel 對象的視圖，該視圖將用於展示新增景點的相關表單。
            return View(model);
        }


        // 創建新增景點
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateModel createModel)
        {
            // 檢查模型的驗證狀態
            if (!ModelState.IsValid)
            {
                // 如果模型驗證失敗，返回包含錯誤信息的視圖
                return View(createModel);
            }

            // 檢查用戶是否已經登錄
            if (Session["Username"] == null)
            {
                // 如果未登錄，重定向到登錄頁面
                return RedirectToAction("Loggin", "Account");
            }

            try
            {
                // 獲取當前登錄用戶的UserId
                int userId = (int)Session["UserId"];

                // 創建新的 CreateModel 對象，包含用戶提交的景點信息
                var newMessage = new CreateModel
                {
                    CreateUserID = userId,//這裡新增人設為登入人ID
                    CreatedAt = DateTime.Now,
                    CategoryName = createModel.CategoryName,
                    CityName = createModel.CityName,
                    AttractionTitle = createModel.AttractionTitle,
                    AttractionDesc = createModel.AttractionDesc
                };

                // 調用 _service 的 AddMessage 方法，將新的景點信息添加到數據庫
                _service.AddMessage(newMessage);

                // 新增成功，重定向到顯示所有景點的頁面
                return RedirectToAction("AttractionInformation", "Home");
            }
            catch (Exception ex)
            {
                // 處理異常情況，記錄錯誤信息
                HandleError(ex, $"Create 操作出錯: {ex.Message}");

                // 在模型狀態中添加錯誤信息，並返回包含錯誤信息的視圖
                ModelState.AddModelError("", "無法創建留言。");
                return View(createModel);
            }
        }


        // 依照區域分類取得城市
        [HttpGet]
        public ActionResult GetCities(string area)
        {
            // 調用 CreateModel 的 GetCitiesByArea 靜態方法，根據區域名稱獲取城市列表
            List<SelectListItem> cities = CreateModel.GetCitiesByArea(area);

            // 將城市列表以 JSON 格式返回，用於異步請求//JsonRequestBehavior.AllowGet允許來自用戶端的 HTTP GET 要求。
            return Json(cities, JsonRequestBehavior.AllowGet);
        }

        // 景點內容(按標題下去會出現描述)
        public ActionResult AttractionContent(string title)
        {
            // 調用 _service 的 GetAttractionByTitle 方法，根據標題獲取景點信息
            var attraction = _service.GetAttractionByTitle(title);

            // 返回一個視圖，視圖中包含景點信息
            return View(attraction);
        }


        // 刪除景點
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteAttraction(int AttractionID)
        {
            try
            {
                // 調用 _service 的 DeleteAttraction 方法，嘗試刪除指定 AttractionID 的景點
                bool success = _service.DeleteAttraction(AttractionID);

                // 根據刪除是否成功返回相應的 JSON 結果
                if (success)
                {
                    return Json(new { Success = true, Message = "景點刪除成功。" });
                }
                else
                {
                    return Json(new { Success = false, Message = "刪除景點失敗。" });
                }
            }
            catch (Exception ex)
            {
                // 處理異常情況，記錄錯誤信息
                HandleError(ex, $"DeleteAttraction Error: {ex.Message}");

                // 返回包含錯誤信息的 JSON 結果
                return Json(new { Success = false, Message = "刪除景點失敗。錯誤: " + ex.Message });
            }
        }

        // 用於獲取現有景點的信息以顯示在編輯表單中
        public ActionResult EditPage(int AttractionID)
        {
            // 根據 AttractionID 獲取對應的景點信息
            var attraction = _service.GetAttractionID(AttractionID);

            // 返回包含景點信息的視圖，用於顯示編輯表單
            return View(attraction);
        }

        [HttpPost]//處理提交的表單
        [ValidateAntiForgeryToken]
        public ActionResult EditPage(InformationDataModel attraction)
        {
            try
            {
                // 獲取當前用戶的ID
                int userId = (int)Session["UserId"];

                // 調用 _service 的 EditPage 方法，嘗試更新景點信息
                bool success = _service.EditPage(attraction);

                // 設置編輯者的ID
                attraction.EditUserID = userId;

                // 根據更新是否成功執行相應的操作
                if (success)
                {
                    // 如果成功，重定向到顯示全部景點的頁面
                    return RedirectToAction("AttractionInformation", "Home");
                }
                else
                {
                    // 如果失敗，返回包含錯誤信息的 JSON 結果
                    return Json(new { Success = false, Message = "編輯景點失敗。" });
                }
            }
            catch (Exception ex)
            {
                // 處理異常情況，記錄錯誤信息
                HandleError(ex, $"EditPage Error: {ex.Message}");

                // 返回包含錯誤信息的 JSON 結果
                return Json(new { Success = false, Message = "編輯景點失敗。錯誤: " + ex.Message });
            }
        }

        // 處理錯誤的私有方法
        private void HandleError(Exception ex, string message)
        {
            // 在控制台中輸出錯誤信息
            Console.WriteLine($"{message}: {ex.Message}");

            // 將錯誤信息添加到模型狀態中，用於顯示給用戶
            ModelState.AddModelError("", "無法顯示留言。");
        }

    }
}

