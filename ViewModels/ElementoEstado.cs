using System.Collections.Generic;
using System.Windows;
using Araci.Models;

namespace Araci.ViewModels
{
    public class ElementoEstado
    {
        public double X { get; }
        public double Y { get; }
        public double X2 { get; }
        public double Y2 { get; }
        public double Rotacao { get; }
        public List<Point> Vertices { get; }

        public ElementoEstado(double x, double y, double x2 = 0, double y2 = 0, double rotacao = 0, IEnumerable<Point>? vertices = null)
        {
            X = x;
            Y = y;
            X2 = x2;
            Y2 = y2;
            Rotacao = rotacao;
            Vertices = vertices != null ? new List<Point>(vertices) : new List<Point>();
        }

        public void AplicarEm(Elemento elemento)
        {
            elemento.PosicaoX = X;
            elemento.PosicaoY = Y;
            elemento.Rotacao = Rotacao;

            if (elemento is ElementoLinear linear)
            {
                linear.PosicaoX2 = X2;
                linear.PosicaoY2 = Y2;
            }

            if (elemento is LinhaAnotativa linha)
            {
                linha.X2 = X2;
                linha.Y2 = Y2;
            }

            if (elemento is RetanguloAnotativo retangulo)
            {
                retangulo.Largura = X2;
                retangulo.Altura = Y2;
            }

            if (elemento is CirculoAnotativo circulo)
                circulo.Raio = X2;

            if (elemento is TextoAnotativo texto)
            {
                texto.PosicaoX = X;
                texto.PosicaoY = Y;

                if (X2 > 0)
                    texto.LarguraCaixa = X2;
            }

            if (elemento is not Cabo cabo)
                return;

            cabo.Vertices.Clear();

            foreach (var p in Vertices)
                cabo.Vertices.Add(p);

            if (cabo.Vertices.Count == 0)
                return;

            cabo.DefinirOrigem(cabo.Vertices[0]);
            cabo.DefinirDestino(cabo.Vertices[^1]);
        }
    }
}
