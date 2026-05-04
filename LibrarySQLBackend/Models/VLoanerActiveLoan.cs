using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LibrarySQLBackend.Models;

[Keyless]
public partial class VLoanerActiveLoan
{
    [Column("loan_id")]
    public int LoanId { get; set; }

    [Column("loan_date", TypeName = "datetime")]
    public DateTime? LoanDate { get; set; }

    [Column("due_date", TypeName = "datetime")]
    public DateTime? DueDate { get; set; }

    [Column("loan_status", TypeName = "enum('overdue','active','returned')")]
    public string? LoanStatus { get; set; }

    [Column("loaner_id")]
    public int LoanerId { get; set; }

    [Column("first_name")]
    [StringLength(45)]
    public string? FirstName { get; set; }

    [Column("last_name")]
    [StringLength(45)]
    public string? LastName { get; set; }

    [Column("email")]
    [StringLength(60)]
    public string? Email { get; set; }

    [Column("tlf")]
    [StringLength(12)]
    public string? Tlf { get; set; }

    [Column("inventory_id")]
    public int InventoryId { get; set; }

    [Column("inventory_status", TypeName = "enum('lost','available','loaned out')")]
    public string? InventoryStatus { get; set; }

    [Column("barcode")]
    [StringLength(45)]
    public string? Barcode { get; set; }

    [Column("item_name")]
    [StringLength(100)]
    public string? ItemName { get; set; }

    [Column("media_type", TypeName = "enum('book','boardgame')")]
    public string? MediaType { get; set; }
}
