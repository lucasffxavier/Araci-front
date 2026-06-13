using System;
using System.Collections.Generic;

namespace Araci.Core.Documents
{
    public enum ProjectSheetFormat
    {
        A4,
        A3,
        A2,
        A1,
        A0,
        Personalizado
    }

    public enum ProjectSheetOrientation
    {
        Paisagem,
        Retrato
    }

    public class ProjectSheet
    {
        public const string UnitLabel = "mm";
        public const double DefaultWidth = 841.0;
        public const double DefaultHeight = 594.0;
        public const double MinDimension = 20.0;

        public const double LegacyDefaultWidth = 1122.0;
        public const double LegacyDefaultHeight = 794.0;

        private const double DimensionComparisonTolerance = 0.5;

        private double _larguraFolha = DefaultWidth;
        private double _alturaFolha = DefaultHeight;

        public Guid Id { get; set; } = Guid.NewGuid();
        public string Nome { get; set; } = string.Empty;
        public string Numero { get; set; } = string.Empty;
        public Guid? SheetTypeId { get; set; }
        public ProjectSheetFormat FormatoFolha { get; set; } = ProjectSheetFormat.A1;
        public ProjectSheetOrientation OrientacaoFolha { get; set; } = ProjectSheetOrientation.Paisagem;

        public double LarguraFolha
        {
            get => _larguraFolha;
            set => _larguraFolha = NormalizarDimensao(value, DefaultWidth);
        }

        public double AlturaFolha
        {
            get => _alturaFolha;
            set => _alturaFolha = NormalizarDimensao(value, DefaultHeight);
        }

        public List<ProjectSheetTableInstance> Tabelas { get; set; } = new();
        public List<ProjectSheetViewInstance> Vistas { get; set; } = new();

        public void AplicarFormato(ProjectSheetFormat formato, ProjectSheetOrientation orientacao)
        {
            FormatoFolha = formato;
            OrientacaoFolha = orientacao;

            if (formato == ProjectSheetFormat.Personalizado)
                return;

            (double largura, double altura) = ObterDimensoesFormato(formato, orientacao);
            LarguraFolha = largura;
            AlturaFolha = altura;
        }

        public static (double Largura, double Altura) ObterDimensoesFormato(ProjectSheetFormat formato, ProjectSheetOrientation orientacao)
        {
            (double larguraPaisagem, double alturaPaisagem) = formato switch
            {
                ProjectSheetFormat.A0 => (1189.0, 841.0),
                ProjectSheetFormat.A1 => (841.0, 594.0),
                ProjectSheetFormat.A2 => (594.0, 420.0),
                ProjectSheetFormat.A3 => (420.0, 297.0),
                ProjectSheetFormat.A4 => (297.0, 210.0),
                _ => (DefaultWidth, DefaultHeight)
            };

            return orientacao == ProjectSheetOrientation.Retrato
                ? (alturaPaisagem, larguraPaisagem)
                : (larguraPaisagem, alturaPaisagem);
        }

        public static (double Largura, double Altura) ObterDimensoesLegadasFormato(ProjectSheetFormat formato, ProjectSheetOrientation orientacao)
        {
            (double larguraPaisagem, double alturaPaisagem) = formato switch
            {
                ProjectSheetFormat.A0 => (1587.0, 1122.0),
                ProjectSheetFormat.A1 => (1122.0, 794.0),
                ProjectSheetFormat.A2 => (794.0, 561.0),
                ProjectSheetFormat.A3 => (561.0, 397.0),
                ProjectSheetFormat.A4 => (397.0, 280.0),
                _ => (LegacyDefaultWidth, LegacyDefaultHeight)
            };

            return orientacao == ProjectSheetOrientation.Retrato
                ? (alturaPaisagem, larguraPaisagem)
                : (larguraPaisagem, alturaPaisagem);
        }

        public static bool DimensoesEquivalentes(double larguraA, double alturaA, double larguraB, double alturaB)
        {
            return Math.Abs(larguraA - larguraB) <= DimensionComparisonTolerance &&
                Math.Abs(alturaA - alturaB) <= DimensionComparisonTolerance;
        }

        private static double NormalizarDimensao(double valor, double fallback)
        {
            return double.IsNaN(valor) || double.IsInfinity(valor) || valor < MinDimension
                ? fallback
                : valor;
        }
    }
}