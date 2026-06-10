using System;
using System.Collections.Generic;

namespace Araci.Core.Documents
{
    public class ProjectSheetType
    {
        public const string DefaultName = "A1 Paisagem - Padrao";

        private double _larguraFolha = ProjectSheet.DefaultWidth;
        private double _alturaFolha = ProjectSheet.DefaultHeight;

        public Guid Id { get; set; } = Guid.NewGuid();
        public string Nome { get; set; } = DefaultName;
        public ProjectSheetFormat FormatoFolha { get; set; } = ProjectSheetFormat.A1;
        public ProjectSheetOrientation OrientacaoFolha { get; set; } = ProjectSheetOrientation.Paisagem;
        public List<ProjectSheetTemplateLine> Linhas { get; set; } = new();
        public List<ProjectSheetTemplateRectangle> Retangulos { get; set; } = new();

        public double LarguraFolha
        {
            get => _larguraFolha;
            set => _larguraFolha = NormalizarDimensao(value, ProjectSheet.DefaultWidth);
        }

        public double AlturaFolha
        {
            get => _alturaFolha;
            set => _alturaFolha = NormalizarDimensao(value, ProjectSheet.DefaultHeight);
        }

        public static ProjectSheetType CriarPadrao()
        {
            return new ProjectSheetType();
        }

        private static double NormalizarDimensao(double valor, double fallback)
        {
            return double.IsNaN(valor) || double.IsInfinity(valor) || valor < ProjectSheet.MinDimension
                ? fallback
                : valor;
        }
    }
}