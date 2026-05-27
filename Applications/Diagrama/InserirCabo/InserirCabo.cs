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
        private CaboViewModel? _previewInicial;
        private Terminal? _terminalPreviewInicial;
        private Terminal? _terminalPreviewFinal;
        private Terminal? _terminalOrigem;
        private bool _inserindo;

        public InserirCaboTool(EditorContext context)
        {
            _context = context;
        }

        public string Nome => "Inserir Cabo";

        public bool MantemBotaoAtivado => true;
        public bool IsBusy => _inserindo || _previewInicial != null;

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

            LimparTerminalCapturado();
            LimparPreviewInicial();
            LimparEstado();
        }

        public void OnMouseDown(
            ElementoViewModel? vm,
            Point position,
            ToolInputState inputState)
        {
            if (ReferenceEquals(vm, _caboAtual) ||
                ReferenceEquals(vm, _previewInicial))
            {
                vm = null;
            }

            Terminal? terminal =
                ObterTerminalParaClique(vm, position);

            Point pontoSnap =
                terminal?.Posicao
                ?? _context.Snap.SnapFromElemento(vm, position, _caboAtual);

            if (!_inserindo)
            {
                if (terminal == null)
                    return;

                if (!OrigemValida(terminal))
                {
                    LimparTerminalCapturado();
                    return;
                }

                AtualizarTerminalCapturado(terminal);
                LimparPreviewInicial();

                _caboAtual =
                    _context.ElementoFactory.CriarCaboVM();

                _caboAtual.Iniciar(pontoSnap);

                _context.Commands.Execute(
                    new AddElementoCommand(
                        _caboAtual.Modelo,
                        _context));

                UsarViewModelDaCena();

                ConectarOrigem(terminal);

                _inserindo = true;

                return;
            }

            if (_caboAtual == null)
                return;

            if (terminal == null)
            {
                AdicionarVerticeIntermediario(
                    AplicarOrtogonalizacao(pontoSnap, inputState));
                return;
            }

            if (!DestinoValido(terminal))
            {
                MostrarTerminalInvalido(terminal, inputState.ScreenPosition);
                return;
            }

            AtualizarTerminalCapturado(terminal);
            _caboAtual.FinalizarNoPonto(pontoSnap);

            ConectarDestino(terminal);

            Finalizar();
        }

        public void OnMouseMove(Point position, ToolInputState inputState)
        {
            if (!_inserindo)
            {
                AtualizarPreviewInicial(position);
                return;
            }

            if (_caboAtual == null)
                return;

            Terminal? terminal =
                ObterTerminalConexao(null, position);

            _terminalPreviewFinal = terminal;

            if (terminal == null)
                LimparTerminalCapturado();
            else if (DestinoValido(terminal))
                AtualizarTerminalCapturado(terminal);
            else
                MostrarTerminalInvalido(terminal, inputState.ScreenPosition);

            Point pontoSnap =
                terminal?.Posicao
                ?? _context.Snap.Snap(position, _caboAtual);

            Point pontoPreview =
                terminal == null
                    ? AplicarOrtogonalizacao(pontoSnap, inputState)
                    : pontoSnap;

            _caboAtual.AtualizarPreview(pontoPreview);
        }

        public void OnMouseUp(Point position, ToolInputState inputState)
        {
        }

        public void OnKeyDown(Key key)
        {
            if (key == Key.Enter)
                return;

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
                    _caboAtual ?? _previewInicial,
                    t => EhElementoConectavel(t.Dono));

            return terminal;
        }

        private Terminal? ObterTerminalParaClique(
            ElementoViewModel? vm,
            Point position)
        {
            Terminal? terminal =
                ObterTerminalConexao(vm, position);

            if (terminal != null)
                return terminal;

            Terminal? terminalPreview =
                _inserindo
                    ? _terminalPreviewFinal
                    : _terminalPreviewInicial;

            return TerminalAindaValido(terminalPreview, position)
                ? terminalPreview
                : null;
        }

        private bool TerminalAindaValido(
            Terminal? terminal,
            Point position)
        {
            if (terminal == null)
                return false;

            double dx = terminal.Posicao.X - position.X;
            double dy = terminal.Posicao.Y - position.Y;
            double tolerancia = _context.Snap.TerminalTolerance;

            return dx * dx + dy * dy <= tolerancia * tolerancia;
        }

        private void AtualizarPreviewInicial(Point position)
        {
            Terminal? terminal =
                ObterTerminalConexao(null, position);

            _terminalPreviewInicial = terminal;
            AtualizarTerminalCapturado(terminal);

            if (terminal == null)
            {
                LimparPreviewInicial();
                return;
            }

            CaboViewModel preview = ObterPreviewInicial();
            preview.Cabo.Vertices.Clear();
            preview.Cabo.Vertices.Add(terminal.Posicao);
            preview.Cabo.DefinirOrigem(terminal.Posicao);
            preview.Cabo.PreviewPonto = position;
            preview.AtualizarAposModeloAlterado();
            _context.SceneQueries.Invalidate();
        }

        private CaboViewModel ObterPreviewInicial()
        {
            if (_previewInicial != null)
                return _previewInicial;

            _previewInicial =
                _context.ElementoFactory.CriarCaboVM();

            _previewInicial.IsPreview = true;
            _context.Scene.Elementos.Add(_previewInicial);
            _context.SceneQueries.Invalidate();

            return _previewInicial;
        }

        private void LimparPreviewInicial()
        {
            if (_previewInicial == null)
                return;

            _previewInicial.IsPreview = false;
            _context.Scene.Elementos.Remove(_previewInicial);
            _previewInicial = null;
            _terminalPreviewInicial = null;
            _context.SceneQueries.Invalidate();
        }

        private void AtualizarTerminalCapturado(Terminal? terminal)
        {
            if (terminal == null)
            {
                LimparTerminalCapturado();
                return;
            }

            _context.TerminalSnap.Mostrar(terminal);
        }

        private void MostrarTerminalInvalido(Terminal terminal, Point cursor)
        {
            _context.TerminalSnap.MostrarInvalido(
                terminal,
                cursor,
                "Conexão inválida");
        }

        private void LimparTerminalCapturado()
        {
            _context.TerminalSnap.Limpar();
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
            _terminalOrigem = terminal;

            _caboAtual.NotificarParametros();
        }

        private void AdicionarVerticeIntermediario(Point ponto)
        {
            if (_caboAtual == null)
                return;

            _caboAtual.AdicionarVerticeIntermediario(ponto);
            _context.SceneQueries.Invalidate();
        }

        private Point AplicarOrtogonalizacao(
            Point ponto,
            ToolInputState inputState)
        {
            if (!inputState.IsShiftPressed ||
                _caboAtual == null ||
                _caboAtual.Cabo.Vertices.Count == 0)
            {
                return ponto;
            }

            Point origem = _caboAtual.Cabo.Vertices[^1];
            Vector delta = ponto - origem;

            if (Math.Abs(delta.X) < 0.0001 && Math.Abs(delta.Y) < 0.0001)
                return origem;

            return Math.Abs(delta.X) >= Math.Abs(delta.Y)
                ? new Point(ponto.X, origem.Y)
                : new Point(origem.X, ponto.Y);
        }

        private bool OrigemValida(Terminal terminal)
        {
            return !string.IsNullOrWhiteSpace(terminal.Dono.Id.ToString()) &&
                !string.IsNullOrWhiteSpace(terminal.Id);
        }

        private bool DestinoValido(Terminal terminal)
        {
            if (_caboAtual == null || _terminalOrigem == null)
            {
                return false;
            }

            return _context.Connectivity.ValidarConexaoCabo(
                _caboAtual.Cabo,
                _terminalOrigem,
                terminal).IsValid;
        }

        private void UsarViewModelDaCena()
        {
            if (_caboAtual == null)
                return;

            if (_context.Viewport?.ObterViewModel(_caboAtual.Modelo)
                is CaboViewModel caboNaCena)
            {
                _caboAtual = caboNaCena;
            }
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
            LimparTerminalCapturado();
            LimparPreviewInicial();
            LimparEstado();

            _context.Tools.VoltarParaSelecao();
        }

        private void LimparEstado()
        {
            _caboAtual?.RemoverPreview();

            _caboAtual = null;
            _terminalPreviewFinal = null;
            _terminalOrigem = null;
            LimparTerminalCapturado();
            _inserindo = false;
        }
    }
}
