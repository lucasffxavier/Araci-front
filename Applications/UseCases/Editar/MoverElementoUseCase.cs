using System;
using System.Collections.Generic;
using System.Linq;
using Araci.Applications.Abstractions;
using Araci.Core.Commands;
using Araci.Models;
using Araci.ViewModels;

namespace Araci.Applications.UseCases.Editar
{
    public class MoverElementoUseCase
    {
        private readonly ICommandHistory _commands;
        private readonly Action<Elemento>? _onStateApplied;

        public MoverElementoUseCase(
            ICommandHistory commands,
            Action<Elemento>? onStateApplied = null)
        {
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
            _onStateApplied = onStateApplied;
        }

        public bool Executar(IEnumerable<MoverElementoItem> items)
        {
            ArgumentNullException.ThrowIfNull(items);

            var alterados = items
                .Where(i => i.Elemento != null && Mudou(i.Antes, i.Depois))
                .ToList();

            if (alterados.Count == 0)
                return false;

            using var transaction = _commands.BeginTransaction();

            foreach (MoverElementoItem item in alterados)
            {
                transaction.Add(new MoveElementoCommand(
                    item.Elemento,
                    item.Antes,
                    item.Depois,
                    _onStateApplied));
            }

            transaction.Commit();
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

    public readonly struct MoverElementoItem
    {
        public MoverElementoItem(Elemento elemento, ElementoEstado antes, ElementoEstado depois)
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
