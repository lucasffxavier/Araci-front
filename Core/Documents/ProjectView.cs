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
        public const double DefaultRecorteX = 0.0;
        public const double DefaultRecorteY = 0.0;
        public const double DefaultRecorteLargura = 1000.0;
        public const double DefaultRecorteAltura = 700.0;
        public const double MinRecorteDimension = 10.0;

        public Guid Id { get; set; } = Guid.NewGuid();
        public string Nome { get; set; } = string.Empty;
        public string Escala { get; set; } = "1:100";
        public ProjectViewDiscipline Disciplina { get; set; } = ProjectViewDiscipline.Eletrica;
        public bool RecortarVista { get; set; }
        public bool RegiaoRecorteVisivel { get; set; } = true;
        public double RecorteX { get; set; } = DefaultRecorteX;
        public double RecorteY { get; set; } = DefaultRecorteY;
        public double RecorteLargura { get; set; } = DefaultRecorteLargura;
        public double RecorteAltura { get; set; } = DefaultRecorteAltura;
        public double CameraX { get; set; }
        public double CameraY { get; set; }
        public double Zoom { get; set; } = 1.0;
    }
}