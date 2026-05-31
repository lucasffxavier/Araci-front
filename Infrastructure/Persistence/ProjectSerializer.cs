using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using Araci.Core.Documents;
using Araci.Models;
using Araci.Models.Tipos;
using Araci.Services;

namespace Araci.Infrastructure.Persistence
{
    public sealed class ProjectSerializer
    {
        public const int CurrentVersion = 1;
        public const string AppName = "Araci Engine";
        public const string UntitledProjectName = "Sem titulo";

        private readonly ElementRegistryService _elements;
        private readonly TerminalLayoutService _terminalLayout;
        private readonly ElementGeometryService _geometry;
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public ProjectSerializer(
            ElementRegistryService elements,
            TerminalLayoutService terminalLayout,
            ElementGeometryService geometry)
        {
            _elements = elements ?? throw new ArgumentNullException(nameof(elements));
            _terminalLayout = terminalLayout ?? throw new ArgumentNullException(nameof(terminalLayout));
            _geometry = geometry ?? throw new ArgumentNullException(nameof(geometry));
        }

        public ProjectFileDto CreateFileDto(
            AraciDocument document,
            ProjectMetadataDto metadata)
        {
            ArgumentNullException.ThrowIfNull(document);
            ArgumentNullException.ThrowIfNull(metadata);

            return new ProjectFileDto
            {
                Version = CurrentVersion,
                AppName = metadata.AppName,
                ProjectName = metadata.ProjectName,
                CreatedAt = metadata.CreatedAt,
                SavedAt = metadata.SavedAt,
                Generator = metadata.Generator,
                Notes = metadata.Notes,
                Elements = document.Elementos
                    .Select(CriarElementoDto)
                    .ToList()
            };
        }

        public IReadOnlyList<Elemento> CreateElements(ProjectFileDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            return dto.Elements
                .Select(CriarElemento)
                .Where(e => e != null)
                .Cast<Elemento>()
                .ToList();
        }

        public string Serialize(ProjectFileDto dto)
        {
            return JsonSerializer.Serialize(dto, _jsonOptions);
        }

        public ProjectFileDto Deserialize(string json)
        {
            return JsonSerializer.Deserialize<ProjectFileDto>(json, _jsonOptions)
                ?? new ProjectFileDto();
        }

        public ProjectMetadataDto PrepareMetadataForSave(
            ProjectMetadataDto currentMetadata,
            string path,
            DateTimeOffset savedAt)
        {
            ArgumentNullException.ThrowIfNull(currentMetadata);

            string projectName = NomeProjetoParaSalvar(currentMetadata, path);

            return new ProjectMetadataDto
            {
                AppName = AppName,
                ProjectName = projectName,
                CreatedAt = currentMetadata.CreatedAt ?? savedAt,
                SavedAt = savedAt,
                Generator = AppName,
                Notes = currentMetadata.Notes
            };
        }

        public ProjectMetadataDto CreateMetadataFromFile(ProjectFileDto dto, string path)
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

        public int GetVersion(ProjectFileDto dto)
        {
            return dto.Version <= 0 ? 1 : dto.Version;
        }

        private static string NomeProjetoParaSalvar(
            ProjectMetadataDto metadata,
            string path)
        {
            if (!string.IsNullOrWhiteSpace(metadata.ProjectName) &&
                !string.Equals(metadata.ProjectName, UntitledProjectName, StringComparison.OrdinalIgnoreCase))
            {
                return metadata.ProjectName;
            }

            string name = Path.GetFileNameWithoutExtension(path);
            return string.IsNullOrWhiteSpace(name) ? UntitledProjectName : name;
        }

        private ElementDto CriarElementoDto(Elemento elemento)
        {
            return new ElementDto
            {
                Kind = ObterKind(elemento),
                DomainRole = elemento.DomainRole.ToString(),
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
            Elemento? elemento = _elements.CreateModel(dto.Kind);

            if (elemento == null)
                return null;

            elemento.Id = dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id;
            elemento.PosicaoX = dto.X;
            elemento.PosicaoY = dto.Y;
            elemento.Rotacao = dto.Rotation;
            elemento.Escala = dto.Scale == 0 ? 1 : dto.Scale;
            elemento.Tipo = ResolverTipo(dto.Kind, dto.Type);

            AplicarParametros(elemento, dto.Parameters);
            NormalizarParametros(elemento);
            AplicarVertices(elemento, dto.Vertices);
            _terminalLayout.AtualizarTerminais(elemento);
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

        private static void NormalizarParametros(Elemento elemento)
        {
            if (elemento is Barra barra)
                barra.Altura = barra.Altura;
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

        private void RestaurarTerminais(Elemento elemento, IEnumerable<TerminalDto> terminais)
        {
            if (elemento is not ITerminalOwner owner)
                return;

            Size tamanho = _geometry.ObterTamanho(elemento);

            foreach (TerminalDto dto in terminais)
            {
                Terminal? terminal = owner.Terminais.FirstOrDefault(t =>
                    string.Equals(t.Id, dto.Id, StringComparison.OrdinalIgnoreCase));

                if (terminal == null)
                    continue;

                if (elemento is Cabo || tamanho.IsEmpty)
                    terminal.DefinirPosicaoVisual(new Point(dto.X, dto.Y));
                else
                    terminal.DefinirPosicaoVisual(new Point(dto.X, dto.Y), tamanho.Width, tamanho.Height);

                terminal.Barra = dto.Barra;
            }
        }

        private TipoElemento? ResolverTipo(string kind, TypeRefDto? dto)
        {
            return _elements.ResolveType(
                kind,
                dto?.NomeTipo,
                dto?.Familia,
                dto?.Categoria);
        }

        private string ObterKind(Elemento elemento)
        {
            return _elements.GetKind(elemento);
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
    }
}
