using Araci.Applications.Commands;
using Araci.Applications.Editar.Base;

namespace Araci.Applications.Editar.Mover
{
    public class MoverTool : ITool
    {
        private readonly MoveCommand _move = new();
        private readonly SelectCommand _select = new();

        public string Nome => "Mover";

        public bool PermiteArrastar => true;

        public bool MantemBotaoAtivado => true;

        public ICommandHandler GetClickCommand()
            => _select;

        public ICommandHandler? GetDragCommand()
            => _move;

        public void Ativar() { }

        public void Desativar() { }
    }
}