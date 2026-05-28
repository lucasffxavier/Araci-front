using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Araci.Core.Commands;
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

            using var transaction = context.BeginTransaction();

            foreach (Elemento modeloCopiado in _copiados)
            {
                Elemento clone = modeloCopiado.Clonar();
                DeslocarElemento(clone, OffsetPadrao, OffsetPadrao);
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
            return elemento is ITerminalOwner owner
                ? owner.Terminais
                : Enumerable.Empty<Terminal>();
        }
    }
}