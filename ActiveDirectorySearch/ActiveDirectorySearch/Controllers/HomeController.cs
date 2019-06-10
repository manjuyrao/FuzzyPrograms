using ActiveDirectorySearch.Helper;
using ActiveDirectorySearch.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ActiveDirectorySearch.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var loginId = User.Identity.Name;
            //If user Id has main name prefix
            var domainIdx = loginId.IndexOf("\\") + 1;
            if (domainIdx != -1)
            {
                loginId = loginId.Substring(domainIdx);
            }
            AdUserModel userDetails = ActiveDirectoryHelper.GetUserDetailsFromLoginId(loginId);

            return View(userDetails);
        }

        [HttpGet]
        public JsonResult GetUserByUserId(string loginId)
        {
            AdUserModel userDetails = ActiveDirectoryHelper.GetUserDetailsFromLoginId(loginId);
            return Json(userDetails, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetUserByEmailId(string emailId)
        {
            string userDisplayName = ActiveDirectoryHelper.GetUserDisplayNameByEmail(emailId);
            return Json(userDisplayName, JsonRequestBehavior.AllowGet);
        }
    }
}