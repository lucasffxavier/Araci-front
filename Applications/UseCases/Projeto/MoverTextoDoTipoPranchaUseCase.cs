using System;
using System.Linq;
using Araci.Applications.Abstractions;
using Araci.Core.Commands;
using Araci.Core.Documents;

namespace Araci.Applications.UseCases.Projeto
{
    public sealed class MoverTextoDoTipoPranchaUseCase
    {
        private const double MinDeltaSquared = 0.0001;

        private readonly AraciDocument _document;
        private readonly ICommandHistory _commands;

        public MoverTextoDoTipoPranchaUseCase(AraciDocument document, ICommandHistory commands)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
        }

        public bool Mover(Guid tipoId, Guid textoId, double deltaX, double deltaY)
        {
            if (!TemDeltaValido(deltaX, deltaY))
                return false;

            if (!TryGetTexto(tipoId, textoId, out ProjectSheetType tipo, out ProjectSheetTemplateText texto))
                return false;

            return AlterarPosicao(tipo, texto, texto.X + deltaX, texto.Y + deltaY);
        }

        public bool AlterarPosicao(Guid tipoId, Guid textoId, double x, double y)
        {
            if (!TryGetTexto(tipoId, textoId, out ProjectSheetType tipo, out ProjectSheetTemplateText texto))
                return false;

            return AlterarPosicao(tipo, texto, x, y);
        }

        private bool AlterarPosicao(ProjectSheetType tipo, ProjectSheetTemplateText texto, double x, double y)
        {
            if (!ValorFinito(x) || !ValorFinito(y))
                return false;

            double deltaSquared = DistanciaQuadrada(texto.X, texto.Y, x, y);

            if (deltaSquared < MinDeltaSquared)
                return false;

            var estadoAnterior = ProjectSheetTemplateTextPositionState.FromText(texto);
            var estadoNovo = new ProjectSheetTemplateTextPositionState(x, y);

            _commands.Execute(new UpdateProjectSheetTypeTextPropertyCommand<ProjectSheetTemplateTextPositionState>(
                _document,
                tipo,
                texto.Id,
                (t, value) => value.Aplicar(t),
                estadoAnterior,
                estadoNovo));

            return true;
        }

        private bool TryGetTexto(Guid tipoId, Guid textoId, out ProjectSheetType tipo, out ProjectSheetTemplateText texto)
        {
            tipo = _document.TiposPrancha.FirstOrDefault(t => t.Id == tipoId)!;
            texto = tipo?.Textos.FirstOrDefault(t => t.Id == textoId)!;

            return tipo != null && texto != null;
        }

        private static bool TemDeltaValido(double deltaX, double deltaY)
        {
            if (!ValorFinito(deltaX) || !ValorFinito(deltaY))
                return false;

            return deltaX * deltaX + deltaY * deltaY >= MinDeltaSquared;
        }

        private static bool ValorFinito(double value)
        {
            return !double.IsNaN(value) && !double.IsInfinity(value);
        }

        private static double DistanciaQuadrada(double x1, double y1, double x2, double y2)
        {
            double dx = x2 - x1;
            double dy = y2 - y1;
            return dx * dx + dy * dy;
        }
    }
}