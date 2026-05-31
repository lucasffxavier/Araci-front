using System;
using System.Collections.Generic;
using System.Linq;
using Araci.Applications.Abstractions;
using Araci.Core.Commands;
using Araci.Core.Documents;
using Araci.Models;
using Araci.Services;
using Araci.ViewModels;

namespace Araci.Applications.UseCases.Editar
{
    public class ExcluirElementoUseCase
    {
        private readonly AraciDocument _document;
        private readonly ConnectivityService _connectivity;
        private readonly ICommandHistory _commands;

        public ExcluirElementoUseCase(
            AraciDocument document,
            ConnectivityService connectivity,
            ICommandHistory commands)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _connectivity = connectivity ?? throw new ArgumentNullException(nameof(connectivity));
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
        }

        public bool Executar(IEnumerable<ElementoViewModel> selecionados)
        {
            ArgumentNullException.ThrowIfNull(selecionados);
            return Executar(selecionados.Select(vm => vm.Modelo));
        }

        public bool Executar(IEnumerable<Elemento> elementosSelecionados)
        {
            ArgumentNullException.ThrowIfNull(elementosSelecionados);

            var elementos = ColetarElementosParaExcluir(elementosSelecionados)
                .OrderBy(e => e is Cabo ? 0 : 1)
                .ToList();

            if (elementos.Count == 0)
                return false;

            using var tx = _commands.BeginTransaction();

            foreach (Elemento elemento in elementos)
                tx.Add(new DeleteElementCommand(elemento, _document));

            tx.Commit();
            return true;
        }

        private IEnumerable<Elemento> ColetarElementosParaExcluir(IEnumerable<Elemento> selecionados)
        {
            var resultado = new List<Elemento>();
            var ids = new HashSet<Guid>();

            foreach (Elemento elemento in selecionados)
            {
                Adicionar(elemento, resultado, ids);

                if (elemento is Cabo)
                    continue;

                foreach (Cabo cabo in _connectivity.ObterCabosConectados(elemento))
                    Adicionar(cabo, resultado, ids);
            }

            return resultado;
        }

        private static void Adicionar(
            Elemento elemento,
            ICollection<Elemento> resultado,
            ISet<Guid> ids)
        {
            if (ids.Add(elemento.Id))
                resultado.Add(elemento);
        }
    }
}
