//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ImFineDatamodel
{
    using System;
    using System.Collections.Generic;
    
    public partial class DeviceMessages
    {
        public int Id { get; set; }
        public string DeviceAlias { get; set; }
        public System.DateTime Timestamp { get; set; }
        public string Direction { get; set; }
        public string MessageId { get; set; }
        public string Message { get; set; }
        public Nullable<System.DateTime> ExpireTime { get; set; }
        public System.DateTime StatusTime { get; set; }
        public string Status { get; set; }
    }
}
