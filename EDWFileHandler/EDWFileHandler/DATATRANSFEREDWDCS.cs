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
    
    public partial class DATATRANSFEREDWDCS
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public DATATRANSFEREDWDCS()
        {
            this.AIRTRACKDCSPAL = new HashSet<AIRTRACKDCSPAL>();
        }
    
        public string DATASUP { get; set; }
        public string SENDSYS { get; set; }
        public int DELIVNO { get; set; }
        public string TYPDELIV { get; set; }
        public Nullable<System.DateTime> DELIVDAT { get; set; }
        public Nullable<System.DateTime> DATE_OF_INSERT { get; set; }
        public int STATUS { get; set; }
        public int LOCAL_DELIVNO { get; set; }
        public Nullable<int> TOTAL_REC { get; set; }
        public Nullable<System.DateTime> LST_UPDTD { get; set; }
        public Nullable<int> NOOF_REC_RJCTD { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<AIRTRACKDCSPAL> AIRTRACKDCSPAL { get; set; }
    }
}
