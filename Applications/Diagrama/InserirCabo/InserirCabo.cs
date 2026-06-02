using System;
using System.Windows;
using System.Windows.Input;
using Araci.Applications.Abstractions;
using Araci.Applications.Editar.Base;
using Araci.Applications.Factories;
using Araci.Applications.UseCases.Diagrama;
using Araci.Core.SceneQueries;
using Araci.Models;
using Araci.Services;
using Araci.ViewModels;
using CoreScene = Araci.Core.Scenes.Scene;
using Araci.Services.Topology;
using Araci.Services.Editing;
using Araci.Services.Interaction;

namespace Araci.Applications.Diagrama.InserirCabo
{
    public class InserirCaboTool : ITool
    {
        private readonly ICommandHistory _commands;
        private readonly ElementoFactory _factory;
        private readonly InserirCaboUseCase _inserirCabo;
        private readonly SnapService _snap;
        private readonly ConnectivityService _connectivity;
        private readonly AlignmentGuideService _alignmentGuides;
        private readonly CoreScene _scene;
        private readonly ISceneQueryService _sceneQueries;
        private readonly TerminalSnapState _terminalSnap;
        private readonly Action _voltarParaSelecao;
        private CaboViewModel? _caboAtual;
        private CaboViewModel? _previewInicial;
        private Terminal? _terminalPreviewInicial;
        private Terminal? _terminalPreviewFinal;
        private Terminal? _terminalOrigem;
        private bool _inserindo;

        public InserirCaboTool(
            ICommandHistory commands,
            ElementoFactory factory,
            InserirCaboUseCase inserirCabo,
            SnapService snap,
            ConnectivityService connectivity,
            AlignmentGuideService alignmentGuides,
            CoreScene scene,
            ISceneQueryService sceneQueries,
            TerminalSnapState terminalSnap,
            Action voltarParaSelecao)
        {
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _inserirCabo = inserirCabo ?? throw new ArgumentNullException(nameof(inserirCabo));
            _snap = snap ?? throw new ArgumentNullException(nameof(snap));
            _connectivity = connectivity ?? throw new ArgumentNullException(nameof(connectivity));
            _alignmentGuides = alignmentGuides ?? throw new ArgumentNullException(nameof(alignmentGuides));
            _scene = scene ?? throw new ArgumentNullException(nameof(scene));
            _sceneQueries = sceneQueries ?? throw new ArgumentNullException(nameof(sceneQueries));
            _terminalSnap = terminalSnap ?? throw new ArgumentNullException(nameof(terminalSnap));
            _voltarParaSelecao = voltarParaSelecao ?? throw new ArgumentNullException(nameof(voltarParaSelecao));
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
                _commands.Undo();

            LimparTerminalCapturado();
            LimparPreviewInicial();
            LimparEstado();
        }

        public void OnMouseDown(ElementoViewModel? vm, Point position, ToolInputState inputState)
        {
            if (ReferenceEquals(vm, _caboAtual) || ReferenceEquals(vm, _previewInicial))
                vm = null;

            Terminal? terminal = ObterTerminalParaClique(vm, position);
            Point pontoSnap = terminal?.Posicao ?? _snap.SnapFromElemento(vm, position, _caboAtual);

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
                _caboAtual = _inserirCabo.Iniciar(pontoSnap, terminal);
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
            _inserirCabo.FinalizarDestino(_caboAtual, terminal, pontoSnap);
            Finalizar();
        }

        public void OnMouseMove(Point position, ToolInputState inputState)
        {
            if (!_inserindo)
            {
                _alignmentGuides.Limpar();
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

            Point pontoSnap = terminal?.Posicao ?? _snap.Snap(position, _caboAtual);
            Point pontoPreview = terminal == null ? AplicarAlinhamentoCabo(pontoSnap, inputState) : pontoSnap;

            if (terminal != null)
                _alignmentGuides.Limpar();

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
            Terminal? terminal = _snap.ObterTerminalMaisProximo(vm, position);

            if (terminal != null && EhElementoConectavel(terminal.Dono))
                return terminal;

            terminal = _snap.ObterTerminalMaisProximo(position, _caboAtual ?? _previewInicial, t => EhElementoConectavel(t.Dono));
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
            double tolerancia = _snap.TerminalTolerance;
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
            _sceneQueries.Invalidate();
        }

        private CaboViewModel ObterPreviewInicial()
        {
            if (_previewInicial != null)
                return _previewInicial;

            _previewInicial = _factory.CriarCaboVM();
            _previewInicial.IsPreview = true;
            _scene.Elementos.Add(_previewInicial);
            _sceneQueries.Invalidate();
            return _previewInicial;
        }

        private void LimparPreviewInicial()
        {
            if (_previewInicial == null)
                return;

            _previewInicial.IsPreview = false;
            _scene.Elementos.Remove(_previewInicial);
            _previewInicial = null;
            _terminalPreviewInicial = null;
            _sceneQueries.Invalidate();
        }

        private void AtualizarTerminalCapturado(Terminal? terminal)
        {
            if (terminal == null)
            {
                LimparTerminalCapturado();
                return;
            }

            _terminalSnap.Mostrar(terminal);
        }

        private void MostrarTerminalInvalido(Terminal terminal, string mensagem)
        {
            _terminalSnap.MostrarInvalido(terminal, terminal.Posicao, mensagem);
        }

        private void LimparTerminalCapturado()
        {
            _terminalSnap.Limpar();
        }

        private void AdicionarVerticeIntermediario(Point ponto)
        {
            if (_caboAtual == null)
                return;

            _caboAtual.AdicionarVerticeIntermediario(ponto);
            _sceneQueries.Invalidate();
        }

        private Point AplicarAlinhamentoCabo(Point ponto, ToolInputState inputState)
        {
            if (_caboAtual == null || _caboAtual.Cabo.Vertices.Count == 0)
                return ponto;

            Point origem = _caboAtual.Cabo.Vertices[^1];

            if (inputState.IsShiftPressed)
            {
                Point ortogonal = AplicarOrtogonalizacao(ponto, origem);
                _alignmentGuides.AplicarSnapPontoCabo(ortogonal, origem, _caboAtual);
                return ortogonal;
            }

            return _alignmentGuides.AplicarSnapPontoCabo(ponto, origem, _caboAtual);
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

            return _connectivity.ValidarTerminalDisponivel(null, terminal);
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

            return _connectivity.ValidarConexaoCabo(_caboAtual.Cabo, _terminalOrigem, terminal);
        }

        private void Finalizar()
        {
            LimparTerminalCapturado();
            LimparPreviewInicial();
            LimparEstado();
            _voltarParaSelecao();
        }

        private void LimparEstado()
        {
            _caboAtual?.RemoverPreview();
            _caboAtual = null;
            _terminalPreviewFinal = null;
            _terminalOrigem = null;
            LimparTerminalCapturado();
            _alignmentGuides.Limpar();
            _inserindo = false;
        }
    }
}
