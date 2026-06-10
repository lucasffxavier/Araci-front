using System;

namespace Araci.Core.Documents
{
    public class ProjectSheetTemplateLine
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public double X1 { get; set; }
        public double Y1 { get; set; }
        public double X2 { get; set; }
        public double Y2 { get; set; }
        public string Stroke { get; set; } = "#FF000000";
        public double StrokeThickness { get; set; } = 1.0;
        public bool Visible { get; set; } = true;

        public ProjectSheetTemplateLine CriarCopia(bool gerarNovoId)
        {
            return new ProjectSheetTemplateLine
            {
                Id = gerarNovoId ? Guid.NewGuid() : Id,
                X1 = X1,
                Y1 = Y1,
                X2 = X2,
                Y2 = Y2,
                Stroke = Stroke,
                StrokeThickness = StrokeThickness,
                Visible = Visible
            };
        }
    }
}
