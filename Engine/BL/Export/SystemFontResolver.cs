using PdfSharp.Fonts;

namespace Aelbry.BL.Export
{
    /// <summary>
    /// PDFsharp 6 (build "Core", multiplataforma) no trae fuentes embebidas por licencia y
    /// exige un IFontResolver explicito. En Windows usa Arial de la carpeta de fuentes del
    /// sistema; en Linux (el contenedor Docker del Modulo 9) usa DejaVu Sans, que debe
    /// instalarse en la imagen via el paquete fonts-dejavu-core (metricas compatibles con
    /// Arial, es la alternativa libre estandar en distros Debian/Ubuntu).
    /// </summary>
    internal class SystemFontResolver : IFontResolver
    {
        public byte[] GetFont(string faceName)
        {
            return File.ReadAllBytes(faceName);
        }

        public FontResolverInfo ResolveTypeface(string familyName, bool bold, bool italic)
        {
            return new FontResolverInfo(FindFontFile(bold, italic));
        }

        private static string FindFontFile(bool bold, bool italic)
        {
            if (OperatingSystem.IsWindows())
            {
                string fileName = (bold, italic) switch
                {
                    (true, true) => "arialbi.ttf",
                    (true, false) => "arialbd.ttf",
                    (false, true) => "ariali.ttf",
                    _ => "arial.ttf",
                };

                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), fileName);
            }

            string linuxFileName = (bold, italic) switch
            {
                (true, true) => "DejaVuSans-BoldOblique.ttf",
                (true, false) => "DejaVuSans-Bold.ttf",
                (false, true) => "DejaVuSans-Oblique.ttf",
                _ => "DejaVuSans.ttf",
            };

            return $"/usr/share/fonts/truetype/dejavu/{linuxFileName}";
        }
    }
}
