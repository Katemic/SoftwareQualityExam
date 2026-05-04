using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LibrarySQLBackend.Models;

[Table("genre")]
public partial class Genre
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    [StringLength(45)]
    public string? Name { get; set; }

    [ForeignKey("GenreId")]
    [InverseProperty("Genres")]
    public virtual ICollection<Item> Items { get; set; } = new List<Item>();
}
