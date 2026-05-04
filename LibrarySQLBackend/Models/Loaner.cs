using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LibrarySQLBackend.Models;

[Table("loaner")]
[Index("Cpr", Name = "cpr_UNIQUE", IsUnique = true)]
[Index("Email", Name = "email_UNIQUE", IsUnique = true)]
public partial class Loaner
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

    [Column("cpr")]
    [StringLength(20)]
    public string? Cpr { get; set; }

    [Column("tlf")]
    [StringLength(12)]
    public string? Tlf { get; set; }

    [Column("email")]
    [StringLength(60)]
    public string? Email { get; set; }

    [Column("password")]
    [StringLength(255)]
    public string? Password { get; set; }

    [InverseProperty("Loaner")]
    public virtual ICollection<Loan> Loans { get; set; } = new List<Loan>();

    [InverseProperty("Loaner")]
    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

    [InverseProperty("Loaner")]
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
}
