using Araci.Models;

namespace Araci.Services
{
    public class TerminalLayoutService
    {
        private readonly ElementGeometryService _geometry;

        public TerminalLayoutService(ElementGeometryService geometry)
        {
            _geometry = geometry
                ?? throw new ArgumentNullException(nameof(geometry));
        }

        public void AtualizarTerminais(Elemento elemento)
        {
            switch (elemento)
            {
                case Barra barra:
                    barra.AtualizarTerminais();
                    break;

                case Carga carga:
                    carga.AtualizarTerminais(_geometry.ObterTamanho(carga).Width);
                    break;

                case Gerador gerador:
                    var tamanho = _geometry.ObterTamanho(gerador);
                    gerador.AtualizarTerminais(tamanho.Width, tamanho.Height);
                    break;

                case Cabo cabo:
                    AtualizarTerminaisCabo(cabo);
                    break;
            }
        }

        private static void AtualizarTerminaisCabo(Cabo cabo)
        {
            if (cabo.Vertices.Count > 0)
                cabo.DefinirOrigem(cabo.Vertices[0]);

            if (cabo.Vertices.Count > 1)
                cabo.DefinirDestino(cabo.Vertices[^1]);
        }
    }
}
