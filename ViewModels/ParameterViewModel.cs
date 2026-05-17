using Araci.Models;
using Araci.ViewModels.Base;

namespace Araci.ViewModels
{
    public class ParameterViewModel
        : ViewModelBase
    {
        private readonly Parameter _parameter;

        public ParameterViewModel(
            Parameter parameter)
        {
            _parameter = parameter;
        }

        public string Nome =>
            _parameter.Nome;

        public object? Valor
        {
            get => _parameter.ValorObjeto;

            set
            {
                if (Equals(
                        _parameter.ValorObjeto,
                        value))
                {
                    return;
                }

                _parameter.ValorObjeto =
                    value;

                OnPropertyChanged();
            }
        }

        public System.Type Tipo =>
            _parameter.Tipo;
    }
}