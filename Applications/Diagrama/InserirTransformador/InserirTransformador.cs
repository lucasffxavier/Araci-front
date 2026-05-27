using System;
using Araci.Applications.Diagrama;
using Araci.Models;
using Araci.Services;
using Araci.ViewModels;

namespace Araci.Applications.Diagrama.InserirTransformador
{
    public class InserirTransformadorApplication
    {
        private readonly EditorContext _context;

        public InserirTransformadorApplication(EditorContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public void Executar()
        {
            _context.Input.ToolAtual = new InserirTransformadorTool(_context);
        }
    }

    public class InserirTransformadorTool : InsertElementToolBase<TransformadorViewModel, Transformador>
    {
        public InserirTransformadorTool(EditorContext context)
            : base(context, context.ElementoFactory.CriarTransformadorVM, vm => vm.Transformador)
        {
        }

        protected override string ToolName => "Inserir Transformador";

        protected override Transformador CriarModeloReal() => Context.ElementoFactory.CriarTransformador();
    }
}
