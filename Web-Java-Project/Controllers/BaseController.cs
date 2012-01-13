using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Diagnostics;
using System.Threading;
using System.IO;
using Web_Java_Project.Utilities;
using System.Web.Hosting;

namespace Web_Java_Project.Controllers
{
    public class BaseController : Controller
    {
        private static StreamWriter logFile = null;
        private static DateTime creationDate = new DateTime(2000, 1, 1);
        private static DateTime lastSaveTime = new DateTime(2000, 1, 1);

        public static void printLog(string message)
        {
            if ((DateTime.Now - creationDate).TotalHours > 24)
            {
                creationDate = DateTime.Now;
                lastSaveTime = creationDate;

                if (logFile != null)
                {
                    logFile.Close();
                    logFile.Dispose();
                }

                logFile = new StreamWriter(HostingEnvironment.MapPath("~/logs/") 
                        + creationDate.Year.ToString() + "-"
                        + creationDate.Month.ToString() + "-"
                        + creationDate.Day.ToString() + ".log.txt");
            }

            logFile.WriteLine("[" + DateTime.Now.ToString() + "] " + message);

            if ((DateTime.Now - lastSaveTime).TotalSeconds > 5)
            {
                lastSaveTime = DateTime.Now;
                logFile.Flush();
            }
        }

        protected override void ExecuteCore()
        {
            string cultureName = null;
            // Attempt to read the culture cookie from Request
            HttpCookie cultureCookie = Request.Cookies["_culture"];
            if (cultureCookie != null)
                cultureName = cultureCookie.Value;
            else
                cultureName = Request.UserLanguages[0]; // obtain it from HTTP header AcceptLanguages

            cultureName = CultureHelper.GetValidCulture(cultureName);
         
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(cultureName);
            Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;
            
            base.ExecuteCore();
        }
    }

}
