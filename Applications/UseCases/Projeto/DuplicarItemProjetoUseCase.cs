using System;
using System.Linq;
using Araci.Applications.Abstractions;
using Araci.Core.Commands;
using Araci.Core.Documents;

namespace Araci.Applications.UseCases.Projeto
{
    public class DuplicarItemProjetoUseCase
    {
        private readonly AraciDocument _document;
        private readonly ICommandHistory _commands;

        public DuplicarItemProjetoUseCase(AraciDocument document, ICommandHistory commands)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
        }

        public bool DuplicarVista(Guid id)
        {
            ProjectView? origem = _document.Vistas.FirstOrDefault(v => v.Id == id);

            if (origem == null)
                return false;

            ProjectView duplicata = _document.CriarDuplicataVista(origem);
            int indice = _document.Vistas.IndexOf(origem) + 1;
            _commands.Execute(DuplicateProjectItemCommand.Vista(_document, duplicata, indice));
            return true;
        }

        public bool DuplicarTabela(Guid id)
        {
            ProjectTable? origem = _document.Tabelas.FirstOrDefault(t => t.Id == id);

            if (origem == null)
                return false;

            ProjectTable duplicata = _document.CriarDuplicataTabela(origem);
            int indice = _document.Tabelas.IndexOf(origem) + 1;
            _commands.Execute(DuplicateProjectItemCommand.Tabela(_document, duplicata, indice));
            return true;
        }

        public bool DuplicarPrancha(Guid id)
        {
            ProjectSheet? origem = _document.Pranchas.FirstOrDefault(p => p.Id == id);

            if (origem == null)
                return false;

            ProjectSheet duplicata = _document.CriarDuplicataPrancha(origem);
            int indice = _document.Pranchas.IndexOf(origem) + 1;
            _commands.Execute(DuplicateProjectItemCommand.Prancha(_document, duplicata, indice));
            return true;
        }

        public bool DuplicarTipoPrancha(Guid id)
        {
            ProjectSheetType? origem = _document.TiposPrancha.FirstOrDefault(t => t.Id == id);

            if (origem == null)
                return false;

            ProjectSheetType duplicata = _document.CriarDuplicataTipoPrancha(origem);
            int indice = _document.TiposPrancha.IndexOf(origem) + 1;
            _commands.Execute(DuplicateProjectItemCommand.TipoPrancha(_document, duplicata, indice));
            return true;
        }
    }
}
