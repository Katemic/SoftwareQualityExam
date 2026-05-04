using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LibrarySQLBackend.Models;

[Table("creator")]
public partial class Creator
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("first_name")]
    [StringLength(45)]
    public string? FirstName { get; set; }

    [Column("last_name")]
    [StringLength(45)]
    public string? LastName { get; set; }

    [Column("birthday")]
    public DateOnly? Birthday { get; set; }

    [Column("description", TypeName = "mediumtext")]
    public string? Description { get; set; }

    [ForeignKey("CreatorId")]
    [InverseProperty("Creators")]
    public virtual ICollection<Item> Items { get; set; } = new List<Item>();
}
