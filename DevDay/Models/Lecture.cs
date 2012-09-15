//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DevDay.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Lecture
    {
        public Lecture()
        {
            this.Users = new HashSet<User>();
        }
    
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int SpeakerID { get; set; }
        public System.DateTime StartingOn { get; set; }
        public System.DateTime EndingOn { get; set; }
        public int SpotID { get; set; }
    
        public virtual Speaker Speaker { get; set; }
        public virtual Spot Spot { get; set; }
        public virtual ICollection<User> Users { get; set; }
    }
}
