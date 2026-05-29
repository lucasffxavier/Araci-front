using System;

namespace Araci.ViewModels
{
    public sealed class InstancePropertyDescriptor
    {
        public InstancePropertyDescriptor(Type ownerType, string propertyName, string displayName, int order, bool isEditable = true, bool allowMixedTypeEdit = false)
        {
            OwnerType = ownerType ?? throw new ArgumentNullException(nameof(ownerType));
            PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
            DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
            Order = order;
            IsEditable = isEditable;
            AllowMixedTypeEdit = allowMixedTypeEdit;
        }

        public Type OwnerType { get; }
        public string PropertyName { get; }
        public string DisplayName { get; }
        public int Order { get; }
        public bool IsEditable { get; }
        public bool AllowMixedTypeEdit { get; }
    }
}