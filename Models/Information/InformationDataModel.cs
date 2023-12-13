using MvcPaging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Attractions.Models.Information
{
    public class InformationDataModel
    {
        /// <summary>
        /// 最後編輯人
        /// </summary>
        public string LastEditorName { get; set; }
        /// <summary>
        /// 使用者姓名
        /// </summary>
        public string Username { get; set; }
        /// <summary>
        ///台灣區域分類名稱
        /// </summary>
        public string CategoryName { get; set; }
        /// <summary>
        /// 台灣城市名稱
        /// </summary>
        public string CityName { get; set; }
        /// <summary>
        /// 景點ID
        /// </summary>
        [Column("AttractionID")]
        public int AttractionID { get; set; }
        /// <summary>
        /// 標題
        /// </summary>
        public string AttractionTitle { get; set; }
        /// <summary>
        /// 景點內容
        /// </summary>
        public string AttractioDesc { get; set; }
        /// <summary>
        /// <summary>
        /// 新增時間
        /// </summary>
        public DateTime CreatedAt { get; set; }
        /// <summary>
        /// 編輯時間
        /// </summary>
        public DateTime? EditAt { get; set; }
        /// <summary>
        /// 新增人
        /// </summary>
        public int CreateUserID { get; set; }
        /// <summary>
        /// 編輯人
        /// </summary>
        public int EditUserID { get; set; }
        /// <summary>
        /// 總訊息
        /// </summary>
        public int TotalMessages { get; set; }
        /// <summary>
        /// 登入人編號
        /// </summary>
        public int UserId { get; set; }


    }
}