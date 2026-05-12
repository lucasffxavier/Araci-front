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

        public event Action<ITool>? FerramentaAlterada;

        // =========================
        // CAMPOS
        // =========================

        private ITool _ferramentaAtual;

        // =========================
        // CONSTRUTOR
        // =========================

        public ToolService()
        {
            _ferramentaAtual =
                new SelecionarTool();

            _ferramentaAtual
                .Ativar();
        }

        // =========================
        // FERRAMENTA ATUAL
        // =========================

        public ITool FerramentaAtual
        {
            get
            {
                return _ferramentaAtual;
            }

            set
            {
                if (_ferramentaAtual == value)
                    return;

                _ferramentaAtual.Desativar();

                _ferramentaAtual =
                    value;

                _ferramentaAtual.Ativar();

                FerramentaAlterada
                    ?.Invoke(_ferramentaAtual);
            }
        }

        // =========================
        // ATIVAR
        // =========================

        public void AtivarFerramenta(
            ITool ferramenta)
        {
            FerramentaAtual =
                ferramenta;
        }

        // =========================
        // SELEÇÃO
        // =========================

        public void VoltarParaSelecao()
        {
            if (_ferramentaAtual
                is SelecionarTool)
            {
                return;
            }

            FerramentaAtual =
                new SelecionarTool();
        }

        // =========================
        // VISUAL
        // =========================

        public bool FerramentaAtivaMantida()
        {
            return FerramentaAtual
                .MantemBotaoAtivado;
        }
    }
}