using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LibrarySQLBackend.Models;

[Keyless]
public partial class VItemFull
{
    [Column("item_id")]
    public int ItemId { get; set; }

    [Column("name")]
    [StringLength(100)]
    public string? Name { get; set; }

    [Column("release_year")]
    public int? ReleaseYear { get; set; }

    [Column("description", TypeName = "mediumtext")]
    public string? Description { get; set; }

    [Column("review_summary", TypeName = "mediumtext")]
    public string? ReviewSummary { get; set; }

    [Column("media_type", TypeName = "enum('book','boardgame')")]
    public string? MediaType { get; set; }

    [Column("image")]
    [StringLength(200)]
    public string? Image { get; set; }

    [Column("average_stars")]
    [Precision(2, 1)]
    public decimal? AverageStars { get; set; }

    [Column("language_name")]
    [StringLength(45)]
    public string? LanguageName { get; set; }

    [Column("publisher_name")]
    [StringLength(70)]
    public string? PublisherName { get; set; }

    [Column("ISBN")]
    [StringLength(25)]
    public string? Isbn { get; set; }

    [Column("no_of_pages")]
    public int? NoOfPages { get; set; }

    [Column("version")]
    [StringLength(45)]
    public string? Version { get; set; }

    [Column("no_of_players")]
    [StringLength(20)]
    public string? NoOfPlayers { get; set; }

    [Column("play_time")]
    [StringLength(20)]
    public string? PlayTime { get; set; }

    [Column("age_group")]
    [StringLength(20)]
    public string? AgeGroup { get; set; }

    [Column("creators", TypeName = "text")]
    public string? Creators { get; set; }

    [Column("genres", TypeName = "text")]
    public string? Genres { get; set; }

    [Column("tags", TypeName = "text")]
    public string? Tags { get; set; }
}
