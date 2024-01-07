using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;

namespace TAN.DomainModels.Entities
{
    [ExcludeFromCodeCoverage]
    public class Module
    {
        public Module()
        {
            CreatedDate = DateTime.Now;
        }
        [Key]
        public int ModuleId { get; set; }
        [Required(ErrorMessage = "Please Enter Name")]
        [Column(TypeName = "varchar(50)")]
        public string Name { get; set; }
        public int Parent { get; set; }
        public string icon { get; set; }
        [Required(ErrorMessage = "Please Enter Path")]
        [Column(TypeName = "varchar(500)")]
        public string Path { get; set; }
        [Required(ErrorMessage = "Please select display preference")]
        [Column(TypeName = "varchar(1)")]
        public string IsDisplay { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public bool IsActive { get; set; }
        public Application Application { get; set; }
		[ForeignKey("Application")]
		public int ApplicationId { get; set; }
	}
}
