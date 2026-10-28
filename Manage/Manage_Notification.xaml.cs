using Businfo.AddEdit;
using Businfo.Helper;
using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Businfo.Manage
{
    /// <summary>
    /// Interaction logic for Manage_Notification.xaml
    /// </summary>
    public partial class Manage_Notification : Window
    {
        FirestoreDb db;
        FirestoreChangeListener notifyyListener;
        public Manage_Notification()
        {
            InitializeComponent();
            db = DBIniter.GetDb();
            LoadNotificationRealtime();
        }

        private void LoadNotificationRealtime()
        {
            var notify = db.Collection("NotificationInfo");

            notifyyListener = notify.Listen(snapshot =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    notification_grid.Items.Clear();

                    foreach (var doc in snapshot.Documents)
                    {
                        var model = new Notification
                        {
                            id = doc.Id,
                            msg_notification = doc.GetValue<string>("Msg"),
                            created = doc.GetValue<string>("created"),
                            updated = doc.GetValue<string>("updated")
                        };
                        notification_grid.Items.Add(model);
                    }
                });
            });
        }

        protected override async void OnClosed(EventArgs e)
        {
            if (notifyyListener != null)
            {
                await notifyyListener.StopAsync();
            }

            base.OnClosed(e);
        }

        
        private void Button_Click(object sender, RoutedEventArgs e) //edit
        {
            if (notification_grid.SelectedItem is Notification notification)
            {
                AddEditNotification gandu = new AddEditNotification(notification);
                gandu.Show();
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e) //del
        {
            if (notification_grid.SelectedItem is Notification notification)
            {
                var id = notification.id;
                db.Collection("NotificationInfo").Document(id).DeleteAsync() ;
                MessageBox.Show("Notification was successfully Deleted");
            }
        }
    }

    public class Notification
    {
        public string msg_notification;
        public string created;
        public string updated;
        public string id;

        public Notification Clone()
        {
            return new Notification
            {
                msg_notification = this.msg_notification,
                created = this.created,
                updated = this.updated,
                id = this.id,
            };
        }
    }
}
