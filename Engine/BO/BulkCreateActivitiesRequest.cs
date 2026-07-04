namespace Aelbry.BO
{
    /// <summary>
    /// Creacion masiva por texto rapido: cada linea no vacia se convierte en una actividad.
    /// </summary>
    public class BulkCreateActivitiesRequest
    {
        public int ProjectId { get; set; }

        public int? ParentActivityId { get; set; }

        public List<string> Lines { get; set; } = new List<string>();
    }
}
