using System;
using System.Windows;

namespace Araci.Models
{
    public static class TerminalPlacement
    {
        public static Point ToWorld(Elemento owner, Point local)
        {
            double scale = owner.Escala == 0 ? 1 : owner.Escala;
            double x = local.X * scale;
            double y = local.Y * scale;

            if (Math.Abs(owner.Rotacao) > 0.000001)
            {
                double radians = owner.Rotacao * Math.PI / 180.0;
                double cos = Math.Cos(radians);
                double sin = Math.Sin(radians);
                double rotatedX = x * cos - y * sin;
                double rotatedY = x * sin + y * cos;

                x = rotatedX;
                y = rotatedY;
            }

            return new Point(owner.PosicaoX + x, owner.PosicaoY + y);
        }

        public static Point ToLocal(Elemento owner, Point world)
        {
            double x = world.X - owner.PosicaoX;
            double y = world.Y - owner.PosicaoY;

            if (Math.Abs(owner.Rotacao) > 0.000001)
            {
                double radians = -owner.Rotacao * Math.PI / 180.0;
                double cos = Math.Cos(radians);
                double sin = Math.Sin(radians);
                double rotatedX = x * cos - y * sin;
                double rotatedY = x * sin + y * cos;

                x = rotatedX;
                y = rotatedY;
            }

            double scale = owner.Escala == 0 ? 1 : owner.Escala;
            return new Point(x / scale, y / scale);
        }
    }
}
