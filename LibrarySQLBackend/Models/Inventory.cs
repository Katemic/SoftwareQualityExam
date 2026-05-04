using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LibrarySQLBackend.Models;

[Table("inventory")]
[Index("Barcode", Name = "barcode_UNIQUE", IsUnique = true)]
[Index("ItemId", Name = "item_id_idx")]
public partial class Inventory
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("item_id")]
    public int ItemId { get; set; }

    [Column("status", TypeName = "enum('lost','available','loaned out')")]
    public string? Status { get; set; }

    [Column("barcode")]
    [StringLength(45)]
    public string? Barcode { get; set; }

    [Column("placement")]
    [StringLength(45)]
    public string? Placement { get; set; }

    [ForeignKey("ItemId")]
    [InverseProperty("Inventories")]
    public virtual Item Item { get; set; } = null!;

    [InverseProperty("Inventory")]
    public virtual ICollection<Loan> Loans { get; set; } = new List<Loan>();
}
