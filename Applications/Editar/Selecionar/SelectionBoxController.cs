using System;
using System.Linq;
using System.Windows;
using Araci.Core.SceneQueries;
using Araci.Models;
using Araci.Services;
using Araci.ViewModels;
using Araci.Services.Editing;

namespace Araci.Applications.Editar.Selecionar
{
    public class SelectionBoxController
    {
        private const double ToleranciaBarra = 6;
        private const double ToleranciaRetangulo = 6;
        private readonly SelectionBoxViewModel _selectionBox;
        private readonly ISceneQueryService _queries;
        private readonly SelectionService _selection;
        private Point _inicio;
        private bool _adicionarAoExistente;

        public SelectionBoxController(SelectionBoxViewModel selectionBox, ISceneQueryService queries, SelectionService selection)
        {
            _selectionBox = selectionBox ?? throw new ArgumentNullException(nameof(selectionBox));
            _queries = queries ?? throw new ArgumentNullException(nameof(queries));
            _selection = selection ?? throw new ArgumentNullException(nameof(selection));
        }

        public bool IsActive { get; private set; }

        public void Begin(Point position, bool adicionarAoExistente)
        {
            _adicionarAoExistente = adicionarAoExistente;

            if (!_adicionarAoExistente)
                _selection.Limpar();

            _inicio = position;
            IsActive = true;
            _selectionBox.Visivel = true;
            _selectionBox.Atualizar(position, position);
        }

        public void Update(Point position)
        {
            if (!IsActive)
                return;

            _selectionBox.Atualizar(_inicio, position);
        }

        public void End()
        {
            if (!IsActive)
                return;

            Rect area = _selectionBox.Bounds;

            foreach (ElementoViewModel item in _queries.Query(area))
            {
                if (IntersectaSelecao(item, area))
                    _selection.Selecionar(item, true);
            }

            Cancel();
        }

        public void Cancel()
        {
            IsActive = false;
            _selectionBox.Visivel = false;
        }

        private static bool IntersectaSelecao(ElementoViewModel vm, Rect area)
        {
            if (vm is CaboViewModel cabo)
                return IntersectaCabo(cabo, area);

            if (vm is LinhaAnotativaViewModel linha)
                return IntersectaLinha(linha, area);

            if (vm is RetanguloAnotativoViewModel retangulo)
                return IntersectaRetangulo(retangulo, area);

            if (vm.Modelo is Barra barra)
                return IntersectaBarra(barra, area);

            return vm.Bounds.IntersectsWith(area);
        }

        private static bool IntersectaLinha(LinhaAnotativaViewModel linha, Rect area)
        {
            return SegmentoIntersectaRect(linha.PontoInicial, linha.PontoFinal, area);
        }

        private static bool IntersectaRetangulo(RetanguloAnotativoViewModel retangulo, Rect area)
        {
            Rect b = retangulo.Bounds;
            Rect areaComTolerancia = Expandir(area, ToleranciaRetangulo);

            Point topLeft = new(b.Left, b.Top);
            Point topRight = new(b.Right, b.Top);
            Point bottomRight = new(b.Right, b.Bottom);
            Point bottomLeft = new(b.Left, b.Bottom);

            return SegmentoIntersectaRect(topLeft, topRight, areaComTolerancia) ||
                SegmentoIntersectaRect(topRight, bottomRight, areaComTolerancia) ||
                SegmentoIntersectaRect(bottomRight, bottomLeft, areaComTolerancia) ||
                SegmentoIntersectaRect(bottomLeft, topLeft, areaComTolerancia);
        }

