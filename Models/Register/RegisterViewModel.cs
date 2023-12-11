using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Attractions.Models.Register
{
    public class RegisterViewModel
    {
        [Required]
        [Display(Name = "帳號")]
        public string Username { get; set; }
        [Required(ErrorMessage = "請輸入密碼")]
        [DataType(DataType.Password)]
        [Display(Name = "密碼")]
        public string Password { get; set; }
        [Required(ErrorMessage = "請確認密碼")]
        [DataType(DataType.Password)]
        [Display(Name = "確認密碼")]
        [Compare("Password", ErrorMessage = "密碼和確認密碼不一致")]
        public string ConfirmPassword { get; set; }
    }
}