﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Attractions.Models
{
    public class UserDataModel
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
    }
}