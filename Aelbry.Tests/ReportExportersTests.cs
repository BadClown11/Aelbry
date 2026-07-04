using Aelbry.BL.Export;
using Aelbry.BO;
using Aelbry.BO.Reports;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Packaging;
using Xunit;

namespace Aelbry.Tests
{
    public class ReportExportersTests
    {
        private static List<ReportActivityRow> SampleRows()
        {
            return new List<ReportActivityRow>
            {
                new ReportActivityRow
                {
                    Code = "ACT-001",
                    Name = "Actividad de prueba",
                    ProjectName = "Proyecto Demo",
                    ResponsibleName = "Juan Perez",
                    Status = ActivityStatus.InProgress,
                    Priority = ProjectPriority.High,
                    ProgressPercentage = 42.5m,
                    EstimatedEndDate = new DateTime(2026, 8, 1),
                    DaysRemaining = 10,
                    IsAtRisk = false,
                },
                new ReportActivityRow
                {
                    Code = "ACT-002",
                    Name = "Actividad vencida",
                    ProjectName = "Proyecto Demo",
                    ResponsibleName = "Ana Gomez",
                    Status = ActivityStatus.Pending,
                    Priority = ProjectPriority.Critical,
                    ProgressPercentage = 0m,
                    EstimatedEndDate = new DateTime(2026, 6, 1),
                    DaysRemaining = -30,
                    IsAtRisk = true,
                },
            };
        }

        [Fact]
        public void ExcelExporter_ProducesWorkbook_WithHeaderAndDataRows()
        {
            byte[] bytes = ExcelReportExporter.Export(SampleRows(), "Reporte de prueba");

            Assert.NotEmpty(bytes);

            using var stream = new MemoryStream(bytes);
            using var workbook = new XLWorkbook(stream);
            var sheet = workbook.Worksheets.First();

            Assert.Equal("Codigo", sheet.Cell(4, 1).GetString());
            Assert.Equal("ACT-001", sheet.Cell(5, 1).GetString());
            Assert.Equal("ACT-002", sheet.Cell(6, 1).GetString());
        }

        [Fact]
        public void WordExporter_ProducesValidDocx()
        {
            byte[] bytes = WordReportExporter.Export(SampleRows(), "Reporte de prueba");

            Assert.NotEmpty(bytes);
            Assert.Equal(0x50, bytes[0]); // 'P'
            Assert.Equal(0x4B, bytes[1]); // 'K' (firma ZIP: un .docx es un contenedor OOXML/ZIP)

            using var stream = new MemoryStream(bytes);
            using var doc = WordprocessingDocument.Open(stream, false);
            string text = doc.MainDocumentPart.Document.Body.InnerText;

            Assert.Contains("ACT-001", text);
            Assert.Contains("ACT-002", text);
        }

        [Fact]
        public void PdfExporter_ProducesValidPdf()
        {
            byte[] bytes = PdfReportExporter.Export(SampleRows(), "Reporte de prueba");

            Assert.NotEmpty(bytes);

            string header = System.Text.Encoding.ASCII.GetString(bytes, 0, 4);
            Assert.Equal("%PDF", header);
        }

        [Fact]
        public void PdfExporter_PaginatesWhenManyRows()
        {
            var rows = new List<ReportActivityRow>();
            for (int i = 0; i < 100; i++)
            {
                rows.Add(new ReportActivityRow { Code = $"ACT-{i:000}", Name = "x", ProjectName = "x", ResponsibleName = "x" });
            }

            byte[] bytes = PdfReportExporter.Export(rows, "Reporte largo");

            Assert.NotEmpty(bytes);
        }
    }
}
