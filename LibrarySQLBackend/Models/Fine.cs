using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LibrarySQLBackend.Models;

[Table("fine")]
[Index("LoanId", Name = "fk_fine_loan1_idx")]
public partial class Fine
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("amount")]
    [Precision(10, 2)]
    public decimal? Amount { get; set; }

    [Column("status", TypeName = "enum('unpaid','paid','late')")]
    public string? Status { get; set; }

    [Column("created_date", TypeName = "datetime")]
    public DateTime? CreatedDate { get; set; }

    [Column("paid_date", TypeName = "datetime")]
    public DateTime? PaidDate { get; set; }

    [Column("due_date", TypeName = "datetime")]
    public DateTime? DueDate { get; set; }

    [Column("loan_id")]
    public int LoanId { get; set; }

    [ForeignKey("LoanId")]
    [InverseProperty("Fines")]
    public virtual Loan Loan { get; set; } = null!;
}
