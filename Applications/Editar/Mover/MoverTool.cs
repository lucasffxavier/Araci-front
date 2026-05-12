using Araci.Applications.Editar.Base;

namespace Araci.Applications.Editar.Mover
{
    public class MoverTool
        : ITool
    {
        // =========================
        // IDENTIFICAÇÃO
        // =========================

        public string Nome
        {
            get;
        }
        = "Mover";

        // =========================
        // COMPORTAMENTO
        // =========================

        // =========================
        // MOVE COM MODO ESPECIAL
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