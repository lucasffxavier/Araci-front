using System.Windows;

namespace Araci.Models
{
    public class Terminal
    {
        public Elemento Dono { get; }
        public string Id { get; }
        public Point Posicao { get; set; }
        public string? Barra { get; set; }

        public Terminal(Elemento dono, Point posicao, string? id = null)
        {
            Dono = dono;
            Id = string.IsNullOrWhiteSpace(id)
                ? System.Guid.NewGuid().ToString("N")
                : id;
            Posicao = posicao;
        }
    }
}
