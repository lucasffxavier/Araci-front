using System;
using Araci.Applications.Diagrama.InserirCabo;
using Araci.Applications.Diagrama.InserirElemento;
using Araci.Applications.Editar.Base;
using Araci.Applications.Editar.Selecionar;

namespace Araci.Services
{
    public class ToolService
    {
        private readonly EditorContext _context;
        private ITool _ferramentaAtual;

        public ToolService(EditorContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _ferramentaAtual = new SelecionarTool(_context);
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

        public bool AtivarInsercaoElemento(string kind)
        {
            ElementDefinition? definition = _context.Elements.FindByKind(kind);

            if (definition == null)
                return false;

            if (definition.Kind == ElementRegistryService.KindCabo || definition.UsaFerramentaEspecial)
            {
                FerramentaAtual = new InserirCaboTool(_context);
                return true;
            }

            FerramentaAtual = new InserirElementoGenericoTool(_context, definition);
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

            FerramentaAtual = new SelecionarTool(_context);
        }
    }
}