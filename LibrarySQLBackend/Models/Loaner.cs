using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LibrarySQLBackend.Models;

[Table("loaner")]
public partial class Loaner
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("first_name")]
    [StringLength(100)]
    public string FirstName { get; set; } = null!;

    [Column("last_name")]
    [StringLength(100)]
    public string LastName { get; set; } = null!;

    [Column("cpr")]
    [StringLength(11)]
    public string? Cpr { get; set; }

    [Column("tlf")]
    [StringLength(20)]
    public string? Tlf { get; set; }

    [Column("email")]
    [StringLength(254)]
    public string Email { get; set; } = null!;

    [Column("password")]
    [StringLength(255)]
    public string Password { get; set; } = null!;

    [InverseProperty("Loaner")]
    public virtual ICollection<Loan> Loans { get; set; } = new List<Loan>();

    [InverseProperty("Loaner")]
    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

    [InverseProperty("Loaner")]
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
}
