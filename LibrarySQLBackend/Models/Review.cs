using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LibrarySQLBackend.Models;

[PrimaryKey("LoanerId", "ItemId")]
[Table("review")]
[Index("ItemId", Name = "item_id_idx")]
public partial class Review
{
    [Key]
    [Column("loaner_id")]
    public int LoanerId { get; set; }

    [Key]
    [Column("item_id")]
    public int ItemId { get; set; }

    [Column("no_of_stars")]
    [Precision(2, 1)]
    public decimal? NoOfStars { get; set; }

    [Column("text", TypeName = "mediumtext")]
    public string? Text { get; set; }

    [ForeignKey("ItemId")]
    [InverseProperty("Reviews")]
    public virtual Item Item { get; set; } = null!;

    [ForeignKey("LoanerId")]
    [InverseProperty("Reviews")]
    public virtual Loaner Loaner { get; set; } = null!;
}
