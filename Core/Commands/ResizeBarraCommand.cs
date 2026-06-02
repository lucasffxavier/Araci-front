using System;
using Araci.Models;
using Araci.Services;
using Araci.Services.Geometry;

namespace Araci.Core.Commands
{
    public class ResizeBarraCommand : IUndoableCommand
    {
        private readonly Barra _barra;
        private readonly double _alturaAntes;
        private readonly double _xAntes;
        private readonly double _yAntes;
        private readonly double _alturaDepois;
        private readonly double _xDepois;
        private readonly double _yDepois;
        private readonly ElementGeometryUpdateService _geometryUpdates;

        public ResizeBarraCommand(
            Barra barra,
            double alturaAntes,
            double xAntes,
            double yAntes,
            double alturaDepois,
            double xDepois,
            double yDepois,
            ElementGeometryUpdateService geometryUpdates)
        {
            _barra = barra ?? throw new ArgumentNullException(nameof(barra));
            _alturaAntes = alturaAntes;
            _xAntes = xAntes;
            _yAntes = yAntes;
            _alturaDepois = alturaDepois;
            _xDepois = xDepois;
            _yDepois = yDepois;
            _geometryUpdates = geometryUpdates ?? throw new ArgumentNullException(nameof(geometryUpdates));
        }

        public void Execute()
        {
            Aplicar(_alturaDepois, _xDepois, _yDepois);
        }

        public void Undo()
        {
            Aplicar(_alturaAntes, _xAntes, _yAntes);
        }

        public void Redo()
        {
            Aplicar(_alturaDepois, _xDepois, _yDepois);
        }

        private void Aplicar(double altura, double x, double y)
        {
            _barra.PosicaoX = x;
            _barra.PosicaoY = y;
            _geometryUpdates.AplicarAlturaBarra(_barra, altura);
        }
    }
}