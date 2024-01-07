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
    public class TANEmployeeDetails
    {
        [Key]
        public int Id { get; set; }
        public AspNetUser AspNetUsers { get; set; }
        [ForeignKey("AspNetUsers")]       
        public string AspNetUsersId { get; set; }
        [Column(TypeName = "varchar(50)")]
        public string FullName { get; set; }
        [Column(TypeName = "varchar(30)")]
        public string UserLogonName { get; set; }
        [Column(TypeName = "varchar(255)")]
        public string EmailAddress { get; set; }
        [Column(TypeName = "varchar(450)")]
        public string ApplicationId { get; set; }
        public Application Application { get; set; }
        [ForeignKey("Application")]       
        public int DefaultappId { get; set; }
        public bool Active { get; set; }
    }
}
