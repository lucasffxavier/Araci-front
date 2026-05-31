namespace Araci.Applications.Abstractions
{
    public interface IProjectPersistenceService
    {
        void Novo();

        void SalvarComDialogo();

        void AbrirComDialogo();

        void Salvar(string path);

        void Abrir(string path);
    }
}
