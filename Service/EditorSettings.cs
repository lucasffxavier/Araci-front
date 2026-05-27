namespace Araci.Services
{
    public class EditorSettings
    {
        public double GridStep { get; set; } = 10.0;

        public bool GridSnapEnabled { get; set; } = true;

        public double ElectricalSnapTolerance { get; set; } = 15.0;
    }
}
