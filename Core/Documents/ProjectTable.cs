using System;
using System.Collections.Generic;

namespace Araci.Core.Documents
{
    public enum ProjectTableElementCategory
    {
        Barras,
        Cabos,
        Cargas,
        Geradores,
        Transformadores,
        Sin
    }

    public class ProjectTableFieldSelection
    {
        public ProjectTableElementCategory Categoria { get; set; }
        public string CampoId { get; set; } = string.Empty;
        public string NomeExibicao { get; set; } = string.Empty;
        public int Ordem { get; set; }
    }

    public enum ProjectTableFilterLogicalMode
    {
        Todas,
        Qualquer
    }

    public enum ProjectTableFilterOperator
    {
        Contem,
        NaoContem,
        ComecaCom,
        TerminaCom,
        IgualA,
        DiferenteDe
    }

    public class ProjectTableFilterRule
    {
        public int Ordem { get; set; }
        public ProjectTableElementCategory Categoria { get; set; }
        public string CampoId { get; set; } = string.Empty;
        public string NomeExibicao { get; set; } = string.Empty;
        public ProjectTableFilterOperator Operador { get; set; } = ProjectTableFilterOperator.Contem;
        public string Valor { get; set; } = string.Empty;
    }

    public enum ProjectTableSortDirection
    {
        Crescente,
        Decrescente
    }

    public class ProjectTableSorting
    {
        public int Ordem { get; set; }
        public ProjectTableElementCategory Categoria { get; set; }
        public string CampoId { get; set; } = string.Empty;
        public string NomeExibicao { get; set; } = string.Empty;
        public ProjectTableSortDirection Direcao { get; set; } = ProjectTableSortDirection.Crescente;
    }

    public enum ProjectTableTextAlignment
    {
        Esquerda,
        Centro,
        Direita
    }

    public class ProjectTableDisplaySettings
    {
        public const string DefaultFontFamily = "Arial";
        public const string DefaultTitleTextColor = "#FFFFFFFF";
        public const string DefaultTitleBackgroundColor = "#FF2F363D";
        public const string DefaultHeaderTextColor = "#FF1C252C";
        public const string DefaultHeaderBackgroundColor = "#FFDDE5EA";
        public const string DefaultBodyTextColor = "#FF1F2933";
        public const string DefaultBodyBackgroundColor = "#FFFFFFFF";
        public const string DefaultAlternateRowBackgroundColor = "#FFF3F6F8";
        public const string DefaultGridColor = "#FFE0E6EA";
        public const string DefaultOutlineColor = "#FF303030";
        public const double MinFontSize = 4.0;
        public const double MaxFontSize = 72.0;
        public const double MinRowHeight = 8.0;
        public const double MaxRowHeight = 120.0;
        public const double MinThickness = 0.0;
        public const double MaxThickness = 10.0;

        public bool ExibirTitulo { get; set; } = true;
        public string FonteTitulo { get; set; } = DefaultFontFamily;
        public double TamanhoFonteTitulo { get; set; } = 11.0;
        public bool TituloNegrito { get; set; } = true;
        public string CorTextoTitulo { get; set; } = DefaultTitleTextColor;
        public string CorFundoTitulo { get; set; } = DefaultTitleBackgroundColor;
        public double AlturaTitulo { get; set; } = 32.0;
        public ProjectTableTextAlignment AlinhamentoTitulo { get; set; } = ProjectTableTextAlignment.Esquerda;

        public bool ExibirCabecalho { get; set; } = true;
        public string FonteCabecalho { get; set; } = DefaultFontFamily;
        public double TamanhoFonteCabecalho { get; set; } = 10.0;
        public bool CabecalhoNegrito { get; set; } = true;
        public string CorTextoCabecalho { get; set; } = DefaultHeaderTextColor;
        public string CorFundoCabecalho { get; set; } = DefaultHeaderBackgroundColor;
        public double AlturaCabecalho { get; set; } = 26.0;
        public ProjectTableTextAlignment AlinhamentoCabecalho { get; set; } = ProjectTableTextAlignment.Esquerda;

