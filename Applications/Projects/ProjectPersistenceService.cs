using System;
using System.IO;
using System.Text.Json;
using Araci.Applications.Abstractions;
using Araci.Core.Documents;
using Araci.Infrastructure.Persistence;
using Araci.Models;
using Araci.Services.Settings;

namespace Araci.Applications.Projects
{
    public class ProjectPersistenceService : IProjectPersistenceService
    {
        private readonly AraciDocument _document;
        private readonly ICommandHistory _commands;
        private readonly ProjectSerializer _serializer;
        private readonly IProjectRepository _repository;
        private readonly IProjectFileDialogService _fileDialogs;
        private readonly IUserDialogService _dialogs;
        private readonly EditorSettings _settings;
        private readonly Action _limparEstadoTransitorio;
        private ProjectMetadataDto _metadata = ProjectMetadataDto.CreateNew(ProjectSerializer.UntitledProjectName);
        private string? _currentPath;

        public ProjectPersistenceService(
            AraciDocument document,
            ICommandHistory commands,
            ProjectSerializer serializer,
            IProjectRepository repository,
            IProjectFileDialogService fileDialogs,
            IUserDialogService dialogs,
            EditorSettings settings,
            Action limparEstadoTransitorio)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _fileDialogs = fileDialogs ?? throw new ArgumentNullException(nameof(fileDialogs));
            _dialogs = dialogs ?? throw new ArgumentNullException(nameof(dialogs));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _limparEstadoTransitorio = limparEstadoTransitorio ?? throw new ArgumentNullException(nameof(limparEstadoTransitorio));
        }

        public void Novo()
        {
            _document.Limpar();
            _serializer.ApplyUnitSettings(null, _settings.Units);
            _serializer.ApplyTypeLibraries(null);
            _limparEstadoTransitorio();
            _commands.Clear();
            _currentPath = null;
            _metadata = ProjectMetadataDto.CreateNew(ProjectSerializer.UntitledProjectName);
        }

        public void SalvarComDialogo()
        {
            string? path = _fileDialogs.ShowSaveDialog();

            if (path != null)
                Salvar(path);
        }

        public void AbrirComDialogo()
        {
            string? path = _fileDialogs.ShowOpenDialog();

            if (path != null)
                Abrir(path);
        }

        public void Salvar(string path)
        {
            try
            {
                DateTimeOffset savedAt = DateTimeOffset.UtcNow;
                ProjectMetadataDto metadata = _serializer.PrepareMetadataForSave(_metadata, path, savedAt);
                ProjectFileDto dto = _serializer.CreateFileDto(_document, metadata, _settings.Units);
                string json = _serializer.Serialize(dto);

                _repository.WriteAllText(path, json);

                _currentPath = path;
                _metadata = metadata;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException or NotSupportedException or ArgumentException)
            {
                _dialogs.ShowError(
                    "Salvar projeto",
                    $"Nao foi possivel salvar o projeto.{Environment.NewLine}{ex.Message}");
            }
        }

        public void Abrir(string path)
        {
            try
            {
                string json = _repository.ReadAllText(path);
                ProjectFileDto dto = _serializer.Deserialize(json);
                int version = _serializer.GetVersion(dto);

                if (version > ProjectSerializer.CurrentVersion)
                {
                    _dialogs.ShowWarning(
                        "Abrir projeto",
                        $"Este projeto foi salvo em uma versao futura ({version}). " +
                        "O Araci tentara abrir de forma conservadora.");
                }

                _serializer.ApplyUnitSettings(dto.Units, _settings.Units);
                _serializer.ApplyTypeLibraries(dto.TypeLibraries);
                var vistas = _serializer.CreateProjectViews(dto);
                var tabelas = _serializer.CreateProjectTables(dto);
                var pranchas = _serializer.CreateProjectSheets(dto);
                Guid? vistaAtivaId = _serializer.GetActiveViewId(dto);
                var elementos = _serializer.CreateElements(dto);

                _document.Limpar();
                _document.SubstituirVistas(vistas);
                _document.DefinirVistaAtiva(vistaAtivaId);
                _document.SubstituirTabelas(tabelas);
                _document.SubstituirPranchas(pranchas);

                foreach (Elemento elemento in elementos)
                    _document.AdicionarElementoPreservandoVista(elemento);

                _metadata = _serializer.CreateMetadataFromFile(dto, path);
                _currentPath = path;

                _limparEstadoTransitorio();
                _commands.Clear();
            }
            catch (JsonException ex)
            {
                MostrarErroAbrir(ex);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException or NotSupportedException or ArgumentException)
            {
                MostrarErroAbrir(ex);
            }
        }

        private void MostrarErroAbrir(Exception ex)
        {
            _dialogs.ShowError(
                "Abrir projeto",
                $"Nao foi possivel abrir o projeto. O projeto atual foi mantido.{Environment.NewLine}{ex.Message}");
        }
    }
}
