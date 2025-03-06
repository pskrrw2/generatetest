using Domain.Common.Const;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("AddOnMaster", Schema = SchemaNames.Dbo)]
public class AddOnMaster
{
    [Key]
    public int Id { get; set; }
    public string? Menu { get; set; }
    public string? CategoryName { get; set; }
    public string? FoodType { get; set; }
    public string? FoodItem { get; set; }
    public string? Price { get; set; }
    public int Quantity { get; set; }
}

