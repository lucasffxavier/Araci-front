using System;
using System.Globalization;
using System.Linq;
using Araci.Applications.Abstractions;
using Araci.Core.Documents;
using Araci.DTOs;
using Araci.Models;

namespace Araci.Services.Simulation
{
    public class SimulationResultApplier : ISimulationResultApplier
    {
        private readonly AraciDocument _document;
        private readonly Action? _notifyViewModels;

        public SimulationResultApplier(
            AraciDocument document,
            Action? notifyViewModels = null)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _notifyViewModels = notifyViewModels;
        }

        public void Apply(SimulationResultDto resultado)
        {
            if (resultado == null)
                throw new ArgumentNullException(nameof(resultado));

            foreach (LineResultDto lineResult in resultado.Lines)
            {
                Cabo? cabo = _document.Elementos
                    .OfType<Cabo>()
                    .FirstOrDefault(c => string.Equals(c.Id.ToString(), lineResult.Id, StringComparison.OrdinalIgnoreCase));

                if (cabo != null)
                    AplicarCorrentes(cabo, lineResult);
            }

            foreach (LoadResultDto loadResult in resultado.Loads)
            {
                Carga? carga = _document.Elementos
                    .OfType<Carga>()
                    .FirstOrDefault(c => string.Equals(c.Id.ToString(), loadResult.Id, StringComparison.OrdinalIgnoreCase));

                if (carga != null)
                    AplicarCorrentes(carga, loadResult);
            }

            _notifyViewModels?.Invoke();
        }

        private static void AplicarCorrentes(Cabo cabo, LineResultDto resultado)
        {
            double corrente = resultado.Corrente;

            cabo.CorrenteLinha = FormatPolar(resultado.CorrenteLinha ?? corrente, 0);
            cabo.CorrenteFaseA = FormatPolar(resultado.CorrenteFaseA ?? corrente, resultado.AnguloFaseA ?? 0);
            cabo.CorrenteFaseB = FormatPolar(resultado.CorrenteFaseB ?? corrente, resultado.AnguloFaseB ?? -120);
            cabo.CorrenteFaseC = FormatPolar(resultado.CorrenteFaseC ?? corrente, resultado.AnguloFaseC ?? 120);
        }

        private static void AplicarCorrentes(Carga carga, LoadResultDto resultado)
        {
            double corrente = resultado.Corrente;

            carga.CorrenteLinha = FormatPolar(resultado.CorrenteLinha ?? corrente, 0);
            carga.CorrenteFaseA = FormatPolar(resultado.CorrenteFaseA ?? corrente, resultado.AnguloFaseA ?? 0);
            carga.CorrenteFaseB = FormatPolar(resultado.CorrenteFaseB ?? corrente, resultado.AnguloFaseB ?? -120);
            carga.CorrenteFaseC = FormatPolar(resultado.CorrenteFaseC ?? corrente, resultado.AnguloFaseC ?? 120);
        }

        private static string FormatPolar(double magnitude, double angle)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0:0.##}\u2220{1:0.##}\u00B0",
                magnitude,
                angle);
        }
    }
}
