using Aelbry.BO.Reports;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;

namespace Aelbry.BL.Export
{
    /// <summary>
    /// Exporta el reporte semanal a PDF usando PDFsharp 6 (licencia MIT, multiplataforma en
    /// .NET 8 sin dependencia de GDI+/System.Drawing). Se eligio sobre QuestPDF porque QuestPDF
    /// exige licencia comercial de pago a partir de cierto nivel de ingresos de la empresa.
    /// </summary>
    public static class PdfReportExporter
    {
        private static readonly string[] Headers = { "Codigo", "Actividad", "Proyecto", "Responsable", "Estado", "Avance %", "Fecha fin", "Riesgo" };
        private static readonly double[] ColumnWidths = { 55, 130, 100, 100, 70, 55, 65, 45 };

        private const double MarginLeft = 40;
        private const double MarginTop = 40;
        private const double MarginBottom = 40;
        private const double RowHeight = 18;

        static PdfReportExporter()
        {
            GlobalFontSettings.FontResolver = new SystemFontResolver();
        }

        public static byte[] Export(List<ReportActivityRow> rows, string title)
        {
            var document = new PdfDocument();
            var titleFont = new XFont("Arial", 16, XFontStyleEx.Bold);
            var dateFont = new XFont("Arial", 9, XFontStyleEx.Italic);
            var headerFont = new XFont("Arial", 8, XFontStyleEx.Bold);
            var bodyFont = new XFont("Arial", 8, XFontStyleEx.Regular);

            PdfPage page = null;
            XGraphics gfx = null;
            double y = 0;

            void StartPage(bool withTitle)
            {
                page = document.AddPage();
                gfx = XGraphics.FromPdfPage(page);
                y = MarginTop;

                if (withTitle)
                {
                    gfx.DrawString(title, titleFont, XBrushes.Black, new XPoint(MarginLeft, y));
                    y += 22;
                    gfx.DrawString($"Generado: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC", dateFont, XBrushes.Gray, new XPoint(MarginLeft, y));
                    y += 20;
                }

                DrawRow(gfx, Headers, y, headerFont, XBrushes.White, XBrushes.RoyalBlue);
                y += RowHeight;
            }

            StartPage(withTitle: true);

            foreach (var row in rows)
            {
                if (y + RowHeight > page.Height.Point - MarginBottom)
                {
                    StartPage(withTitle: false);
                }

                var cells = new[]
                {
                    row.Code,
                    row.Name,
                    row.ProjectName,
                    row.ResponsibleName,
                    row.Status.ToString(),
                    $"{row.ProgressPercentage:0.##}%",
                    row.EstimatedEndDate.HasValue ? row.EstimatedEndDate.Value.ToString("yyyy-MM-dd") : string.Empty,
                    row.IsAtRisk ? "Si" : "No",
                };

                DrawRow(gfx, cells, y, bodyFont, XBrushes.Black, null);
                y += RowHeight;
            }

            using var stream = new MemoryStream();
            document.Save(stream);
            return stream.ToArray();
        }

        private static void DrawRow(XGraphics gfx, string[] values, double y, XFont font, XBrush textBrush, XBrush backgroundBrush)
        {
            double x = MarginLeft;

            for (int i = 0; i < values.Length; i++)
            {
                double width = ColumnWidths[i];

                if (backgroundBrush != null)
                {
                    gfx.DrawRectangle(backgroundBrush, x, y - 2, width, RowHeight);
                }

                var rect = new XRect(x + 2, y, width - 4, RowHeight);
                gfx.DrawString(values[i] ?? string.Empty, font, textBrush, rect, XStringFormats.TopLeft);
                x += width;
            }
        }
    }
}
