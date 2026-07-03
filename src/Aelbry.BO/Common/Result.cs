namespace Aelbry.BO.Common
{
    /// <summary>
    /// Envoltorio est&aacute;ndar de respuesta que todo Controller regresa como JsonResult.
    /// result = C.OK en &eacute;xito, o el mensaje de error (ex.Message) en caso de falla.
    /// </summary>
    public class Result
    {
        public string result { get; set; }

        public object data { get; set; }
    }
}
