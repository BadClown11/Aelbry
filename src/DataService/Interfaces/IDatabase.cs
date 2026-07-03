using System.Data;

namespace DataService.Interfaces
{
    /// <summary>
    /// Contrato que todo proveedor de base de datos del est&aacute;ndar interno debe implementar
    /// (ej. MSSqlDatabase, MariaDatabase, OracleDatabase).
    /// </summary>
    public interface IDatabase
    {
        IDbConnection CreateConnection();

        IDbConnection CreateOpenConnection();

        IDbCommand CreateCommand();

        IDbCommand CreateCommand(string commandText, IDbConnection connection);

        IDbCommand CreateCommand(string commandText, IDbConnection connection, IDbTransaction transaction);

        IDbCommand CreateStoredProcCommand(string procName, IDbConnection connection, IDbTransaction transaction);

        IDataParameter CreateParameter(string parameterName, object parameterValue);

        IDataParameter CreateParameterOut(string parameterName);

        IDataParameter CreateParameterOut(string parameterName, DbType dbType, int iSize);

        IDataParameter CreateParameterInOut(string parameterName, object parameterValue, DbType dbType, int iSize);

        DataTable GetDataTable(IDbCommand cmd);

        void SetConnectionString(string connStringName);

        void SetConnectionStringValue(string connStringValue);
    }
}
