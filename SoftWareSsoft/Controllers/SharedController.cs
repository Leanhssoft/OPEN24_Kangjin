﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SoftWareSsoft.Controllers
{
    public class SharedController : Controller
    {
        // GET: Shared
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult _Header()
        {
            return PartialView();
        }

        public ActionResult _Footer()
        {
            return PartialView();
        }
    }
}