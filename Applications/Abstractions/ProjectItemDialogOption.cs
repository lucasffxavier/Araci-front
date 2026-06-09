using System;

namespace Araci.Applications.Abstractions
{
    public sealed class ProjectItemDialogOption
    {
        public ProjectItemDialogOption(Guid id, string nome)
        {
            Id = id;
            Nome = nome;
        }

        public Guid Id { get; }
        public string Nome { get; }
    }
}
