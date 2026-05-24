using System;
using System.IO;
using System.Windows;
using Forms = System.Windows.Forms;

namespace Araci.Applications.Analisar.FluxoDeCorrente
{
    public partial class FluxoDeCorrenteWindow : Window
    {
        public FluxoDeCorrenteWindow()
        {
            InitializeComponent();

            Options = new FluxoDeCorrenteOptions
            {
                PastaSaida = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                NomeArquivo = "circuito"
            };

            PastaTextBox.Text = Options.PastaSaida;
            NomeArquivoTextBox.Text = Options.NomeArquivo;
        }

        public FluxoDeCorrenteOptions Options { get; }

        private void SelecionarPastaButton_Click(object sender, RoutedEventArgs e)
        {
            using Forms.FolderBrowserDialog dialog = new()
            {
                Description = "Selecione a pasta para salvar o arquivo DSS.",
                SelectedPath = Directory.Exists(PastaTextBox.Text)
                    ? PastaTextBox.Text
                    : Options.PastaSaida,
                UseDescriptionForTitle = true
            };

            if (dialog.ShowDialog() == Forms.DialogResult.OK)
                PastaTextBox.Text = dialog.SelectedPath;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            string pasta = PastaTextBox.Text.Trim();
            string nomeArquivo = NomeArquivoTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(pasta) || !Directory.Exists(pasta))
            {
                MessageBox.Show(
                    "Selecione uma pasta de saída válida.",
                    "Fluxo de Corrente",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(nomeArquivo))
            {
                MessageBox.Show(
                    "Informe o nome do arquivo DSS.",
                    "Fluxo de Corrente",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            Options.PastaSaida = pasta;
            Options.NomeArquivo = nomeArquivo;
            DialogResult = true;
        }

        private void CancelarButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
