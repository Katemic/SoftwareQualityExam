using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LibrarySQLBackend.Models;

[Table("loan")]
[Index("LoanerId", "Status", Name = "idx_loan_loaner_status")]
[Index("InventoryId", Name = "inventory_id_idx")]
[Index("LoanerId", Name = "loaner_id_idx")]
public partial class Loan
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("loan_date", TypeName = "datetime")]
    public DateTime LoanDate { get; set; }

    [Column("due_date", TypeName = "datetime")]
    public DateTime DueDate { get; set; }

    [Column("return_date", TypeName = "datetime")]
    public DateTime? ReturnDate { get; set; }

    [Column("status", TypeName = "enum('overdue','active','returned')")]
    public string Status { get; set; } = null!;

    [Column("loaner_id")]
    public int LoanerId { get; set; }

    [Column("inventory_id")]
    public int InventoryId { get; set; }

    [InverseProperty("Loan")]
    public virtual ICollection<Fine> Fines { get; set; } = new List<Fine>();

    [ForeignKey("InventoryId")]
    [InverseProperty("Loans")]
    public virtual Inventory Inventory { get; set; } = null!;

    [ForeignKey("LoanerId")]
    [InverseProperty("Loans")]
    public virtual Loaner Loaner { get; set; } = null!;
}
