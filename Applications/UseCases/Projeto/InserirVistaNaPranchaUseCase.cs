using System;
using System.Linq;
using Araci.Applications.Abstractions;
using Araci.Core.Commands;
using Araci.Core.Documents;

namespace Araci.Applications.UseCases.Projeto
{
    public sealed class InserirVistaNaPranchaUseCase
    {
        public const double DefaultX = 40.0;
        public const double DefaultY = 40.0;
        public const double DefaultWidth = 260.0;
        public const double DefaultHeight = 180.0;
        public const double MaxInitialSheetWidthRatio = 0.55;
        public const double MaxInitialSheetHeightRatio = 0.55;

        private readonly AraciDocument _document;
        private readonly ICommandHistory _commands;

        public InserirVistaNaPranchaUseCase(AraciDocument document, ICommandHistory commands)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
        }

        public ProjectSheetViewInstance? Inserir(
            Guid sheetId,
            Guid viewId,
            double? x = null,
            double? y = null,
            double? width = null,
            double? height = null,
            Action? onChanged = null)
        {
            ProjectSheet? sheet = _document.Pranchas.FirstOrDefault(p => p.Id == sheetId);
            ProjectView? vista = _document.Vistas.FirstOrDefault(v => v.Id == viewId);

            if (sheet == null || vista == null)
                return null;

            (double larguraInicial, double alturaInicial) = CalcularTamanhoInicial(vista, sheet);

            var instance = new ProjectSheetViewInstance
            {
                ViewId = viewId,
                X = NormalizePosition(x ?? DefaultX),
                Y = NormalizePosition(y ?? DefaultY),
                Width = NormalizeDimension(width ?? larguraInicial, ProjectSheetViewInstance.MinWidth),
                Height = NormalizeDimension(height ?? alturaInicial, ProjectSheetViewInstance.MinHeight)
            };

            _commands.Execute(new AddProjectSheetViewInstanceCommand(sheet, instance, onChanged: onChanged));
            return instance;
        }

        public static double NormalizePosition(double value)
        {
            return double.IsNaN(value) || double.IsInfinity(value)
                ? 0.0
                : value;
        }

        public static double NormalizeDimension(double value, double minimum)
        {
            double safeMinimum = double.IsNaN(minimum) || double.IsInfinity(minimum) || minimum < 0
                ? 0
                : minimum;

            return double.IsNaN(value) || double.IsInfinity(value) || value < safeMinimum
                ? safeMinimum
                : value;
        }

        private static (double Largura, double Altura) CalcularTamanhoInicial(ProjectView vista, ProjectSheet sheet)
        {
            double recorteLargura = NormalizeDimension(vista.RecorteLargura, ProjectView.MinRecorteDimension);
            double recorteAltura = NormalizeDimension(vista.RecorteAltura, ProjectView.MinRecorteDimension);
            double limiteLargura = Math.Max(ProjectSheetViewInstance.MinWidth, sheet.LarguraFolha * MaxInitialSheetWidthRatio);
            double limiteAltura = Math.Max(ProjectSheetViewInstance.MinHeight, sheet.AlturaFolha * MaxInitialSheetHeightRatio);
            double fator = Math.Min(limiteLargura / recorteLargura, limiteAltura / recorteAltura);

            if (double.IsNaN(fator) || double.IsInfinity(fator) || fator <= 0)
                return (DefaultWidth, DefaultHeight);

            double largura = recorteLargura * fator;
            double altura = recorteAltura * fator;

            return (
                NormalizeDimension(largura, ProjectSheetViewInstance.MinWidth),
                NormalizeDimension(altura, ProjectSheetViewInstance.MinHeight));
        }
    }
}