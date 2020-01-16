using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Recommender_system.Models
{
    public class MessageJS
    {
        public int ID { get; set; }
        public string Model { get; set; } 
        public bool Status { get; set; }
        public string Message { get; set; } 
        public int? UserID { get; set; } 
        public bool editing { get; set; }
    }
}