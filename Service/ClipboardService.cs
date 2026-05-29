using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Araci.Core.Commands;
using Araci.Core.Rendering;
using Araci.Models;
using Araci.ViewModels;

namespace Araci.Services
{
    public static class ClipboardService
    {
        private const double OffsetPadrao = 30;
        private static readonly List<Elemento> _copiados = new();

        public static void CopiarSelecionados(EditorContext context)
        {
            ArgumentNullException.ThrowIfNull(context);
            _copiados.Clear();
            foreach (ElementoViewModel vm in context.Selection.Selecionados)
            {
                Elemento clone = vm.Modelo.Clonar();
                _copiados.Add(clone);
            }
        }

        public static void Colar(EditorContext context)
        {
            ArgumentNullException.ThrowIfNull(context);
            if (_copiados.Count == 0)
                return;

            var colados = new List<Elemento>();
            Point destino = ObterDestinoColagem(context);
            Point centroCopiados = CalcularCentro(_copiados);
            Vector deslocamento = destino - centroCopiados;

            using var transaction = context.BeginTransaction();
            foreach (Elemento modeloCopiado in _copiados)
            {
                Elemento clone = modeloCopiado.Clonar();
                DeslocarElemento(clone, deslocamento.X, deslocamento.Y);
                LimparConexoesSeNecessario(clone);
                transaction.Add(new AddElementoCommand(clone, context));
                colados.Add(clone);
            }

            transaction.Commit();
            context.Selection.Limpar();

            foreach (Elemento elemento in colados)
            {
                ElementoViewModel? vm = context.Viewport?.ObterViewModel(elemento);
                if (vm != null)
                    context.Selection.Selecionar(vm, true);
            }

            context.SceneQueries.Invalidate();
            context.CableVertexEdit.Refresh();
        }

        private static Point ObterDestinoColagem(EditorContext context)
        {
            if (context.Input.PossuiUltimaPosicaoMouseMundo)
                return context.Input.UltimaPosicaoMouseMundo;

            if (context.Viewport != null)
                return context.Viewport.ScreenToWorld(context.Viewport.CentroTela);

            Point centro = CalcularCentro(_copiados);
            return new Point(centro.X + OffsetPadrao, centro.Y + OffsetPadrao);
        }

        private static Point CalcularCentro(IReadOnlyList<Elemento> elementos)
        {
            if (elementos.Count == 0)
                return new Point();

            Rect total = ObterBounds(elementos[0]);
            for (int i = 1; i < elementos.Count; i++)
                total.Union(ObterBounds(elementos[i]));

            return new Point(total.Left + total.Width / 2, total.Top + total.Height / 2);
        }

        private static Rect ObterBounds(Elemento elemento)
        {
            Rect bounds = elemento switch
            {
                Cabo cabo when cabo.Vertices.Count > 0 => ObterBoundsCabo(cabo),
                Barra barra => new Rect(barra.PosicaoX, barra.PosicaoY, ElementGeometryDefaults.BarraLargura, barra.Altura),
                Transformador transformador => new Rect(transformador.PosicaoX, transformador.PosicaoY, ElementGeometryDefaults.TransformadorLargura, ElementGeometryDefaults.TransformadorAltura),
                ElementoEquipamento equipamento => new Rect(equipamento.PosicaoX, equipamento.PosicaoY, ElementGeometryDefaults.EquipamentoLargura, ElementGeometryDefaults.EquipamentoAltura),
                ElementoLinear linear => new Rect(new Point(linear.PosicaoX, linear.PosicaoY), new Point(linear.PosicaoX2, linear.PosicaoY2)),
                _ => new Rect(elemento.PosicaoX, elemento.PosicaoY, ElementGeometryDefaults.EquipamentoLargura, ElementGeometryDefaults.EquipamentoAltura)
            };

            return Math.Abs(elemento.Rotacao) < 0.0001 ? bounds : ObterBoundsRotacionado(bounds, elemento.Rotacao);
        }

