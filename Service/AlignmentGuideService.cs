using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Araci.ViewModels;

namespace Araci.Services
{
    public class AlignmentGuideService
    {
        private const double Tolerancia = 4;
        private const double Margem = 80;
        private readonly EditorContext _context;

        public AlignmentGuideService(EditorContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public ObservableCollection<AlignmentGuideLineViewModel> Linhas { get; } = new();

        public Vector AplicarSnap(IEnumerable<ElementoViewModel> selecionados, Vector deltaPretendido)
        {
            Linhas.Clear();
            var selecionadosList = selecionados.Distinct().Where(e => !e.IsPreview && !e.Bounds.IsEmpty).ToList();

            if (selecionadosList.Count == 0)
                return deltaPretendido;

            var referencias = ObterReferencias(selecionadosList);

            if (referencias.Count == 0)
                return deltaPretendido;

            Rect boundsAtual = CalcularBounds(selecionadosList);
            Rect boundsPretendido = Deslocar(boundsAtual, deltaPretendido);
            Vector ajuste = CalcularAjusteETracejado(boundsPretendido, referencias, out _, out _);
            return deltaPretendido + ajuste;
        }

        public Vector AplicarSnapPreview(ElementoViewModel preview)
        {
            Linhas.Clear();

            if (preview == null || preview.Bounds.IsEmpty)
                return default;

            var referencias = _context.Scene.Elementos
                .Where(e => !ReferenceEquals(e, preview) && !e.IsPreview && !e.Bounds.IsEmpty)
                .ToList();

            if (referencias.Count == 0)
                return default;

            return CalcularAjusteETracejado(preview.Bounds, referencias, out _, out _);
        }

        public Point AplicarSnapPontoCabo(Point ponto, Point origem, ElementoViewModel? ignorar = null)
        {
            Linhas.Clear();
            var referencias = ObterReferenciasParaPonto(ignorar);
            AlignmentVertical? vertical = EncontrarMelhorVerticalPonto(referencias, ponto.X);
            AlignmentHorizontal? horizontal = EncontrarMelhorHorizontalPonto(referencias, ponto.Y);
            Point ajustado = ponto;

            if (Math.Abs(origem.X - ponto.X) <= Tolerancia)
                vertical = EscolherMelhorVertical(vertical, new AlignmentVertical(origem.X, origem.X - ponto.X, Math.Abs(origem.X - ponto.X), CriarBoundsPonto(origem)));

            if (Math.Abs(origem.Y - ponto.Y) <= Tolerancia)
                horizontal = EscolherMelhorHorizontal(horizontal, new AlignmentHorizontal(origem.Y, origem.Y - ponto.Y, Math.Abs(origem.Y - ponto.Y), CriarBoundsPonto(origem)));

            if (vertical.HasValue)
                ajustado.X += vertical.Value.Ajuste;

            if (horizontal.HasValue)
                ajustado.Y += horizontal.Value.Ajuste;

            if (vertical.HasValue)
                AdicionarVertical(vertical.Value.X, CriarBoundsSegmento(origem, ajustado), vertical.Value.Bounds);

            if (horizontal.HasValue)
                AdicionarHorizontal(horizontal.Value.Y, CriarBoundsSegmento(origem, ajustado), horizontal.Value.Bounds);

            return ajustado;
        }

        public void MostrarReferenciaVertical(double x, Rect boundsReferencia)
        {
            Linhas.Clear();
            AdicionarVertical(x, boundsReferencia, boundsReferencia);
        }

        public void MostrarReferenciaHorizontal(double y, Rect boundsReferencia)
        {
            Linhas.Clear();
            AdicionarHorizontal(y, boundsReferencia, boundsReferencia);
        }

        public void MostrarAlinhamentoVertical(double x, Rect boundsReferencia, Rect boundsAlvo)
        {
            Linhas.Clear();
            AdicionarVertical(x, boundsAlvo, boundsReferencia);
        }

        public void MostrarAlinhamentoHorizontal(double y, Rect boundsReferencia, Rect boundsAlvo)
        {
            Linhas.Clear();
            AdicionarHorizontal(y, boundsAlvo, boundsReferencia);
        }

        public void MostrarDuasReferenciasVerticais(double referenceX, Rect referenceBounds, double targetX, Rect targetBounds, Rect targetFinalBounds)
        {
            Linhas.Clear();
            AdicionarVertical(referenceX, referenceBounds, targetFinalBounds);
            AdicionarVertical(targetX, targetBounds, targetBounds);
        }

        public void MostrarDuasReferenciasHorizontais(double referenceY, Rect referenceBounds, double targetY, Rect targetBounds, Rect targetFinalBounds)
        {
            Linhas.Clear();
            AdicionarHorizontal(referenceY, referenceBounds, targetFinalBounds);
            AdicionarHorizontal(targetY, targetBounds, targetBounds);
        }

        public void Atualizar(IEnumerable<ElementoViewModel> selecionados)
        {
            Linhas.Clear();
            var selecionadosList = selecionados.Distinct().Where(e => !e.IsPreview && !e.Bounds.IsEmpty).ToList();

            if (selecionadosList.Count == 0)
                return;

            var referencias = ObterReferencias(selecionadosList);

            if (referencias.Count == 0)
                return;

            Rect boundsSelecionados = CalcularBounds(selecionadosList);
            CalcularAjusteETracejado(boundsSelecionados, referencias, out _, out _);
        }

        public void Limpar()
        {
            Linhas.Clear();
        }

        private Vector CalcularAjusteETracejado(Rect boundsAlvo, IReadOnlyList<ElementoViewModel> referencias, out AlignmentVertical? vertical, out AlignmentHorizontal? horizontal)
        {
            vertical = EncontrarMelhorVertical(referencias, boundsAlvo);
            horizontal = EncontrarMelhorHorizontal(referencias, boundsAlvo);
            Vector ajuste = default;
            Rect boundsAjustado = boundsAlvo;

            if (vertical.HasValue)
            {
                ajuste.X = vertical.Value.Ajuste;
                boundsAjustado = Deslocar(boundsAjustado, new Vector(vertical.Value.Ajuste, 0));
                AdicionarVertical(vertical.Value.X, boundsAjustado, vertical.Value.Bounds);
            }

            if (horizontal.HasValue)
            {
                ajuste.Y = horizontal.Value.Ajuste;
                boundsAjustado = Deslocar(boundsAjustado, new Vector(0, horizontal.Value.Ajuste));
                AdicionarHorizontal(horizontal.Value.Y, boundsAjustado, horizontal.Value.Bounds);
            }

            return ajuste;
        }

        private IReadOnlyList<ElementoViewModel> ObterReferencias(IReadOnlyList<ElementoViewModel> ignorar)
        {
            return _context.Scene.Elementos
                .Where(e => !e.IsPreview && !ignorar.Contains(e) && !e.Bounds.IsEmpty)
                .ToList();
        }

        private IReadOnlyList<ElementoViewModel> ObterReferenciasParaPonto(ElementoViewModel? ignorar)
        {
            return _context.Scene.Elementos
                .Where(e => !e.IsPreview && !ReferenceEquals(e, ignorar) && !e.Bounds.IsEmpty)
                .ToList();
        }

        private static AlignmentVertical? EncontrarMelhorVertical(IEnumerable<ElementoViewModel> referencias, Rect boundsSelecionados)
        {
            AlignmentVertical? melhor = null;
            double left = boundsSelecionados.Left;
            double centerX = boundsSelecionados.Left + boundsSelecionados.Width / 2;
            double right = boundsSelecionados.Right;

            foreach (ElementoViewModel vm in referencias)
            {
                Rect b = vm.Bounds;
                TestarVertical(ref melhor, left, b.Left, b);
                TestarVertical(ref melhor, left, b.Left + b.Width / 2, b);
                TestarVertical(ref melhor, left, b.Right, b);
                TestarVertical(ref melhor, centerX, b.Left, b);
                TestarVertical(ref melhor, centerX, b.Left + b.Width / 2, b);
                TestarVertical(ref melhor, centerX, b.Right, b);
                TestarVertical(ref melhor, right, b.Left, b);
                TestarVertical(ref melhor, right, b.Left + b.Width / 2, b);
                TestarVertical(ref melhor, right, b.Right, b);
            }

            return melhor;
        }

        private static AlignmentHorizontal? EncontrarMelhorHorizontal(IEnumerable<ElementoViewModel> referencias, Rect boundsSelecionados)
        {
            AlignmentHorizontal? melhor = null;
            double top = boundsSelecionados.Top;
            double centerY = boundsSelecionados.Top + boundsSelecionados.Height / 2;
            double bottom = boundsSelecionados.Bottom;

            foreach (ElementoViewModel vm in referencias)
            {
                Rect b = vm.Bounds;
                TestarHorizontal(ref melhor, top, b.Top, b);
                TestarHorizontal(ref melhor, top, b.Top + b.Height / 2, b);
                TestarHorizontal(ref melhor, top, b.Bottom, b);
                TestarHorizontal(ref melhor, centerY, b.Top, b);
                TestarHorizontal(ref melhor, centerY, b.Top + b.Height / 2, b);
                TestarHorizontal(ref melhor, centerY, b.Bottom, b);
                TestarHorizontal(ref melhor, bottom, b.Top, b);
                TestarHorizontal(ref melhor, bottom, b.Top + b.Height / 2, b);
                TestarHorizontal(ref melhor, bottom, b.Bottom, b);
            }

            return melhor;
        }

        private static AlignmentVertical? EncontrarMelhorVerticalPonto(IEnumerable<ElementoViewModel> referencias, double x)
        {
            AlignmentVertical? melhor = null;

            foreach (ElementoViewModel vm in referencias)
            {
                Rect b = vm.Bounds;
                TestarVertical(ref melhor, x, b.Left, b);
                TestarVertical(ref melhor, x, b.Left + b.Width / 2, b);
                TestarVertical(ref melhor, x, b.Right, b);
            }

            return melhor;
        }

        private static AlignmentHorizontal? EncontrarMelhorHorizontalPonto(IEnumerable<ElementoViewModel> referencias, double y)
        {
            AlignmentHorizontal? melhor = null;

            foreach (ElementoViewModel vm in referencias)
            {
                Rect b = vm.Bounds;
                TestarHorizontal(ref melhor, y, b.Top, b);
                TestarHorizontal(ref melhor, y, b.Top + b.Height / 2, b);
                TestarHorizontal(ref melhor, y, b.Bottom, b);
            }

            return melhor;
        }

        private static void TestarVertical(ref AlignmentVertical? melhor, double valorSelecionado, double valorReferencia, Rect boundsReferencia)
        {
            double ajuste = valorReferencia - valorSelecionado;
            double distancia = Math.Abs(ajuste);

            if (distancia > Tolerancia)
                return;

            if (!melhor.HasValue || distancia < melhor.Value.Distancia)
                melhor = new AlignmentVertical(valorReferencia, ajuste, distancia, boundsReferencia);
        }

        private static void TestarHorizontal(ref AlignmentHorizontal? melhor, double valorSelecionado, double valorReferencia, Rect boundsReferencia)
        {
            double ajuste = valorReferencia - valorSelecionado;
            double distancia = Math.Abs(ajuste);

            if (distancia > Tolerancia)
                return;

            if (!melhor.HasValue || distancia < melhor.Value.Distancia)
                melhor = new AlignmentHorizontal(valorReferencia, ajuste, distancia, boundsReferencia);
        }

        private static AlignmentVertical EscolherMelhorVertical(AlignmentVertical? atual, AlignmentVertical candidato)
        {
            return !atual.HasValue || candidato.Distancia < atual.Value.Distancia ? candidato : atual.Value;
        }

        private static AlignmentHorizontal EscolherMelhorHorizontal(AlignmentHorizontal? atual, AlignmentHorizontal candidato)
        {
            return !atual.HasValue || candidato.Distancia < atual.Value.Distancia ? candidato : atual.Value;
        }

        private void AdicionarVertical(double x, Rect boundsSelecionados, Rect boundsReferencia)
        {
            double y1 = Math.Min(boundsSelecionados.Top, boundsReferencia.Top) - Margem;
            double y2 = Math.Max(boundsSelecionados.Bottom, boundsReferencia.Bottom) + Margem;
            Linhas.Add(new AlignmentGuideLineViewModel { X1 = x, Y1 = y1, X2 = x, Y2 = y2 });
        }

        private void AdicionarHorizontal(double y, Rect boundsSelecionados, Rect boundsReferencia)
        {
            double x1 = Math.Min(boundsSelecionados.Left, boundsReferencia.Left) - Margem;
            double x2 = Math.Max(boundsSelecionados.Right, boundsReferencia.Right) + Margem;
            Linhas.Add(new AlignmentGuideLineViewModel { X1 = x1, Y1 = y, X2 = x2, Y2 = y });
        }

        private static Rect CalcularBounds(IReadOnlyList<ElementoViewModel> items)
        {
            Rect total = items[0].Bounds;

            for (int i = 1; i < items.Count; i++)
                total.Union(items[i].Bounds);

            return total;
        }

        private static Rect Deslocar(Rect rect, Vector delta)
        {
            return new Rect(rect.X + delta.X, rect.Y + delta.Y, rect.Width, rect.Height);
        }

        private static Rect CriarBoundsPonto(Point ponto)
        {
            return new Rect(ponto.X, ponto.Y, 1, 1);
        }

        private static Rect CriarBoundsSegmento(Point a, Point b)
        {
            double x = Math.Min(a.X, b.X);
            double y = Math.Min(a.Y, b.Y);
            double largura = Math.Max(1, Math.Abs(a.X - b.X));
            double altura = Math.Max(1, Math.Abs(a.Y - b.Y));
            return new Rect(x, y, largura, altura);
        }

        private readonly record struct AlignmentVertical(double X, double Ajuste, double Distancia, Rect Bounds);
        private readonly record struct AlignmentHorizontal(double Y, double Ajuste, double Distancia, Rect Bounds);
    }
}