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
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalMessages { get; set; }
        public string ErrorMessage { get; set; }
        public string ExceptionStackTrace { get; set; }
    }
}