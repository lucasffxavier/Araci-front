using System;
using System.Windows;
using Araci.Core.SceneQueries;
using Araci.Models;
using Araci.Services;
using Araci.ViewModels;
using CoreScene = Araci.Core.Scenes.Scene;
using Araci.Services.Geometry;
using Araci.Services.Editing;
using Araci.Services.Interaction;

namespace Araci.Applications.Diagrama
{
    public class InsertPreviewController<TViewModel, TModel>
        where TViewModel : ElementoViewModel
        where TModel : Elemento
    {
        private readonly Func<TViewModel> _criarPreview;
        private readonly Func<TViewModel, TModel> _obterModelo;
        private readonly SnapService _snap;
        private readonly ElementGeometryService _geometry;
        private readonly TerminalLayoutService _terminalLayout;
        private readonly AlignmentGuideService _alignmentGuides;
        private readonly CoreScene _scene;
        private readonly ISceneQueryService _sceneQueries;
        private Point _ultimoCentroPreview;
        private double _currentRotation;

        public InsertPreviewController(
            Func<TViewModel> criarPreview,
            Func<TViewModel, TModel> obterModelo,
            SnapService snap,
            ElementGeometryService geometry,
            TerminalLayoutService terminalLayout,
            AlignmentGuideService alignmentGuides,
            CoreScene scene,
            ISceneQueryService sceneQueries)
        {
            _criarPreview = criarPreview ?? throw new ArgumentNullException(nameof(criarPreview));
            _obterModelo = obterModelo ?? throw new ArgumentNullException(nameof(obterModelo));
            _snap = snap ?? throw new ArgumentNullException(nameof(snap));
            _geometry = geometry ?? throw new ArgumentNullException(nameof(geometry));
            _terminalLayout = terminalLayout ?? throw new ArgumentNullException(nameof(terminalLayout));
            _alignmentGuides = alignmentGuides ?? throw new ArgumentNullException(nameof(alignmentGuides));
            _scene = scene ?? throw new ArgumentNullException(nameof(scene));
            _sceneQueries = sceneQueries ?? throw new ArgumentNullException(nameof(sceneQueries));
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
            Point pontoSnap = _snap.SnapFromElemento(elementoSobMouse, position, preview);
            _ultimoCentroPreview = pontoSnap;
            preview.Rotacao = _currentRotation;
            PosicionarPorCentroVisual(preview, modelo, pontoSnap);
            _terminalLayout.AtualizarTerminais(modelo);
            preview.AtualizarAposModeloAlterado();
            PosicionarPorCentroVisual(preview, modelo, pontoSnap);
            preview.AtualizarAposModeloAlterado();

            Vector ajuste = _alignmentGuides.AplicarSnapPreview(preview);

            if (Math.Abs(ajuste.X) > 0.000001 || Math.Abs(ajuste.Y) > 0.000001)
            {
                modelo.PosicaoX += ajuste.X;
                modelo.PosicaoY += ajuste.Y;
                _ultimoCentroPreview = new Point(_ultimoCentroPreview.X + ajuste.X, _ultimoCentroPreview.Y + ajuste.Y);
                _terminalLayout.AtualizarTerminais(modelo);
                preview.AtualizarAposModeloAlterado();
            }

            _sceneQueries.Invalidate();
        }

        public bool RotateClockwise()
        {
            _currentRotation = RotationService.RotateClockwise(_currentRotation);

            if (Preview == null)
                return true;

            Preview.Rotacao = _currentRotation;
            TModel modelo = _obterModelo(Preview);
            PosicionarPorCentroVisual(Preview, modelo, _ultimoCentroPreview);
            _terminalLayout.AtualizarTerminais(modelo);
            Preview.AtualizarAposModeloAlterado();
            _alignmentGuides.AplicarSnapPreview(Preview);
            _sceneQueries.Invalidate();
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
                _alignmentGuides.Limpar();
                return;
            }

            Preview.IsPreview = false;
            _scene.Elementos.Remove(Preview);
            Preview = null;
            _currentRotation = 0;
            _alignmentGuides.Limpar();
            _sceneQueries.Invalidate();
        }

        private TViewModel ObterPreview()
        {
            if (Preview != null)
                return Preview;

            Preview = _criarPreview();
            Preview.Rotacao = _currentRotation;
            TModel modelo = _obterModelo(Preview);
            _terminalLayout.AtualizarTerminais(modelo);
            Preview.IsPreview = true;
            _scene.Elementos.Add(Preview);
            Preview.AtualizarAposModeloAlterado();
            _sceneQueries.Invalidate();
            return Preview;
        }

        private void PosicionarPorCentroVisual(TViewModel preview, TModel modelo, Point centro)
        {
            Size tamanho = ObterTamanhoAtual(preview, modelo);
            modelo.PosicaoX = centro.X - tamanho.Width / 2;
            modelo.PosicaoY = centro.Y - tamanho.Height / 2;
        }

        private Size ObterTamanhoAtual(TViewModel preview, TModel modelo)
        {
            Size tamanho = new Size(preview.Largura, preview.Altura);

            if (tamanho.Width > 0 && tamanho.Height > 0)
                return tamanho;

            tamanho = _geometry.ObterTamanho(modelo);

            if (tamanho.Width > 0 && tamanho.Height > 0)
                return tamanho;

            return new Size(1, 1);
        }
    }
}