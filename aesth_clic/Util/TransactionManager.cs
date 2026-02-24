using aesth_clic.Data;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aesth_clic.Util
{
    internal class TransactionManager
    {
        private readonly DbConnectionFactory _db;

        public TransactionManager(DbConnectionFactory db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public async Task<T> ExecuteAsync<T>(
            Func<MySqlConnection, MySqlTransaction, Task<T>> operation)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            using var transaction = await conn.BeginTransactionAsync();

            try
            {
                var result = await operation(conn, (MySqlTransaction)transaction);
                await transaction.CommitAsync();
                return result;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                Console.WriteLine($"Transaction failed: {ex.Message}");
                Console.WriteLine(ex.StackTrace);

                throw;
            }
        }

        public async Task ExecuteAsync(
            Func<MySqlConnection, MySqlTransaction, Task> operation)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            using var transaction = await conn.BeginTransactionAsync();

            try
            {
                await operation(conn, (MySqlTransaction)transaction);
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                Console.WriteLine($"Transaction failed: {ex.Message}");
                Console.WriteLine(ex.StackTrace);

                throw;
            }
        }
    }
}
