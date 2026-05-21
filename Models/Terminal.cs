using System.Windows;

namespace Araci.Models
{
    public class Terminal
    {
        public Elemento Dono { get; }
        public Point Posicao { get; set; }
        public string? Barra { get; set; }

        public Terminal(Elemento dono, Point posicao)
        {
            Dono = dono;
            Posicao = posicao;
        }
    }
}