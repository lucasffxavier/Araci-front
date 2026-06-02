using System;
using Araci.Services.Settings;

namespace Araci.Applications.UseCases.Projeto
{
    public class AlterarUnidadesProjetoUseCase
    {
        private readonly EditorSettings _settings;
        private readonly Action _refreshProperties;

        public AlterarUnidadesProjetoUseCase(EditorSettings settings, Action refreshProperties)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _refreshProperties = refreshProperties ?? throw new ArgumentNullException(nameof(refreshProperties));
        }

        public void Executar(UnitDisplaySettings novasUnidades)
        {
            if (novasUnidades == null)
                throw new ArgumentNullException(nameof(novasUnidades));

            _settings.Units.Length = novasUnidades.Length;
            _settings.Units.Voltage = novasUnidades.Voltage;
            _settings.Units.Current = novasUnidades.Current;
            _settings.Units.ActivePower = novasUnidades.ActivePower;
            _settings.Units.ReactivePower = novasUnidades.ReactivePower;
            _settings.Units.ApparentPower = novasUnidades.ApparentPower;
            _settings.Units.Percent = novasUnidades.Percent;

            _refreshProperties();
        }
    }
}
