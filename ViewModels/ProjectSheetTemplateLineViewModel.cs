using System;
using System.Windows.Media;
using Araci.Core.Documents;

namespace Araci.ViewModels
{
    public sealed class ProjectSheetTemplateLineViewModel
    {
        private readonly ProjectSheetTemplateLine _linha;

        public ProjectSheetTemplateLineViewModel(ProjectSheetTemplateLine linha)
        {
            _linha = linha ?? throw new ArgumentNullException(nameof(linha));
        }

        public Guid Id => _linha.Id;
        public double X1 => _linha.X1;
        public double Y1 => _linha.Y1;
        public double X2 => _linha.X2;
        public double Y2 => _linha.Y2;
        public Brush StrokeBrush => CriarBrush(_linha.Stroke);
        public double StrokeThickness => _linha.StrokeThickness;
        public bool Visible => _linha.Visible;

        private static Brush CriarBrush(string stroke)
        {
            try
            {
                if (ColorConverter.ConvertFromString(string.IsNullOrWhiteSpace(stroke) ? "#FF000000" : stroke) is Color color)
                    return new SolidColorBrush(color);
            }
            catch (FormatException)
            {
            }

            return Brushes.Black;
        }
    }
}
