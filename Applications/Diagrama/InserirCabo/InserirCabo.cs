using System.Windows;
using System.Windows.Input;
using Araci.Applications.Editar.Base;
using Araci.Core.Commands;
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
        private bool _aguardandoPrimeiroMove;

        public InserirCaboTool(EditorContext context)
        {
            _context = context;
        }

        public string Nome => "Inserir Cabo";
        public bool MantemBotaoAtivado => true;

        public void Ativar() { }

        public void Desativar()
        {
            Cancelar();
        }

        public void OnMouseDown(
            ElementoViewModel? vm,
            Point position,
            ToolInputState inputState)
        {
            var p = _context.Snap.SnapFromElemento(
                vm,
                position,
                _context.Scene);

            if (!_inserindo)
            {
                _caboAtual =
                    _context.ElementoFactory.CriarCaboVM();

                // 🔥 inicia exatamente no terminal
                _caboAtual.Iniciar(p);

                _context.Commands.Execute(
                    new AddElementoCommand(
                        _caboAtual,
                        _context));

                _inserindo = true;

                // 🔥 bloqueia preview até primeiro movimento
                _aguardandoPrimeiroMove = true;
                return;
            }

            _caboAtual?.ConfirmarSegmento(p);
        }

        public void OnMouseMove(Point position)
        {
            if (!_inserindo || _caboAtual == null)
                return;

            var p = _context.Snap.Snap(
                position,
                _context.Scene);

            // 🔥 ignora primeiro move "fantasma"
            if (_aguardandoPrimeiroMove)
            {
                _aguardandoPrimeiroMove = false;
                return;
            }

            _caboAtual.AtualizarPreview(p);
        }

        public void OnMouseUp(Point position) { }

        public void OnKeyDown(Key key)
        {
            if (key == Key.Enter)
            {
                Finalizar();
                return;
            }

            if (key == Key.Escape)
            {
                Cancelar();
            }
        }

        private void Finalizar()
        {
            if (_caboAtual == null)
                return;

            _caboAtual.RemoverPreview();
            _caboAtual = null;
            _inserindo = false;

            _context.Tools.VoltarParaSelecao();
        }

        private void Cancelar()
        {
            if (_caboAtual != null)
                _caboAtual.RemoverPreview();

            _caboAtual = null;
            _inserindo = false;
            _aguardandoPrimeiroMove = false;
        }
    }
}