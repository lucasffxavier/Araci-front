// =============================================================
// FASE 2 — EVENTOS DE DOMÍNIO DO EDITOR
// =============================================================
// Cada evento é um registro imutável (readonly struct ou class).
// Commands disparam. Services escutam. UI pode observar.
// NUNCA conter lógica — apenas dados do que ocorreu.
// =============================================================

using System.Collections.Generic;
using Araci.ViewModels;

namespace Araci.Core.Events
{
    // ----------------------------------------------------------
    // ELEMENTOS
    // ----------------------------------------------------------

    public sealed class ElementoAdicionadoEvent : IEditorEvent
    {
        public ElementoViewModel Elemento { get; }

        public ElementoAdicionadoEvent(
            ElementoViewModel elemento)
        {
            Elemento = elemento;
        }
    }

    public sealed class ElementoRemovidoEvent : IEditorEvent
    {
        public ElementoViewModel Elemento { get; }

        public ElementoRemovidoEvent(
            ElementoViewModel elemento)
        {
            Elemento = elemento;
        }
    }

    public sealed class ElementoMovidoEvent : IEditorEvent
    {
        public ElementoViewModel Elemento { get; }

        public ElementoMovidoEvent(
            ElementoViewModel elemento)
        {
            Elemento = elemento;
        }
    }

    // ----------------------------------------------------------
    // SELEÇÃO
    // ----------------------------------------------------------

    public sealed class SelecaoAlteradaEvent : IEditorEvent
    {
        public IReadOnlyList<ElementoViewModel>
            Selecionados
        { get; }

        public SelecaoAlteradaEvent(
            IReadOnlyList<ElementoViewModel> selecionados)
        {
            Selecionados = selecionados;
        }
    }

    // ----------------------------------------------------------
    // DOCUMENTO
    // ----------------------------------------------------------

    public sealed class DocumentoCarregadoEvent : IEditorEvent
    {
        public string CaminhoArquivo { get; }

        public DocumentoCarregadoEvent(string caminho)
        {
            CaminhoArquivo = caminho;
        }
    }

    public sealed class DocumentoSalvoEvent : IEditorEvent
    {
        public string CaminhoArquivo { get; }

        public DocumentoSalvoEvent(string caminho)
        {
            CaminhoArquivo = caminho;
        }
    }
}