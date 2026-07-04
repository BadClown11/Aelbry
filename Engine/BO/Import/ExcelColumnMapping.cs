namespace Aelbry.BO.Import
{
    /// <summary>
    /// Relaciona cada campo de Activity con el nombre de columna detectado en el Excel subido.
    /// Nulo/vacio significa "no mapear este campo".
    /// </summary>
    public class ExcelColumnMapping
    {
        public string NameColumn { get; set; }

        public string DescriptionColumn { get; set; }

        public string EstimatedHoursColumn { get; set; }

        public string CategoryColumn { get; set; }

        public string ResponsibleColumn { get; set; }
    }
}
