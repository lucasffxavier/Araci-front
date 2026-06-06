using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using Araci.Applications.Abstractions;
using Araci.Core.Documents;
using Araci.Models;
using Araci.Models.Tipos;
using Araci.Services;
using Araci.Services.Catalog;
using Araci.Services.Geometry;
using Araci.Services.Settings;

namespace Araci.Infrastructure.Persistence
{
    public sealed class ProjectSerializer
    {
        public const int CurrentVersion = 1;
        public const string AppName = "Araci Engine";
        public const string UntitledProjectName = "Sem titulo";

        private readonly ElementRegistryService _elements;
        private readonly IElementModelFactory _modelFactory;
        private readonly TerminalLayoutService _terminalLayout;
        private readonly ElementGeometryService _geometry;
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public ProjectSerializer(
            ElementRegistryService elements,
            IElementModelFactory modelFactory,
            TerminalLayoutService terminalLayout,
            ElementGeometryService geometry)
        {
            _elements = elements ?? throw new ArgumentNullException(nameof(elements));
            _modelFactory = modelFactory ?? throw new ArgumentNullException(nameof(modelFactory));
            _terminalLayout = terminalLayout ?? throw new ArgumentNullException(nameof(terminalLayout));
            _geometry = geometry ?? throw new ArgumentNullException(nameof(geometry));
        }

        public ProjectFileDto CreateFileDto(
            AraciDocument document,
            ProjectMetadataDto metadata,
            UnitDisplaySettings units)
        {
            ArgumentNullException.ThrowIfNull(document);
            ArgumentNullException.ThrowIfNull(metadata);
            ArgumentNullException.ThrowIfNull(units);

            return new ProjectFileDto
            {
                Version = CurrentVersion,
                AppName = metadata.AppName,
                ProjectName = metadata.ProjectName,
                CreatedAt = metadata.CreatedAt,
                SavedAt = metadata.SavedAt,
                Generator = metadata.Generator,
                Notes = metadata.Notes,
                Units = CreateUnitSettingsDto(units),
                TypeLibraries = CreateTypeLibrariesDto(),
                Elements = document.Elementos
                    .Select(CriarElementoDto)
                    .ToList()
            };
        }

        public ProjectUnitSettingsDto CreateUnitSettingsDto(UnitDisplaySettings units)
        {
            ArgumentNullException.ThrowIfNull(units);

            return new ProjectUnitSettingsDto
            {
                Length = units.Length.ToString(),
                Voltage = units.Voltage.ToString(),
                Current = units.Current.ToString(),
                ActivePower = units.ActivePower.ToString(),
                ReactivePower = units.ReactivePower.ToString(),
                ApparentPower = units.ApparentPower.ToString(),
                Percent = units.Percent.ToString()
            };
        }

        public void ApplyUnitSettings(ProjectUnitSettingsDto? dto, UnitDisplaySettings target)
        {
            ArgumentNullException.ThrowIfNull(target);

            var defaults = new UnitDisplaySettings();

            target.Length = ParseUnit(dto?.Length, UnitQuantityKind.Length, defaults.Length);
            target.Voltage = ParseUnit(dto?.Voltage, UnitQuantityKind.Voltage, defaults.Voltage);
            target.Current = ParseUnit(dto?.Current, UnitQuantityKind.Current, defaults.Current);
            target.ActivePower = ParseUnit(dto?.ActivePower, UnitQuantityKind.ActivePower, defaults.ActivePower);
            target.ReactivePower = ParseUnit(dto?.ReactivePower, UnitQuantityKind.ReactivePower, defaults.ReactivePower);
            target.ApparentPower = ParseUnit(dto?.ApparentPower, UnitQuantityKind.ApparentPower, defaults.ApparentPower);
            target.Percent = ParseUnit(dto?.Percent, UnitQuantityKind.Percent, defaults.Percent);
        }

