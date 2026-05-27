using System;
using System.Windows;
using Araci.Models;
using Araci.Services;
using Araci.ViewModels;

namespace Araci.Applications.Diagrama
{
    public class InsertPreviewController<TViewModel, TModel>
        where TViewModel : ElementoViewModel
        where TModel : Elemento
    {
        private readonly EditorContext _context;
        private readonly Func<TViewModel> _criarPreview;
        private readonly Func<TViewModel, TModel> _obterModelo;

        public InsertPreviewController(
            EditorContext context,
            Func<TViewModel> criarPreview,
            Func<TViewModel, TModel> obterModelo)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _criarPreview = criarPreview ?? throw new ArgumentNullException(nameof(criarPreview));
            _obterModelo = obterModelo ?? throw new ArgumentNullException(nameof(obterModelo));
        }

        public TViewModel? Preview { get; private set; }

        public bool HasPreview => Preview != null;

        public bool IsPreview(ElementoViewModel? vm)
        {
            return ReferenceEquals(vm, Preview);
        }

        public void Update(Point position)
        {
            TViewModel preview = ObterPreview();
            TModel modelo = _obterModelo(preview);
            Point pontoSnap = _context.Snap.Snap(position, preview);
            Point posicao = _context.Geometry.CalcularTopoEsquerdoPorCentro(modelo, pontoSnap);

            modelo.PosicaoX = posicao.X;
            modelo.PosicaoY = posicao.Y;
            preview.AtualizarAposModeloAlterado();
            _context.SceneQueries.Invalidate();
        }

        public bool RotateClockwise()
        {
            if (Preview == null)
                return false;

            TModel modelo = _obterModelo(Preview);
            modelo.Rotacao = RotationService.RotateClockwise(modelo.Rotacao);
            _context.TerminalLayout.AtualizarTerminais(modelo);
            Preview.AtualizarAposModeloAlterado();
            _context.SceneQueries.Invalidate();
            return true;
        }

        public double CurrentRotation
        {
            get
            {
                if (Preview == null)
                    return 0;

                return _obterModelo(Preview).Rotacao;
            }
        }

        public void Clear()
        {
            if (Preview == null)
                return;

            Preview.IsPreview = false;
            _context.Scene.Elementos.Remove(Preview);
            Preview = null;
            _context.SceneQueries.Invalidate();
        }

        private TViewModel ObterPreview()
        {
            if (Preview != null)
                return Preview;

            Preview = _criarPreview();
            Preview.IsPreview = true;
            _context.Scene.Elementos.Add(Preview);
            _context.SceneQueries.Invalidate();

            return Preview;
        }
    }
}
