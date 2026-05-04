using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LibrarySQLBackend.Models;

[Table("publisher")]
public partial class Publisher
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    [StringLength(70)]
    public string? Name { get; set; }

    [InverseProperty("Publisher")]
    public virtual ICollection<Item> Items { get; set; } = new List<Item>();
}
