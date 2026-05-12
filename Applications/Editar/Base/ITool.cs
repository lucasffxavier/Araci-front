using Araci.Applications.Commands;

namespace Araci.Applications.Editar.Base
{
    public interface ITool
    {
        string Nome { get; }

        bool PermiteArrastar { get; }

        bool MantemBotaoAtivado { get; }

        ICommandHandler GetClickCommand();

        ICommandHandler? GetDragCommand();

        void Ativar();

        void Desativar();
    }
}