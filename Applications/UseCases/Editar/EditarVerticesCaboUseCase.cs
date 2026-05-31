using System;
using System.Linq;
using Araci.Applications.Abstractions;
using Araci.Core.Commands;
using Araci.Models;
using Araci.ViewModels;

namespace Araci.Applications.UseCases.Editar
{
    public class EditarVerticesCaboUseCase
    {
        private readonly ICommandHistory _commands;
        private readonly Action<Elemento>? _onStateApplied;

        public EditarVerticesCaboUseCase(
            ICommandHistory commands,
            Action<Elemento>? onStateApplied = null)
        {
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
            _onStateApplied = onStateApplied;
        }

        public bool Executar(EditarVerticesCaboRequest request)
        {
            return Executar(request, _onStateApplied);
        }

        public bool Executar(EditarVerticesCaboRequest request, Action<Elemento>? onStateApplied)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (VerticesIguais(request.Antes, request.Depois))
                return false;

            _commands.Execute(new MoveElementoCommand(
                request.Cabo,
                request.Antes,
                request.Depois,
                onStateApplied));

            return true;
        }

        private static bool VerticesIguais(ElementoEstado antes, ElementoEstado depois)
        {
            return antes.Vertices.SequenceEqual(depois.Vertices);
        }
    }

    public sealed class EditarVerticesCaboRequest
    {
        public EditarVerticesCaboRequest(Cabo cabo, ElementoEstado antes, ElementoEstado depois)
        {
            Cabo = cabo ?? throw new ArgumentNullException(nameof(cabo));
            Antes = antes ?? throw new ArgumentNullException(nameof(antes));
            Depois = depois ?? throw new ArgumentNullException(nameof(depois));
        }

        public Cabo Cabo { get; }
        public ElementoEstado Antes { get; }
        public ElementoEstado Depois { get; }
    }
}
