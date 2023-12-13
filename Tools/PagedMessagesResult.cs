using Attractions.Models.Information;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Attractions.Tools
{
    public class PagedMessagesResult<T>
    {
        public List<T> Messages { get; set; }
        /// <summary>
        /// 當前頁面
        /// </summary>
        public int CurrentPage { get; set; }
        /// <summary>
        /// 頁面會出現幾項景點
        /// </summary>
        public int PageSize { get; set; }
        /// <summary>
        /// 總頁數
        /// </summary>
        public int TotalPages { get; set; }
        /// <summary>
        /// 總景點數
        /// </summary>
        public int TotalMessages { get; set; }

        public string ErrorMessage { get; set; }
        public string ExceptionStackTrace { get; set; }
    }
}