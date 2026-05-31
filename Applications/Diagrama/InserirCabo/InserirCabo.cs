using System;
using System.Windows;
using System.Windows.Input;
using Araci.Applications.Editar.Base;
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
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public void Executar()
        {
            _context.Input.ToolAtual = new InserirCaboTool(_context);
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

        public void OnMouseDown(ElementoViewModel? vm, Point position, ToolInputState inputState)
        {
            if (ReferenceEquals(vm, _caboAtual) || ReferenceEquals(vm, _previewInicial))
                vm = null;

            Terminal? terminal = ObterTerminalParaClique(vm, position);
            Point pontoSnap = terminal?.Posicao ?? _context.Snap.SnapFromElemento(vm, position, _caboAtual);

            if (!_inserindo)
            {
                if (terminal == null)
                    return;

                ConnectionValidationResult validacaoOrigem = ValidarOrigem(terminal);

                if (!validacaoOrigem.IsValid)
                {
                    MostrarTerminalInvalido(terminal, validacaoOrigem.Message ?? "Conexão inválida");
                    return;
                }

                AtualizarTerminalCapturado(terminal);
                LimparPreviewInicial();
                _caboAtual = _context.InserirCabo.Iniciar(pontoSnap, terminal);
                _inserindo = true;
                _terminalOrigem = terminal;
                return;
            }

            if (_caboAtual == null)
                return;

            if (terminal == null)
            {
                AdicionarVerticeIntermediario(AplicarAlinhamentoCabo(pontoSnap, inputState));
                return;
            }

            ConnectionValidationResult validacao = ValidarDestino(terminal);

            if (!validacao.IsValid)
            {
                MostrarTerminalInvalido(terminal, validacao.Message ?? "Conexão inválida");
                return;
            }

            AtualizarTerminalCapturado(terminal);
            _context.InserirCabo.FinalizarDestino(_caboAtual, terminal, pontoSnap);
            Finalizar();
        }

        public void OnMouseMove(Point position, ToolInputState inputState)
        {
            if (!_inserindo)
            {
                _context.AlignmentGuides.Limpar();
                AtualizarPreviewInicial(position);
                return;
            }

            if (_caboAtual == null)
                return;

            Terminal? terminal = ObterTerminalConexao(null, position);
            _terminalPreviewFinal = terminal;

            if (terminal == null)
            {
                LimparTerminalCapturado();
            }
            else
            {
                ConnectionValidationResult validacao = ValidarDestino(terminal);

                if (validacao.IsValid)
                    AtualizarTerminalCapturado(terminal);
                else
                    MostrarTerminalInvalido(terminal, validacao.Message ?? "Conexão inválida");
            }

            Point pontoSnap = terminal?.Posicao ?? _context.Snap.Snap(position, _caboAtual);
            Point pontoPreview = terminal == null ? AplicarAlinhamentoCabo(pontoSnap, inputState) : pontoSnap;

            if (terminal != null)
                _context.AlignmentGuides.Limpar();

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

        private static bool EhElementoConectavel(Elemento? elemento)
        {
            return elemento is ElementoEquipamento || elemento is Barra;
        }

        private Terminal? ObterTerminalConexao(ElementoViewModel? vm, Point position)
        {
            Terminal? terminal = _context.Snap.ObterTerminalMaisProximo(vm, position);

            if (terminal != null && EhElementoConectavel(terminal.Dono))
                return terminal;

            terminal = _context.Snap.ObterTerminalMaisProximo(position, _caboAtual ?? _previewInicial, t => EhElementoConectavel(t.Dono));
            return terminal;
        }

        private Terminal? ObterTerminalParaClique(ElementoViewModel? vm, Point position)
        {
            Terminal? terminal = ObterTerminalConexao(vm, position);

            if (terminal != null)
                return terminal;

            Terminal? terminalPreview = _inserindo ? _terminalPreviewFinal : _terminalPreviewInicial;
            return TerminalAindaValido(terminalPreview, position) ? terminalPreview : null;
        }

        private bool TerminalAindaValido(Terminal? terminal, Point position)
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
            Terminal? terminal = ObterTerminalConexao(null, position);
            _terminalPreviewInicial = terminal;

            if (terminal == null)
            {
                LimparTerminalCapturado();
                LimparPreviewInicial();
                return;
            }

            ConnectionValidationResult validacao = ValidarOrigem(terminal);

            if (!validacao.IsValid)
            {
                MostrarTerminalInvalido(terminal, validacao.Message ?? "Conexão inválida");
                LimparPreviewInicial();
                return;
            }

            AtualizarTerminalCapturado(terminal);
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

            _previewInicial = _context.ElementoFactory.CriarCaboVM();
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

        private void MostrarTerminalInvalido(Terminal terminal, string mensagem)
        {
            _context.TerminalSnap.MostrarInvalido(terminal, terminal.Posicao, mensagem);
        }

        private void LimparTerminalCapturado()
        {
            _context.TerminalSnap.Limpar();
        }

        private void AdicionarVerticeIntermediario(Point ponto)
        {
            if (_caboAtual == null)
                return;

            _caboAtual.AdicionarVerticeIntermediario(ponto);
            _context.SceneQueries.Invalidate();
        }

        private Point AplicarAlinhamentoCabo(Point ponto, ToolInputState inputState)
        {
            if (_caboAtual == null || _caboAtual.Cabo.Vertices.Count == 0)
                return ponto;

            Point origem = _caboAtual.Cabo.Vertices[^1];

            if (inputState.IsShiftPressed)
            {
                Point ortogonal = AplicarOrtogonalizacao(ponto, origem);
                _context.AlignmentGuides.AplicarSnapPontoCabo(ortogonal, origem, _caboAtual);
                return ortogonal;
            }

            return _context.AlignmentGuides.AplicarSnapPontoCabo(ponto, origem, _caboAtual);
        }

        private static Point AplicarOrtogonalizacao(Point ponto, Point origem)
        {
            Vector delta = ponto - origem;

            if (Math.Abs(delta.X) < 0.0001 && Math.Abs(delta.Y) < 0.0001)
                return origem;

            return Math.Abs(delta.X) >= Math.Abs(delta.Y)
                ? new Point(ponto.X, origem.Y)
                : new Point(origem.X, ponto.Y);
        }

        private ConnectionValidationResult ValidarOrigem(Terminal terminal)
        {
            if (!OrigemValida(terminal))
                return ConnectionValidationResult.Invalid("Conexão inválida");

            return _context.Connectivity.ValidarTerminalDisponivel(null, terminal);
        }

        private bool OrigemValida(Terminal terminal)
        {
            return !string.IsNullOrWhiteSpace(terminal.Dono.Id.ToString()) && !string.IsNullOrWhiteSpace(terminal.Id);
        }

        private ConnectionValidationResult ValidarDestino(Terminal terminal)
        {
            if (_caboAtual == null)
                return ConnectionValidationResult.Invalid("Cabo atual não encontrado.");

            if (_terminalOrigem == null)
                return ConnectionValidationResult.Invalid("Origem do cabo não definida.");

            return _context.Connectivity.ValidarConexaoCabo(_caboAtual.Cabo, _terminalOrigem, terminal);
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
            _context.AlignmentGuides.Limpar();
            _inserindo = false;
        }
    }
}
