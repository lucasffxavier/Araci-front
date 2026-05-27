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
        private const string FileFilter =
            "Projeto Araci (*.araci)|*.araci|JSON (*.json)|*.json|Todos os arquivos (*.*)|*.*";

        private readonly EditorContext _context;
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
            var dto = new ProjectFileDto
            {
                Version = CurrentVersion,
                Elements = _context.Document.Elementos
                    .Select(CriarElementoDto)
                    .ToList()
            };

            string json = JsonSerializer.Serialize(dto, _jsonOptions);
            File.WriteAllText(path, json);
        }

        public void Abrir(string path)
        {
            string json = File.ReadAllText(path);
            var dto = JsonSerializer.Deserialize<ProjectFileDto>(json, _jsonOptions)
                ?? new ProjectFileDto();

            var elementos = dto.Elements
                .Select(CriarElemento)
                .Where(e => e != null)
                .Cast<Elemento>()
                .ToList();

            _context.Document.Limpar();

            foreach (Elemento elemento in elementos)
                _context.Document.AdicionarElemento(elemento);

            LimparEstadoTransitorio();
            _context.Commands.Clear();
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
            public List<ElementDto> Elements { get; set; } = new();
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
