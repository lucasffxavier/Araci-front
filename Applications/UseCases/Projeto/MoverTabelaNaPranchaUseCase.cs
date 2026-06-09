using System;
using System.Linq;
using Araci.Applications.Abstractions;
using Araci.Core.Commands;
using Araci.Core.Documents;

namespace Araci.Applications.UseCases.Projeto
{
    public sealed class MoverTabelaNaPranchaUseCase
    {
        public const double LarguraPadraoPrancha = 1122.0;
        public const double AlturaPadraoPrancha = 794.0;

        private const double Tolerancia = 0.000001;

        private readonly AraciDocument _document;
        private readonly ICommandHistory _commands;

        public MoverTabelaNaPranchaUseCase(AraciDocument document, ICommandHistory commands)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
        }

        public bool Mover(Guid sheetId, Guid instanceId, double novoX, double novoY, Action? onChanged = null)
        {
            return Mover(sheetId, instanceId, novoX, novoY, LarguraPadraoPrancha, AlturaPadraoPrancha, onChanged);
        }

        public bool Mover(
            Guid sheetId,
            Guid instanceId,
            double novoX,
            double novoY,
            double larguraPrancha,
            double alturaPrancha,
            Action? onChanged = null)
        {
            ProjectSheet? sheet = _document.Pranchas.FirstOrDefault(p => p.Id == sheetId);
            ProjectSheetTableInstance? instance = sheet?.Tabelas.FirstOrDefault(i => i.Id == instanceId);

            if (sheet == null || instance == null)
                return false;

            if (!double.IsFinite(novoX) || !double.IsFinite(novoY))
                return false;

            double xNormalizado = NormalizePosition(novoX);
            double yNormalizado = NormalizePosition(novoY);

            if (Math.Abs(instance.X - xNormalizado) < Tolerancia && Math.Abs(instance.Y - yNormalizado) < Tolerancia)
                return false;

            _commands.Execute(new MoveProjectSheetTableInstanceCommand(
                instance,
                instance.X,
                instance.Y,
                xNormalizado,
                yNormalizado,
                onChanged));

            return true;
        }

        public static double ClampPosition(double valor, double dimensaoInstancia, double dimensaoPrancha)
        {
            return NormalizePosition(valor);
        }

        public static double NormalizePosition(double valor)
        {
            return double.IsNaN(valor) || double.IsInfinity(valor)
                ? 0
                : valor;
        }
    }
}
