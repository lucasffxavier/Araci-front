using Araci.Core.Documents;

namespace Araci.Applications.Abstractions
{
    public sealed class ExibicaoTabelaDialogResult
    {
        public ExibicaoTabelaDialogResult(ProjectTableDisplaySettings exibicao)
        {
            Exibicao = exibicao?.CriarCopia() ?? new ProjectTableDisplaySettings();
        }

        public ProjectTableDisplaySettings Exibicao { get; }
    }
}