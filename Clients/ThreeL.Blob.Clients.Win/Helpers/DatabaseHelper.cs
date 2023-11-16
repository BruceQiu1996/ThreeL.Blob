using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ThreeL.Blob.Clients.Win.Helpers
{
    public class DatabaseHelper
    {
        private object _lock = new object();
        private readonly string _connectionString;
        private readonly SqliteConnection _queryConnection;
        public DatabaseHelper(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("Sqlite")!;
            _queryConnection = new SqliteConnection(_connectionString);
            _queryConnection.Open();
        }

        ~DatabaseHelper() 
        {
            _queryConnection.Close();
            _queryConnection.Dispose();
        }

        public async Task<IEnumerable<T>> QueryListAsync<T>(string sql, object parameters)
        {
            return await _queryConnection.QueryAsync<T>(sql,parameters);
        }

        public async Task<T> QueryFirstOrDefaultAsync<T>(string sql, object parameters)
        {
            try
            {
                return await _queryConnection.QueryFirstOrDefaultAsync<T>(sql, parameters);
            }
            catch (Exception ex) 
            {
                return default;
            }
        }

        public void Excute(string sql, object parameters)
        {
            lock (_lock)
            {
                using (var conn = new SqliteConnection(_connectionString))
                {
                    conn.Open();
                    conn.Execute(sql, param: parameters);
                }
            }
        }

        public void ExcuteMulti(IEnumerable<(string sql, object parameters)> sqls)
        {
            lock (_lock)
            {
                using (var conn = new SqliteConnection(_connectionString))
                {
                    conn.Open();
                    using (var transaction = conn.BeginTransaction())
                    {

                        foreach (var sql in sqls)
                        {
                            conn.Execute(sql.sql, param: sql.parameters, transaction: transaction);
                        }

                        transaction.Commit();
                    }
                }
            }
        }
    }
}
