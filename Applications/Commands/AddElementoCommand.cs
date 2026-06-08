using System;
using Araci.Core.Documents;
using Araci.Models;
using Araci.Services;
using Araci.Services.Naming;

namespace Araci.Core.Commands
{
    public class AddElementoCommand : IUndoableCommand
    {
        private readonly Elemento _elemento;
        private readonly AraciDocument _document;
        private readonly NameService _names;
        private bool _inicializado;
        private string? _nomeFinal;
        private Guid? _viewIdFinal;

        public AddElementoCommand(
            Elemento elemento,
            AraciDocument document,
            NameService names)
        {
            _elemento = elemento ?? throw new ArgumentNullException(nameof(elemento));
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _names = names ?? throw new ArgumentNullException(nameof(names));
        }

        public void Execute()
        {
            PrepararPrimeiraExecucao();
            _document.AdicionarElemento(_elemento);
            _viewIdFinal = _elemento.ViewId;
        }

        public void Undo()
        {
            _document.RemoverElemento(_elemento);
        }

        public void Redo()
        {
            RestaurarEstadoInicializado();
            _document.AdicionarElementoPreservandoVista(_elemento);
        }

        private void PrepararPrimeiraExecucao()
        {
            if (_inicializado)
            {
                RestaurarEstadoInicializado();
                return;
            }

            _names.GarantirNomeUnico(_elemento);
            _nomeFinal = _elemento.Nome;
            _viewIdFinal = _elemento.ViewId;
            _inicializado = true;
        }

        private void RestaurarEstadoInicializado()
        {
            if (_nomeFinal != null)
                _elemento.Nome = _nomeFinal;

            _elemento.ViewId = _viewIdFinal;
        }
    }
}
