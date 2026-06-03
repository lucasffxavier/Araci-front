using System;
using Araci.Services;
using Araci.Services.Settings;

namespace Araci.ViewModels
{
    public sealed class InstancePropertyDescriptor
    {
        public InstancePropertyDescriptor(Type ownerType, string propertyName, string displayName, int order, bool isEditable = true, bool allowMixedTypeEdit = false, UnitKind unit = UnitKind.None, bool isColor = false)
        {
            OwnerType = ownerType ?? throw new ArgumentNullException(nameof(ownerType));
            PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
            DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
            Order = order;
            IsEditable = isEditable;
            AllowMixedTypeEdit = allowMixedTypeEdit;
            Unit = unit;
            IsColor = isColor;
        }

        public Type OwnerType { get; }
        public string PropertyName { get; }
        public string DisplayName { get; }
        public int Order { get; }
        public bool IsEditable { get; }
        public bool AllowMixedTypeEdit { get; }
        public UnitKind Unit { get; }
        public bool IsColor { get; }
        public bool HasUnit => Unit != UnitKind.None;
        public string UnitSymbol => UnitFormatter.GetSymbol(Unit);
    }
}
