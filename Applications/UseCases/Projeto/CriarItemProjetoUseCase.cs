using System;
using Araci.Applications.Abstractions;
using Araci.Core.Commands;
using Araci.Core.Documents;

namespace Araci.Applications.UseCases.Projeto
{
    public class CriarItemProjetoUseCase
    {
        private readonly AraciDocument _document;
        private readonly ICommandHistory _commands;

        public CriarItemProjetoUseCase(AraciDocument document, ICommandHistory commands)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
        }

        public ProjectView CriarVista()
        {
            ProjectView vista = _document.CriarModeloNovaVista();
            _commands.Execute(AddProjectItemCommand.Vista(_document, vista));
            return vista;
        }

        public ProjectTable CriarTabela()
        {
            ProjectTable tabela = _document.CriarModeloNovaTabela();
            _commands.Execute(AddProjectItemCommand.Tabela(_document, tabela));
            return tabela;
        }

        public ProjectSheet CriarPrancha()
        {
            ProjectSheet prancha = _document.CriarModeloNovaPrancha();
            _commands.Execute(AddProjectItemCommand.Prancha(_document, prancha));
            return prancha;
        }

        public ProjectSheetType CriarTipoPrancha()
        {
            ProjectSheetType tipo = _document.CriarModeloNovoTipoPrancha();
            _commands.Execute(AddProjectItemCommand.TipoPrancha(_document, tipo));
            return tipo;
        }
    }
}
