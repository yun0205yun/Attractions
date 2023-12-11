using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Attractions.Models.Loggin
{
    public class LogginViewModel
    {
        /// <summary>
        /// 使用者名稱
        /// </summary>

        [Display(Name = "帳號")]
        public string Username { get; set; }
        /// <summary>
        /// 使用者密碼
        /// </summary>
        [Display(Name = "密碼")]
        public string Password { get; set; }
        /// <summary>
        /// 是否記得我
        /// </summary>
        [Display(Name = "記住帳密")]
        public bool RememberMe { get; set; }
    }
}