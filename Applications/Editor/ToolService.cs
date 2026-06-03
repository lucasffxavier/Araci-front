using System;
using Araci.Applications.Abstractions;
using Araci.Applications.Editar.Base;
using Araci.Applications.Editar.Selecionar;

namespace Araci.Applications.Editor
{
    public class ToolService
    {
        private readonly IElementCatalog _elements;
        private readonly Func<ITool> _criarSelecionarTool;
        private readonly Func<ITool> _criarMoverTool;
        private readonly Func<ITool> _criarAlinharTool;
        private readonly Func<ITool> _criarDeletarTool;
        private readonly Func<ITool> _criarInserirCaboTool;
        private readonly Func<ElementDefinition, ITool> _criarInserirElementoTool;
        private readonly Func<ITool> _criarInserirLinhaAnotativaTool;
        private ITool _ferramentaAtual;

        public ToolService(
            IElementCatalog elements,
            Func<ITool> criarSelecionarTool,
            Func<ITool> criarMoverTool,
            Func<ITool> criarAlinharTool,
            Func<ITool> criarDeletarTool,
            Func<ITool> criarInserirCaboTool,
            Func<ElementDefinition, ITool> criarInserirElementoTool,
            Func<ITool> criarInserirLinhaAnotativaTool)
        {
            _elements = elements ?? throw new ArgumentNullException(nameof(elements));
            _criarSelecionarTool = criarSelecionarTool ?? throw new ArgumentNullException(nameof(criarSelecionarTool));
            _criarMoverTool = criarMoverTool ?? throw new ArgumentNullException(nameof(criarMoverTool));
            _criarAlinharTool = criarAlinharTool ?? throw new ArgumentNullException(nameof(criarAlinharTool));
            _criarDeletarTool = criarDeletarTool ?? throw new ArgumentNullException(nameof(criarDeletarTool));
            _criarInserirCaboTool = criarInserirCaboTool ?? throw new ArgumentNullException(nameof(criarInserirCaboTool));
            _criarInserirElementoTool = criarInserirElementoTool ?? throw new ArgumentNullException(nameof(criarInserirElementoTool));
            _criarInserirLinhaAnotativaTool = criarInserirLinhaAnotativaTool ?? throw new ArgumentNullException(nameof(criarInserirLinhaAnotativaTool));
            _ferramentaAtual = _criarSelecionarTool();
            _ferramentaAtual.Ativar();
        }

        public event Action<ITool>? FerramentaAlterada;

        public ITool FerramentaAtual
        {
            get => _ferramentaAtual;
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                if (ReferenceEquals(_ferramentaAtual, value))
                    return;

                if (_ferramentaAtual.IsBusy)
                    _ferramentaAtual.Cancelar();

                _ferramentaAtual.Desativar();
                _ferramentaAtual = value;
                _ferramentaAtual.Ativar();
                FerramentaAlterada?.Invoke(_ferramentaAtual);
            }
        }

        public void AtivarFerramenta(ITool ferramenta)
        {
            FerramentaAtual = ferramenta;
        }

        public void AtivarMover()
        {
            FerramentaAtual = _criarMoverTool();
        }

        public void AtivarAlinhar()
        {
            FerramentaAtual = _criarAlinharTool();
        }

        public void AtivarDeletar()
        {
            FerramentaAtual = _criarDeletarTool();
        }

        public void AtivarInserirLinhaAnotativa()
        {
            FerramentaAtual = _criarInserirLinhaAnotativaTool();
        }

        public bool AtivarInsercaoElemento(string kind)
        {
            ElementDefinition? definition = _elements.FindByKind(kind);

            if (definition == null)
                return false;

            if (definition.Kind == ElementKinds.Cabo || definition.UsaFerramentaEspecial)
            {
                FerramentaAtual = _criarInserirCaboTool();
                return true;
            }

            FerramentaAtual = _criarInserirElementoTool(definition);
            return true;
        }

        public void VoltarParaSelecao()
        {
            if (_ferramentaAtual is SelecionarTool)
            {
                if (_ferramentaAtual.IsBusy)
                    _ferramentaAtual.Cancelar();

                return;
            }

            FerramentaAtual = _criarSelecionarTool();
        }
    }
}
