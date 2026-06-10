using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Araci.Models.Tipos
{
    public class TipoLinhaAnotativa : TipoElemento, INotifyPropertyChanged
    {
        public const string PARAM_ESTILO_LINHA = "EstiloLinha";

        public TipoLinhaAnotativa()
        {
            NomeTipo = "Linha contínua";
            Familia = "Anotações";
            Categoria = "Linhas";

            DefinirParametro(new Parameter<string>(PARAM_ESTILO_LINHA, "Contínuo"));
        }

        public string EstiloLinha
        {
            get => Obter<string>(PARAM_ESTILO_LINHA);
            set
            {
                string normalizado = NormalizarEstilo(value);

                if (Obter<string>(PARAM_ESTILO_LINHA) == normalizado)
                    return;

                Definir(PARAM_ESTILO_LINHA, normalizado);
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public static string NormalizarEstilo(string? valor)
        {
            return valor switch
            {
                "Tracejado" => "Tracejado",
                "Traço ponto" => "Traço ponto",
                "Traço dois pontos" => "Traço dois pontos",
                _ => "Contínuo"
            };
        }

        public static string NormalizarCor(string? valor)
        {
            string cor = (valor ?? string.Empty).Trim();

            if (cor.Length == 7 && cor[0] == '#' && EhHexadecimal(cor, 1))
                return "#FF" + cor[1..].ToUpperInvariant();

            if (cor.Length == 9 && cor[0] == '#' && EhHexadecimal(cor, 1))
                return cor.ToUpperInvariant();

            return "#FF000000";
        }

        public static double NormalizarEspessura(double valor)
        {
            if (double.IsNaN(valor) || double.IsInfinity(valor) || valor < 0.1)
                return 1.0;

            return valor;
        }

        private static bool EhHexadecimal(string valor, int inicio)
        {
            for (int i = inicio; i < valor.Length; i++)
            {
                char c = valor[i];
                bool hex = c is >= '0' and <= '9' or >= 'a' and <= 'f' or >= 'A' and <= 'F';

                if (!hex)
                    return false;
            }

            return true;
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}