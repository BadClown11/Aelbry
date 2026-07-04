namespace DataService.Common
{
    /// <summary>
    /// Excepci&oacute;n lanzada por el DAL cuando el par&aacute;metro de salida OUT_RESULT
    /// de un Stored Procedure regresa un valor distinto de C.OK.
    /// </summary>
    public class DataBaseException : Exception
    {
        public DataBaseException(string result)
            : base(result)
        {
        }

        public DataBaseException(string result, Exception innerException)
            : base(result, innerException)
        {
        }
    }
}
