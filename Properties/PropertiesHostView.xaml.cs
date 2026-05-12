using System.Windows.Controls;

using Araci.Services;

namespace Araci.Properties
{
    public partial class PropertiesHostView
        : UserControl
    {
        // =========================
        // CONSTRUTOR
        // =========================

        public PropertiesHostView()
        {
            InitializeComponent();

            DataContext =
                AppServices.Editor;
        }
    }
}