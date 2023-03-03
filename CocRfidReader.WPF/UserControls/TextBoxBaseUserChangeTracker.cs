using System;
using System.Collections.Generic;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Windows.Input;

namespace CocRfidReader.WPF.UserControls
{
    public partial class FilterableComboBox
    {
        private class TextBoxBaseUserChangeTracker
        {
            private bool IsTextInput { get; set; }

            public TextBoxBase TextBoxBase { get; set; }
            private List<Key> PressedKeys = new List<Key>();
            public event EventHandler UserTextChanged;
            private string LastText;

            public TextBoxBaseUserChangeTracker(TextBoxBase textBoxBase)
            {
                TextBoxBase = textBoxBase;
                LastText = TextBoxBase.ToString();

                textBoxBase.PreviewTextInput += (s, e) => {
                    IsTextInput = true;
                };

                textBoxBase.TextChanged += (s, e) => {
                    var isUserChange = PressedKeys.Count > 0 || IsTextInput || LastText == TextBoxBase.ToString();
                    IsTextInput = false;
                    LastText = TextBoxBase.ToString();
                    if (isUserChange)
                        UserTextChanged?.Invoke(this, e);
                };

                textBoxBase.PreviewKeyDown += (s, e) => {
                    switch (e.Key)
                    {
                        case Key.Back:
                        case Key.Space:
                            if (!PressedKeys.Contains(e.Key))
                                PressedKeys.Add(e.Key);
                            break;
                    }
                    if (e.Key == Key.Back)
                    {
                        var textBox = textBoxBase as TextBox;
                        if (textBox.SelectionStart > 0 && textBox.SelectionLength > 0 && (textBox.SelectionStart + textBox.SelectionLength) == textBox.Text.Length)
                        {
                            textBox.SelectionStart--;
                            textBox.SelectionLength++;
                            e.Handled = true;
                            UserTextChanged?.Invoke(this, e);
                        }
                    }
                };

                textBoxBase.PreviewKeyUp += (s, e) => {
                    if (PressedKeys.Contains(e.Key))
                        PressedKeys.Remove(e.Key);
                };

                textBoxBase.LostFocus += (s, e) => {
                    PressedKeys.Clear();
                    IsTextInput = false;
                };
            }
        }
    }
}
