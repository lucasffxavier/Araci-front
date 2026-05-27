using System;
using System.Collections.Generic;
using Araci.Models;

namespace Araci.Services
{
    public class ElectricGraphNode
    {
        public ElectricGraphNode(
            string elementId,
            Guid elementGuid,
            string name,
            string kind,
            Elemento sourceElement,
            IReadOnlyList<ElectricGraphTerminal> terminals)
        {
            ElementId = elementId;
            ElementGuid = elementGuid;
            Name = name;
            Kind = kind;
            SourceElement = sourceElement;
            Terminals = terminals;
        }

        public string ElementId { get; }
        public Guid ElementGuid { get; }
        public string Name { get; }
        public string Kind { get; }
        public Elemento SourceElement { get; }
        public IReadOnlyList<ElectricGraphTerminal> Terminals { get; }
    }
}
