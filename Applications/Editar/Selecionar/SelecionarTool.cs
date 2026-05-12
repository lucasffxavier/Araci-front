using Araci.Applications.Commands;
using Araci.Applications.Editar.Base;

namespace Araci.Applications.Editar.Selecionar
{
    public class SelecionarTool : ITool
    {
        private readonly SelectCommand _select = new();
        private readonly MoveCommand _move = new();

        public string Nome => "Selecionar";

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