using System;
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
                if (ReferenceEquals(_ferramentaAtual, value))
                    return;

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

        public void VoltarParaSelecao()
        {
            if (_ferramentaAtual is SelecionarTool)
                return;

            FerramentaAtual = new SelecionarTool(_context);
        }
    }
}
