using System;

namespace Araci.Core.Documents
{
    public class ProjectSheet
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Nome { get; set; } = string.Empty;
        public string Numero { get; set; } = string.Empty;
    }
}
