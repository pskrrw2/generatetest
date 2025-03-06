using Domain.Common.Const;
using Domain.Contracts;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("ContactUs", Schema = SchemaNames.Dbo)]
    public class ContactUs : AuditableEntity
    {
        [Key]
        public int Id { get; set; }
        public required string Message { get; set; }
    }
}