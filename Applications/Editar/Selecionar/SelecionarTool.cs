using Araci.Applications.Editar.Base;

namespace Araci.Applications.Editar.Selecionar
{
    public class SelecionarTool
        : ITool
    {
        // =========================
        // IDENTIFICAÇÃO
        // =========================

        public string Nome
        {
            get;
        }
        = "Selecionar";

        // =========================
        // COMPORTAMENTO
        // =========================

        // =========================
        // MOVE NORMALMENTE
        // =========================

        public bool PermiteArrastar
        {
            get;
        }
        = true;

        public bool MantemBotaoAtivado
        {
            get;
        }
        = true;

        // =========================
        // CICLO
        // =========================

        public void Ativar()
        {

        }

        public void Desativar()
        {

        }
    }
}