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
        private bool _aguardandoPrimeiroMove;
        private ElementoViewModel? _origem;

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
            var p = _context.Snap.SnapFromElemento(vm, position, _context.Scene);

            if (!_inserindo)
            {
                _caboAtual = _context.ElementoFactory.CriarCaboVM();
                _caboAtual.Iniciar(p);
                _context.Commands.Execute(new AddElementoCommand(_caboAtual, _context));

                _origem = vm;
                ConectarOrigem(vm);

                _inserindo = true;
                _aguardandoPrimeiroMove = true;
                return;
            }

            _caboAtual.ConfirmarSegmento(p);
            ConectarDestino(vm);
        }

        public void OnMouseMove(Point position)
        {
            if (!_inserindo || _caboAtual == null)
                return;

            var p = _context.Snap.Snap(position, _context.Scene);

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
                Cancelar();
        }

        private void ConectarOrigem(ElementoViewModel? vm)
        {
            if (_caboAtual == null || vm?.Modelo is not ElementoEquipamento equipamento)
                return;

            _caboAtual.BarraOrigem = equipamento.Nome;
            equipamento.Barra = _caboAtual.Nome;
            _caboAtual.NotificarParametros();
            vm.NotificarParametros();
        }

        private void ConectarDestino(ElementoViewModel? vm)
        {
            if (_caboAtual == null || vm?.Modelo is not ElementoEquipamento equipamento)
                return;

            _caboAtual.BarraDestino = equipamento.Nome;
            equipamento.Barra = _caboAtual.Nome;

            _caboAtual.NotificarParametros();
            vm.NotificarParametros();
        }

        private void Finalizar()
        {
            if (_caboAtual == null)
                return;

            _caboAtual.RemoverPreview();
            _caboAtual = null;
            _origem = null;
            _inserindo = false;
            _context.Tools.VoltarParaSelecao();
        }

        private void Cancelar()
        {
            _caboAtual?.RemoverPreview();
            _caboAtual = null;
            _origem = null;
            _inserindo = false;
            _aguardandoPrimeiroMove = false;
        }
    }
}