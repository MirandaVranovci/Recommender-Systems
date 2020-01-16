using Recommender_system.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Recommender_system.Controllers
{

    [Authorize]
    public class HomeController : BaseController
    {
        SRDB db = new SRDB();
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public async Task<ActionResult> GetUserInfo()
        {
            UserInfoModel model = new UserInfoModel();
            var user = await GetUser();  
            model.Emri = user.Emri + " " + user.Mbiemri; 
            model.ID = user.ID;
            return View(model);
        }
    }
}