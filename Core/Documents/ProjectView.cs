using System;

namespace Araci.Core.Documents
{
    public enum ProjectViewDiscipline
    {
        Coordenacao,
        Eletrica,
        Solar,
        Eolica,
        Distribuicao,
        Subestacao
    }

    public class ProjectView
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Nome { get; set; } = string.Empty;
        public string Escala { get; set; } = "1:100";
        public ProjectViewDiscipline Disciplina { get; set; } = ProjectViewDiscipline.Eletrica;
        public bool RecortarVista { get; set; }
        public double CameraX { get; set; }
        public double CameraY { get; set; }
        public double Zoom { get; set; } = 1.0;
    }
}
