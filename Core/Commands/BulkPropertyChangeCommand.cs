using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Araci.ViewModels;

namespace Araci.Core.Commands
{
    public class BulkPropertyChangeCommand : IUndoableCommand
    {
        private readonly IReadOnlyList<Item> _items;

        public BulkPropertyChangeCommand(IEnumerable<Item> items)
        {
            _items = items?.Where(i => i.Elemento != null && !string.IsNullOrWhiteSpace(i.PropertyName)).ToList() ?? throw new ArgumentNullException(nameof(items));
        }

        public bool IsEmpty => _items.Count == 0;

        public void Execute()
        {
            AplicarDepois();
        }

        public void Undo()
        {
            for (int i = _items.Count - 1; i >= 0; i--)
                Aplicar(_items[i], _items[i].ValorAntes);
        }

        public void Redo()
        {
            AplicarDepois();
        }

        private void AplicarDepois()
        {
            foreach (var item in _items)
                Aplicar(item, item.ValorDepois);
        }

        private static void Aplicar(Item item, object? valor)
        {
            PropertyInfo? prop = item.Elemento.GetType().GetProperty(item.PropertyName, BindingFlags.Instance | BindingFlags.Public);

            if (prop == null || !prop.CanWrite || prop.GetIndexParameters().Length > 0)
                return;

            prop.SetValue(item.Elemento, valor);
            item.Elemento.NotificarPropriedades(item.PropertyName);

            if (item.PropertyName == nameof(ElementoViewModel.Tipo))
                item.Elemento.NotificarPropriedades(nameof(ElementoViewModel.TipoViewModel));
        }

        public readonly struct Item
        {
            public Item(ElementoViewModel elemento, string propertyName, object? valorAntes, object? valorDepois)
            {
                Elemento = elemento ?? throw new ArgumentNullException(nameof(elemento));
                PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
                ValorAntes = valorAntes;
                ValorDepois = valorDepois;
            }

            public ElementoViewModel Elemento { get; }
            public string PropertyName { get; }
            public object? ValorAntes { get; }
            public object? ValorDepois { get; }
        }
    }
}