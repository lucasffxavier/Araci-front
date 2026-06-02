namespace Araci.Infrastructure.Persistence
{
    public sealed class ProjectUnitSettingsDto
    {
        public string? Length { get; set; }
        public string? Voltage { get; set; }
        public string? Current { get; set; }
        public string? ActivePower { get; set; }
        public string? ReactivePower { get; set; }
        public string? ApparentPower { get; set; }
        public string? Percent { get; set; }
    }
}
