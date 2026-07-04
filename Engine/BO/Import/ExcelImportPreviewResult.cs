namespace Aelbry.BO.Import
{
    /// <summary>
    /// Resultado de la vista previa: columnas detectadas y unas filas de muestra para que el
    /// usuario decida el mapeo de columnas antes de confirmar la importacion masiva.
    /// </summary>
    public class ExcelImportPreviewResult
    {
        public string Token { get; set; }

        public List<string> Columns { get; set; } = new List<string>();

        public List<Dictionary<string, string>> SampleRows { get; set; } = new List<Dictionary<string, string>>();

        public int TotalRows { get; set; }
    }
}
