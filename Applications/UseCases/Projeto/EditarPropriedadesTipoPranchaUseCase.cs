using System;
using System.Linq;
using Araci.Applications.Abstractions;
using Araci.Core.Commands;
using Araci.Core.Documents;

namespace Araci.Applications.UseCases.Projeto
{
    public sealed class EditarPropriedadesTipoPranchaUseCase
    {
        private readonly AraciDocument _document;
        private readonly ICommandHistory _commands;

        public EditarPropriedadesTipoPranchaUseCase(AraciDocument document, ICommandHistory commands)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
        }

        public bool AlterarFormato(Guid id, ProjectSheetFormat formato)
        {
            ProjectSheetType? tipo = ObterTipo(id);

            if (tipo == null)
                return false;

            return AlterarEstadoFolha(
                tipo,
                new SheetTypePageState(tipo.FormatoFolha, tipo.OrientacaoFolha, tipo.LarguraFolha, tipo.AlturaFolha),
                CriarEstadoFormato(formato, tipo.OrientacaoFolha, tipo.LarguraFolha, tipo.AlturaFolha));
        }

        public bool AlterarOrientacao(Guid id, ProjectSheetOrientation orientacao)
        {
            ProjectSheetType? tipo = ObterTipo(id);

            if (tipo == null)
                return false;

            return AlterarEstadoFolha(
                tipo,
                new SheetTypePageState(tipo.FormatoFolha, tipo.OrientacaoFolha, tipo.LarguraFolha, tipo.AlturaFolha),
                CriarEstadoFormato(tipo.FormatoFolha, orientacao, tipo.LarguraFolha, tipo.AlturaFolha));
        }

        public bool AlterarLargura(Guid id, double largura)
        {
            double normalizada = NormalizarDimensao(largura, ProjectSheet.DefaultWidth);
            ProjectSheetType? tipo = ObterTipo(id);

            if (tipo == null)
                return false;

            return AlterarEstadoFolha(
                tipo,
                new SheetTypePageState(tipo.FormatoFolha, tipo.OrientacaoFolha, tipo.LarguraFolha, tipo.AlturaFolha),
                new SheetTypePageState(ProjectSheetFormat.Personalizado, tipo.OrientacaoFolha, normalizada, tipo.AlturaFolha));
        }

        public bool AlterarAltura(Guid id, double altura)
        {
            double normalizada = NormalizarDimensao(altura, ProjectSheet.DefaultHeight);
            ProjectSheetType? tipo = ObterTipo(id);

            if (tipo == null)
                return false;

            return AlterarEstadoFolha(
                tipo,
                new SheetTypePageState(tipo.FormatoFolha, tipo.OrientacaoFolha, tipo.LarguraFolha, tipo.AlturaFolha),
                new SheetTypePageState(ProjectSheetFormat.Personalizado, tipo.OrientacaoFolha, tipo.LarguraFolha, normalizada));
        }

        private bool AlterarEstadoFolha(ProjectSheetType tipo, SheetTypePageState anterior, SheetTypePageState novo)
        {
            if (anterior.Equals(novo))
                return true;

            _commands.Execute(new UpdateProjectSheetTypePropertyCommand<SheetTypePageState>(
                _document,
                tipo,
                AplicarEstadoFolha,
                anterior,
                novo));

            return true;
        }

        private ProjectSheetType? ObterTipo(Guid id)
        {
            return _document.TiposPrancha.FirstOrDefault(t => t.Id == id);
        }

        private static SheetTypePageState CriarEstadoFormato(ProjectSheetFormat formato, ProjectSheetOrientation orientacao, double larguraAtual, double alturaAtual)
        {
            if (formato == ProjectSheetFormat.Personalizado)
                return new SheetTypePageState(formato, orientacao, larguraAtual, alturaAtual);

            (double largura, double altura) = ProjectSheet.ObterDimensoesFormato(formato, orientacao);
            return new SheetTypePageState(formato, orientacao, largura, altura);
        }

        private static void AplicarEstadoFolha(ProjectSheetType tipo, SheetTypePageState estado)
        {
            tipo.FormatoFolha = estado.Formato;
            tipo.OrientacaoFolha = estado.Orientacao;
            tipo.LarguraFolha = estado.Largura;
            tipo.AlturaFolha = estado.Altura;
        }

        private static double NormalizarDimensao(double valor, double fallback)
        {
            return double.IsNaN(valor) || double.IsInfinity(valor) || valor < ProjectSheet.MinDimension
                ? fallback
                : valor;
        }

        private readonly record struct SheetTypePageState(
            ProjectSheetFormat Formato,
            ProjectSheetOrientation Orientacao,
            double Largura,
            double Altura);
    }
}
