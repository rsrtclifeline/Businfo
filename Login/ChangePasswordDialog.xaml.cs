using Businfo.Helper;
using System.Windows;

namespace Businfo.Login
{
    public partial class ChangePasswordDialog : Window
    {
        public ChangePasswordDialog()
        {
            InitializeComponent();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private async void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            var oldP = txtOldPass.Password?.Trim();
            var newP = txtNewPass.Password?.Trim();
            var confP = txtConfirmPass.Password?.Trim();

            if (string.IsNullOrWhiteSpace(oldP) || string.IsNullOrWhiteSpace(newP) || string.IsNullOrWhiteSpace(confP))
            {
                MessageBox.Show("Sabhi fields bharna anivarya hai.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (newP != confP)
            {
                MessageBox.Show("New password aur Confirm password match nahi ho rahe.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var username = string.IsNullOrEmpty(ApiClient.CurrentUsername) ? "admin_yhgindian" : ApiClient.CurrentUsername;

            var (success, msg) = await ApiClient.ChangePasswordAsync(username, oldP, newP);

            if (success)
            {
                MessageBox.Show("Password successfully update ho gaya!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                MessageBox.Show($"Password change fail: {msg}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}