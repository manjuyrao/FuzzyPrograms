using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ReadDataFromExcel.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult UploadExcel(HttpPostedFileBase excelFile)
        {
            DataSet dataSet = new DataSet();
            try
            {
                var fileName = Path.GetFileName(excelFile.FileName);
                var physicalPath = Path.Combine(Server.MapPath("~/App_Data"), fileName);

                excelFile.SaveAs(physicalPath);

                string sheetName = "Sheet1";

                ExcelHelper excelHelper = new ExcelHelper();

                dataSet = excelHelper.GetDataBySheetName(physicalPath, sheetName);
            }
            catch
            {
                return Content("Failed");
                //TODO handle error case here
            }

            if (dataSet.Tables.Count > 0)
            {
                return View("Content", dataSet.Tables[0]);
            }
            else
            {
                return Content("Failed");
            }
        }
    }
}