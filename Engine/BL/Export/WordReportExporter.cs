using Aelbry.BO.Reports;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Aelbry.BL.Export
{
    /// <summary>
    /// Exporta el reporte semanal a un documento Word (.docx) usando DocumentFormat.OpenXml,
    /// el SDK oficial de Microsoft para OOXML (licencia MIT, sin costo).
    /// </summary>
    public static class WordReportExporter
    {
        private static readonly string[] Headers = { "Codigo", "Actividad", "Proyecto", "Responsable", "Estado", "Avance %", "Fecha fin", "En riesgo" };

        public static byte[] Export(List<ReportActivityRow> rows, string title)
        {
            using var stream = new MemoryStream();

            using (var doc = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document))
            {
                var mainPart = doc.AddMainDocumentPart();
                mainPart.Document = new Document();
                var body = mainPart.Document.AppendChild(new Body());

                var titleRun = new Run(new RunProperties(new Bold(), new FontSize { Val = "32" }), new Text(title));
                body.AppendChild(new Paragraph(titleRun));

                var dateRun = new Run(new RunProperties(new Italic(), new FontSize { Val = "18" }), new Text($"Generado: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC"));
                body.AppendChild(new Paragraph(dateRun));
                body.AppendChild(new Paragraph(new Run(new Text(string.Empty))));

                var table = new Table();
                table.AppendChild(new TableProperties(
                    new TableBorders(
                        new TopBorder { Val = BorderValues.Single, Size = 4 },
                        new BottomBorder { Val = BorderValues.Single, Size = 4 },
                        new LeftBorder { Val = BorderValues.Single, Size = 4 },
                        new RightBorder { Val = BorderValues.Single, Size = 4 },
                        new InsideHorizontalBorder { Val = BorderValues.Single, Size = 4 },
                        new InsideVerticalBorder { Val = BorderValues.Single, Size = 4 }),
                    new TableWidth { Type = TableWidthUnitValues.Pct, Width = "5000" }));

                table.AppendChild(BuildRow(Headers, bold: true));

                foreach (var row in rows)
                {
                    table.AppendChild(BuildRow(new[]
                    {
                        row.Code,
                        row.Name,
                        row.ProjectName,
                        row.ResponsibleName,
                        row.Status.ToString(),
                        $"{row.ProgressPercentage:0.##}%",
                        row.EstimatedEndDate.HasValue ? row.EstimatedEndDate.Value.ToString("yyyy-MM-dd") : string.Empty,
                        row.IsAtRisk ? "Si" : "No",
                    }, bold: false));
                }

                body.AppendChild(table);
                mainPart.Document.Save();
            }

            return stream.ToArray();
        }

        private static TableRow BuildRow(string[] values, bool bold)
        {
            var tableRow = new TableRow();

            foreach (var value in values)
            {
                var runProperties = bold ? new RunProperties(new Bold()) : new RunProperties();
                var cell = new TableCell(new Paragraph(new Run(runProperties, new Text(value ?? string.Empty))));
                cell.AppendChild(new TableCellProperties(new TableCellWidth { Type = TableWidthUnitValues.Auto }));
                tableRow.AppendChild(cell);
            }

            return tableRow;
        }
    }
}
