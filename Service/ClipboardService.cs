using System.Collections.Generic;

using Araci.Core.Commands;
using Araci.Models;
using Araci.ViewModels;

namespace Araci.Services
{
    public static class ClipboardService
    {
        // =========================
        // BUFFER
        // =========================

        private static readonly List<Elemento> _buffer = new();

        // =========================
        // OFFSET
        // =========================

        private const double OFFSET_X = 30;

        private const double OFFSET_Y = 30;

        // =========================
        // COPIAR
        // =========================

        public static void CopiarSelecionados(
            EditorContext context)
        {
            var editorContext =
                context
                ?? throw new System.ArgumentNullException(nameof(context));

            _buffer.Clear();

            foreach (var item in editorContext.Selection.Selecionados)
            {
                _buffer.Add(item.Modelo.Clonar());
            }
        }

        // =========================
        // COLAR
        // =========================

        public static void Colar(
            EditorContext context)
        {
            var editorContext =
                context
                ?? throw new System.ArgumentNullException(nameof(context));

            if (_buffer.Count == 0)
                return;

            var novos = new List<ElementoViewModel>();

            using var transaction =
                editorContext.BeginTransaction();

            foreach (var item in _buffer)
            {
                var clone = item.Clonar();

                AplicarOffset(clone);

                var vm = editorContext.ElementoFactory
                    .CriarViewModel(clone);

                if (vm == null)
                    continue;

                novos.Add(vm);

                transaction.Add(
                    new AddElementoCommand(
                        vm,
                        editorContext));
            }

            transaction.Commit();

            AtualizarSelecao(
                editorContext,
                novos);

            AtualizarBuffer(novos);
        }

        // =========================
        // OFFSET
        // =========================

        private static void AplicarOffset(Elemento elemento)
        {
            elemento.PosicaoX += OFFSET_X;
            elemento.PosicaoY += OFFSET_Y;

            if (elemento is Cabo cabo)
            {
                cabo.PosicaoX2 += OFFSET_X;
                cabo.PosicaoY2 += OFFSET_Y;
            }
        }

        // =========================
        // SELEÇÃO
        // =========================

        private static void AtualizarSelecao(
            EditorContext context,
            List<ElementoViewModel> elementos)
        {
            context.Selection.Limpar();

            foreach (var item in elementos)
            {
                context.Selection.Selecionar(item, true);
            }
        }

        // =========================
        // BUFFER
        // =========================

        private static void AtualizarBuffer(List<ElementoViewModel> elementos)
        {
            _buffer.Clear();

            foreach (var item in elementos)
            {
                _buffer.Add(item.Modelo.Clonar());
            }
        }
    }
}
