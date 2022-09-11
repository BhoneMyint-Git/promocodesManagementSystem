using System;
using System.Collections.Generic;

namespace PromocodesManagementSystem.Model
{
    public partial class TblPromocode
    {
        public string Id { get; set; } = null!;
        public string PromoCodes { get; set; } = null!;
        public byte[] Qrimage { get; set; } = null!;
        public string? Phone { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsUsed { get; set; }
        public bool Active { get; set; }
        public double Amount { get; set; }
    }
}
