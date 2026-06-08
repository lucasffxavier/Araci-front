using System;

namespace Araci.Core.Documents
{
    public class ProjectTable
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Nome { get; set; } = string.Empty;
    }
}
