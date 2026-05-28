using Araci.Models;

namespace Araci.Services
{
    public class TerminalLayoutService
    {
        private readonly ElementRegistryService? _registry;
        private readonly ElementGeometryService _geometry;

        public TerminalLayoutService(ElementGeometryService geometry)
        {
            _geometry = geometry
                ?? throw new ArgumentNullException(nameof(geometry));
        }

        public TerminalLayoutService(
            ElementRegistryService registry,
            ElementGeometryService geometry)
            : this(geometry)
        {
            _registry = registry
                ?? throw new ArgumentNullException(nameof(registry));
        }

        public void AtualizarTerminais(Elemento elemento)
        {
            if (_registry?.UpdateTerminals(elemento) == true)
                return;

            switch (elemento)
            {
                case Barra barra:
                    barra.AtualizarTerminais(_geometry.ObterTamanho(barra).Width);
                    break;

                case Carga carga:
                    var tamanhoCarga = _geometry.ObterTamanho(carga);
                    carga.AtualizarTerminais(tamanhoCarga.Width, tamanhoCarga.Height);
                    break;

                case Gerador gerador:
                    var tamanho = _geometry.ObterTamanho(gerador);
                    gerador.AtualizarTerminais(tamanho.Width, tamanho.Height);
                    break;

                case Sin sin:
                    var tamanhoSin = _geometry.ObterTamanho(sin);
                    sin.AtualizarTerminais(tamanhoSin.Width, tamanhoSin.Height);
                    break;

                case Transformador transformador:
                    var tamanhoTransformador = _geometry.ObterTamanho(transformador);
                    transformador.AtualizarTerminais(tamanhoTransformador.Width, tamanhoTransformador.Height);
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
