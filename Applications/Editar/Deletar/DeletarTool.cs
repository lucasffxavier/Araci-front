using Araci.Applications.Editar.Base;

namespace Araci.Applications.Editar.Deletar
{
    public class DeletarTool
        : ITool
    {
        // =========================
        // IDENTIFICAÇÃO
        // =========================

        public string Nome
        {
            get;
        }
        = "Deletar";

        // =========================
        // COMPORTAMENTO
        // =========================

        public bool PermiteArrastar
        {
            get;
        }
        = false;

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