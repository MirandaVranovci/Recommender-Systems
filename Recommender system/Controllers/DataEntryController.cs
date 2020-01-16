using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using Recommender_system.Models;

namespace Recommender_system.Controllers
{
    public class DataEntryController : BaseController
    {
        SRDB db = new SRDB();
        // GET: DataEntry
        public ActionResult Index()
        {
            return View();
        }

        // GET: DataEntry/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: DataEntry/Create
        public async Task<ActionResult> Create()
        {
            var user = await GetUser();
            return View();
        }

        // POST: DataEntry/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(DataEntryUser data_entry)
        {
            Session["perdoruesi"] = null;
            var user = await GetUser();
            MessageJS returnmodel = new MessageJS();
            if (ModelState.IsValid)
            {
                try
                {
                    USER_DATA addmodel = new USER_DATA();
                    addmodel.ID = data_entry.ID;
                    addmodel.BloodGlucose = data_entry.Glukoza;
                    addmodel.UserID = user.ID;
                    var bmi = Convert.ToDouble(data_entry.Pesha) / Math.Pow(Convert.ToDouble(data_entry.Gjatesia/100),2);
                    addmodel.BMI = Convert.ToDecimal(bmi.ToString("0.00"));

                    var lista = db.USER_DATA.Where(q=>q.UserID!=user.ID).ToList();
               
                    List<ListaKnn> lista_knn = new List<ListaKnn>();
                    double neighbors_distance = 0;
                    foreach (var item in lista)
                    {
                        var distanca = Math.Pow(Convert.ToDouble(item.BMI.Value - Convert.ToDecimal(bmi)), 2) + Math.Pow(Convert.ToDouble(item.BloodGlucose.Value - data_entry.Glukoza), 2);
                        neighbors_distance = Math.Sqrt(distanca);
                        lista_knn.Add(new ListaKnn()
                        {
                            distanca = neighbors_distance,
                            userID = item.UserID.Value,
                            classification=item.HasDiabet.Value
                            
                        });
                    }
                    //knn
                    int k = 5;
                    var selekto_perdoruesit = (from t in lista_knn orderby t.distanca descending select t).ToList().Take(k);
                    var count_false = (from bashkesia in selekto_perdoruesit where bashkesia.classification = false select bashkesia).ToList();
                    var count_true = (from bashkesia in selekto_perdoruesit where bashkesia.classification = true select bashkesia).ToList();

                    bool klasifikimi = false;
                    var perdoruesit_me_afert = count_false.OrderByDescending(q => q.distanca).FirstOrDefault();

                    if (count_false.Count() < count_true.Count())
                    {
                        klasifikimi = true;
                        perdoruesit_me_afert = count_true.OrderByDescending(q => q.distanca).FirstOrDefault();
                    } 
                    Session["perdoruesi"] = db.USER_DATA.Where(q => q.UserID == perdoruesit_me_afert.userID).FirstOrDefault();
                    addmodel.HasDiabet = klasifikimi;
                    db.USER_DATA.Add(addmodel);
                    await db.SaveChangesAsync();
                    returnmodel.Status = true;
                    returnmodel.ID = addmodel.ID;
                    return Json(returnmodel, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    returnmodel.Status = false;
                    returnmodel.Message = "Ka ndodhur një gabim";
                    return Json(returnmodel, JsonRequestBehavior.DenyGet);
                }
            }
            else
            {
                var errors = ModelState.Select(x => x.Value.Errors)
                       .Where(y => y.Count > 0)
                       .ToList();
            }
            returnmodel.Status = false;
            return Json(returnmodel, JsonRequestBehavior.DenyGet);

        }

        // GET: DataEntry/Edit/5
        public async Task<ActionResult> Rekomandimet()
        {
            var user = await GetUser();
            var rekomandimet =(USER_DATA)Session["perdoruesi"];
            var meals = db.USER_RATING.ToList();

            if(rekomandimet !=null)
            {
                 meals = db.USER_RATING.Where(q => q.UserID == rekomandimet.UserID).OrderByDescending(q => q.RatingID).Take(4).ToList();
                if(meals.Count()==0)
                {
                    var rand = new Random();
                    var lista = db.USER_RATING.Take(30).ToList();
                    List<USER_RATING> ushqimet = new List<USER_RATING>();
                    for (int i = 0; i < 6; i++)
                    {
                        int r = rand.Next(lista.Count);
                        USER_RATING m = lista.Skip(r).First();
                        if (!ushqimet.Contains(m))
                        {
                            ushqimet.Add(m);
                        }
                    }
                    meals = ushqimet;
                }
             
            }
            else
            {
                 meals = db.USER_RATING.Take(4).ToList();
            }


            return View(meals);
        }

        // POST: DataEntry/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: DataEntry/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: DataEntry/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
