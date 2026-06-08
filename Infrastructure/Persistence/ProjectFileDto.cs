using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Araci.Infrastructure.Persistence
{
    public sealed class ProjectFileDto
    {
        public int Version { get; set; }
        public string? AppName { get; set; }
        public string? ProjectName { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }
        public DateTimeOffset? SavedAt { get; set; }
        public string? Generator { get; set; }
        public string? Notes { get; set; }
        public ProjectUnitSettingsDto? Units { get; set; }
        public TypeLibrariesDto? TypeLibraries { get; set; }
        public Guid? ActiveViewId { get; set; }
        public List<ProjectViewDto> Views { get; set; } = new();
        public List<ProjectTableDto> Tables { get; set; } = new();
        public List<ProjectSheetDto> Sheets { get; set; } = new();
        public List<ElementDto> Elements { get; set; } = new();
    }

    public sealed class ProjectViewDto
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Escala { get; set; } = "1:100";
        public string Disciplina { get; set; } = "Eletrica";
        public bool RecortarVista { get; set; }
        public bool? RegiaoRecorteVisivel { get; set; }
        public double CameraX { get; set; }
        public double CameraY { get; set; }
        public double Zoom { get; set; } = 1.0;
    }

    public sealed class ProjectTableDto
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Disciplina { get; set; } = "Eletrica";
        public List<string> CategoriasElementos { get; set; } = new();
        public List<ProjectTableFieldSelectionDto> CamposSelecionados { get; set; } = new();
        public Guid? FiltroVistaId { get; set; }
        public string ModoFiltro { get; set; } = "Todas";
        public List<ProjectTableFilterRuleDto> Filtros { get; set; } = new();
    }

    public sealed class ProjectTableFieldSelectionDto
    {
        public string Categoria { get; set; } = string.Empty;
        public string CampoId { get; set; } = string.Empty;
        public string NomeExibicao { get; set; } = string.Empty;
        public int Ordem { get; set; }
    }

    public sealed class ProjectTableFilterRuleDto
    {
        public int Ordem { get; set; }
        public string Categoria { get; set; } = string.Empty;
        public string CampoId { get; set; } = string.Empty;
        public string NomeExibicao { get; set; } = string.Empty;
        public string Operador { get; set; } = string.Empty;
        public string Valor { get; set; } = string.Empty;
    }

    public sealed class ProjectSheetDto
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Numero { get; set; } = string.Empty;
    }

    public sealed class ProjectMetadataDto
    {
        public string AppName { get; set; } = ProjectSerializer.AppName;
        public string ProjectName { get; set; } = ProjectSerializer.UntitledProjectName;
        public DateTimeOffset? CreatedAt { get; set; }
        public DateTimeOffset? SavedAt { get; set; }
        public string Generator { get; set; } = ProjectSerializer.AppName;
        public string? Notes { get; set; }

        public static ProjectMetadataDto CreateNew(string projectName)
        {
            return new ProjectMetadataDto
            {
                AppName = ProjectSerializer.AppName,
                ProjectName = string.IsNullOrWhiteSpace(projectName) ? ProjectSerializer.UntitledProjectName : projectName,
                Generator = ProjectSerializer.AppName
            };
        }
    }

    public sealed class TypeLibrariesDto
    {
        public List<TextAnnotationTypeDto> TextAnnotationTypes { get; set; } = new();
    }

    public sealed class TextAnnotationTypeDto
    {
        public string NomeTipo { get; set; } = string.Empty;
        public string Familia { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public string CorTexto { get; set; } = "#FF000000";
        public string Fonte { get; set; } = "Arial";
        public double AlturaTexto { get; set; } = 14.0;
        public string AlinhamentoHorizontal { get; set; } = "Esquerda";
        public string LeaderEstiloSeta { get; set; } = "Seta preenchida";
        public string LeaderCor { get; set; } = "#FF000000";
        public double LeaderEspessura { get; set; } = 1.2;
        public double LeaderTamanhoSeta { get; set; } = 10.0;
    }

    public sealed class ElementDto
    {
        public string Kind { get; set; } = string.Empty;
        public string? DomainRole { get; set; }
        public Guid Id { get; set; }
        public Guid? ViewId { get; set; }
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
