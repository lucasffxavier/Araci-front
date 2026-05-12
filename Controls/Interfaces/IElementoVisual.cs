using Araci.Models;

namespace Araci.Controls.Interfaces
{
    public interface IElementoVisual
    {
        // =========================
        // MODELO BIM
        // =========================

        Elemento Modelo { get; }

        // =========================
        // MOVIMENTO
        // =========================

        void Mover(
            double deltaX,
            double deltaY);
    }
}