using System.Windows;
using System.Windows.Input;
using Araci.Applications.Editar.Base;
using Araci.Core.Commands;
using Araci.Models;
using Araci.Services;
using Araci.ViewModels;

namespace Araci.Applications.Diagrama.InserirCabo
{
    public class InserirCaboApplication
    {
        private readonly EditorContext _context;

        public InserirCaboApplication(EditorContext context)
        {
            _context = context
                ?? throw new System.ArgumentNullException(nameof(context));
        }

        public void Executar()
        {
            _context.Input.ToolAtual =
                new InserirCaboTool(_context);
        }
    }

    public class InserirCaboTool : ITool
    {
        private readonly EditorContext _context;

        private CaboViewModel? _caboAtual;
        private bool _inserindo;

        public InserirCaboTool(EditorContext context)
        {
            _context = context;
        }

        public string Nome => "Inserir Cabo";

        public bool MantemBotaoAtivado => true;
        public bool IsBusy => _inserindo;

        public void Ativar()
        {
        }

        public void Desativar()
        {
            Cancelar();
        }

        public void Cancelar()
        {
            if (_caboAtual != null)
                _context.Commands.Undo();

            LimparEstado();
        }

        public void OnMouseDown(
            ElementoViewModel? vm,
            Point position,
            ToolInputState inputState)
        {
            if (ReferenceEquals(vm, _caboAtual))
                vm = null;

            Terminal? terminal =
                ObterTerminalConexao(vm, position);

            Point pontoSnap =
                terminal?.Posicao
                ?? _context.Snap.SnapFromElemento(vm, position, _caboAtual);

            if (!_inserindo)
            {
                if (terminal == null)
                    return;

                _caboAtual =
                    _context.ElementoFactory.CriarCaboVM();

                _caboAtual.Iniciar(pontoSnap);

                _context.Commands.Execute(
                    new AddElementoCommand(
                        _caboAtual.Modelo,
                        _context));

                ConectarOrigem(terminal);

                _inserindo = true;

                return;
            }

            if (_caboAtual == null)
                return;

            if (terminal == null)
                return;

            _caboAtual.FinalizarNoPonto(pontoSnap);

            ConectarDestino(terminal);

            Finalizar();
        }

        public void OnMouseMove(Point position, ToolInputState inputState)
        {
            if (!_inserindo || _caboAtual == null)
                return;

            Point pontoSnap =
                _context.Snap.Snap(position, _caboAtual);

            _caboAtual.AtualizarPreview(pontoSnap);
        }

        public void OnMouseUp(Point position, ToolInputState inputState)
        {
        }

        public void OnKeyDown(Key key)
        {
            if (key == Key.Enter)
            {
                Finalizar();
                return;
            }

            if (key == Key.Escape)
                Cancelar();
        }

        private static bool EhElementoConectavel(
            Elemento? elemento)
        {
            return elemento is ElementoEquipamento
                || elemento is Barra;
        }

        private Terminal? ObterTerminalConexao(
            ElementoViewModel? vm,
            Point position)
        {
            Terminal? terminal =
                _context.Snap.ObterTerminalMaisProximo(vm, position);

            if (terminal != null && EhElementoConectavel(terminal.Dono))
                return terminal;

            terminal =
                _context.Snap.ObterTerminalMaisProximo(
                    position,
                    _caboAtual,
                    t => EhElementoConectavel(t.Dono));

            return terminal;
        }

        private void ConectarOrigem(
            Terminal? terminal)
        {
            if (_caboAtual == null || terminal == null)
                return;

            Elemento elemento = terminal.Dono;

            _caboAtual.OrigemId =
                elemento.Id.ToString();

            _caboAtual.OrigemTerminalId =
                terminal.Id;

            _caboAtual.BarraOrigem =
                ObterRotuloTerminal(terminal);

            _caboAtual.Cabo.DefinirOrigem(terminal.Posicao);

            _caboAtual.NotificarParametros();
        }

        private void ConectarDestino(
            Terminal? terminal)
        {
            if (_caboAtual == null || terminal == null)
                return;

            Elemento elemento = terminal.Dono;

            _caboAtual.DestinoId =
                elemento.Id.ToString();

            _caboAtual.DestinoTerminalId =
                terminal.Id;

            _caboAtual.BarraDestino =
                ObterRotuloTerminal(terminal);

            _caboAtual.Cabo.DefinirDestino(terminal.Posicao);

            _caboAtual.NotificarParametros();
        }

        private static string ObterRotuloTerminal(Terminal terminal)
        {
            return string.IsNullOrWhiteSpace(terminal.Barra)
                ? terminal.Dono.Nome
                : terminal.Barra;
        }

        private void Finalizar()
        {
            LimparEstado();

            _context.Tools.VoltarParaSelecao();
        }

        private void LimparEstado()
        {
            _caboAtual?.RemoverPreview();

            _caboAtual = null;
            _inserindo = false;
        }
    }
}
