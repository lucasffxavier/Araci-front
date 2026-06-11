using System;
using System.Linq;
using Araci.Applications.Abstractions;
using Araci.Core.Commands;
using Araci.Core.Documents;

namespace Araci.Applications.UseCases.Projeto
{
    public sealed class InserirTextoNoTipoPranchaUseCase
    {
        private readonly AraciDocument _document;
        private readonly ICommandHistory _commands;

        public InserirTextoNoTipoPranchaUseCase(AraciDocument document, ICommandHistory commands)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
        }

        public ProjectSheetTemplateText? Inserir(Guid tipoId, double x, double y)
        {
            return InserirComLarguraNatural(tipoId, x, y, ProjectSheetTemplateText.DefaultText);
        }

        public ProjectSheetTemplateText? InserirComLarguraNatural(Guid tipoId, double x, double y, string texto)
        {
            string textoFinal = NormalizarTextoInicial(texto);
            double larguraNatural = ProjectSheetTemplateText.CalcularLarguraNatural(textoFinal, ProjectSheetTemplateText.DefaultTextHeight);
            return Inserir(tipoId, x, y, textoFinal, larguraNatural);
        }

        public ProjectSheetTemplateText? Inserir(Guid tipoId, double x, double y, string texto, double larguraCaixa)
        {
            ProjectSheetType? tipo = _document.TiposPrancha.FirstOrDefault(t => t.Id == tipoId);

            if (tipo == null)
                return null;

            string textoFinal = NormalizarTextoInicial(texto);
            double larguraFinal = CalcularLarguraFinal(tipo, larguraCaixa);
            double xFinal = Limitar(NormalizarCoordenada(x), 0.0, Math.Max(0.0, tipo.LarguraFolha - larguraFinal));
            double yFinal = Limitar(NormalizarCoordenada(y), 0.0, Math.Max(0.0, tipo.AlturaFolha));

            var item = new ProjectSheetTemplateText
            {
                X = xFinal,
                Y = yFinal,
                Texto = textoFinal,
                LarguraCaixa = larguraFinal
            };

            _commands.Execute(new AddProjectSheetTypeTextCommand(_document, tipo, item));
            return item;
        }

        private static double CalcularLarguraFinal(ProjectSheetType tipo, double larguraCaixa)
        {
            double larguraNormalizada = ProjectSheetTemplateText.NormalizarLargura(larguraCaixa);
            double larguraFolha = NormalizarDimensaoFolha(tipo.LarguraFolha);
            double larguraMaxima = Math.Max(ProjectSheetTemplateText.MinBoxWidth, larguraFolha);
            return Math.Min(larguraNormalizada, larguraMaxima);
        }

        private static string NormalizarTextoInicial(string? texto)
        {
            return string.IsNullOrWhiteSpace(texto) ? ProjectSheetTemplateText.DefaultText : texto.Trim();
        }

        private static double NormalizarDimensaoFolha(double valor)
        {
            return double.IsNaN(valor) || double.IsInfinity(valor) || valor < ProjectSheet.MinDimension
                ? ProjectSheet.MinDimension
                : valor;
        }

        private static double NormalizarCoordenada(double valor)
        {
            return double.IsNaN(valor) || double.IsInfinity(valor) ? 0.0 : valor;
        }

        private static double Limitar(double valor, double minimo, double maximo)
        {
            if (maximo < minimo)
                return minimo;

            if (valor < minimo)
                return minimo;

            if (valor > maximo)
                return maximo;

            return valor;
        }
    }
}