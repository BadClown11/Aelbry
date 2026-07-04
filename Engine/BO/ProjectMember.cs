namespace Aelbry.BO
{
    /// <summary>
    /// Resumen de un usuario asignado como miembro de un proyecto (no es la entidad User completa).
    /// </summary>
    public class ProjectMember
    {
        public int UserId { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string JobTitle { get; set; }

        public string PhotoUrl { get; set; }

        public DateTime AddedDate { get; set; }
    }
}
