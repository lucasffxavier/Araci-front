using System;
using System.Linq;
using Araci.Applications.Abstractions;
using Araci.Core.Commands;
using Araci.Core.Documents;

namespace Araci.Applications.UseCases.Projeto
{
    public sealed class RedimensionarTabelaNaPranchaUseCase
    {
        private const double Tolerancia = 0.000001;

        private readonly AraciDocument _document;
        private readonly ICommandHistory _commands;

        public RedimensionarTabelaNaPranchaUseCase(AraciDocument document, ICommandHistory commands)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
        }

        public bool Redimensionar(Guid sheetId, Guid instanceId, double novaLargura, double novaAltura, Action? onChanged = null)
        {
            return Redimensionar(
                sheetId,
                instanceId,
                novaLargura,
                novaAltura,
                MoverTabelaNaPranchaUseCase.LarguraPadraoPrancha,
                MoverTabelaNaPranchaUseCase.AlturaPadraoPrancha,
                onChanged);
        }

        public bool Redimensionar(
            Guid sheetId,
            Guid instanceId,
            double novaLargura,
            double novaAltura,
            double larguraPrancha,
            double alturaPrancha,
            Action? onChanged = null)
        {
            ProjectSheet? sheet = _document.Pranchas.FirstOrDefault(p => p.Id == sheetId);
            ProjectSheetTableInstance? instance = sheet?.Tabelas.FirstOrDefault(i => i.Id == instanceId);

            if (sheet == null || instance == null)
                return false;

            if (!double.IsFinite(novaLargura) || !double.IsFinite(novaAltura))
                return false;

            double larguraNormalizada = NormalizeDimension(novaLargura, ProjectSheetTableInstance.MinWidth);
            double alturaNormalizada = NormalizeDimension(novaAltura, ProjectSheetTableInstance.MinHeight);

            if (Math.Abs(instance.Width - larguraNormalizada) < Tolerancia &&
                Math.Abs(instance.Height - alturaNormalizada) < Tolerancia)
                return false;

            _commands.Execute(new ResizeProjectSheetTableInstanceCommand(
                instance,
                instance.Width,
                instance.Height,
                larguraNormalizada,
                alturaNormalizada,
                onChanged));

            return true;
        }

        public static double ClampDimension(double valor, double minimo, double maximo)
        {
            return NormalizeDimension(valor, minimo);
        }

        public static double NormalizeDimension(double valor, double minimo)
        {
            double minimoSeguro = NormalizarMinimo(minimo);

            if (double.IsNaN(valor) || double.IsInfinity(valor) || valor < minimoSeguro)
                return minimoSeguro;

            return valor;
        }

        private static double NormalizarMinimo(double valor)
        {
            if (double.IsNaN(valor) || double.IsInfinity(valor) || valor < 0)
                return 0;

            return valor;
        }
    }
}