        private static Rect ObterBoundsCabo(Cabo cabo)
        {
            double minX = cabo.Vertices.Min(p => p.X);
            double minY = cabo.Vertices.Min(p => p.Y);
            double maxX = cabo.Vertices.Max(p => p.X);
            double maxY = cabo.Vertices.Max(p => p.Y);

            return new Rect(new Point(minX, minY), new Point(maxX, maxY));
        }

        private static Rect ObterBoundsRotacionado(Rect bounds, double rotacao)
        {
            Point centro = new(bounds.Left + bounds.Width / 2, bounds.Top + bounds.Height / 2);
            Point p1 = Rotacionar(new Point(bounds.Left, bounds.Top), centro, rotacao);
            Point p2 = Rotacionar(new Point(bounds.Right, bounds.Top), centro, rotacao);
            Point p3 = Rotacionar(new Point(bounds.Right, bounds.Bottom), centro, rotacao);
            Point p4 = Rotacionar(new Point(bounds.Left, bounds.Bottom), centro, rotacao);

            double minX = Math.Min(Math.Min(p1.X, p2.X), Math.Min(p3.X, p4.X));
            double minY = Math.Min(Math.Min(p1.Y, p2.Y), Math.Min(p3.Y, p4.Y));
            double maxX = Math.Max(Math.Max(p1.X, p2.X), Math.Max(p3.X, p4.X));
            double maxY = Math.Max(Math.Max(p1.Y, p2.Y), Math.Max(p3.Y, p4.Y));

            return new Rect(new Point(minX, minY), new Point(maxX, maxY));
        }

        private static Point Rotacionar(Point point, Point centro, double angulo)
        {
            double rad = angulo * Math.PI / 180.0;
            double cos = Math.Cos(rad);
            double sin = Math.Sin(rad);
            double x = point.X - centro.X;
            double y = point.Y - centro.Y;

            return new Point(
                centro.X + x * cos - y * sin,
                centro.Y + x * sin + y * cos);
        }

        private static void DeslocarElemento(Elemento elemento, double dx, double dy)
        {
            elemento.PosicaoX += dx;
            elemento.PosicaoY += dy;

            if (elemento is ElementoLinear linear)
            {
                linear.PosicaoX2 += dx;
                linear.PosicaoY2 += dy;
            }

            if (elemento is Cabo cabo)
                DeslocarCabo(cabo, dx, dy);

            foreach (Terminal terminal in ObterTerminais(elemento))
            {
                terminal.DefinirPosicaoVisual(new Point(
                    terminal.Posicao.X + dx,
                    terminal.Posicao.Y + dy));
            }
        }

        private static void DeslocarCabo(Cabo cabo, double dx, double dy)
        {
            for (int i = 0; i < cabo.Vertices.Count; i++)
            {
                Point p = cabo.Vertices[i];
                cabo.Vertices[i] = new Point(p.X + dx, p.Y + dy);
            }

            if (cabo.Vertices.Count > 0)
                cabo.DefinirOrigem(cabo.Vertices[0]);

            if (cabo.Vertices.Count > 1)
                cabo.DefinirDestino(cabo.Vertices[^1]);

            if (cabo.PreviewPonto is Point preview)
                cabo.PreviewPonto = new Point(preview.X + dx, preview.Y + dy);
        }

        private static void LimparConexoesSeNecessario(Elemento elemento)
        {
            if (elemento is not Cabo cabo)
                return;

            cabo.OrigemId = string.Empty;
            cabo.DestinoId = string.Empty;
            cabo.OrigemTerminalId = string.Empty;
            cabo.DestinoTerminalId = string.Empty;
            cabo.BarraOrigem = string.Empty;
            cabo.BarraDestino = string.Empty;
        }

        private static IEnumerable<Terminal> ObterTerminais(Elemento elemento)
        {
            return elemento is ITerminalOwner owner ? owner.Terminais : Enumerable.Empty<Terminal>();
        }
    }
}