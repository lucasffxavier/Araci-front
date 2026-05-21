using System.Collections.Generic;

namespace Araci.Models
{
    public interface ITerminalOwner
    {
        IReadOnlyList<Terminal> Terminais { get; }
    }
}