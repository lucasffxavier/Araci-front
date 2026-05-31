using System.IO;

namespace Araci.Infrastructure.Persistence
{
    public sealed class FileSystemProjectRepository : IProjectRepository
    {
        public string ReadAllText(string path)
        {
            return File.ReadAllText(path);
        }

        public void WriteAllText(string path, string content)
        {
            File.WriteAllText(path, content);
        }
    }
}
