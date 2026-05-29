using System;
using Araci.Applications.Diagrama;
using Araci.Models;
using Araci.Services;
using Araci.ViewModels;

namespace Araci.Applications.Diagrama.InserirBarra
{
    public class InserirBarraApplication
    {
        private readonly EditorContext _context;

        public InserirBarraApplication(EditorContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public void Executar()
        {
            _context.Input.ToolAtual = new InserirBarraTool(_context);
        }
    }

    public class InserirBarraTool : InsertElementToolBase<BarraViewModel, Barra>
    {
        public InserirBarraTool(EditorContext context)
            : base(context, ElementRegistryService.KindBarra, vm => vm.Barra)
        {
        }

        protected override string ToolName => "Inserir Barra";
    }
}