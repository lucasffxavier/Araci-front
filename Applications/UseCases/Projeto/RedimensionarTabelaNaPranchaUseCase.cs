using System;
using System.Linq;
using Araci.Applications.Abstractions;
using Araci.Core.Commands;
using Araci.Core.Documents;

namespace Araci.Applications.UseCases.Projeto
{
    public sealed class RedimensionarTabelaNaPranchaUseCase
    {
        private readonly AraciDocument _document;
        private readonly ICommandHistory _commands;

        public RedimensionarTabelaNaPranchaUseCase(AraciDocument document, ICommandHistory commands)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
        }

        public bool Redimensionar(Guid sheetId, Guid instanceId, double novaLargura, double novaAltura, Action? onChanged = null)
        {
            ProjectSheet? sheet = _document.Pranchas.FirstOrDefault(p => p.Id == sheetId);
            ProjectSheetTableInstance? instance = sheet?.Tabelas.FirstOrDefault(i => i.Id == instanceId);

            if (sheet == null || instance == null)
                return false;

            double larguraNormalizada = NormalizarDimensao(novaLargura, ProjectSheetTableInstance.MinWidth);
            double alturaNormalizada = NormalizarDimensao(novaAltura, ProjectSheetTableInstance.MinHeight);

            if (Math.Abs(instance.Width - larguraNormalizada) < 0.000001 &&
                Math.Abs(instance.Height - alturaNormalizada) < 0.000001)
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

        private static double NormalizarDimensao(double valor, double minimo)
        {
            return double.IsNaN(valor) || double.IsInfinity(valor) || valor < minimo
                ? minimo
                : valor;
        }
    }
}