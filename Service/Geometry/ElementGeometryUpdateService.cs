using System;
using System.Collections.Generic;
using Araci.Models;
using Araci.Services;
using Araci.Services.Topology;

namespace Araci.Services.Geometry
{
    public class ElementGeometryUpdateService
    {
        private readonly TerminalLayoutService _terminalLayout;
        private readonly ConnectivityService _connectivity;
        private readonly VisualUpdateService _visualUpdates;

        public ElementGeometryUpdateService(
            TerminalLayoutService terminalLayout,
            ConnectivityService connectivity,
            VisualUpdateService visualUpdates)
        {
            _terminalLayout = terminalLayout ?? throw new ArgumentNullException(nameof(terminalLayout));
            _connectivity = connectivity ?? throw new ArgumentNullException(nameof(connectivity));
            _visualUpdates = visualUpdates ?? throw new ArgumentNullException(nameof(visualUpdates));
        }

        public void AplicarAlturaBarra(Barra barra, double altura)
        {
            if (barra == null)
                throw new ArgumentNullException(nameof(barra));

            barra.Altura = altura;
            AtualizarElementoECabos(barra);
        }

        public void AtualizarElementoECabos(Elemento elemento)
        {
            if (elemento == null)
                throw new ArgumentNullException(nameof(elemento));

            if (elemento is Barra barra)
                _terminalLayout.AtualizarTerminais(barra, _connectivity.ObterTerminalIdsOcupados(barra));
            else
                _terminalLayout.AtualizarTerminais(elemento);

            IReadOnlyList<Cabo> cabos = _connectivity.ReancorarCabosConectados(elemento);

            foreach (Cabo cabo in cabos)
                _terminalLayout.AtualizarTerminais(cabo);

            _visualUpdates.AtualizarGeometriaElementoECabos(elemento, cabos);
        }
    }
}
