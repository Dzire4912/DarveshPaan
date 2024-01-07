using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAN.DomainModels.Entities
{
    public class InvoiceData
    {
        public InvoiceData()
        {
            IsActive = true;
            IsPaid = false;
        }
        public int Id { get; set; }
        public int UploadFileId { get; set; }
        public string? AccountNumber { get; set; }
        public string? InvoiceNumber { get; set; }
        public DateTime BillingDate { get; set; }
        public decimal TotalAmountDue { get; set; }
        public DateTime InvoiceDueDate { get; set; }
        public bool IsPaid { get; set; }
        public DateTime CreateDate { get; set; } = DateTime.Now;
        public string? CreateBy { get; set; }
        public DateTime UpdateDate { get; set; }
        public string? UpdateBy { get; set; }
        public bool IsActive { get; set; }
    }
}
