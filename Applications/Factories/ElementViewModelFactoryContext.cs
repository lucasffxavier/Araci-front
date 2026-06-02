using System;
using Araci.Services;
using Araci.Services.Geometry;
using Araci.Services.UI;

namespace Araci.Applications.Factories
{
    public class ElementViewModelFactoryContext
    {
        public ElementViewModelFactoryContext(
            NameService names,
            TypePropertiesDialogService typePropertiesDialogs,
            TerminalLayoutService terminalLayout)
        {
            Names = names ?? throw new ArgumentNullException(nameof(names));
            TypePropertiesDialogs = typePropertiesDialogs ?? throw new ArgumentNullException(nameof(typePropertiesDialogs));
            TerminalLayout = terminalLayout ?? throw new ArgumentNullException(nameof(terminalLayout));
        }

        public NameService Names { get; }
        public TypePropertiesDialogService TypePropertiesDialogs { get; }
        public TerminalLayoutService TerminalLayout { get; }
    }
}
