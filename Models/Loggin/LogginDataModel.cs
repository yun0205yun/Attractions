using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Attractions.Models.Loggin
{
    public class LogginDataModel
    {
        public string Username { get; set; }

        /// <summary>
        /// 使用者密碼
        /// </summary>
        public int UserId { get; set; }
        public string Password { get; set; }
        /// <summary>
        /// 是否記得我
        /// </summary>
        public bool RememberMe { get; set; }
        public bool IsLoginSuccessful { get; internal set; }
    }
}