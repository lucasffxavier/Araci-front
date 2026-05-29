using System;
using Araci.Applications.Diagrama;
using Araci.Models;
using Araci.Services;
using Araci.ViewModels;

namespace Araci.Applications.Diagrama.InserirSin
{
    public class InserirSinApplication
    {
        private readonly EditorContext _context;

        public InserirSinApplication(EditorContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public void Executar()
        {
            _context.Input.ToolAtual = new InserirSinTool(_context);
        }
    }

    public class InserirSinTool : InsertElementToolBase<SinViewModel, Sin>
    {
        public InserirSinTool(EditorContext context)
            : base(context, ElementRegistryService.KindSin, vm => vm.Sin)
        {
        }

        protected override string ToolName => "Inserir SIN";
    }
}