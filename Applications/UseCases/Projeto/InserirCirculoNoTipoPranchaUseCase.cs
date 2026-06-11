using System;
using System.Linq;
using Araci.Applications.Abstractions;
using Araci.Core.Commands;
using Araci.Core.Documents;

namespace Araci.Applications.UseCases.Projeto
{
    public sealed class InserirCirculoNoTipoPranchaUseCase
    {
        private readonly AraciDocument _document;
        private readonly ICommandHistory _commands;

        public InserirCirculoNoTipoPranchaUseCase(AraciDocument document, ICommandHistory commands)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
        }

        public ProjectSheetTemplateCircle? Inserir(Guid tipoId, double x, double y)
        {
            return Inserir(tipoId, x, y, ProjectSheetTemplateCircle.DefaultRadius);
        }

        public ProjectSheetTemplateCircle? Inserir(Guid tipoId, double x, double y, double raio)
        {
            ProjectSheetType? tipo = _document.TiposPrancha.FirstOrDefault(t => t.Id == tipoId);

            if (tipo == null)
                return null;

            double raioNormalizado = NormalizarRaio(raio, ProjectSheetTemplateCircle.DefaultRadius);
            double raioMaximo = CalcularRaioMaximo(tipo);
            double raioFinal = Math.Min(raioNormalizado, raioMaximo);
            double xFinal = Limitar(NormalizarCoordenada(x), raioFinal, Math.Max(raioFinal, tipo.LarguraFolha - raioFinal));
            double yFinal = Limitar(NormalizarCoordenada(y), raioFinal, Math.Max(raioFinal, tipo.AlturaFolha - raioFinal));

            var circulo = new ProjectSheetTemplateCircle
            {
                X = xFinal,
                Y = yFinal,
                Raio = raioFinal
            };

            _commands.Execute(new AddProjectSheetTypeCircleCommand(_document, tipo, circulo));
            return circulo;
        }

        private static double CalcularRaioMaximo(ProjectSheetType tipo)
        {
            double largura = NormalizarDimensaoFolha(tipo.LarguraFolha);
            double altura = NormalizarDimensaoFolha(tipo.AlturaFolha);
            double menorDimensao = Math.Min(largura, altura);
            return Math.Max(ProjectSheetTemplateCircle.MinRadius, menorDimensao / 2.0);
        }

        private static double NormalizarDimensaoFolha(double valor)
        {
            return double.IsNaN(valor) || double.IsInfinity(valor) || valor < ProjectSheet.MinDimension
                ? ProjectSheet.MinDimension
                : valor;
        }

        private static double NormalizarRaio(double valor, double fallback)
        {
            return double.IsNaN(valor) || double.IsInfinity(valor) || valor < ProjectSheetTemplateCircle.MinRadius
                ? fallback
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

            return valor > maximo ? maximo : valor;
        }
    }
}