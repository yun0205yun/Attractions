using System;
using System.Web;
using System.Web.Mvc;
using Attractions.Models.Loggin;
using Attractions.Models.Register;

namespace Attractions.Controllers
{

    public class AccountController : Controller
    {
        private readonly Repository _repository = new Repository();
        public ActionResult Loggin()
        {
            var model = new LogginViewModel();

            // 檢查是否存在 "RememberMe" Cookie 並相應地設置模型
            var rememberMeCookie = Request.Cookies["RememberMe"];
            if (rememberMeCookie != null)
            {
                model.RememberMe = true;
                model.Username = rememberMeCookie.Value;
            }

            // 檢查是否在 Session 中存在成功登入的標誌
            var loginIsSuccessful = Session["LoginIsSuccessful"] as bool?;
            if (loginIsSuccessful != null && loginIsSuccessful.Value)
            {
                // 清除 Session 中的標誌
                Session.Remove("LoginIsSuccessful");
                return RedirectToAction("AttractionInformation", "Home");
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult Loggin(LogginViewModel model)
        {

            // 檢查登入是否成功
            var user = _repository.IsLoginSuccessful(model);

            if (user.IsLoginSuccessful)
            {
                // 在 Session 中設置使用者資訊
                Session["UserId"] = user.UserId; // 假設您的 User 對象中有一個 UserId 屬性
                // 在 Session 中設置使用者資訊
                Session["Username"] = model.Username; // 請根據 User 物件中的實際屬性進行替換

                // 如果勾選了 "RememberMe"，則設置 Cookie
                if (model.RememberMe)
                {
                    SetRememberMeCookie(model.Username);
                }
                // 清除之前登入的用戶名
                Session.Remove("RememberedUsername");
                // 重定向到 "Front"
                return RedirectToAction("AttractionInformation", "Home");
            }

            // 如果登入失敗，則添加模型錯誤
            ModelState.AddModelError("", "登入失敗，請檢查帳號密碼");
            return View(model);
        }
        public ActionResult Logout()
        {
            // 清除 Session 和 Cookie
            Session.Clear();
            //Response.Cookies["RememberMe"].Expires = DateTime.Now.AddDays(-1);

            // 存儲登出的用戶名到 ViewBag
            ViewBag.LoggedOutUsername = GetRememberMeCookie();

            // 重定向到登入頁面，並將登出的用戶名保存在 Cookie 中
            return RedirectToAction("Loggin", "Account");
        }
        public ActionResult Register()
        {
            // 初始化視圖模型
            var viewModel = new RegisterViewModel();

            // 你的動作方法邏輯

            return View(viewModel);
        }
        [HttpPost]//接收到RegisterViewModel裡的數據去執行
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)//模型驗證
            {
                if (model.Password != model.ConfirmPassword)
                {
                    ModelState.AddModelError("ConfirmPassword", "密碼和確認密碼不一致");
                    return View(model);
                }
                // 嘗試註冊使用者
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
        private void SetRememberMeCookie(string username)
        {
            // 使用 Cookie 存儲使用者名稱
            var cookie = new HttpCookie("RememberMe")
            {
                Value = username,
                Expires = DateTime.Now.AddMonths(1)
            };

            Response.Cookies.Add(cookie);

        }
        private string GetRememberMeCookie()
        {
            var rememberMeCookie = Request.Cookies["RememberMe"];
            return rememberMeCookie?.Value;
        }
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