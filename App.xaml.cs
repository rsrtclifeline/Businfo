using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Businfo
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        private void DarkEditableComboBox_KeyUp(object sender, KeyEventArgs e)
        {
            ComboBox combo = sender as ComboBox;
            if (!combo.IsEditable) return;
            // Dropdown khol do always
            combo.IsDropDownOpen = true;

            string text = combo.Text?.ToLower() ?? "";

            // ITEMSOURCE wala case
            if (combo.ItemsSource != null)
            {
                CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(combo.ItemsSource);

                view.Filter = o =>
                {
                    if (o is string s)
                        return s.ToLower().Contains(text);

                    return false;
                };
                return;
            }

            // NORMAL ComboBoxItem wala case
            foreach (var item in combo.Items)
            {
                if (item is ComboBoxItem cbi)
                {
                    string content = cbi.Content?.ToString()?.ToLower() ?? "";

                    // match hua → item dikhayenge
                    cbi.Visibility = content.Contains(text)
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                }
            }
        }
    }

}