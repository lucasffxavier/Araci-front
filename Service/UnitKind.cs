namespace Araci.Services
{
    public enum UnitKind
    {
        None,

        LengthMeter,
        LengthKilometer,

        VoltageVolt,
        VoltageKV,

        CurrentAmpere,

        ActivePowerW,
        ActivePowerKW,
        ActivePowerMW,

        ReactivePowerVAr,
        ReactivePowerKVAr,
        ReactivePowerMVAr,

        ApparentPowerVA,
        ApparentPowerKVA,
        ApparentPowerMVA,

        Percent
    }

    public enum UnitQuantityKind
    {
        None,
        Length,
        Voltage,
        Current,
        ActivePower,
        ReactivePower,
        ApparentPower,
        Percent
    }
}