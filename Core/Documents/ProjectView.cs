using System;

namespace Araci.Core.Documents
{
    public class ProjectView
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Nome { get; set; } = string.Empty;
        public double CameraX { get; set; }
        public double CameraY { get; set; }
        public double Zoom { get; set; } = 1.0;
    }
}
