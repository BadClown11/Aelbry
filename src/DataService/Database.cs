using System.Data;

namespace DataService
{
    /// <summary>
    /// Clase base abstracta del est&aacute;ndar interno de acceso a datos.
    /// Cada motor concreto (MSSqlDatabase, MariaDatabase, OracleDatabase) hereda de esta
    /// clase e implementa el detalle espec&iacute;fico del proveedor ADO.NET.
    /// </summary>
    public abstract class Database : IDatabase
    {
        protected string connectionString;
        protected IDbConnection conn;
        protected IDbTransaction txn;

        public abstract IDbConnection CreateConnection();

        public abstract IDbConnection CreateOpenConnection();

        public abstract IDbCommand CreateCommand();

        public abstract IDbCommand CreateCommand(string commandText, IDbConnection connection);

        public abstract IDbCommand CreateCommand(string commandText, IDbConnection connection, IDbTransaction transaction);

        public abstract IDbCommand CreateStoredProcCommand(string procName, IDbConnection connection, IDbTransaction transaction);

        public abstract IDataParameter CreateParameter(string parameterName, object parameterValue);

        public abstract IDataParameter CreateParameterOut(string parameterName);

        public abstract IDataParameter CreateParameterOut(string parameterName, DbType dbType, int iSize);

        public abstract IDataParameter CreateParameterInOut(string parameterName, object parameterValue, DbType dbType, int iSize);

        public abstract DataTable GetDataTable(IDbCommand cmd);

        public abstract void SetConnectionString(string connStringName);

        public abstract void SetConnectionStringValue(string connStringValue);

        public virtual IDbTransaction BeginTransaction()
        {
            txn = conn.BeginTransaction();
            return txn;
        }

        public virtual void CommitTransaction()
        {
            if (txn != null)
            {
                txn.Commit();
                txn.Dispose();
                txn = null;
            }
        }

        public virtual void RollbackTransaction()
        {
            if (txn != null)
            {
                txn.Rollback();
                txn.Dispose();
                txn = null;
            }
        }
    }
}
