namespace DataService.Common
{
    /// <summary>
    /// Helper est&aacute;ndar de la empresa para el mapeo seguro de valores provenientes
    /// de un IDataReader/IDataParameter hacia los objetos BO, evitando excepciones por DBNull.
    /// </summary>
    public class Validate
    {
        private static Validate _instance;

        public static Validate Instance
        {
            get { return _instance ??= new Validate(); }
        }

        private Validate()
        {
        }

        public static int getDefaultIntIfDBNull(object value)
        {
            return (value == null || value == DBNull.Value) ? 0 : Convert.ToInt32(value);
        }

        public static decimal getDefaultDecimalIfDBNull(object value)
        {
            return (value == null || value == DBNull.Value) ? 0m : Convert.ToDecimal(value);
        }

        public static bool getDefaultBoolIfDBNull(object value)
        {
            return (value == null || value == DBNull.Value) ? false : Convert.ToBoolean(value);
        }

        public static string getDefaultStringIfDBNull(object value)
        {
            return (value == null || value == DBNull.Value) ? string.Empty : value.ToString();
        }

        public static DateTime getDefaultDateIfDBNull(object value)
        {
            return (value == null || value == DBNull.Value) ? DateTime.MinValue : Convert.ToDateTime(value);
        }

        public static DateTime? getDefaultNullableDateIfDBNull(object value)
        {
            return (value == null || value == DBNull.Value) ? (DateTime?)null : Convert.ToDateTime(value);
        }

        public static int? getDefaultNullableIntIfDBNull(object value)
        {
            return (value == null || value == DBNull.Value) ? (int?)null : Convert.ToInt32(value);
        }

        public static object getDefaultIfDBNull(object value, TypeCode typeCode)
        {
            if (value != null && value != DBNull.Value)
            {
                return value;
            }

            return typeCode switch
            {
                TypeCode.String => string.Empty,
                TypeCode.Int32 => 0,
                TypeCode.Decimal => 0m,
                TypeCode.Boolean => false,
                TypeCode.DateTime => DateTime.MinValue,
                _ => null,
            };
        }
    }
}
