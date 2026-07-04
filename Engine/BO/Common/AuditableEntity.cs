namespace Aelbry.BO.Common
{
    /// <summary>
    /// Campos de auditor&iacute;a y borrado l&oacute;gico comunes a toda entidad de negocio.
    /// </summary>
    public abstract class AuditableEntity
    {
        public int CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public int? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public bool IsDeleted { get; set; }
    }
}
