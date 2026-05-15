using System.Windows;
using System.Windows.Media;

namespace Araci.Core.Rendering
{
    public class ElementoRenderData
    {
        public ElementoRenderData(
            double largura,
            double altura,
            Point pontoLocalInicial,
            Point pontoLocalFinal,
            Brush stroke,
            double strokeThickness)
        {
            Largura = largura;
            Altura = altura;
            PontoLocalInicial = pontoLocalInicial;
            PontoLocalFinal = pontoLocalFinal;
            Stroke = stroke;
            StrokeThickness = strokeThickness;
        }

        public double Largura
        { get; }

        public double Altura
        { get; }

        public Point PontoLocalInicial
        { get; }

        public Point PontoLocalFinal
        { get; }

        public Brush Stroke
        { get; }

        public double StrokeThickness
        { get; }
    }
}
