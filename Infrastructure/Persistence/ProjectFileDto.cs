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
        public List<ProjectSheetTypeDto> SheetTypes { get; set; } = new();
        public List<ProjectSheetDto> Sheets { get; set; } = new();
        public List<ElementDto> Elements { get; set; } = new();
    }

    public sealed class ProjectSheetTypeDto
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string FormatoFolha { get; set; } = string.Empty;
        public string OrientacaoFolha { get; set; } = string.Empty;
        public double? LarguraFolha { get; set; }
        public double? AlturaFolha { get; set; }
        public List<ProjectSheetTemplateLineDto> Linhas { get; set; } = new();
        public List<ProjectSheetTemplateRectangleDto> Retangulos { get; set; } = new();
        public List<ProjectSheetTemplateCircleDto> Circulos { get; set; } = new();
        public List<ProjectSheetTemplateTextDto> Textos { get; set; } = new();
    }

    public sealed class ProjectSheetTemplateLineDto
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public double X1 { get; set; }
        public double Y1 { get; set; }
        public double X2 { get; set; }
        public double Y2 { get; set; }
        public TypeRefDto? Type { get; set; }
        public string Stroke { get; set; } = "#FF000000";
        public double StrokeThickness { get; set; } = 1.0;
        public bool Visible { get; set; } = true;
    }

    public sealed class ProjectSheetTemplateRectangleDto
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public double X { get; set; }
        public double Y { get; set; }
        public double Largura { get; set; }
        public double Altura { get; set; }
        public TypeRefDto? Type { get; set; }
        public string Stroke { get; set; } = "#FF000000";
        public double StrokeThickness { get; set; } = 1.0;
        public bool Visible { get; set; } = true;
    }

    public sealed class ProjectSheetTemplateCircleDto
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public double X { get; set; }
        public double Y { get; set; }
        public double Raio { get; set; }
        public TypeRefDto? Type { get; set; }
        public string Stroke { get; set; } = "#FF000000";
        public double StrokeThickness { get; set; } = 1.0;
        public bool Visible { get; set; } = true;
    }

    public sealed class ProjectSheetTemplateTextDto
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public double X { get; set; }
        public double Y { get; set; }
        public string Texto { get; set; } = string.Empty;
        public double LarguraCaixa { get; set; }
        public TypeRefDto? Type { get; set; }
        public string CorTexto { get; set; } = "#FF000000";
        public string Fonte { get; set; } = "Arial";
        public double AlturaTexto { get; set; } = 14.0;
        public string AlinhamentoHorizontal { get; set; } = "Esquerda";
        public double Rotacao { get; set; }
        public bool LeaderAtivo { get; set; }
        public double LeaderX { get; set; }
        public double LeaderY { get; set; }
        public bool LeaderComCotovelo { get; set; }
        public double LeaderCotoveloX { get; set; }
        public double LeaderCotoveloY { get; set; }
        public bool LeaderCotoveloManual { get; set; }
        public bool Visible { get; set; } = true;
    }

    public sealed class ProjectViewDto
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Escala { get; set; } = "1:100";
        public string Disciplina { get; set; } = "Eletrica";
        public bool RecortarVista { get; set; }
        public bool? RegiaoRecorteVisivel { get; set; }
        public double? RecorteX { get; set; }
        public double? RecorteY { get; set; }
        public double? RecorteLargura { get; set; }
        public double? RecorteAltura { get; set; }
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
        public ProjectTableSortingDto? Ordenacao { get; set; }
        public List<ProjectTableSortingDto> Ordenacoes { get; set; } = new();
        public ProjectTableDisplaySettingsDto? Exibicao { get; set; }
    }

    public sealed class ProjectTableDisplaySettingsDto
    {
        public bool? ExibirTitulo { get; set; }
        public string? FonteTitulo { get; set; }
        public double? TamanhoFonteTitulo { get; set; }
        public bool? TituloNegrito { get; set; }
        public string? CorTextoTitulo { get; set; }
        public string? CorFundoTitulo { get; set; }
        public double? AlturaTitulo { get; set; }
        public string? AlinhamentoTitulo { get; set; }
        public bool? ExibirCabecalho { get; set; }
        public string? FonteCabecalho { get; set; }
        public double? TamanhoFonteCabecalho { get; set; }
        public bool? CabecalhoNegrito { get; set; }
        public string? CorTextoCabecalho { get; set; }
        public string? CorFundoCabecalho { get; set; }
        public double? AlturaCabecalho { get; set; }
        public string? AlinhamentoCabecalho { get; set; }
        public string? FonteCorpo { get; set; }
        public double? TamanhoFonteCorpo { get; set; }
        public string? CorTextoCorpo { get; set; }
        public string? CorFundoCorpo { get; set; }
        public double? AlturaLinhaCorpo { get; set; }
        public string? AlinhamentoCorpo { get; set; }
        public bool? UsarLinhasAlternadas { get; set; }
        public string? CorLinhaAlternada { get; set; }
        public bool? ExibirLinhasGrade { get; set; }
        public string? CorGrade { get; set; }
        public double? EspessuraGrade { get; set; }
        public bool? ExibirContornoExterno { get; set; }
        public string? CorContorno { get; set; }
        public double? EspessuraContorno { get; set; }
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

    public sealed class ProjectTableSortingDto
    {
        public int Ordem { get; set; }
        public string Categoria { get; set; } = string.Empty;
        public string CampoId { get; set; } = string.Empty;
        public string NomeExibicao { get; set; } = string.Empty;
        public string Direcao { get; set; } = string.Empty;
    }

    public sealed class ProjectSheetDto
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Numero { get; set; } = string.Empty;
        public Guid? SheetTypeId { get; set; }
        public string FormatoFolha { get; set; } = string.Empty;
        public string OrientacaoFolha { get; set; } = string.Empty;
        public double? LarguraFolha { get; set; }
        public double? AlturaFolha { get; set; }
        public List<ProjectSheetTableInstanceDto> Tabelas { get; set; } = new();
    }

    public sealed class ProjectSheetTableInstanceDto
    {
        public Guid Id { get; set; }
        public Guid TableId { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public int RowStartIndex { get; set; }
        public int? RowCount { get; set; }
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
        public List<LineAnnotationTypeDto> LineAnnotationTypes { get; set; } = new();
        public List<TextAnnotationTypeDto> TextAnnotationTypes { get; set; } = new();
    }

    public sealed class LineAnnotationTypeDto
    {
        public string NomeTipo { get; set; } = string.Empty;
        public string Familia { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public string EstiloLinha { get; set; } = "Contínuo";
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