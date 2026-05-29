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
        private double _currentRotation;

        public InsertPreviewController(EditorContext context, Func<TViewModel> criarPreview, Func<TViewModel, TModel> obterModelo)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _criarPreview = criarPreview ?? throw new ArgumentNullException(nameof(criarPreview));
            _obterModelo = obterModelo ?? throw new ArgumentNullException(nameof(obterModelo));
        }

        public TViewModel? Preview { get; private set; }
        public bool HasPreview => Preview != null;
        public double CurrentRotation => _currentRotation;

        public bool IsPreview(ElementoViewModel? vm)
        {
            return ReferenceEquals(vm, Preview);
        }

        public void Update(Point position)
        {
            Update(position, null);
        }

        public void Update(Point position, ElementoViewModel? elementoSobMouse)
        {
            TViewModel preview = ObterPreview();
            TModel modelo = _obterModelo(preview);
            Point pontoSnap = _context.Snap.SnapFromElemento(elementoSobMouse, position, preview);
            Point posicao = _context.Geometry.CalcularTopoEsquerdoPorCentro(modelo, pontoSnap);
            modelo.PosicaoX = posicao.X;
            modelo.PosicaoY = posicao.Y;
            preview.Rotacao = _currentRotation;
            _context.TerminalLayout.AtualizarTerminais(modelo);
            preview.AtualizarAposModeloAlterado();

            Vector ajuste = _context.AlignmentGuides.AplicarSnapPreview(preview);

            if (Math.Abs(ajuste.X) > 0.000001 || Math.Abs(ajuste.Y) > 0.000001)
            {
                modelo.PosicaoX += ajuste.X;
                modelo.PosicaoY += ajuste.Y;
                _context.TerminalLayout.AtualizarTerminais(modelo);
                preview.AtualizarAposModeloAlterado();
            }

            _context.SceneQueries.Invalidate();
        }

        public bool RotateClockwise()
        {
            _currentRotation = RotationService.RotateClockwise(_currentRotation);

            if (Preview == null)
                return true;

            Preview.Rotacao = _currentRotation;
            TModel modelo = _obterModelo(Preview);
            _context.TerminalLayout.AtualizarTerminais(modelo);
            _context.AlignmentGuides.AplicarSnapPreview(Preview);
            _context.SceneQueries.Invalidate();
            return true;
        }

        public TModel ObterModeloPreview()
        {
            if (Preview == null)
                throw new InvalidOperationException("Preview de inserção não inicializado.");

            return _obterModelo(Preview);
        }

        public void Clear()
        {
            if (Preview == null)
            {
                _currentRotation = 0;
                _context.AlignmentGuides.Limpar();
                return;
            }

            Preview.IsPreview = false;
            _context.Scene.Elementos.Remove(Preview);
            Preview = null;
            _currentRotation = 0;
            _context.AlignmentGuides.Limpar();
            _context.SceneQueries.Invalidate();
        }

        private TViewModel ObterPreview()
        {
            if (Preview != null)
                return Preview;

            Preview = _criarPreview();
            Preview.Rotacao = _currentRotation;
            TModel modelo = _obterModelo(Preview);
            _context.TerminalLayout.AtualizarTerminais(modelo);
            Preview.IsPreview = true;
            _context.Scene.Elementos.Add(Preview);
            _context.SceneQueries.Invalidate();
            return Preview;
        }
    }
}