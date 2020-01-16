using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Recommender_system.Models
{
    public class DataEntryUser
    {
        public decimal Pesha { get; set; }
        public decimal Gjatesia { get; set; }
        public decimal Glukoza { get; set; }
        public int UserID { get; set; }
        public int ID { get; set; }
    }

    public class ListaKnn
    {
      public double distanca { get; set; }
        public int userID { get; set; }
        public bool classification { get; set; }
    }

    
    public class Pearson_Corelation
    {
        public int UserID { get; set; }
        public float PearsonScore { get; set; }
    }

    public class Weighted_Sum
    {
        public int MealID { get; set; }
        public double Average_Score { get; set; }
    }

}