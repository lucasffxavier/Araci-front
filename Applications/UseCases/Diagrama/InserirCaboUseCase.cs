using System;
using System.Windows;
using Araci.Applications.Abstractions;
using Araci.Applications.Factories;
using Araci.Core.Commands;
using Araci.Core.Documents;
using Araci.Models;
using Araci.Services;
using Araci.ViewModels;
using Araci.Services.Naming;

namespace Araci.Applications.UseCases.Diagrama
{
    public class InserirCaboUseCase
    {
        private readonly ElementoFactory _factory;
        private readonly ICommandHistory _commands;
        private readonly AraciDocument _document;
        private readonly NameService _names;
        private readonly Func<Cabo, CaboViewModel?> _obterViewModelDaCena;

        public InserirCaboUseCase(
            ElementoFactory factory,
            ICommandHistory commands,
            AraciDocument document,
            NameService names,
            Func<Cabo, CaboViewModel?> obterViewModelDaCena)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _names = names ?? throw new ArgumentNullException(nameof(names));
            _obterViewModelDaCena = obterViewModelDaCena ?? throw new ArgumentNullException(nameof(obterViewModelDaCena));
        }

        public CaboViewModel Iniciar(Point pontoOrigem, Terminal terminalOrigem)
        {
            ArgumentNullException.ThrowIfNull(terminalOrigem);

            CaboViewModel cabo = _factory.CriarCaboVM();
            cabo.Iniciar(pontoOrigem);
            _commands.Execute(new AddElementoCommand(cabo.Modelo, _document, _names));

            if (_obterViewModelDaCena(cabo.Cabo) is CaboViewModel caboNaCena)
                cabo = caboNaCena;

            ConectarOrigem(cabo, terminalOrigem);
            return cabo;
        }

        public void ConectarOrigem(CaboViewModel cabo, Terminal terminal)
        {
            ArgumentNullException.ThrowIfNull(cabo);
            ArgumentNullException.ThrowIfNull(terminal);

            Elemento elemento = terminal.Dono;
            cabo.OrigemId = elemento.Id.ToString();
            cabo.OrigemTerminalId = terminal.Id;
            cabo.BarraOrigem = ObterRotuloTerminal(terminal);
            cabo.Cabo.DefinirOrigem(terminal.Posicao);
            cabo.NotificarParametros();
        }

        public void FinalizarDestino(CaboViewModel cabo, Terminal terminal, Point pontoDestino)
        {
            ArgumentNullException.ThrowIfNull(cabo);
            ArgumentNullException.ThrowIfNull(terminal);

            cabo.FinalizarNoPonto(pontoDestino);
            ConectarDestino(cabo, terminal);
        }

        public void ConectarDestino(CaboViewModel cabo, Terminal terminal)
        {
            ArgumentNullException.ThrowIfNull(cabo);
            ArgumentNullException.ThrowIfNull(terminal);

            Elemento elemento = terminal.Dono;
            cabo.DestinoId = elemento.Id.ToString();
            cabo.DestinoTerminalId = terminal.Id;
            cabo.BarraDestino = ObterRotuloTerminal(terminal);
            cabo.Cabo.DefinirDestino(terminal.Posicao);
            cabo.NotificarParametros();
        }

        private static string ObterRotuloTerminal(Terminal terminal)
        {
            return string.IsNullOrWhiteSpace(terminal.Barra) ? terminal.Dono.Nome : terminal.Barra;
        }
    }
}