        public string FonteCorpo { get; set; } = DefaultFontFamily;
        public double TamanhoFonteCorpo { get; set; } = 10.5;
        public string CorTextoCorpo { get; set; } = DefaultBodyTextColor;
        public string CorFundoCorpo { get; set; } = DefaultBodyBackgroundColor;
        public double AlturaLinhaCorpo { get; set; } = 24.0;
        public ProjectTableTextAlignment AlinhamentoCorpo { get; set; } = ProjectTableTextAlignment.Esquerda;
        public bool UsarLinhasAlternadas { get; set; }
        public string CorLinhaAlternada { get; set; } = DefaultAlternateRowBackgroundColor;

        public bool ExibirLinhasGrade { get; set; } = true;
        public string CorGrade { get; set; } = DefaultGridColor;
        public double EspessuraGrade { get; set; } = 1.0;
        public bool ExibirContornoExterno { get; set; } = true;
        public string CorContorno { get; set; } = DefaultOutlineColor;
        public double EspessuraContorno { get; set; } = 1.0;

        public ProjectTableDisplaySettings CriarCopia()
        {
            return new ProjectTableDisplaySettings
            {
                ExibirTitulo = ExibirTitulo,
                FonteTitulo = FonteTitulo,
                TamanhoFonteTitulo = TamanhoFonteTitulo,
                TituloNegrito = TituloNegrito,
                CorTextoTitulo = CorTextoTitulo,
                CorFundoTitulo = CorFundoTitulo,
                AlturaTitulo = AlturaTitulo,
                AlinhamentoTitulo = AlinhamentoTitulo,
                ExibirCabecalho = ExibirCabecalho,
                FonteCabecalho = FonteCabecalho,
                TamanhoFonteCabecalho = TamanhoFonteCabecalho,
                CabecalhoNegrito = CabecalhoNegrito,
                CorTextoCabecalho = CorTextoCabecalho,
                CorFundoCabecalho = CorFundoCabecalho,
                AlturaCabecalho = AlturaCabecalho,
                AlinhamentoCabecalho = AlinhamentoCabecalho,
                FonteCorpo = FonteCorpo,
                TamanhoFonteCorpo = TamanhoFonteCorpo,
                CorTextoCorpo = CorTextoCorpo,
                CorFundoCorpo = CorFundoCorpo,
                AlturaLinhaCorpo = AlturaLinhaCorpo,
                AlinhamentoCorpo = AlinhamentoCorpo,
                UsarLinhasAlternadas = UsarLinhasAlternadas,
                CorLinhaAlternada = CorLinhaAlternada,
                ExibirLinhasGrade = ExibirLinhasGrade,
                CorGrade = CorGrade,
                EspessuraGrade = EspessuraGrade,
                ExibirContornoExterno = ExibirContornoExterno,
                CorContorno = CorContorno,
                EspessuraContorno = EspessuraContorno
            };
        }
    }

    public class ProjectTable
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Nome { get; set; } = string.Empty;
        public ProjectViewDiscipline Disciplina { get; set; } = ProjectViewDiscipline.Eletrica;
        public List<ProjectTableElementCategory> CategoriasElementos { get; set; } = new();
        public List<ProjectTableFieldSelection> CamposSelecionados { get; set; } = new();
        public Guid? FiltroVistaId { get; set; }
        public ProjectTableFilterLogicalMode ModoFiltro { get; set; } = ProjectTableFilterLogicalMode.Todas;
        public List<ProjectTableFilterRule> Filtros { get; set; } = new();
        public List<ProjectTableSorting> Ordenacoes { get; set; } = new();
        public ProjectTableDisplaySettings Exibicao { get; set; } = new();
    }
}