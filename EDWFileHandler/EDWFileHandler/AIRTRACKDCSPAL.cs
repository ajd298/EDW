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
    
    public partial class AIRTRACKDCSPAL
    {
        public long AIRTRACK_FTL_PFS_ID { get; set; }
        public string FLTNO { get; set; }
        public string FLTSUF { get; set; }
        public System.DateTime DATOP { get; set; }
        public string STNON { get; set; }
        public string CCTYPME { get; set; }
        public string CCID { get; set; }
        public string DOCNO { get; set; }
        public string PAXNM { get; set; }
        public string SERVCL { get; set; }
        public string RECLOC { get; set; }
        public string STNOFF { get; set; }
        public string BKGCLS { get; set; }
        public string PSGRNT { get; set; }
        public string MKTFLTNO { get; set; }
        public Nullable<System.DateTime> INSERTED_ON { get; set; }
        public Nullable<int> PROCESSED_STATUS { get; set; }
        public int DTEDWDCSID { get; set; }
        public string FBCODE { get; set; }
        public string COUPNO { get; set; }
        public Nullable<int> RESP_CODE { get; set; }
        public string CMPID { get; set; }
        public string CMPCNSPNTYPE { get; set; }
        public string AUTOCHECKIN { get; set; }
        public string TICKETLESSCODE { get; set; }
    
        public virtual DATATRANSFEREDWDCS DATATRANSFEREDWDCS { get; set; }
    }
}
