using System.Text;
using System.Windows;
using Araci.DTOs;

namespace Araci.Services.Simulation
{
    public class SimulationMessageBuilder
    {
        public SimulationMessage Build(SimulationResultDto resultado, string? dssPath)
        {
            var message = new StringBuilder();

            message.AppendLine(resultado.Sucesso ? "Fluxo resolvido com sucesso." : "Fluxo retornou falha.");

            if (!string.IsNullOrWhiteSpace(resultado.Mensagem))
                message.AppendLine(resultado.Mensagem);

            if (!string.IsNullOrWhiteSpace(dssPath))
            {
                message.AppendLine();
                message.AppendLine("Arquivo DSS salvo em:");
                message.AppendLine(dssPath);
            }

            if (resultado.Avisos.Count > 0)
            {
                message.AppendLine();
                message.AppendLine("Avisos:");

                foreach (string aviso in resultado.Avisos)
                    message.AppendLine($"- {aviso}");
            }

            if (!string.IsNullOrWhiteSpace(resultado.Script))
            {
                message.AppendLine();
                message.AppendLine("Script DSS gerado:");
                message.AppendLine(resultado.Script);
            }

            return new SimulationMessage(
                "Fluxo de corrente",
                message.ToString(),
                resultado.Sucesso ? MessageBoxImage.Information : MessageBoxImage.Warning);
        }
    }

    public class SimulationMessage
    {
        public SimulationMessage(string title, string text, MessageBoxImage icon)
        {
            Title = title;
            Text = text;
            Icon = icon;
        }

        public string Title { get; }

        public string Text { get; }

        public MessageBoxImage Icon { get; }
    }
}
