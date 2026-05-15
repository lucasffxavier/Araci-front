using System;
using Araci.Applications.Editar.Base;
using Araci.Applications.Editar.Selecionar;

namespace Araci.Services
{
    public class ToolService
    {
        // =========================
        // EVENTOS
        // =========================

        public event Action<ITool>?
            FerramentaAlterada;

        // =========================
        // ESTADO
        // =========================

        private ITool _ferramentaAtual;

        private readonly EditorContext _context;

        // =========================
        // CONSTRUTOR
        // =========================

        public ToolService(EditorContext context)
        {
            _context = context
                ?? throw new ArgumentNullException(nameof(context));

            _ferramentaAtual =
                new SelecionarTool(_context);

            _ferramentaAtual.Ativar();
        }

        // =========================
        // TOOL ATUAL
        // =========================

        public ITool FerramentaAtual
        {
            get => _ferramentaAtual;

            set
            {
                if (_ferramentaAtual == value)
                    return;

                _ferramentaAtual.Desativar();

                _ferramentaAtual = value;

                _ferramentaAtual.Ativar();

                FerramentaAlterada?.Invoke(
                    _ferramentaAtual);
            }
        }

        // =========================
        // ATIVAR
        // =========================

        public void AtivarFerramenta(ITool ferramenta)
        {
            FerramentaAtual = ferramenta;
        }

        // =========================
        // VOLTAR SELEÇÃO
        // =========================

        public void VoltarParaSelecao()
        {
            if (_ferramentaAtual is SelecionarTool)
                return;

            FerramentaAtual =
                new SelecionarTool(_context);
        }

    }
}
