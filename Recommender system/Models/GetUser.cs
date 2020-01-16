using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Recommender_system.Models
{
    public class GetUser
    { 
        public int ID { get; set; }
        public string UserID { get; set; }
        public string Emri { get; set; }
        public string Mbiemri { get; set; } 
        public string Adresa { get; set; } 
        public string Email { get; set; } 
    }
}