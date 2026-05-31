using System;
using Araci.Applications.Abstractions;
using Araci.Core.Commands;
using Araci.Core.Documents;
using Araci.Models;
using Araci.Services;

namespace Araci.Applications.UseCases.Diagrama
{
    public class InserirElementoUseCase
    {
        private readonly ElementoFactory _factory;
        private readonly TerminalLayoutService _terminalLayout;
        private readonly ICommandHistory _commands;
        private readonly AraciDocument _document;
        private readonly NameService _names;

        public InserirElementoUseCase(
            ElementoFactory factory,
            TerminalLayoutService terminalLayout,
            ICommandHistory commands,
            AraciDocument document,
            NameService names)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _terminalLayout = terminalLayout ?? throw new ArgumentNullException(nameof(terminalLayout));
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _names = names ?? throw new ArgumentNullException(nameof(names));
        }

        public Elemento Executar(string kind, double x, double y, double rotacao)
        {
            Elemento modelo = _factory.CriarModelo(kind);
            modelo.PosicaoX = x;
            modelo.PosicaoY = y;
            modelo.Rotacao = rotacao;
            _terminalLayout.AtualizarTerminais(modelo);
            _commands.Execute(new AddElementoCommand(modelo, _document, _names));
            return modelo;
        }
    }
}
