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

            var referencias = _context.Scene.Elementos
                .Where(e => !e.IsPreview && !selecionadosList.Contains(e) && !e.Bounds.IsEmpty)
                .ToList();

            if (referencias.Count == 0)
                return deltaPretendido;

            Rect boundsAtual = CalcularBounds(selecionadosList);
            Rect boundsPretendido = Deslocar(boundsAtual, deltaPretendido);
            AlignmentVertical? vertical = EncontrarMelhorVertical(referencias, boundsPretendido);
            AlignmentHorizontal? horizontal = EncontrarMelhorHorizontal(referencias, boundsPretendido);
            Vector deltaFinal = deltaPretendido;

            if (vertical.HasValue)
            {
                deltaFinal.X += vertical.Value.Ajuste;
                Rect ajustado = Deslocar(boundsAtual, deltaFinal);
                AdicionarVertical(vertical.Value.X, ajustado, vertical.Value.Bounds);
            }

            if (horizontal.HasValue)
            {
                deltaFinal.Y += horizontal.Value.Ajuste;
                Rect ajustado = Deslocar(boundsAtual, deltaFinal);
                AdicionarHorizontal(horizontal.Value.Y, ajustado, horizontal.Value.Bounds);
            }

            return deltaFinal;
        }

        public void Atualizar(IEnumerable<ElementoViewModel> selecionados)
        {
            Linhas.Clear();
            var selecionadosList = selecionados.Distinct().Where(e => !e.IsPreview && !e.Bounds.IsEmpty).ToList();

            if (selecionadosList.Count == 0)
                return;

            var referencias = _context.Scene.Elementos
                .Where(e => !e.IsPreview && !selecionadosList.Contains(e) && !e.Bounds.IsEmpty)
                .ToList();

            if (referencias.Count == 0)
                return;

            Rect boundsSelecionados = CalcularBounds(selecionadosList);
            AlignmentVertical? vertical = EncontrarMelhorVertical(referencias, boundsSelecionados);
            AlignmentHorizontal? horizontal = EncontrarMelhorHorizontal(referencias, boundsSelecionados);

            if (vertical.HasValue)
                AdicionarVertical(vertical.Value.X, boundsSelecionados, vertical.Value.Bounds);

            if (horizontal.HasValue)
                AdicionarHorizontal(horizontal.Value.Y, boundsSelecionados, horizontal.Value.Bounds);
        }

        public void Limpar()
        {
            Linhas.Clear();
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

        private void AdicionarVertical(double x, Rect boundsSelecionados, Rect boundsReferencia)
        {
            double y1 = Math.Min(boundsSelecionados.Top, boundsReferencia.Top) - Margem;
            double y2 = Math.Max(boundsSelecionados.Bottom, boundsReferencia.Bottom) + Margem;

            Linhas.Add(new AlignmentGuideLineViewModel
            {
                X1 = x,
                Y1 = y1,
                X2 = x,
                Y2 = y2
            });
        }

        private void AdicionarHorizontal(double y, Rect boundsSelecionados, Rect boundsReferencia)
        {
            double x1 = Math.Min(boundsSelecionados.Left, boundsReferencia.Left) - Margem;
            double x2 = Math.Max(boundsSelecionados.Right, boundsReferencia.Right) + Margem;

            Linhas.Add(new AlignmentGuideLineViewModel
            {
                X1 = x1,
                Y1 = y,
                X2 = x2,
                Y2 = y
            });
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

        private readonly record struct AlignmentVertical(double X, double Ajuste, double Distancia, Rect Bounds);
        private readonly record struct AlignmentHorizontal(double Y, double Ajuste, double Distancia, Rect Bounds);
    }
}