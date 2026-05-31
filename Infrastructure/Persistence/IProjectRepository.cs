namespace Araci.Infrastructure.Persistence
{
    public interface IProjectRepository
    {
        string ReadAllText(string path);

        void WriteAllText(string path, string content);
    }
}
