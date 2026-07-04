using Microsoft.Extensions.Options;

namespace Aelbry.BL.Documents
{
    /// <summary>
    /// Guarda/lee/borra el binario de los adjuntos en disco, bajo {RootPath}/{ProjectId}/{StoredFileName}.
    /// StoredFileName siempre es un GUID+extension generado por el servidor (nunca el nombre
    /// original del usuario) para evitar colisiones y path traversal. No depende de
    /// IWebHostEnvironment para mantener el Engine libre de referencias a ASP.NET Core Web,
    /// siguiendo la misma convencion que usa MSSqlDatabase con Directory.GetCurrentDirectory().
    /// </summary>
    public class FileStorageService
    {
        private readonly string _rootPath;

        public FileStorageService(IOptions<StorageOptions> options)
        {
            _rootPath = Path.Combine(Directory.GetCurrentDirectory(), options.Value.RootPath);
        }

        public string GenerateStoredFileName(string originalFileName)
        {
            string extension = Path.GetExtension(originalFileName);
            return $"{Guid.NewGuid():N}{extension}";
        }

        public async Task SaveAsync(int projectId, string storedFileName, Stream content)
        {
            string folder = Path.Combine(_rootPath, projectId.ToString());
            Directory.CreateDirectory(folder);

            string fullPath = Path.Combine(folder, storedFileName);
            using var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
            await content.CopyToAsync(fileStream);
        }

        public Stream OpenRead(int projectId, string storedFileName)
        {
            string fullPath = Path.Combine(_rootPath, projectId.ToString(), storedFileName);
            return new FileStream(fullPath, FileMode.Open, FileAccess.Read);
        }
    }
}
