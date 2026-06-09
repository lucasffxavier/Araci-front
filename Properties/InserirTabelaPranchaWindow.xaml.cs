using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Araci.Applications.Abstractions;

namespace Araci.Properties
{
    public partial class InserirTabelaPranchaWindow : Window
    {
        public InserirTabelaPranchaWindow(
            IReadOnlyList<ProjectItemDialogOption> pranchas,
            IReadOnlyList<ProjectItemDialogOption> tabelas)
        {
            InitializeComponent();

            PranchaComboBox.ItemsSource = pranchas ?? new List<ProjectItemDialogOption>();
            TabelaComboBox.ItemsSource = tabelas ?? new List<ProjectItemDialogOption>();
            PranchaComboBox.SelectedItem = PranchaComboBox.Items.OfType<ProjectItemDialogOption>().FirstOrDefault();
            TabelaComboBox.SelectedItem = TabelaComboBox.Items.OfType<ProjectItemDialogOption>().FirstOrDefault();
        }

        public ProjectItemDialogOption? PranchaSelecionada => PranchaComboBox.SelectedItem as ProjectItemDialogOption;
        public ProjectItemDialogOption? TabelaSelecionada => TabelaComboBox.SelectedItem as ProjectItemDialogOption;

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = PranchaSelecionada != null && TabelaSelecionada != null;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
