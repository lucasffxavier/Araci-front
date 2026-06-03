using System;
using System.Collections.Generic;
using System.Windows;
using Araci.Applications.Editar.Base;
using Araci.ViewModels;

namespace Araci.Applications.Editar.Selecionar
{
    public class LinhaEndpointInteractionController
    {
        private const double HandleTolerance = 8.0;
        private const double HandleToleranceSquared = HandleTolerance * HandleTolerance;

        public LinhaAnotativaViewModel? LinhaAtiva { get; private set; }
        public LinhaEndpointKind? PontaAtiva { get; private set; }
        public bool IsDragging => LinhaAtiva != null && PontaAtiva.HasValue;

        public LinhaEndpointHandleViewModel? HitTest(IEnumerable<LinhaEndpointHandleViewModel> handles, Point position)
        {
            LinhaEndpointHandleViewModel? melhor = null;
            double melhorDistancia = HandleToleranceSquared;

            foreach (LinhaEndpointHandleViewModel handle in handles)
            {
                double dx = handle.X - position.X;
                double dy = handle.Y - position.Y;
                double distancia = dx * dx + dy * dy;

                if (distancia <= melhorDistancia)
                {
                    melhorDistancia = distancia;
                    melhor = handle;
                }
            }

            return melhor;
        }

        public void BeginDrag(LinhaEndpointHandleViewModel handle)
        {
            LinhaAtiva = handle.Linha;
            PontaAtiva = handle.Kind;
        }

        public Point AplicarRestricaoOrtogonal(Point position, ToolInputState inputState)
        {
            if (!inputState.IsShiftPressed || LinhaAtiva == null || !PontaAtiva.HasValue)
                return position;

            Point referencia = PontaAtiva.Value == LinhaEndpointKind.Fim
                ? LinhaAtiva.PontoInicial
                : LinhaAtiva.PontoFinal;

            Vector delta = position - referencia;

            if (Math.Abs(delta.X) >= Math.Abs(delta.Y))
                return new Point(position.X, referencia.Y);

            return new Point(referencia.X, position.Y);
        }

        public void ClearDrag()
        {
            LinhaAtiva = null;
            PontaAtiva = null;
        }
    }
}
