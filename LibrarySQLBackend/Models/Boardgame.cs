using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LibrarySQLBackend.Models;

[Table("boardgame")]
[Index("ItemId", Name = "item_id_idx", IsUnique = true)]
public partial class Boardgame
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("no_of_players")]
    [StringLength(20)]
    public string? NoOfPlayers { get; set; }

    [Column("play_time")]
    [StringLength(20)]
    public string? PlayTime { get; set; }

    [Column("age_group")]
    [StringLength(20)]
    public string? AgeGroup { get; set; }

    [Column("item_id")]
    public int ItemId { get; set; }

    [ForeignKey("ItemId")]
    [InverseProperty("Boardgame")]
    public virtual Item Item { get; set; } = null!;
}