        public void ApplyTypeLibraries(TypeLibrariesDto? dto)
        {
            IEnumerable<TextAnnotationTypeDto> textTypes = dto?.TextAnnotationTypes != null && dto.TextAnnotationTypes.Count > 0
                ? dto.TextAnnotationTypes
                : CreateDefaultTextAnnotationTypeDtos();

            List<TipoTextoAnotativo> tipos = textTypes
                .Select(CriarTipoTextoAnotativo)
                .Where(t => !string.IsNullOrWhiteSpace(t.NomeTipo))
                .GroupBy(CriarChaveTipo, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.Last())
                .ToList();

            if (tipos.Count == 0)
                tipos = CreateDefaultTextAnnotationTypeDtos().Select(CriarTipoTextoAnotativo).ToList();

            _elements.ReplaceTypes(ElementKinds.TextoAnotativo, tipos);
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

        private TypeLibrariesDto CreateTypeLibrariesDto()
        {
            return new TypeLibrariesDto
            {
                TextAnnotationTypes = _elements.GetTypes(ElementKinds.TextoAnotativo)
                    .OfType<TipoTextoAnotativo>()
                    .Select(CriarTextAnnotationTypeDto)
                    .ToList()
            };
        }

        private static IEnumerable<TextAnnotationTypeDto> CreateDefaultTextAnnotationTypeDtos()
        {
            yield return new TextAnnotationTypeDto { NomeTipo = "Texto padrão", Familia = "Anotações", Categoria = "Textos", CorTexto = "#FF000000", Fonte = "Arial", AlturaTexto = 14.0, AlinhamentoHorizontal = "Esquerda", LeaderEstiloSeta = "Seta preenchida", LeaderCor = "#FF000000", LeaderEspessura = 1.2, LeaderTamanhoSeta = 10.0 };
            yield return new TextAnnotationTypeDto { NomeTipo = "Texto pequeno", Familia = "Anotações", Categoria = "Textos", CorTexto = "#FF000000", Fonte = "Arial", AlturaTexto = 10.0, AlinhamentoHorizontal = "Esquerda", LeaderEstiloSeta = "Seta preenchida", LeaderCor = "#FF000000", LeaderEspessura = 1.2, LeaderTamanhoSeta = 10.0 };
            yield return new TextAnnotationTypeDto { NomeTipo = "Texto título", Familia = "Anotações", Categoria = "Textos", CorTexto = "#FF000000", Fonte = "Arial", AlturaTexto = 20.0, AlinhamentoHorizontal = "Centro", LeaderEstiloSeta = "Seta preenchida", LeaderCor = "#FF000000", LeaderEspessura = 1.2, LeaderTamanhoSeta = 10.0 };
        }

        private static TextAnnotationTypeDto CriarTextAnnotationTypeDto(TipoTextoAnotativo tipo)
        {
            return new TextAnnotationTypeDto
            {
                NomeTipo = tipo.NomeTipo,
                Familia = tipo.Familia,
                Categoria = tipo.Categoria,
                CorTexto = tipo.CorTexto,
                Fonte = tipo.Fonte,
                AlturaTexto = tipo.AlturaTexto,
                AlinhamentoHorizontal = tipo.AlinhamentoHorizontal,
                LeaderEstiloSeta = tipo.LeaderEstiloSeta,
                LeaderCor = tipo.LeaderCor,
                LeaderEspessura = tipo.LeaderEspessura,
                LeaderTamanhoSeta = tipo.LeaderTamanhoSeta
            };
        }

        private static TipoTextoAnotativo CriarTipoTextoAnotativo(TextAnnotationTypeDto dto)
        {
            var tipo = new TipoTextoAnotativo
            {
                NomeTipo = string.IsNullOrWhiteSpace(dto.NomeTipo) ? "Texto padrão" : dto.NomeTipo.Trim(),
                Familia = string.IsNullOrWhiteSpace(dto.Familia) ? "Anotações" : dto.Familia.Trim(),
                Categoria = string.IsNullOrWhiteSpace(dto.Categoria) ? "Textos" : dto.Categoria.Trim()
            };

            tipo.CorTexto = dto.CorTexto;
            tipo.Fonte = dto.Fonte;
            tipo.AlturaTexto = dto.AlturaTexto;
            tipo.AlinhamentoHorizontal = dto.AlinhamentoHorizontal;
            tipo.LeaderEstiloSeta = dto.LeaderEstiloSeta;
            tipo.LeaderCor = dto.LeaderCor;
            tipo.LeaderEspessura = dto.LeaderEspessura;
            tipo.LeaderTamanhoSeta = dto.LeaderTamanhoSeta;
            return tipo;
        }

        private static string CriarChaveTipo(TipoTextoAnotativo tipo)
        {
            return $"{tipo.NomeTipo.Trim()}|{tipo.Familia.Trim()}|{tipo.Categoria.Trim()}";
        }

        private static UnitKind ParseUnit(string? value, UnitQuantityKind quantity, UnitKind fallback)
        {
            if (string.IsNullOrWhiteSpace(value))
                return fallback;

            if (!Enum.TryParse(value, ignoreCase: true, out UnitKind unit))
                return fallback;

            return UnitFormatter.GetQuantity(unit) == quantity ? unit : fallback;
        }

        private static string NomeProjetoParaSalvar(ProjectMetadataDto metadata, string path)
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
            Elemento? elemento = _modelFactory.CreateModel(dto.Kind);

            if (elemento == null)
                return null;

            elemento.Id = dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id;
            elemento.PosicaoX = NormalizarCoordenada(dto.X);
            elemento.PosicaoY = NormalizarCoordenada(dto.Y);
            elemento.Rotacao = NormalizarRotacao(dto.Rotation);
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

            if (elemento is TextoAnotativo texto)
                NormalizarTextoAnotativo(texto);
        }

