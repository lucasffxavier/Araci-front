using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Araci.Applications.Abstractions;

namespace Araci.Properties
{
    public partial class InserirVistaPranchaWindow : Window
    {
        public InserirVistaPranchaWindow(
            IReadOnlyList<ProjectItemDialogOption> pranchas,
            IReadOnlyList<ProjectViewDialogOption> vistas)
        {
            InitializeComponent();
            PranchaComboBox.ItemsSource = pranchas ?? new List<ProjectItemDialogOption>();
            VistaComboBox.ItemsSource = vistas ?? new List<ProjectViewDialogOption>();
            PranchaComboBox.SelectedItem = PranchaComboBox.Items.OfType<ProjectItemDialogOption>().FirstOrDefault();
            VistaComboBox.SelectedItem = VistaComboBox.Items.OfType<ProjectViewDialogOption>().FirstOrDefault(v => v.Id.HasValue);
        }

        public ProjectItemDialogOption? PranchaSelecionada => PranchaComboBox.SelectedItem as ProjectItemDialogOption;
        public ProjectViewDialogOption? VistaSelecionada => VistaComboBox.SelectedItem as ProjectViewDialogOption;

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = PranchaSelecionada != null && VistaSelecionada?.Id != null;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}