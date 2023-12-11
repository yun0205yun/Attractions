using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Attractions.Tools
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }//T可以任意型態
    }
}