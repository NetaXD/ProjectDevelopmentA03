using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Net;

namespace DAL {
    public class DBManager : IDisposable {

        const Int32 DEFAULT_TIMEOUT = 120;
        private SqlConnection sqlConnection = null;
        private SqlCommand sqlCommand = null;
        private SqlTransaction sqlTransaction = null;
        private readonly string connectionString;
        private bool disposeFlag = false;
        private readonly int commandTimeout;

        public DBManager() : this("Main", DEFAULT_TIMEOUT) { }
        public DBManager(string connectionString, Int32 commandTimeout) {
            this.connectionString = connectionString;
            this.commandTimeout = commandTimeout;
            GetConnection();
            GetCommand();
        }
        public DBManager(string connectionString) : this(connectionString, DEFAULT_TIMEOUT) { }
        private void GetConnection() {
            sqlConnection = new SqlConnection(connectionString);
            sqlConnection.Open();
        }
        private void GetCommand() {
            sqlCommand = new SqlCommand();
        }
        private void SetCommand(string setCommandText, CommandType setCommandType) {
            sqlCommand.CommandText = setCommandText;
            sqlCommand.Connection = sqlConnection;
            sqlCommand.CommandType = setCommandType;
            sqlCommand.Parameters.Clear();
        }
        private void SetTransaction() {
            sqlTransaction = sqlConnection.BeginTransaction(IsolationLevel.ReadCommitted);
            sqlCommand.Transaction = sqlTransaction;
        }
        private void ComitTrans() {
            sqlTransaction.Commit();
        }
        private void RollbackTransaction() {
            sqlTransaction.Rollback();
        }
        private bool SetTransactionCheck() {
            if (sqlCommand.Transaction == null) {
                SetTransaction();
                return true;
            }
            else {
                return false;
            }
        }
        public DataSet GetSelectQuery(List<SqlParameter> paraList, string procedureName) {
            SqlDataAdapter sdAdapter = null;
            DataSet dbSet = null;
            try {
                SetCommand(procedureName, CommandType.StoredProcedure);
                if (paraList != null) {
                    sqlCommand.Parameters.AddRange(paraList.ToArray());
                }
                sdAdapter = new SqlDataAdapter(sqlCommand);
                dbSet = new DataSet();
                sdAdapter.Fill(dbSet);
                return dbSet;
            }
            catch (Exception e) {
                if (sdAdapter != null) {
                    sdAdapter.Dispose();
                }
                if (dbSet != null) {
                    dbSet.Dispose();
                }
                throw e;
            }

        }
        public void Dispose() {
            if (sqlCommand != null) {
                sqlCommand.Dispose();
                sqlCommand = null;
            }
            if (sqlConnection != null) {
                sqlConnection.Dispose();
                sqlConnection = null;
            }
            if (sqlTransaction != null) {
                sqlCommand.Dispose();
                sqlCommand = null;
            }
        }
        ~DBManager() {
            if (disposeFlag == false) {
                this.Dispose();
            }
        }
    }
}
