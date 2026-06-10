using System;
using System.Linq;
using Araci.Applications.Abstractions;
using Araci.Core.Commands;
using Araci.Core.Documents;

namespace Araci.Applications.UseCases.Projeto
{
    public sealed class InserirRetanguloNoTipoPranchaUseCase
    {
        private readonly AraciDocument _document;
        private readonly ICommandHistory _commands;

        public InserirRetanguloNoTipoPranchaUseCase(AraciDocument document, ICommandHistory commands)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
        }

        public ProjectSheetTemplateRectangle? Inserir(Guid tipoId, double x, double y)
        {
            return Inserir(
                tipoId,
                x,
                y,
                ProjectSheetTemplateRectangle.DefaultWidth,
                ProjectSheetTemplateRectangle.DefaultHeight);
        }

        public ProjectSheetTemplateRectangle? Inserir(Guid tipoId, double x, double y, double largura, double altura)
        {
            ProjectSheetType? tipo = _document.TiposPrancha.FirstOrDefault(t => t.Id == tipoId);

            if (tipo == null)
                return null;

            double larguraNormalizada = NormalizarDimensao(largura, ProjectSheetTemplateRectangle.DefaultWidth);
            double alturaNormalizada = NormalizarDimensao(altura, ProjectSheetTemplateRectangle.DefaultHeight);
            double larguraFinal = Math.Min(larguraNormalizada, Math.Max(ProjectSheetTemplateRectangle.MinDimension, tipo.LarguraFolha));
            double alturaFinal = Math.Min(alturaNormalizada, Math.Max(ProjectSheetTemplateRectangle.MinDimension, tipo.AlturaFolha));
            double xFinal = Limitar(NormalizarCoordenada(x), 0.0, Math.Max(0.0, tipo.LarguraFolha - larguraFinal));
            double yFinal = Limitar(NormalizarCoordenada(y), 0.0, Math.Max(0.0, tipo.AlturaFolha - alturaFinal));

            var retangulo = new ProjectSheetTemplateRectangle
            {
                X = xFinal,
                Y = yFinal,
                Largura = larguraFinal,
                Altura = alturaFinal
            };

            _commands.Execute(new AddProjectSheetTypeRectangleCommand(_document, tipo, retangulo));
            return retangulo;
        }

        private static double NormalizarDimensao(double valor, double fallback)
        {
            return double.IsNaN(valor) || double.IsInfinity(valor) || valor < ProjectSheetTemplateRectangle.MinDimension
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