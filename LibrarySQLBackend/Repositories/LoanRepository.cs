using LibrarySQLBackend.Context;
using LibrarySQLBackend.Models;
using LibrarySQLBackend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibrarySQLBackend.Repositories
{
    public class LoanRepository : ILoanRepository
    {
        private readonly AppDbContext _context;

        public LoanRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Loan?> GetByIdAsync(int id)
        {
            return await _context.Loans
                .Include(l => l.Inventory)
                .Include(l => l.Loaner)
                .FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task<int> CreateLoanAsync(int loanerId, int inventoryId)
        {
            var connection = _context.Database.GetDbConnection();
            var shouldClose = connection.State != ConnectionState.Open;

            if (shouldClose)
                await connection.OpenAsync();

            try
            {
                await using var command = connection.CreateCommand();

                command.CommandText = "sp_create_loan";
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(CreateParameter(command, "p_loaner_id", loanerId));
                command.Parameters.Add(CreateParameter(command, "p_inventory_id", inventoryId));

                var outputParameter = CreateParameter(command, "p_new_loan_id", DBNull.Value);
                outputParameter.Direction = ParameterDirection.Output;
                command.Parameters.Add(outputParameter);

                await command.ExecuteNonQueryAsync();

                return Convert.ToInt32(outputParameter.Value);
            }
            finally
            {
                if (shouldClose)
                    await connection.CloseAsync();
            }
        }

        public async Task ReturnLoanAsync(int loanId)
        {
            var connection = _context.Database.GetDbConnection();
            var shouldClose = connection.State != ConnectionState.Open;

            if (shouldClose)
                await connection.OpenAsync();

            try
            {
                await using var command = connection.CreateCommand();

                command.CommandText = "sp_return_loan";
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(CreateParameter(command, "p_loan_id", loanId));

                await command.ExecuteNonQueryAsync();
            }
            finally
            {
                if (shouldClose)
                    await connection.CloseAsync();
            }
        }

        private static DbParameter CreateParameter(DbCommand command, string name, object? value)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value ?? DBNull.Value;
            return parameter;
        }
    }
}
