using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace Businfo.Helper
{
    public static class SearchableComboBehavior
    {
        public static readonly DependencyProperty IsSearchableProperty =
            DependencyProperty.RegisterAttached(
                "IsSearchable",
                typeof(bool),
                typeof(SearchableComboBehavior),
                new PropertyMetadata(false, OnIsSearchableChanged));

        public static bool GetIsSearchable(DependencyObject obj) => (bool)obj.GetValue(IsSearchableProperty);
        public static void SetIsSearchable(DependencyObject obj, bool value) => obj.SetValue(IsSearchableProperty, value);

        private static void OnIsSearchableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not ComboBox cb) return;

            if ((bool)e.NewValue)
            {
                cb.DropDownOpened += OnDropDownOpened;
            }
            else
            {
                cb.DropDownOpened -= OnDropDownOpened;
            }
        }

        private static void OnDropDownOpened(object? sender, EventArgs e)
        {
            if (sender is not ComboBox cb) return;

            cb.ApplyTemplate();
            if (cb.Template.FindName("PART_Popup", cb) is Popup popup &&
                popup.Child is Border border &&
                border.Child is Grid grid &&
                grid.Children.Count > 0 &&
                grid.Children[0] is Border searchBorder &&
                searchBorder.Child is Border innerBorder &&
                innerBorder.Child is Grid sGrid &&
                sGrid.Children.Count > 1 &&
                sGrid.Children[1] is TextBox searchBox)
            {
                searchBox.Tag = cb;
                searchBox.Text = string.Empty;

                searchBox.TextChanged -= OnSearchTextChanged;
                searchBox.TextChanged += OnSearchTextChanged;

                searchBox.Focus();
            }
        }

        private static void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is not TextBox tb || tb.Tag is not ComboBox combo || combo.ItemsSource == null)
                return;

            var query = tb.Text.Trim().ToLowerInvariant();
            var view = CollectionViewSource.GetDefaultView(combo.ItemsSource);
            if (view == null) return;

            if (string.IsNullOrWhiteSpace(query))
            {
                view.Filter = null;
                return;
            }

            string displayProp = combo.DisplayMemberPath;

            view.Filter = item =>
            {
                if (item == null) return false;

                // 1. Plain String case (e.g. "All Vendors", "Jaipur")
                if (item is string str)
                {
                    if (str.StartsWith("All ", StringComparison.OrdinalIgnoreCase))
                        return true;

                    return str.ToLowerInvariant().Contains(query);
                }

                // 2. Specific DisplayMemberPath (e.g. CityName, VendorName)
                if (!string.IsNullOrEmpty(displayProp))
                {
                    var prop = item.GetType().GetProperty(displayProp, BindingFlags.Public | BindingFlags.Instance);
                    if (prop != null)
                    {
                        var val = prop.GetValue(item)?.ToString();
                        if (!string.IsNullOrEmpty(val) && val.ToLowerInvariant().Contains(query))
                            return true;
                    }
                }

                // 3. Fallback: Any public string property
                var properties = item.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
                foreach (var p in properties)
                {
                    if (p.PropertyType == typeof(string))
                    {
                        var val = p.GetValue(item) as string;
                        if (!string.IsNullOrEmpty(val) && val.ToLowerInvariant().Contains(query))
                            return true;
                    }
                }

                // 4. Fallback ToString()
                var toStr = item.ToString();
                return toStr != null && toStr.ToLowerInvariant().Contains(query);
            };
        }
    }
}