namespace Aelbry.BO
{
    /// <summary>
    /// Actividad "esqueleto" de una plantilla de proyecto (lista plana, sin jerarquia).
    /// Al aplicar la plantilla se clona como una Activity real de nivel raiz.
    /// </summary>
    public class ProjectTemplateActivity
    {
        public int ProjectTemplateActivityId { get; set; }

        public int ProjectTemplateId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public decimal EstimatedHours { get; set; }

        public int Sequence { get; set; }

        public int CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
