using LibrarySQLBackend.Context;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace LibraryTestUtilities
{
    public class TestDatabaseHelper
    {
        private readonly string _connectionString;
        private static readonly SemaphoreSlim DatabaseLock = new(1, 1);

        public TestDatabaseHelper(string connectionString)
        {
            _connectionString = connectionString;
        }

        public AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseMySql(_connectionString, ServerVersion.AutoDetect(_connectionString))
                .Options;

            return new AppDbContext(options);
        }

        public async Task ResetAndSeedDatabaseAsync()
        {
            await DatabaseLock.WaitAsync();

            try
            {
                await using var context = CreateContext();

                await ResetDatabaseAsync(context);
                await SeedDatabaseAsync(context);
            }
            finally
            {
                DatabaseLock.Release();
            }
        }

        private static async Task ResetDatabaseAsync(AppDbContext context)
        {
            var connection = context.Database.GetDbConnection();

            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync();

            await using var command = connection.CreateCommand();

            command.CommandText = @"
                SET FOREIGN_KEY_CHECKS = 0;

                TRUNCATE TABLE `item_tag`;
                TRUNCATE TABLE `item_genre`;
                TRUNCATE TABLE `item_creator`;

                TRUNCATE TABLE `fine`;
                TRUNCATE TABLE `review`;
                TRUNCATE TABLE `reservation`;
                TRUNCATE TABLE `loan`;
                TRUNCATE TABLE `inventory`;

                TRUNCATE TABLE `boardgame`;
                TRUNCATE TABLE `book`;
                TRUNCATE TABLE `item`;

                TRUNCATE TABLE `loaner`;
                TRUNCATE TABLE `creator`;
                TRUNCATE TABLE `tag`;
                TRUNCATE TABLE `genre`;
                TRUNCATE TABLE `publisher`;
                TRUNCATE TABLE `language`;

                SET FOREIGN_KEY_CHECKS = 1;
            ";

            await command.ExecuteNonQueryAsync();
            await connection.CloseAsync();
        }

        private static async Task SeedDatabaseAsync(AppDbContext context)
        {
            var seedFilePath = Path.Combine(
                AppContext.BaseDirectory,
                "TestData",
                "test-seed.sql");

            if (!File.Exists(seedFilePath))
                throw new FileNotFoundException("Seed file was not found.", seedFilePath);

            var seedSql = await File.ReadAllTextAsync(seedFilePath);

            var statements = SplitSqlStatements(seedSql);

            var connection = context.Database.GetDbConnection();

            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync();

            await using var command = connection.CreateCommand();

            foreach (var statement in statements)
            {
                command.CommandText = statement;
                await command.ExecuteNonQueryAsync();
            }

            await connection.CloseAsync();
        }

        private static IEnumerable<string> SplitSqlStatements(string sql)
        {
            var lines = new List<string>();

            using var reader = new StringReader(sql);

            while (reader.ReadLine() is { } line)
            {
                var trimmedLine = line.TrimStart();

                if (trimmedLine.StartsWith("--"))
                    continue;

                if (string.IsNullOrWhiteSpace(trimmedLine))
                    continue;

                lines.Add(line);
            }

            var sqlWithoutComments = string.Join(Environment.NewLine, lines);

            return sqlWithoutComments
                .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(statement => !string.IsNullOrWhiteSpace(statement));
        }
    }
}