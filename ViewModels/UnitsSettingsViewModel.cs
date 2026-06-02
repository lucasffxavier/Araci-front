using System;
using System.Collections.Generic;
using Araci.Services.Settings;

namespace Araci.ViewModels
{
    public sealed class UnitsSettingsViewModel
    {
        public UnitsSettingsViewModel(UnitDisplaySettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            Length = settings.Length;
            Voltage = settings.Voltage;
            Current = settings.Current;
            ActivePower = settings.ActivePower;
            ReactivePower = settings.ReactivePower;
            ApparentPower = settings.ApparentPower;
            Percent = settings.Percent;
        }

        public IReadOnlyList<UnitOption> LengthOptions { get; } = CreateOptions(UnitKind.LengthMeter, UnitKind.LengthKilometer);
        public IReadOnlyList<UnitOption> VoltageOptions { get; } = CreateOptions(UnitKind.VoltageVolt, UnitKind.VoltageKV);
        public IReadOnlyList<UnitOption> CurrentOptions { get; } = CreateOptions(UnitKind.CurrentAmpere);
        public IReadOnlyList<UnitOption> ActivePowerOptions { get; } = CreateOptions(UnitKind.ActivePowerW, UnitKind.ActivePowerKW, UnitKind.ActivePowerMW);
        public IReadOnlyList<UnitOption> ReactivePowerOptions { get; } = CreateOptions(UnitKind.ReactivePowerVAr, UnitKind.ReactivePowerKVAr, UnitKind.ReactivePowerMVAr);
        public IReadOnlyList<UnitOption> ApparentPowerOptions { get; } = CreateOptions(UnitKind.ApparentPowerVA, UnitKind.ApparentPowerKVA, UnitKind.ApparentPowerMVA);
        public IReadOnlyList<UnitOption> PercentOptions { get; } = CreateOptions(UnitKind.Percent);

        public UnitKind Length { get; set; }
        public UnitKind Voltage { get; set; }
        public UnitKind Current { get; set; }
        public UnitKind ActivePower { get; set; }
        public UnitKind ReactivePower { get; set; }
        public UnitKind ApparentPower { get; set; }
        public UnitKind Percent { get; set; }

        public void ApplyTo(UnitDisplaySettings target)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            target.Length = Length;
            target.Voltage = Voltage;
            target.Current = Current;
            target.ActivePower = ActivePower;
            target.ReactivePower = ReactivePower;
            target.ApparentPower = ApparentPower;
            target.Percent = Percent;
        }

        private static IReadOnlyList<UnitOption> CreateOptions(params UnitKind[] units)
        {
            var options = new List<UnitOption>(units.Length);

            foreach (UnitKind unit in units)
                options.Add(new UnitOption(unit, UnitFormatter.GetSymbol(unit)));

            return options;
        }
    }

    public sealed class UnitOption
    {
        public UnitOption(UnitKind unit, string text)
        {
            Unit = unit;
            Text = text;
        }

        public UnitKind Unit { get; }
        public string Text { get; }
    }
}
