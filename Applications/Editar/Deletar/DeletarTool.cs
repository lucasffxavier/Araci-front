using Araci.Applications.Commands;
using Araci.Applications.Editar.Base;

namespace Araci.Applications.Editar.Deletar
{
    public class DeletarTool : ITool
    {
        private readonly DeleteCommand _delete = new();

        public string Nome => "Deletar";

        public bool PermiteArrastar => false;

        public bool MantemBotaoAtivado => true;

        public ICommandHandler GetClickCommand()
            => _delete;

        public ICommandHandler? GetDragCommand()
            => null;

        public void Ativar() { }

        public void Desativar() { }
    }
}