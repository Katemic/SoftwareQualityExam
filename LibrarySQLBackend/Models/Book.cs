using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LibrarySQLBackend.Models;

[Table("book")]
[Index("Isbn", Name = "ISBN_UNIQUE", IsUnique = true)]
[Index("ItemId", Name = "item_id_idx", IsUnique = true)]
public partial class Book
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("ISBN")]
    [StringLength(25)]
    public string? Isbn { get; set; }

    [Column("no_of_pages")]
    public int? NoOfPages { get; set; }

    [Column("version")]
    [StringLength(45)]
    public string? Version { get; set; }

    [Column("item_id")]
    public int ItemId { get; set; }

    [ForeignKey("ItemId")]
    [InverseProperty("Book")]
    public virtual Item Item { get; set; } = null!;
}
