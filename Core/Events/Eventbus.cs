// =============================================================
// FASE 2 — SISTEMA DE EVENTOS INTERNO
// =============================================================
// Permite que Commands e Services disparem eventos sem
// depender de referências diretas entre si.
// Uso: context.Events.Publish(new ElementoAdicionadoEvent(vm));
// =============================================================

using System;
using System.Collections.Generic;

namespace Araci.Core.Events
{
    // ----------------------------------------------------------
    // CONTRATO BASE
    // ----------------------------------------------------------

    public interface IEditorEvent { }

    // ----------------------------------------------------------
    // INTERFACE DO BUS
    // ----------------------------------------------------------

    public interface IEventBus
    {
        void Publish<TEvent>(TEvent evento)
            where TEvent : IEditorEvent;

        IDisposable Subscribe<TEvent>(
            Action<TEvent> handler)
            where TEvent : IEditorEvent;
    }

    // ----------------------------------------------------------
    // IMPLEMENTAÇÃO
    // ----------------------------------------------------------

    public sealed class EventBus : IEventBus
    {
        private readonly Dictionary<Type, List<object>>
            _handlers = new();

        // =========================
        // PUBLISH
        // =========================

        public void Publish<TEvent>(TEvent evento)
            where TEvent : IEditorEvent
        {
            var tipo = typeof(TEvent);

            if (!_handlers.TryGetValue(tipo, out var lista))
                return;

            foreach (var h in lista)
            {
                ((Action<TEvent>)h).Invoke(evento);
            }
        }

        // =========================
        // SUBSCRIBE
        // =========================

        public IDisposable Subscribe<TEvent>(
            Action<TEvent> handler)
            where TEvent : IEditorEvent
        {
            var tipo = typeof(TEvent);

            if (!_handlers.ContainsKey(tipo))
                _handlers[tipo] = new List<object>();

            _handlers[tipo].Add(handler);

            return new Subscription(() =>
                _handlers[tipo].Remove(handler));
        }

        // ----------------------------------------------------------
        // SUBSCRIPTION (auto-dispose)
        // ----------------------------------------------------------

        private sealed class Subscription : IDisposable
        {
            private readonly Action _remover;

            public Subscription(Action remover)
            {
                _remover = remover;
            }

            public void Dispose() => _remover();
        }
    }
}