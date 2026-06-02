namespace Araci.Services.Settings
{
    public class EditorSettings
    {
        public double GridStep { get; set; } = 10.0;
        public bool GridSnapEnabled { get; set; } = true;
        public double ElectricalSnapTolerance { get; set; } = 15.0;
        public UnitDisplaySettings Units { get; } = new UnitDisplaySettings();
    }

    public class UnitDisplaySettings
    {
        public UnitKind Length { get; set; } = UnitKind.LengthMeter;
        public UnitKind Voltage { get; set; } = UnitKind.VoltageKV;
        public UnitKind Current { get; set; } = UnitKind.CurrentAmpere;
        public UnitKind ActivePower { get; set; } = UnitKind.ActivePowerKW;
        public UnitKind ReactivePower { get; set; } = UnitKind.ReactivePowerKVAr;
        public UnitKind ApparentPower { get; set; } = UnitKind.ApparentPowerKVA;
        public UnitKind Percent { get; set; } = UnitKind.Percent;

        public UnitKind Resolve(UnitQuantityKind quantity)
        {
            return quantity switch
            {
                UnitQuantityKind.Length => Length,
                UnitQuantityKind.Voltage => Voltage,
                UnitQuantityKind.Current => Current,
                UnitQuantityKind.ActivePower => ActivePower,
                UnitQuantityKind.ReactivePower => ReactivePower,
                UnitQuantityKind.ApparentPower => ApparentPower,
                UnitQuantityKind.Percent => Percent,
                _ => UnitKind.None
            };
        }
    }
}