        private static bool IntersectaBarra(Barra barra, Rect area)
        {
            Terminal[] terminais = barra.Terminais.ToArray();

            if (terminais.Length == 0)
                return false;

            Rect areaComTolerancia = Expandir(area, ToleranciaBarra);

            foreach (Terminal terminal in terminais)
            {
                if (areaComTolerancia.Contains(terminal.Posicao))
                    return true;
            }

            if (terminais.Length == 1)
                return false;

            Point a = terminais[0].Posicao;
            Point b = terminais[1].Posicao;
            double maiorDistancia = DistanciaQuadrada(a, b);

            for (int i = 0; i < terminais.Length; i++)
            {
                for (int j = i + 1; j < terminais.Length; j++)
                {
                    Point p1 = terminais[i].Posicao;
                    Point p2 = terminais[j].Posicao;
                    double distancia = DistanciaQuadrada(p1, p2);

                    if (distancia > maiorDistancia)
                    {
                        maiorDistancia = distancia;
                        a = p1;
                        b = p2;
                    }
                }
            }

            return SegmentoIntersectaRect(a, b, areaComTolerancia);
        }

        private static bool IntersectaCabo(CaboViewModel cabo, Rect area)
        {
            var vertices = cabo.Cabo.Vertices;

            if (vertices.Count < 2)
                return cabo.Bounds.IntersectsWith(area);

            for (int i = 0; i < vertices.Count; i++)
            {
                if (area.Contains(vertices[i]))
                    return true;
            }

            for (int i = 0; i < vertices.Count - 1; i++)
            {
                if (SegmentoIntersectaRect(vertices[i], vertices[i + 1], area))
                    return true;
            }

            return false;
        }

        private static Rect Expandir(Rect rect, double margem)
        {
            return new Rect(
                rect.Left - margem,
                rect.Top - margem,
                rect.Width + margem * 2,
                rect.Height + margem * 2);
        }

        private static double DistanciaQuadrada(Point a, Point b)
        {
            double dx = a.X - b.X;
            double dy = a.Y - b.Y;
            return dx * dx + dy * dy;
        }

        private static bool SegmentoIntersectaRect(Point a, Point b, Rect rect)
        {
            if (rect.Contains(a) || rect.Contains(b))
                return true;

            Point topLeft = new(rect.Left, rect.Top);
            Point topRight = new(rect.Right, rect.Top);
            Point bottomRight = new(rect.Right, rect.Bottom);
            Point bottomLeft = new(rect.Left, rect.Bottom);

            return SegmentosIntersectam(a, b, topLeft, topRight) ||
                SegmentosIntersectam(a, b, topRight, bottomRight) ||
                SegmentosIntersectam(a, b, bottomRight, bottomLeft) ||
                SegmentosIntersectam(a, b, bottomLeft, topLeft);
        }

        private static bool SegmentosIntersectam(Point a, Point b, Point c, Point d)
        {
            double o1 = Orientacao(a, b, c);
            double o2 = Orientacao(a, b, d);
            double o3 = Orientacao(c, d, a);
            double o4 = Orientacao(c, d, b);

            if (SinaisOpostos(o1, o2) && SinaisOpostos(o3, o4))
                return true;

            return EhZero(o1) && EstaNoSegmento(a, c, b) ||
                EhZero(o2) && EstaNoSegmento(a, d, b) ||
                EhZero(o3) && EstaNoSegmento(c, a, d) ||
                EhZero(o4) && EstaNoSegmento(c, b, d);
        }

        private static double Orientacao(Point a, Point b, Point c)
        {
            return (b.X - a.X) * (c.Y - a.Y) -
                (b.Y - a.Y) * (c.X - a.X);
        }

        private static bool SinaisOpostos(double a, double b)
        {
            return a > 0 && b < 0 || a < 0 && b > 0;
        }

        private static bool EhZero(double value)
        {
            return Math.Abs(value) < 0.000001;
        }

        private static bool EstaNoSegmento(Point a, Point p, Point b)
        {
            return p.X >= Math.Min(a.X, b.X) - 0.000001 &&
                p.X <= Math.Max(a.X, b.X) + 0.000001 &&
                p.Y >= Math.Min(a.Y, b.Y) - 0.000001 &&
                p.Y <= Math.Max(a.Y, b.Y) + 0.000001;
        }
    }
}