using System;
using System.Collections.Generic;
using Araci.Models;
using Araci.ViewModels;

namespace Araci.Core.Commands
{
    public class RotateElementoCommand : IUndoableCommand
    {
        private readonly IReadOnlyList<Item> _items;
        private readonly Action<Elemento>? _onStateApplied;

        public RotateElementoCommand(
            IEnumerable<Item> items,
            Action<Elemento>? onStateApplied = null)
        {
            _items = new List<Item>(items ?? throw new ArgumentNullException(nameof(items)));
            _onStateApplied = onStateApplied;
        }

        public void Execute()
        {
            AplicarDepois();
        }

        public void Undo()
        {
            for (int i = _items.Count - 1; i >= 0; i--)
                Aplicar(_items[i].Elemento, _items[i].Antes);
        }

        public void Redo()
        {
            AplicarDepois();
        }

        private void AplicarDepois()
        {
            foreach (Item item in _items)
                Aplicar(item.Elemento, item.Depois);
        }

        private void Aplicar(Elemento elemento, ElementoEstado estado)
        {
            estado.AplicarEm(elemento);
            _onStateApplied?.Invoke(elemento);
        }

        public readonly struct Item
        {
            public Item(
                Elemento elemento,
                ElementoEstado antes,
                ElementoEstado depois)
            {
                Elemento = elemento ?? throw new ArgumentNullException(nameof(elemento));
                Antes = antes ?? throw new ArgumentNullException(nameof(antes));
                Depois = depois ?? throw new ArgumentNullException(nameof(depois));
            }

            public Elemento Elemento { get; }

            public ElementoEstado Antes { get; }

            public ElementoEstado Depois { get; }
        }
    }
}
