using Aelbry.BO;
using Aelbry.BO.Import;
using Aelbry.DAL;
using ClosedXML.Excel;
using Microsoft.Extensions.Caching.Memory;

namespace Aelbry.BL.Import
{
    /// <summary>
    /// Importacion masiva desde Excel (Modulo 4): Preview lee el archivo y cachea las filas
    /// bajo un token efimero; Commit aplica el mapeo de columnas elegido por el usuario y crea
    /// una Activity por fila valida, reutilizando ActivityBL.Create.
    /// </summary>
    public class ExcelActivityImportBL
    {
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(15);

        private readonly IMemoryCache _cache;
        private readonly ActivityBL _activityBL;

        public ExcelActivityImportBL(IMemoryCache cache, ActivityBL activityBL)
        {
            _cache = cache;
            _activityBL = activityBL;
        }

        public ExcelImportPreviewResult Preview(Stream fileStream)
        {
            var rows = ReadRows(fileStream);

            if (rows.Count == 0)
            {
                throw new InvalidOperationException("El archivo no contiene filas de datos.");
            }

            string token = Guid.NewGuid().ToString("N");
            _cache.Set(token, rows, CacheDuration);

            return new ExcelImportPreviewResult
            {
                Token = token,
                Columns = rows[0].Keys.ToList(),
                SampleRows = rows.Take(5).ToList(),
                TotalRows = rows.Count,
            };
        }

        public ExcelImportCommitResult Commit(ExcelImportCommitRequest request, int currentUserId)
        {
            if (!_cache.TryGetValue(request.Token, out List<Dictionary<string, string>> rows) || rows == null)
            {
                throw new InvalidOperationException("La vista previa expiro o no existe. Vuelve a subir el archivo.");
            }

            var result = new ExcelImportCommitResult();

            for (int i = 0; i < rows.Count; i++)
            {
                int rowNumber = i + 2; // +1 por encabezado, +1 porque el usuario cuenta desde 1
                var row = rows[i];

                string name = GetValue(row, request.Mapping.NameColumn)?.Trim();
                if (string.IsNullOrWhiteSpace(name))
                {
                    result.Warnings.Add($"Fila {rowNumber}: nombre vacio, se omitio.");
                    continue;
                }

                decimal estimatedHours = 0;
                string hoursRaw = GetValue(row, request.Mapping.EstimatedHoursColumn);
                if (!string.IsNullOrWhiteSpace(hoursRaw) && !decimal.TryParse(hoursRaw, out estimatedHours))
                {
                    result.Warnings.Add($"Fila {rowNumber}: horas estimadas invalidas ('{hoursRaw}'), se uso 0.");
                    estimatedHours = 0;
                }

                int responsibleUserId = ResolveResponsible(GetValue(row, request.Mapping.ResponsibleColumn), currentUserId, result, rowNumber);

                var activity = new Activity
                {
                    ProjectId = request.ProjectId,
                    ParentActivityId = request.ParentActivityId,
                    Name = name,
                    Description = GetValue(row, request.Mapping.DescriptionColumn),
                    Category = GetValue(row, request.Mapping.CategoryColumn),
                    ColorHex = "#4C6EF5",
                    Status = ActivityStatus.Pending,
                    Priority = ProjectPriority.Medium,
                    ResponsibleUserId = responsibleUserId,
                    Weight = 1,
                    EstimatedHours = estimatedHours,
                    CreatedBy = currentUserId,
                };

                _activityBL.Create(activity);
                result.CreatedCount++;
            }

            _cache.Remove(request.Token);
            return result;
        }

        private int ResolveResponsible(string raw, int fallbackUserId, ExcelImportCommitResult result, int rowNumber)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return fallbackUserId;
            }

            if (int.TryParse(raw, out int userId))
            {
                return userId;
            }

            using var userDal = UserDAL.Instance;
            var user = userDal.GetByEmail(raw.Trim());

            if (user == null)
            {
                result.Warnings.Add($"Fila {rowNumber}: no se encontro un usuario con '{raw}', se asigno el responsable actual.");
                return fallbackUserId;
            }

            return user.UserId;
        }

        private static string GetValue(Dictionary<string, string> row, string column)
        {
            if (string.IsNullOrEmpty(column) || !row.TryGetValue(column, out var value))
            {
                return null;
            }

            return value;
        }

        private static List<Dictionary<string, string>> ReadRows(Stream fileStream)
        {
            using var workbook = new XLWorkbook(fileStream);
            var worksheet = workbook.Worksheets.First();

            var headerRow = worksheet.Row(1);
            var headers = headerRow.CellsUsed().Select(c => c.GetString().Trim()).ToList();

            var rows = new List<Dictionary<string, string>>();

            foreach (var dataRow in worksheet.RowsUsed().Skip(1))
            {
                var dict = new Dictionary<string, string>();

                for (int i = 0; i < headers.Count; i++)
                {
                    dict[headers[i]] = dataRow.Cell(i + 1).GetString();
                }

                rows.Add(dict);
            }

            return rows;
        }
    }
}
