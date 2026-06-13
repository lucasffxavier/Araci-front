using System.Collections.Generic;
using System.Linq;
using Araci.Applications.Abstractions;
using Araci.Core.Documents;

namespace Araci
{
    public partial class MainWindow
    {
        public void InserirVistaNaPrancha()
        {
            if (_context.Document.Pranchas.Count == 0)
            {
                _context.Dialogs.ShowWarning("Inserir vista na prancha", "Crie uma prancha antes de inserir uma vista.");
                return;
            }

            if (_context.Document.Vistas.Count == 0)
            {
                _context.Dialogs.ShowWarning("Inserir vista na prancha", "Crie uma vista antes de inseri-la em uma prancha.");
                return;
            }

            List<ProjectItemDialogOption> pranchas = _context.Document.Pranchas
                .Select(p => new ProjectItemDialogOption(p.Id, string.IsNullOrWhiteSpace(p.Numero) ? p.Nome : $"{p.Numero} - {p.Nome}"))
                .ToList();
            List<ProjectViewDialogOption> vistas = _context.Document.Vistas
                .Select(v => new ProjectViewDialogOption(v.Id, v.Nome))
                .ToList();

            InserirVistaPranchaDialogResult? result = _context.Dialogs.ShowInserirVistaPranchaDialog(pranchas, vistas);

            if (result == null || result.ViewId == System.Guid.Empty)
                return;

            ProjectSheetViewInstance? instance = _context.InserirVistaNaPrancha.Inserir(result.SheetId, result.ViewId, onChanged: AtualizarPranchaAtual);

            if (instance == null)
            {
                _context.Dialogs.ShowWarning("Inserir vista na prancha", "Nao foi possivel inserir a vista na prancha selecionada.");
                return;
            }

            if (_projectSheetViewModel?.SheetId == result.SheetId)
                _projectSheetViewModel.Refresh();

            _context.Dialogs.ShowInfo("Inserir vista na prancha", "Vista inserida na prancha.");
        }
    }
}