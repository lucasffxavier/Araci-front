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

        public void Ativar()
        {
        }

        public void Desativar()
        {
            LimparEstado();
        }

        public void OnMouseDown(
            ElementoViewModel? vm,
            Point position,
            ToolInputState inputState)
        {
            Point pontoSnap =
                _context.Snap.SnapFromElemento(vm, position);

            if (!_inserindo)
            {
                _caboAtual =
                    _context.ElementoFactory.CriarCaboVM();

                _caboAtual.Iniciar(pontoSnap);

                _context.Commands.Execute(
                    new AddElementoCommand(
                        _caboAtual,
                        _context));

                ConectarOrigem(vm);

                _inserindo = true;

                return;
            }

            if (_caboAtual == null)
                return;

            _caboAtual.FinalizarNoPonto(pontoSnap);

            if (EhElementoConectavel(vm))
                ConectarDestino(vm);

            Finalizar();
        }

        public void OnMouseMove(Point position)
        {
            if (!_inserindo || _caboAtual == null)
                return;

            Point pontoSnap =
                _context.Snap.Snap(position);

            _caboAtual.AtualizarPreview(pontoSnap);
        }

        public void OnMouseUp(Point position)
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
            ElementoViewModel? vm)
        {
            return vm?.Modelo is ElementoEquipamento
                || vm?.Modelo is Barra;
        }

        private void ConectarOrigem(
            ElementoViewModel? vm)
        {
            if (_caboAtual == null || vm?.Modelo == null)
                return;

            switch (vm.Modelo)
            {
                case ElementoEquipamento equipamento:

                    _caboAtual.OrigemId =
                        equipamento.Id.ToString();

                    _caboAtual.BarraOrigem =
                        equipamento.Nome;

                    break;

                case Barra barra:

                    _caboAtual.OrigemId =
                        barra.Id.ToString();

                    _caboAtual.BarraOrigem =
                        barra.Nome;

                    break;

                default:
                    return;
            }

            _caboAtual.NotificarParametros();
        }

        private void ConectarDestino(
            ElementoViewModel? vm)
        {
            if (_caboAtual == null || vm?.Modelo == null)
                return;

            switch (vm.Modelo)
            {
                case ElementoEquipamento equipamento:

                    _caboAtual.DestinoId =
                        equipamento.Id.ToString();

                    _caboAtual.BarraDestino =
                        equipamento.Nome;

                    break;

                case Barra barra:

                    _caboAtual.DestinoId =
                        barra.Id.ToString();

                    _caboAtual.BarraDestino =
                        barra.Nome;

                    break;

                default:
                    return;
            }

            _caboAtual.NotificarParametros();
        }

        private void Finalizar()
        {
            LimparEstado();

            _context.Tools.VoltarParaSelecao();
        }

        private void Cancelar()
        {
            if (_caboAtual != null)
                _context.Commands.Undo();

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
