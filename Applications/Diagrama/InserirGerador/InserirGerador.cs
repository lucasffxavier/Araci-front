using System;
using Araci.Applications.Diagrama;
using Araci.Models;
using Araci.Services;
using Araci.ViewModels;

namespace Araci.Applications.Diagrama.InserirGerador
{
    public class InserirGeradorApplication
    {
        private readonly EditorContext _context;

        public InserirGeradorApplication(EditorContext context)
        {
            _context = context ?? throw new System.ArgumentNullException(nameof(context));
        }

        public void Executar()
        {
            _context.Input.ToolAtual = new InserirGeradorTool(_context);
        }
    }

    public class InserirGeradorTool : InsertElementToolBase<GeradorViewModel, Gerador>
    {
        public InserirGeradorTool(EditorContext context)
            : base(context, context.ElementoFactory.CriarGeradorVM, vm => vm.Gerador)
        {
        }

        protected override string ToolName => "Inserir Gerador";

        protected override Gerador CriarModeloReal() => Context.ElementoFactory.CriarGerador();
    }
}
