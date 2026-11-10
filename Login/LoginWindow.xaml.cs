using Businfo.Helper;
using System.Windows;
using System.Windows.Input;

namespace Businfo.Login
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            this.MouseDown += LoginWindow_MouseDown;
        }

        private void LoginWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Sirf tab DragMove call karo jab left button actual me pressed state me ho
            if (e.ChangedButton == MouseButton.Left && e.ButtonState == MouseButtonState.Pressed)
            {
                try
                {
                    this.DragMove();
                }
                catch (InvalidOperationException)
                {
                    // Mouse button release race-condition ko silently swallow karega
                }
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                DoLogin();
            }
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            DoLogin();
        }

        private async void ForgotPassword_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var user = UsernameBox.Text?.Trim();

            if (string.IsNullOrWhiteSpace(user))
            {
                MessageBox.Show("Kripya pehle apna 'Operator Username' enter karein jiska password reset karna hai.",
                                "Forgot Password", MessageBoxButton.OK, MessageBoxImage.Information);
                UsernameBox.Focus();
                return;
            }

            var confirm = MessageBox.Show($"Kya aap username '{user}' ke liye password reset request submit karna chahte hain?",
                                          "Confirm Reset Request", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (confirm != MessageBoxResult.Yes) return;

            var (success, msg) = await ApiClient.RequestPasswordResetAsync(user);

            if (success)
            {
                MessageBox.Show(msg, "Request Submitted", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show($"Request failed: {msg}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void DoLogin()
        {
            var user = UsernameBox.Text?.Trim();
            var pass = PasswordBox.Password?.Trim();

            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
            {
                MessageBox.Show("Please enter both username and password.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            btnLogin.IsEnabled = false;
            btnLogin.Content = "Authenticating...";

            var (success, msg, _, mustChangePassword) = await ApiClient.LoginAsync(user, pass);

            btnLogin.IsEnabled = true;
            btnLogin.Content = "Sign In to Console";

            if (success)
            {
                if (mustChangePassword)
                {
                    MessageBox.Show("Security Notice:\nAapka password temporary hai. Kripya naya permanent password set karein.",
                                    "Password Change Required", MessageBoxButton.OK, MessageBoxImage.Information);

                    var changePassDialog = new ChangePasswordDialog();
                    bool? dialogResult = changePassDialog.ShowDialog();

                    if (dialogResult != true)
                    {
                        ApiClient.SetAuthToken(string.Empty);
                        MessageBox.Show("Password change anivarya hai. Login cancel kar diya gaya.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                        PasswordBox.Clear();
                        PasswordBox.Focus();
                        return;
                    }
                }

                var main = new MainWindow();
                main.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show(msg, "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                PasswordBox.Clear();
                PasswordBox.Focus();
            }
        }
    }
}