using Aelbry.BO.Reports;
using ClosedXML.Excel;

namespace Aelbry.BL.Export
{
    /// <summary>
    /// Exporta el reporte semanal de actividades a un libro de Excel usando ClosedXML
    /// (ya es dependencia del proyecto para la importacion masiva del Modulo 4).
    /// Todas las celdas se escriben como texto formateado para evitar ambiguedades con
    /// tipos nullable al asignar IXLCell.Value.
    /// </summary>
    public static class ExcelReportExporter
    {
        private static readonly string[] Headers =
        {
            "Codigo", "Actividad", "Proyecto", "Responsable", "Estado", "Prioridad",
            "Avance %", "Fecha estimada fin", "Dias restantes", "En riesgo",
        };

        public static byte[] Export(List<ReportActivityRow> rows, string title)
        {
            using var workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add("Reporte");

            sheet.Cell(1, 1).Value = title;
            sheet.Cell(1, 1).Style.Font.Bold = true;
            sheet.Cell(1, 1).Style.Font.FontSize = 14;
            sheet.Cell(2, 1).Value = $"Generado: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC";

            for (int i = 0; i < Headers.Length; i++)
            {
                var cell = sheet.Cell(4, i + 1);
                cell.Value = Headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#4C6EF5");
                cell.Style.Font.FontColor = XLColor.White;
            }

            int r = 5;
            foreach (var row in rows)
            {
                sheet.Cell(r, 1).Value = row.Code;
                sheet.Cell(r, 2).Value = row.Name;
                sheet.Cell(r, 3).Value = row.ProjectName;
                sheet.Cell(r, 4).Value = row.ResponsibleName;
                sheet.Cell(r, 5).Value = row.Status.ToString();
                sheet.Cell(r, 6).Value = row.Priority.ToString();
                sheet.Cell(r, 7).Value = $"{row.ProgressPercentage:0.##}%";
                sheet.Cell(r, 8).Value = row.EstimatedEndDate.HasValue ? row.EstimatedEndDate.Value.ToString("yyyy-MM-dd") : string.Empty;
                sheet.Cell(r, 9).Value = row.DaysRemaining.HasValue ? row.DaysRemaining.Value.ToString() : string.Empty;
                sheet.Cell(r, 10).Value = row.IsAtRisk ? "Si" : "No";
                r++;
            }

            sheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}
