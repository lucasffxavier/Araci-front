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
            _context = context ?? throw new System.ArgumentNullException(nameof(context));
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
        private bool _inserindo;

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

        public void OnMouseDown(ElementoViewModel? vm, Point position, ToolInputState inputState)
        {
            if (!_inserindo)
            {
                _caboAtual = _context.ElementoFactory.CriarCaboVM();
                _caboAtual.Iniciar(position);

                _context.Commands.Execute(new AddElementoCommand(_caboAtual, _context));

                _inserindo = true;
                return;
            }

            _caboAtual?.ConfirmarSegmento(position);
        }

        public void OnMouseMove(Point position)
        {
            if (!_inserindo || _caboAtual == null) return;

            _caboAtual.AtualizarPreview(position);
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
            if (_caboAtual == null) return;

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
        }
    }
}