using System;
using System.IO;
using System.Text.Json;
using Araci.Applications.Abstractions;
using Araci.Infrastructure.Persistence;
using Araci.Models;

namespace Araci.Services
{
    public class ProjectPersistenceService : IProjectPersistenceService
    {
        private readonly EditorContext _context;
        private readonly ProjectSerializer _serializer;
        private readonly IProjectRepository _repository;
        private readonly IProjectFileDialogService _fileDialogs;
        private readonly IUserDialogService _dialogs;
        private ProjectMetadataDto _metadata = ProjectMetadataDto.CreateNew(ProjectSerializer.UntitledProjectName);
        private string? _currentPath;

        public ProjectPersistenceService(EditorContext context)
            : this(
                context,
                new ProjectSerializer(context.Elements, context.TerminalLayout, context.Geometry),
                new FileSystemProjectRepository(),
                new ProjectFileDialogService(),
                context.Dialogs)
        {
        }

        public ProjectPersistenceService(
            EditorContext context,
            ProjectSerializer serializer,
            IProjectRepository repository,
            IProjectFileDialogService fileDialogs,
            IUserDialogService dialogs)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _fileDialogs = fileDialogs ?? throw new ArgumentNullException(nameof(fileDialogs));
            _dialogs = dialogs ?? throw new ArgumentNullException(nameof(dialogs));
        }

        public void Novo()
        {
            _context.Document.Limpar();
            LimparEstadoTransitorio();
            _context.Commands.Clear();
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
                ProjectFileDto dto = _serializer.CreateFileDto(_context.Document, metadata);
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

                var elementos = _serializer.CreateElements(dto);

                _context.Document.Limpar();

                foreach (Elemento elemento in elementos)
                    _context.Document.AdicionarElemento(elemento);

                _metadata = _serializer.CreateMetadataFromFile(dto, path);
                _currentPath = path;

                LimparEstadoTransitorio();
                _context.Commands.Clear();
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

        private void LimparEstadoTransitorio()
        {
            _context.Selection.Limpar();
            _context.Hover.Clear();
            _context.CableVertexEdit.Clear();
            _context.TerminalSnap.Limpar();
            _context.SelectionBox.Visivel = false;
            _context.MoveHud.Visivel = false;
            _context.MoveHud.Reset();
            _context.SceneQueries.Invalidate();
            _context.Tools.VoltarParaSelecao();
        }
    }
}
