namespace Araci.Applications.Editar.Base
{
    public interface ITool
    {
        // =========================
        // IDENTIFICAÇÃO
        // =========================

        string Nome
        {
            get;
        }

        // =========================
        // COMPORTAMENTO
        // =========================

        bool PermiteArrastar
        {
            get;
        }

        bool MantemBotaoAtivado
        {
            get;
        }

        // =========================
        // CICLO
        // =========================

        void Ativar();

        void Desativar();
    }
}