using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using Araci.Models;
using Araci.Models.Tipos;
using Microsoft.Win32;

namespace Araci.Services
{
    public class ProjectPersistenceService
    {
        private const int CurrentVersion = 1;
        private const string AppName = "Araci Engine";
        private const string UntitledProjectName = "Sem titulo";
        private const string FileFilter =
            "Projeto Araci (*.araci)|*.araci|JSON (*.json)|*.json|Todos os arquivos (*.*)|*.*";

        private readonly EditorContext _context;
        private ProjectMetadataDto _metadata = ProjectMetadataDto.CreateNew(UntitledProjectName);
        private string? _currentPath;
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public ProjectPersistenceService(EditorContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public void Novo()
        {
            _context.Document.Limpar();
            LimparEstadoTransitorio();
            _context.Commands.Clear();
            _currentPath = null;
            _metadata = ProjectMetadataDto.CreateNew(UntitledProjectName);
        }

        public void SalvarComDialogo()
        {
            var dialog = new SaveFileDialog
            {
                Filter = FileFilter,
                DefaultExt = ".araci",
                AddExtension = true
            };

            if (dialog.ShowDialog() == true)
                Salvar(dialog.FileName);
        }

        public void AbrirComDialogo()
        {
            var dialog = new OpenFileDialog
            {
                Filter = FileFilter,
                DefaultExt = ".araci",
                CheckFileExists = true
            };

            if (dialog.ShowDialog() == true)
                Abrir(dialog.FileName);
        }

        public void Salvar(string path)
        {
            try
            {
                DateTimeOffset savedAt = DateTimeOffset.UtcNow;
                ProjectMetadataDto metadata = PrepararMetadadosParaSalvar(path, savedAt);

                var dto = new ProjectFileDto
                {
                    Version = CurrentVersion,
                    AppName = metadata.AppName,
                    ProjectName = metadata.ProjectName,
                    CreatedAt = metadata.CreatedAt,
                    SavedAt = metadata.SavedAt,
                    Generator = metadata.Generator,
                    Notes = metadata.Notes,
                    Elements = _context.Document.Elementos
                        .Select(CriarElementoDto)
                        .ToList()
                };

                string json = JsonSerializer.Serialize(dto, _jsonOptions);
                File.WriteAllText(path, json);

                _currentPath = path;
                _metadata = metadata;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException or NotSupportedException or ArgumentException)
            {
                _context.Dialogs.ShowError(
                    "Salvar projeto",
                    $"Nao foi possivel salvar o projeto.{Environment.NewLine}{ex.Message}");
            }
        }

        public void Abrir(string path)
        {
            try
            {
                string json = File.ReadAllText(path);
                var dto = JsonSerializer.Deserialize<ProjectFileDto>(json, _jsonOptions)
                    ?? new ProjectFileDto();

                int version = ObterVersao(dto);

                if (version > CurrentVersion)
                {
                    _context.Dialogs.ShowWarning(
                        "Abrir projeto",
                        $"Este projeto foi salvo em uma versao futura ({version}). " +
                        "O Araci tentara abrir de forma conservadora.");
                }

                var elementos = dto.Elements
                    .Select(CriarElemento)
                    .Where(e => e != null)
                    .Cast<Elemento>()
                    .ToList();

                _context.Document.Limpar();

                foreach (Elemento elemento in elementos)
                    _context.Document.AdicionarElemento(elemento);

                _metadata = CriarMetadadosDoArquivo(dto, path);
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

        private ProjectMetadataDto PrepararMetadadosParaSalvar(
            string path,
            DateTimeOffset savedAt)
        {
            string projectName = NomeProjetoParaSalvar(path);

            return new ProjectMetadataDto
            {
                AppName = AppName,
                ProjectName = projectName,
                CreatedAt = _metadata.CreatedAt ?? savedAt,
                SavedAt = savedAt,
                Generator = AppName,
                Notes = _metadata.Notes
            };
        }

        private string NomeProjetoParaSalvar(string path)
        {
            if (!string.IsNullOrWhiteSpace(_metadata.ProjectName) &&
                !string.Equals(_metadata.ProjectName, UntitledProjectName, StringComparison.OrdinalIgnoreCase))
            {
                return _metadata.ProjectName;
            }

            string name = Path.GetFileNameWithoutExtension(path);
            return string.IsNullOrWhiteSpace(name) ? UntitledProjectName : name;
        }

        private static int ObterVersao(ProjectFileDto dto)
        {
            return dto.Version <= 0 ? 1 : dto.Version;
        }

        private static ProjectMetadataDto CriarMetadadosDoArquivo(ProjectFileDto dto, string path)
        {
            DateTimeOffset now = DateTimeOffset.UtcNow;
            string projectName = string.IsNullOrWhiteSpace(dto.ProjectName)
                ? Path.GetFileNameWithoutExtension(path)
                : dto.ProjectName;

            return new ProjectMetadataDto
            {
                AppName = string.IsNullOrWhiteSpace(dto.AppName) ? AppName : dto.AppName,
                ProjectName = string.IsNullOrWhiteSpace(projectName) ? UntitledProjectName : projectName,
                CreatedAt = dto.CreatedAt ?? dto.SavedAt ?? now,
                SavedAt = dto.SavedAt ?? now,
                Generator = string.IsNullOrWhiteSpace(dto.Generator) ? AppName : dto.Generator,
                Notes = dto.Notes
            };
        }

        private void MostrarErroAbrir(Exception ex)
        {
            _context.Dialogs.ShowError(
                "Abrir projeto",
                $"Nao foi possivel abrir o projeto. O projeto atual foi mantido.{Environment.NewLine}{ex.Message}");
        }

        private ElementDto CriarElementoDto(Elemento elemento)
        {
            return new ElementDto
            {
                Kind = ObterKind(elemento),
                Id = elemento.Id,
                X = elemento.PosicaoX,
                Y = elemento.PosicaoY,
                Rotation = elemento.Rotacao,
                Scale = elemento.Escala,
                Type = CriarTypeRef(elemento.Tipo),
                Parameters = elemento.Parametros.Values
                    .Select(CriarParameterDto)
                    .ToList(),
                Terminals = (elemento as ITerminalOwner)?.Terminais
                    .Select(CriarTerminalDto)
                    .ToList() ?? new List<TerminalDto>(),
                Vertices = elemento is Cabo cabo
                    ? cabo.Vertices.Select(CriarPointDto).ToList()
                    : new List<PointDto>()
            };
        }

        private Elemento? CriarElemento(ElementDto dto)
        {
            Elemento? elemento = dto.Kind switch
            {
                "Barra" => _context.ElementoFactory.CriarBarra(),
                "Carga" => _context.ElementoFactory.CriarCarga(),
                "Gerador" => _context.ElementoFactory.CriarGerador(),
                "Cabo" => _context.ElementoFactory.CriarCabo(),
                _ => null
            };

            if (elemento == null)
                return null;

            elemento.Id = dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id;
            elemento.PosicaoX = dto.X;
            elemento.PosicaoY = dto.Y;
            elemento.Rotacao = dto.Rotation;
            elemento.Escala = dto.Scale == 0 ? 1 : dto.Scale;
            elemento.Tipo = ResolverTipo(dto.Kind, dto.Type);

            AplicarParametros(elemento, dto.Parameters);
            AplicarVertices(elemento, dto.Vertices);
            _context.TerminalLayout.AtualizarTerminais(elemento);
            RestaurarTerminais(elemento, dto.Terminals);

            return elemento;
        }

        private void AplicarParametros(Elemento elemento, IEnumerable<ParameterDto> parametros)
        {
            foreach (ParameterDto dto in parametros)
            {
                if (!elemento.PossuiParametro(dto.Name))
                    continue;

                Parameter parameter = elemento.Parametros[dto.Name];
                parameter.ValorObjeto = ConverterValor(dto, parameter.Tipo);
            }
        }

        private static object? ConverterValor(ParameterDto dto, Type destino)
        {
            if (dto.Value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
                return null;

            if (destino == typeof(string))
                return dto.Value.GetString() ?? string.Empty;

            if (destino == typeof(int))
                return dto.Value.GetInt32();

            if (destino == typeof(double))
                return dto.Value.GetDouble();

            if (destino == typeof(bool))
                return dto.Value.GetBoolean();

            if (destino == typeof(Guid))
                return dto.Value.GetGuid();

            return dto.Value.ToString();
        }

        private void AplicarVertices(Elemento elemento, IEnumerable<PointDto> vertices)
        {
            if (elemento is not Cabo cabo)
                return;

            cabo.Vertices.Clear();

            foreach (PointDto p in vertices)
                cabo.Vertices.Add(new Point(p.X, p.Y));

            cabo.PreviewPonto = null;

            if (cabo.Vertices.Count > 0)
                cabo.DefinirOrigem(cabo.Vertices[0]);

            if (cabo.Vertices.Count > 1)
                cabo.DefinirDestino(cabo.Vertices[^1]);
        }

        private static void RestaurarTerminais(Elemento elemento, IEnumerable<TerminalDto> terminais)
        {
            if (elemento is not ITerminalOwner owner)
                return;

            foreach (TerminalDto dto in terminais)
            {
                Terminal? terminal = owner.Terminais.FirstOrDefault(t =>
                    string.Equals(t.Id, dto.Id, StringComparison.OrdinalIgnoreCase));

                if (terminal == null)
                    continue;

                terminal.Posicao = new Point(dto.X, dto.Y);
                terminal.Barra = dto.Barra;
            }
        }

        private TipoElemento? ResolverTipo(string kind, TypeRefDto? dto)
        {
            IEnumerable<TipoElemento> candidatos = kind switch
            {
                "Barra" => _context.Types.TiposBarras,
                "Carga" => _context.Types.TiposCargas,
                "Gerador" => _context.Types.TiposGeradores,
                "Cabo" => _context.Types.TiposCabos,
                _ => Enumerable.Empty<TipoElemento>()
            };

            TipoElemento? tipo = null;

            if (dto != null)
            {
                tipo = candidatos.FirstOrDefault(t =>
                    string.Equals(t.NomeTipo, dto.NomeTipo, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(t.Familia, dto.Familia, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(t.Categoria, dto.Categoria, StringComparison.OrdinalIgnoreCase));
            }

            return tipo ?? kind switch
            {
                "Barra" => _context.Types.TipoBarraPadrao,
                "Carga" => _context.Types.TipoCargaPadrao,
                "Gerador" => _context.Types.TipoGeradorPadrao,
                "Cabo" => _context.Types.TipoCaboPadrao,
                _ => null
            };
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

        private static string ObterKind(Elemento elemento)
        {
            return elemento switch
            {
                Barra => "Barra",
                Carga => "Carga",
                Gerador => "Gerador",
                Cabo => "Cabo",
                _ => elemento.GetType().Name
            };
        }

        private static TypeRefDto? CriarTypeRef(TipoElemento? tipo)
        {
            return tipo == null
                ? null
                : new TypeRefDto
                {
                    NomeTipo = tipo.NomeTipo,
                    Familia = tipo.Familia,
                    Categoria = tipo.Categoria
                };
        }

        private static ParameterDto CriarParameterDto(Parameter parameter)
        {
            return new ParameterDto
            {
                Name = parameter.Nome,
                Type = parameter.Tipo.FullName ?? parameter.Tipo.Name,
                Value = JsonSerializer.SerializeToElement(parameter.ValorObjeto, parameter.Tipo)
            };
        }

        private static TerminalDto CriarTerminalDto(Terminal terminal)
        {
            return new TerminalDto
            {
                Id = terminal.Id,
                X = terminal.Posicao.X,
                Y = terminal.Posicao.Y,
                Barra = terminal.Barra
            };
        }

        private static PointDto CriarPointDto(Point point)
        {
            return new PointDto { X = point.X, Y = point.Y };
        }

        public sealed class ProjectFileDto
        {
            public int Version { get; set; }
            public string? AppName { get; set; }
            public string? ProjectName { get; set; }
            public DateTimeOffset? CreatedAt { get; set; }
            public DateTimeOffset? SavedAt { get; set; }
            public string? Generator { get; set; }
            public string? Notes { get; set; }
            public List<ElementDto> Elements { get; set; } = new();
        }

        private sealed class ProjectMetadataDto
        {
            public string AppName { get; set; } = ProjectPersistenceService.AppName;
            public string ProjectName { get; set; } = UntitledProjectName;
            public DateTimeOffset? CreatedAt { get; set; }
            public DateTimeOffset? SavedAt { get; set; }
            public string Generator { get; set; } = ProjectPersistenceService.AppName;
            public string? Notes { get; set; }

            public static ProjectMetadataDto CreateNew(string projectName)
            {
                return new ProjectMetadataDto
                {
                    AppName = ProjectPersistenceService.AppName,
                    ProjectName = string.IsNullOrWhiteSpace(projectName) ? UntitledProjectName : projectName,
                    Generator = ProjectPersistenceService.AppName
                };
            }
        }

        public sealed class ElementDto
        {
            public string Kind { get; set; } = string.Empty;
            public Guid Id { get; set; }
            public double X { get; set; }
            public double Y { get; set; }
            public double Rotation { get; set; }
            public double Scale { get; set; } = 1;
            public TypeRefDto? Type { get; set; }
            public List<ParameterDto> Parameters { get; set; } = new();
            public List<TerminalDto> Terminals { get; set; } = new();
            public List<PointDto> Vertices { get; set; } = new();
        }

        public sealed class TypeRefDto
        {
            public string NomeTipo { get; set; } = string.Empty;
            public string Familia { get; set; } = string.Empty;
            public string Categoria { get; set; } = string.Empty;
        }

        public sealed class ParameterDto
        {
            public string Name { get; set; } = string.Empty;
            public string Type { get; set; } = string.Empty;
            public JsonElement Value { get; set; }
        }

        public sealed class TerminalDto
        {
            public string Id { get; set; } = string.Empty;
            public double X { get; set; }
            public double Y { get; set; }
            public string? Barra { get; set; }
        }

        public sealed class PointDto
        {
            public double X { get; set; }
            public double Y { get; set; }
        }
    }
}
