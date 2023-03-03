using System;

namespace CocRfidReader.WPF.UserControls
{
    public partial class FilterableComboBox
    {
        private class UserChange<T>
        {
            private Action<T> action;

            public bool IsUserChange { get; private set; } = true;

            public UserChange(Action<T> action)
            {
                this.action = action;
            }

            public void Set(T val)
            {
                try
                {
                    IsUserChange = false;
                    action(val);
                }
                finally
                {
                    IsUserChange = true;
                }
            }
        }
    }
}
