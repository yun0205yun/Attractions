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

        // 使用依賴注入
        public HomeController(Service service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        // 空的無參數建構函式
        public HomeController()
        {
            _service = new Service(); // 初始化 _service，這裡假設 Repository 也具有無參數建構函式
        }

        // 顯示全部景點的頁面
        public ActionResult AttractionInformation(string searchText, int? page, string sortBy, string sortOrder)
        {
            try
            {
                int correctedPage = page.GetValueOrDefault();
                ViewBag.searchText = searchText;

                // 使用排序參數
                var paginatedMessages = GetPagedMessagesResult(searchText, correctedPage, PageSize, null, null, sortBy, sortOrder);

                var pages = paginatedMessages.Messages.ToPagedList(correctedPage, PageSize, paginatedMessages.TotalMessages);

                // 將排序參數存儲到 ViewBag 中，以便在 View 中使用
                ViewBag.SortOrder = sortOrder;

                return View(pages);
            }
            catch (Exception ex)
            {
                HandleError(ex, "前端操作出錯");
                return View(new PagedList<InformationDataModel>(new List<InformationDataModel>(), 1, PageSize));
            }
        }

        // 用來換部分頁面
        public ActionResult AjaxPage(string searchText, int? page, List<string> selectedAreas, List<string> selectedCities, string sortBy, string sortOrder)
        {
            int correctedPage = page ?? 1;
            ViewBag.searchText = searchText;

            try
            {
                var paginatedMessages = GetPagedMessagesResult(searchText, correctedPage, PageSize, selectedAreas, selectedCities, sortBy, sortOrder);

                if (Request.IsAjaxRequest())
                {
                    return PartialView("partialArea", paginatedMessages.Messages.ToPagedList(correctedPage, PageSize, paginatedMessages.TotalMessages + 1));
                }
                else
                {
                    return View(paginatedMessages.Messages.ToPagedList(correctedPage, PageSize, paginatedMessages.TotalMessages + 1));
                }
            }
            catch (Exception ex)
            {
                HandleError(ex, "AjaxPage 方法出錯");
                // 記錄詳細的錯誤信息
                ModelState.AddModelError("", "無法顯示留言。");
                return PartialView("partialArea", new PagedList<InformationDataModel>(new List<InformationDataModel>(), correctedPage, PageSize));
            }
        }

        // 來判斷是輸入文字搜尋  還是沒有輸入
        private PagedMessagesResult<InformationDataModel> GetPagedMessagesResult(string searchText, int correctedPage, int pageSize, List<string> selectedAreas, List<string> selectedCities, string sortBy, string sortOrder)
        {
            if (string.IsNullOrEmpty(searchText))
            {
                return _service.GetPagedMessages(correctedPage, pageSize, sortBy, sortOrder);
            }
            else
            {
                return _service.SearchAttractions(searchText, correctedPage, pageSize, selectedAreas, selectedCities);
            }
        }

        // 新增景點
        public ActionResult Create()
        {
            CreateModel model = new CreateModel();
            return View(model);
        }

        // 創建新增景點
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateModel createModel)
        {
            if (!ModelState.IsValid)
            {
                return View(createModel);
            }

            if (Session["Username"] == null)
            {
                return RedirectToAction("Loggin", "Account");
            }
            try
            {
                int userId = (int)Session["UserId"];

                var newMessage = new CreateModel
                {
                    CreateUserID = userId,
                    CreatedAt = DateTime.Now,
                    CategoryName = createModel.CategoryName,
                    CityName = createModel.CityName,
                    AttractionTitle = createModel.AttractionTitle,
                    AttractionDesc = createModel.AttractionDesc
                };

                _service.AddMessage(newMessage);

                return RedirectToAction("AttractionInformation", "Home");
            }
            catch (Exception ex)
            {
                HandleError(ex, $"Create 操作出錯: {ex.Message}");
                ModelState.AddModelError("", "無法創建留言。");
                return View(createModel);
            }
        }

        // 依照區域分類取得城市
        [HttpGet]
        public ActionResult GetCities(string area)
        {
            List<SelectListItem> cities = CreateModel.GetCitiesByArea(area);
            return Json(cities, JsonRequestBehavior.AllowGet);
        }

        // 景點內容(按標題下去會出現描述)
        public ActionResult AttractionContent(string title)
        {
            var attraction = _service.GetAttractionByTitle(title);
            return View(attraction);
        }

        // 刪除景點
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteAttraction(int AttractionID)
        {
            try
            {
                bool success = _service.DeleteAttraction(AttractionID);

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
                HandleError(ex, $"DeleteAttraction Error: {ex.Message}");
                return Json(new { Success = false, Message = "刪除景點失敗。錯誤: " + ex.Message });
            }
        }

        // 編輯更新景色
        public ActionResult EditPage(int AttractionID)
        {
            var attraction = _service.GetAttractionID(AttractionID);
            return View(attraction);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditPage(InformationDataModel attraction)
        {
            try
            {
                int userId = (int)Session["UserId"];
                bool success = _service.EditPage(attraction);
                attraction.EditUserID = userId;
                if (success)
                {
                    return RedirectToAction("AttractionInformation", "Home");
                }
                else
                {
                    return Json(new { Success = false, Message = "編輯景點失敗。" });
                }
            }
            catch (Exception ex)
            {
                HandleError(ex, $"EditPage Error: {ex.Message}");
                return Json(new { Success = false, Message = "編輯景點失敗。錯誤: " + ex.Message });
            }
        }

        private void HandleError(Exception ex, string message)
        {
            Console.WriteLine($"{message}: {ex.Message}");
            ModelState.AddModelError("", "無法顯示留言。");
        }
    }
}

