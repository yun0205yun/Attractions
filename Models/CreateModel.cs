using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static System.Net.Mime.MediaTypeNames;

namespace Attractions.Models
{
    public class CreateModel
    {
        public int UserId { get; set; }
        /// <summary>
        /// 新增人
        /// </summary>
        public int CreateUserID { get; set; }
        /// <summary>
        /// <summary>
        /// 新增時間
        /// </summary>

        public DateTime CreatedAt { get; set; }
        /// <summary>
        ///台灣區域分類名稱
        /// </summary>

        public string CategoryName { get; set; }
        /// <summary>
        /// 台灣城市名稱
        /// </summary>
        public string CityName { get; set; }
         
        /// <summary>
        /// 標題
        /// </summary>
        public string AttractionTitle { get; set; }
        /// <summary>
        /// 景點描述
        /// </summary>
        public string AttractionDesc { get; set; }
        public string ContentText { get; internal set; }

        public static List<SelectListItem> GetAreaCategories()
        {
            var areaCategories = new List<CreateModel>
            {
                new CreateModel { CategoryName = "北基宜"},
                new CreateModel { CategoryName = "桃竹苗"},
                new CreateModel { CategoryName = "中彰投"},
                new CreateModel { CategoryName = "雲嘉南"},
            };

            var selectList = areaCategories.Select(c => new SelectListItem
            {
                Value = c.CategoryName,
                Text=c.CategoryName,

            }).ToList();

            return selectList;
        }

        public static List<SelectListItem> GetCitiesByArea(string area)
        {

            List<SelectListItem> cities = new List<SelectListItem>();

            if (area == "北基宜")
            {
                cities.Add(new SelectListItem { Value = "台北", Text = "台北" });
                cities.Add(new SelectListItem { Value = "新北", Text = "新北" });
                cities.Add(new SelectListItem { Value = "基隆", Text = "基隆" });
                cities.Add(new SelectListItem { Value = "宜蘭", Text = "宜蘭" });
            }
            if (area == "桃竹苗")
            {
                cities.Add(new SelectListItem { Value = "桃園", Text = "桃園" });
                cities.Add(new SelectListItem { Value = "新竹", Text = "新竹" });
                cities.Add(new SelectListItem { Value = "苗栗", Text = "苗栗" });
            }
            if (area == "中彰投")
            {
                cities.Add(new SelectListItem { Value = "台中", Text = "台中" });
                cities.Add(new SelectListItem { Value = "彰化", Text = "彰化" });
                cities.Add(new SelectListItem { Value = "南投", Text = "南投" });
            }
            if (area == "雲嘉南")
            {
                cities.Add(new SelectListItem { Value = "雲林", Text = "雲林" });
                cities.Add(new SelectListItem { Value = "嘉義", Text = "嘉義" });
                cities.Add(new SelectListItem { Value = "台南", Text = "台南" });
            }
            return cities;
        }

    }
}