//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace EDWFileHandler
{
    using System;
    using System.Collections.Generic;
    
    public partial class AirportsIATA
    {
        public int AirportId { get; set; }
        public string IATACode { get; set; }
        public string Name { get; set; }
        public System.Data.Entity.Spatial.DbGeography Geography { get; set; }
        public string CityCode { get; set; }
        public string ContinentCode { get; set; }
        public string CountryCode { get; set; }
    }
}