using Recommender_system.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Recommender_system.Controllers
{
    public class RekomandimetController : BaseController
    {
        SRDB db = new SRDB();
        public ActionResult Index()
        {
           var rand = new Random();
            var lista = db.MEALS.ToList();
            List<MEAL> ushqimet = new List<MEAL>();
            for (int i = 0; i < 6; i++)
            {
                int r = rand.Next(lista.Count);
                MEAL m = lista.Skip(r).First();
                if (!ushqimet.Contains(m))
                {
                    ushqimet.Add(m);
                }  
            }
            return View(ushqimet);
        }
        public async Task<ActionResult> CalculatePearson(IList<USER_RATING> meals_model)
        {
            Session["meals"] = null;
            Session["listaPerfundimtare"] = null;
            var user = await GetUser();

            MessageJS returnmodel = new MessageJS();
            List<Pearson_Corelation> pearson_corelation = new List<Pearson_Corelation>();

            if (ModelState.IsValid)
            {
                try
                { 
                    //kshyrmi ne db qe contains krejt qeto meals qe i ban rating useri

                    var meals_db = db.USER_RATING.Where(q => q.UserID != user.ID).Select(q => q.MealID).ToList();

                    var meals_common = meals_model.Where(c => meals_db.Contains(c.MealID)).Select(q=>q.MealID).ToList();

                    //mirret lista e itemsave nga db per qeto meals (vetem userat e ngjajshem)
                    var lista = db.USER_RATING.Where(q => meals_common.Contains(q.MealID)).ToList();

                    float Pearson_Score = 0;
 
                    //grupimi ne baze te userave
                    foreach (var item in lista.GroupBy(q=>q.UserID))
                    {
                        List<int> item_in_corelation = new List<int>();

                        List<int> item_in_corelation_db = new List<int>();

                        //secili rating per meals te njejte te user i qasur dhe userave te tjere futet ne listat perkatese
                        foreach (var item1 in item)
                        {
                            var lista_userit = meals_model.Where(q => q.MealID == item1.MealID).FirstOrDefault();
                            if(lista_userit != null)
                            {
                                item_in_corelation.Add(lista_userit.RatingID);

                                item_in_corelation_db.Add(item1.RatingID);
                            }
                            else
                            {
                                break;
                            }
                        }
                        //kalkulohet pearson
                       
                        Pearson_Score= Coleration(item_in_corelation, item_in_corelation_db, item_in_corelation.Count());

                        pearson_corelation.Add(new Pearson_Corelation { UserID = item.Key, PearsonScore= Pearson_Score});
                    }

                    //most similar scores;
                    var lista_perfundimtare = pearson_corelation.OrderByDescending(q => q.PearsonScore).Take(3).ToList();

                    Session["listaPerfundimtare"] = lista_perfundimtare;

                    Session["meals"] = meals_model.ToList();

                    //insertimi i te dhenave ne baze

                    
                    foreach(var item in meals_model)
                    {
                        USER_RATING model = new USER_RATING();

                        var exist = db.USER_RATING.Any(q => q.MealID == item.MealID && q.UserID == user.ID);

                        var ratings = db.USER_RATING.Where(q => q.MealID == item.MealID && q.UserID == user.ID).ToList();

                        model.UserID = user.ID;

                        var average_rating = (item.RatingID + ratings.Sum(q => q.RatingID))/(1+ratings.Count());

                        model.RatingID = item.RatingID;

                        model.MealID = item.MealID;

                        
                        if(!exist)
                        {
                            db.USER_RATING.Add(model);
                        }
                     
                        
                        await db.SaveChangesAsync();
                    }
                  

                    returnmodel.Message = "Sukses";
                    returnmodel.Status = true;
                    return Json(returnmodel, JsonRequestBehavior.AllowGet);

                }
                catch (Exception ex)
                {
                    returnmodel.Message = "Ka ndodhur nje gabim";
                    returnmodel.Status = false;
                    return Json(returnmodel, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(returnmodel, JsonRequestBehavior.AllowGet);
        }



       public static float Coleration(List<int> X, List<int> Y, int n)
        {
            int sum_X = 0, sum_Y = 0, sum_XY = 0;
            int squareSum_X = 0, squareSum_Y = 0;

            for (int i = 0; i < n; i++)
            {
                // shuma e elementeve te listes X 
                sum_X = sum_X + X[i];

                // shuma e elementeve te listes Y 
                sum_Y = sum_Y + Y[i];

                //shuma  X[i] * Y[i]
                sum_XY = sum_XY + X[i] * Y[i];

                // shuma e katroreve te elementeve te listave 
                squareSum_X = squareSum_X + X[i] * X[i];
                squareSum_Y = squareSum_Y + Y[i] * Y[i];
            }

            // formula per kalkulimin e Korelacionit te Pearsonit
            float correlation = (float)(n * sum_XY - sum_X * sum_Y) /
                         (float)(Math.Sqrt((n * squareSum_X -
                         sum_X * sum_X) * (n * squareSum_Y -
                         sum_Y * sum_Y)));

            return correlation;
        }

        public async Task<ActionResult> WeightedAverageSum()
        {
            var user = await GetUser();
            var scores_users = (List<Pearson_Corelation>)Session["listaPerfundimtare"];
            var meals= (List<USER_RATING>)Session["meals"];

            List<MEAL> rekomandimet = new List<MEAL>();
            var lista1 = scores_users.Select(q => q.UserID).ToList();
            var lista2 = meals.Select(q => q.MealID).ToList();

            var others= db.USER_RATING.Where(p => lista1.Contains(p.UserID) && 
            !lista2.Contains(p.MealID)).ToList();

            if(others.Count()>0)
            {
                List<Weighted_Sum> weighted_Sums = new List<Weighted_Sum>();
                var particioni = others.GroupBy(q => q.MealID);

                float average_weighted_sum = 0;
                float sum_of_similarities = 0;

                foreach (var item in particioni)
                {
                    //duhet veq per ratings e perbashket
                    if (item.Count() == others.GroupBy(q => q.UserID).Count())
                    {

                        foreach (var item1 in item)
                        {
                            float similarity = scores_users.Where(q => q.UserID == item1.UserID).Select(q => q.PearsonScore).FirstOrDefault();
                            average_weighted_sum += item1.RatingID * similarity;
                            sum_of_similarities += similarity;
                        }

                        weighted_Sums.Add(new Weighted_Sum { MealID = item.Key, Average_Score = Math.Round(average_weighted_sum / sum_of_similarities, 0, MidpointRounding.AwayFromZero)});
                    } 
                }
                //marrim itemsat me rating me te larte
               var get_the_best= weighted_Sums.OrderByDescending(q => q.Average_Score).Select(q=>q.MealID).Take(5);
               var lista_fundit = db.MEALS.Where(q=> get_the_best.Contains(q.ID)).ToList();
               rekomandimet = lista_fundit;
            }
            else
            {
                Random rand = new Random();
                var lista = db.MEALS.Take(20).ToList();
                for (int i = 0; i < 5; i++)
                {
                    int r = rand.Next(lista.Count);
                    MEAL m = lista.Skip(r).First();
                    if (!rekomandimet.Contains(m))
                    {
                        rekomandimet.Add(m);
                    }         
                }
            }

            return View(rekomandimet);
        }
    }

}