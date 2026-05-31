using System;
using Araci.Applications.Abstractions;
using Araci.Core.Commands;
using Araci.Models;
using Araci.Services;

namespace Araci.Applications.UseCases.Editar
{
    public class RedimensionarBarraUseCase
    {
        private readonly ICommandHistory _commands;
        private readonly ElementGeometryUpdateService _geometryUpdates;

        public RedimensionarBarraUseCase(EditorContext context)
            : this(
                context?.Commands ?? throw new ArgumentNullException(nameof(context)),
                context.GeometryUpdates)
        {
        }

        public RedimensionarBarraUseCase(
            ICommandHistory commands,
            ElementGeometryUpdateService geometryUpdates)
        {
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
            _geometryUpdates = geometryUpdates ?? throw new ArgumentNullException(nameof(geometryUpdates));
        }

        public bool Executar(RedimensionarBarraRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (!Mudou(request))
                return false;

            _commands.Execute(new ResizeBarraCommand(
                request.Barra,
                request.AlturaAntes,
                request.XAntes,
                request.YAntes,
                request.AlturaDepois,
                request.XDepois,
                request.YDepois,
                _geometryUpdates));

            return true;
        }

        private static bool Mudou(RedimensionarBarraRequest request)
        {
            return Math.Abs(request.AlturaAntes - request.AlturaDepois) > 0.0001 ||
                Math.Abs(request.XAntes - request.XDepois) > 0.0001 ||
                Math.Abs(request.YAntes - request.YDepois) > 0.0001;
        }
    }

    public sealed class RedimensionarBarraRequest
    {
        public RedimensionarBarraRequest(
            Barra barra,
            double alturaAntes,
            double xAntes,
            double yAntes,
            double alturaDepois,
            double xDepois,
            double yDepois)
        {
            Barra = barra ?? throw new ArgumentNullException(nameof(barra));
            AlturaAntes = alturaAntes;
            XAntes = xAntes;
            YAntes = yAntes;
            AlturaDepois = alturaDepois;
            XDepois = xDepois;
            YDepois = yDepois;
        }

        public Barra Barra { get; }
        public double AlturaAntes { get; }
        public double XAntes { get; }
        public double YAntes { get; }
        public double AlturaDepois { get; }
        public double XDepois { get; }
        public double YDepois { get; }
    }
}
