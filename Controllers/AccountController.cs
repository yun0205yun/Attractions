using System;
using System.Web;
using System.Web.Mvc;
using Attractions.Models.Loggin;
using Attractions.Models.Register;

namespace Attractions.Controllers
{
    public class AccountController : Controller
    {
        //宣告了一個名為 _repository 的私有變數
        private readonly Repository _repository = new Repository();

        // GET: /Account/Loggin
        public ActionResult Loggin()
        {
            // 創建 LogginViewModel 模型對象
            var model = new LogginViewModel();

            // 檢查是否存在 "RememberMe" Cookie 並相應地設置模型
            var rememberMeCookie = Request.Cookies["RememberMe"];
            if (rememberMeCookie != null)
            {
                //如果存在，將模型的 RememberMe 屬性設為 true，並將使用者名稱設為該 Cookie 的值。
                model.RememberMe = true;
                model.Username = rememberMeCookie.Value;
            }

            // 檢查是否在 Session 中存在成功登入的標誌
            var loginIsSuccessful = Session["LoginIsSuccessful"] as bool?;
            if (loginIsSuccessful != null && loginIsSuccessful.Value)
            {
                // 清除 Session 中的標誌，然後重定向到 AttractionInformation 頁面
                Session.Remove("LoginIsSuccessful");
                return RedirectToAction("AttractionInformation", "Home");
            }

            // 顯示登入頁面
            return View(model);
        }

        // POST: /Account/Loggin
        [HttpPost]
        public ActionResult Loggin(LogginViewModel model)
        {
            // 調用 _repository 的 IsLoginSuccessful 方法檢查登入是否成功
            var user = _repository.IsLoginSuccessful(model);
            // 檢查登入是否成功
            if (user.IsLoginSuccessful)
            {
                // 在 Session 中設置使用者資訊
                Session["UserId"] = user.UserId;  
                // 在 Session 中設置使用者資訊
                Session["Username"] = model.Username;  

                // 如果勾選了 "RememberMe"，則設置 Cookie
                if (model.RememberMe)
                {
                    SetRememberMeCookie(model.Username);
                }
                // 清除之前登入的用戶名
                Session.Remove("RememberedUsername");
                // 重定向到 "AttractionInformation" 頁面
                return RedirectToAction("AttractionInformation", "Home");
            }

            // 如果登入失敗，則添加模型錯誤
            ModelState.AddModelError("", "登入失敗，請檢查帳號密碼");
            return View(model);
        }

        // GET: /Account/Logout
        public ActionResult Logout()
        {
            // 清除 Session 和 Cookie
            Session.Clear();

            // 存儲登出的用戶名到 ViewBag
            ViewBag.LoggedOutUsername = GetRememberMeCookie();

            // 重定向到登入頁面，並將登出的用戶名保存在 Cookie 中
            return RedirectToAction("Loggin", "Account");
        }

        // GET: /Account/Register
        public ActionResult Register()
        {
            // 初始化視圖模型
            var viewModel = new RegisterViewModel();

            // 你的動作方法邏輯

            return View(viewModel);
        }

        // POST: /Account/Register
        [HttpPost] //接收來自 RegisterViewModel 的 POST 請求
        [ValidateAntiForgeryToken]// 防止 CSRF 攻擊的屬性
        public ActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)//模型驗證
            {
                if (model.Password != model.ConfirmPassword)
                {
                    // 如果不相符，將錯誤添加到模型
                    ModelState.AddModelError("ConfirmPassword", "密碼和確認密碼不一致");
                    // 返回帶有錯誤的視圖
                    return View(model);
                }
                //嘗試註冊使用者，獲取註冊狀態
                string registrationStatus = _repository.RegisterViewModel(model.Username, model.Password);

                if (registrationStatus == "註冊成功")
                {
                    // 註冊成功，重定向到登入頁面
                    return RedirectToAction("Loggin");
                }
                else
                {
                    // 如果有註冊錯誤，顯示錯誤
                    ModelState.AddModelError("", registrationStatus);
                }
            }

            // 如果 ModelState 無效或註冊失敗，返回帶有錯誤的視圖
            return View(model);
        }

        // 設置 RememberMe Cookie
        private void SetRememberMeCookie(string username)
        {
            // 創建一個名稱為 "RememberMe" 的 HTTP Cookie
            var cookie = new HttpCookie("RememberMe")
            {
                Value = username, // 將使用者名稱設置為 Cookie 的值                      
                Expires = DateTime.Now.AddMonths(1)   // 設置 Cookie 過期時間為一個月後
            };

            // 將 Cookie 添加到 HTTP 響應的 Cookies 集合中
            Response.Cookies.Add(cookie);
        }


        // 獲取 RememberMe Cookie 的值
        private string GetRememberMeCookie()
        {
            // 從請求中獲取名稱為 "RememberMe" 的 Cookie
            var rememberMeCookie = Request.Cookies["RememberMe"];

            // 返回 Cookie 的值，如果 Cookie 存在的話；否則返回 null
            return rememberMeCookie?.Value;
        }


        // GET: /Account/Front
        public ActionResult Front()
        {
            // 檢查使用者是否已經登入
            if (Session["Username"] != null)
            {
                // 使用者已經登入 
                return RedirectToAction("AttractionInformation", "Home");
            }
            else
            {
                // 使用者尚未登入，重定向到登入頁面
                return RedirectToAction("Loggin", "Account");
            }
        }
    }
}
