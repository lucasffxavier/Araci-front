using System;
using System.Collections.Generic;
using System.Linq;
using Araci.Applications.Abstractions;
using Araci.Core.Commands;
using Araci.Models;
using Araci.Services;
using Araci.ViewModels;

namespace Araci.Applications.UseCases.Editar
{
    public class RotacionarElementoUseCase
    {
        private readonly ICommandHistory _commands;
        private readonly Action<Elemento>? _onStateApplied;

        public RotacionarElementoUseCase(EditorContext context)
            : this(context?.Commands ?? throw new ArgumentNullException(nameof(context)), null)
        {
        }

        public RotacionarElementoUseCase(
            ICommandHistory commands,
            Action<Elemento>? onStateApplied = null)
        {
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
            _onStateApplied = onStateApplied;
        }

        public bool Executar(IEnumerable<RotacionarElementoItem> items)
        {
            return Executar(items, _onStateApplied);
        }

        public bool Executar(IEnumerable<RotacionarElementoItem> items, Action<Elemento>? onStateApplied)
        {
            ArgumentNullException.ThrowIfNull(items);

            var alterados = items
                .Where(i => i.Elemento != null && Mudou(i.Antes, i.Depois))
                .Select(i => new RotateElementoCommand.Item(i.Elemento, i.Antes, i.Depois))
                .ToList();

            if (alterados.Count == 0)
                return false;

            _commands.Execute(new RotateElementoCommand(alterados, onStateApplied));
            return true;
        }

        private static bool Mudou(ElementoEstado antes, ElementoEstado depois)
        {
            return antes.X != depois.X ||
                antes.Y != depois.Y ||
                antes.X2 != depois.X2 ||
                antes.Y2 != depois.Y2 ||
                antes.Rotacao != depois.Rotacao ||
                !antes.Vertices.SequenceEqual(depois.Vertices);
        }
    }

    public readonly struct RotacionarElementoItem
    {
        public RotacionarElementoItem(Elemento elemento, ElementoEstado antes, ElementoEstado depois)
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
