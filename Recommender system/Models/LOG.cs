//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Recommender_system.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class LOG
    {
        public int ID { get; set; }
        public System.Guid LogID { get; set; }
        public string EventType { get; set; }
        public string TableName { get; set; }
        public string RecordID { get; set; }
        public string ColumName { get; set; }
        public string NewValue { get; set; }
        public string OldValue { get; set; }
        public int CreatedBy { get; set; }
        public System.DateTime Created_date { get; set; }
    }
}
