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
        public List<ElementDto> Elements { get; set; } = new();
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
    }

    public sealed class ElementDto
    {
        public string Kind { get; set; } = string.Empty;
        public string? DomainRole { get; set; }
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