        private static void NormalizarTextoAnotativo(TextoAnotativo texto)
        {
            texto.Texto = texto.Texto;
            texto.LarguraCaixa = texto.LarguraCaixa;
            texto.Rotacao = NormalizarRotacao(texto.Rotacao);
            texto.LeaderAtivo = texto.LeaderAtivo;
            texto.LeaderX = NormalizarCoordenada(texto.LeaderX);
            texto.LeaderY = NormalizarCoordenada(texto.LeaderY);
        }

        private static object? ConverterValor(ParameterDto dto, Type destino)
        {
            if (dto.Value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
                return null;

            if (destino == typeof(string))
                return dto.Value.ValueKind == JsonValueKind.String
                    ? dto.Value.GetString() ?? string.Empty
                    : dto.Value.ToString();

            if (destino == typeof(int))
                return dto.Value.ValueKind == JsonValueKind.Number && dto.Value.TryGetInt32(out int i)
                    ? i
                    : 0;

            if (destino == typeof(double))
                return dto.Value.ValueKind == JsonValueKind.Number && dto.Value.TryGetDouble(out double d)
                    ? d
                    : 0.0;

            if (destino == typeof(bool))
                return dto.Value.ValueKind == JsonValueKind.True || dto.Value.ValueKind == JsonValueKind.False
                    ? dto.Value.GetBoolean()
                    : false;

            if (destino == typeof(Guid))
                return dto.Value.ValueKind == JsonValueKind.String && dto.Value.TryGetGuid(out Guid guid)
                    ? guid
                    : Guid.Empty;

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

        private static double NormalizarRotacao(double valor)
        {
            if (double.IsNaN(valor) || double.IsInfinity(valor))
                return 0;

            double normalizada = valor % 360;

            if (normalizada < 0)
                normalizada += 360;

            return normalizada >= 360 ? 0 : normalizada;
        }

        private static double NormalizarCoordenada(double valor)
        {
            return double.IsNaN(valor) || double.IsInfinity(valor) ? 0.0 : valor;
        }
    }
}