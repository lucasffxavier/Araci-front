using System.Collections.Generic;
using System.Linq;

namespace Araci.Services.Topology
{
    public class TopologyValidationResult
    {
        private readonly List<TopologyIssue> _issues = new();

        public IReadOnlyList<TopologyIssue> Issues => _issues;

        public IEnumerable<TopologyIssue> Errors =>
            _issues.Where(i => i.Severity == TopologyIssueSeverity.Error);

        public IEnumerable<TopologyIssue> Warnings =>
            _issues.Where(i => i.Severity == TopologyIssueSeverity.Warning);

        public bool IsValid => !Errors.Any();

        public void AddError(string message)
        {
            _issues.Add(new TopologyIssue(TopologyIssueSeverity.Error, message));
        }

        public void AddWarning(string message)
        {
            _issues.Add(new TopologyIssue(TopologyIssueSeverity.Warning, message));
        }

        public string FormatErrors()
        {
            return string.Join(
                System.Environment.NewLine,
                Errors.Select(i => $"- {i.Message}"));
        }
    }
}
