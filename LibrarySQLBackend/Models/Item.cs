using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LibrarySQLBackend.Models;

[Table("item")]
[Index("MediaType", Name = "idx_item_media_type")]
[Index("Name", Name = "idx_item_name")]
[Index("LanguageId", Name = "language_id_idx")]
[Index("PublisherId", Name = "publisher_id_idx")]
public partial class Item
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = null!;

    [Column("release_year")]
    public short? ReleaseYear { get; set; }

    [Column("description", TypeName = "text")]
    public string? Description { get; set; }

    [Column("review_summary", TypeName = "text")]
    public string? ReviewSummary { get; set; }

    [Column("media_type", TypeName = "enum('book','boardgame')")]
    public string MediaType { get; set; } = null!;

    [Column("image")]
    [StringLength(2048)]
    public string? Image { get; set; }

    [Column("language_id")]
    public int LanguageId { get; set; }

    [Column("publisher_id")]
    public int PublisherId { get; set; }

    [Column("average_stars")]
    [Precision(2, 1)]
    public decimal? AverageStars { get; set; }

    [InverseProperty("Item")]
    public virtual Boardgame? Boardgame { get; set; }

    [InverseProperty("Item")]
    public virtual Book? Book { get; set; }

    [InverseProperty("Item")]
    public virtual ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();

    [ForeignKey("LanguageId")]
    [InverseProperty("Items")]
    public virtual Language Language { get; set; } = null!;

    [ForeignKey("PublisherId")]
    [InverseProperty("Items")]
    public virtual Publisher Publisher { get; set; } = null!;

    [InverseProperty("Item")]
    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

    [InverseProperty("Item")]
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    [ForeignKey("ItemId")]
    [InverseProperty("Items")]
    public virtual ICollection<Creator> Creators { get; set; } = new List<Creator>();

    [ForeignKey("ItemId")]
    [InverseProperty("Items")]
    public virtual ICollection<Genre> Genres { get; set; } = new List<Genre>();

    [ForeignKey("ItemId")]
    [InverseProperty("Items")]
    public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();
}
