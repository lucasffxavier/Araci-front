using System;
using Araci.Applications.Diagrama;
using Araci.Models;
using Araci.Services;
using Araci.ViewModels;

namespace Araci.Applications.Diagrama.InserirCarga
{
    public class InserirCargaApplication
    {
        private readonly EditorContext _context;

        public InserirCargaApplication(EditorContext context)
        {
            _context = context ?? throw new System.ArgumentNullException(nameof(context));
        }

        public void Executar()
        {
            _context.Input.ToolAtual = new InserirCargaTool(_context);
        }
    }

    public class InserirCargaTool : InsertElementToolBase<CargaViewModel, Carga>
    {
        public InserirCargaTool(EditorContext context)
            : base(context, context.ElementoFactory.CriarCargaVM, vm => vm.Carga)
        {
        }

        protected override string ToolName => "Inserir Carga";

        protected override Carga CriarModeloReal() => Context.ElementoFactory.CriarCarga();
    }
}
