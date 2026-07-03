using DataService.Interfaces;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace DataService.MSSQL
{
    public class MSSqlDatabase : Database, IDisposable
    {
        public MSSqlDatabase(string connStringName)
        {
            this.SetConnectionString(connStringName);
            this.CreateOpenConnection();
        }
        public MSSqlDatabase(string connStringValue, bool isValue)
        {
            this.SetConnectionStringValue(connStringValue);
            this.CreateOpenConnection();
        }

        public override IDbCommand CreateCommand()
        {
            return new SqlCommand();
        }

        public override IDbCommand CreateCommand(string commandText, IDbConnection connection)
        {
            SqlCommand command = (SqlCommand)CreateCommand();

            command.CommandText = commandText;
            command.Connection = (SqlConnection)connection;
            command.CommandType = CommandType.Text;

            return command;
        }

        public override IDbCommand CreateCommand(string commandText, IDbConnection connection, IDbTransaction transaction)
        {
            SqlCommand command = (SqlCommand)CreateCommand();

            command.CommandText = commandText;
            command.Connection = (SqlConnection)connection;
            command.CommandType = CommandType.Text;
            command.Transaction = (SqlTransaction)transaction;

            return command;
        }

        public override IDbConnection CreateConnection()
        {
            return new SqlConnection(base.connectionString);
        }

        public override IDbConnection CreateOpenConnection()
        {
            base.conn = (SqlConnection)CreateConnection();
            base.conn.Open();

            return base.conn;
        }

        public override IDataParameter CreateParameter(string parameterName, object parameterValue)
        {
            return new SqlParameter(parameterName, parameterValue);
        }

        public override IDataParameter CreateParameterInOut(string parameterName, object parameterValue, DbType dbType, int iSize)
        {
            SqlParameter p = new SqlParameter(parameterName, parameterValue);
            p.Direction = ParameterDirection.InputOutput;
            p.DbType = dbType;
            p.Size = iSize;
            return p;
        }

        public override IDataParameter CreateParameterOut(string parameterName)
        {
            SqlParameter p = new SqlParameter(parameterName, string.Empty);
            p.Direction = ParameterDirection.Output;
            return p;
        }

        public override IDataParameter CreateParameterOut(string parameterName, DbType dbType, int iSize)
        {
            SqlParameter p = new SqlParameter(parameterName, string.Empty);
            p.Direction = ParameterDirection.Output;
            p.DbType = dbType;
            p.Size = iSize;
            return p;
        }

        public override IDbCommand CreateStoredProcCommand(string procName, IDbConnection connection, IDbTransaction transaction)
        {
            SqlCommand command = (SqlCommand)CreateCommand();

            command.CommandText = procName;
            command.Connection = (SqlConnection)connection;
            command.CommandType = CommandType.StoredProcedure;
            if (transaction != null)
            {
                command.Transaction = (SqlTransaction)transaction;
            }
            return command;
        }

        public void Dispose()
        {
            if (txn != null) { txn.Dispose(); }

            if (conn != null) { conn.Close(); conn.Dispose(); }


        }

        public override DataTable GetDataTable(IDbCommand cmd)
        {
            throw new NotImplementedException();
        }

        public override void SetConnectionString(string connStringName)
        {
            if (string.IsNullOrEmpty(connStringName)) throw new Exception("connection string name is required");

            try
            {

                var config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build();

                base.connectionString = config.GetConnectionString(connStringName);
            }
            catch (Exception exe)
            {
                throw new Exception("Error al leer el ConnectionString [" + connStringName + "], contacte al administrador del sistema. Detalles:" + exe.Message);
            }
        }

        public override void SetConnectionStringValue(string connStringValue)
        {
            base.connectionString = connStringValue;
        }
    }
}
