using System;

namespace Araci.Core.Documents
{
    public class ProjectView
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Nome { get; set; } = string.Empty;
    }
}
