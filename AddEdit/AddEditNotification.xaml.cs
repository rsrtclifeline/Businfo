using Businfo.Helper;
using Businfo.Manage;
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

namespace Businfo.AddEdit
{
    /// <summary>
    /// Interaction logic for AddEditNotification.xaml
    /// </summary>
    public partial class AddEditNotification : Window
    {
        FirestoreDb db;
        Notification edit;
        public AddEditNotification()
        {
            InitializeComponent();
            this.db = DBIniter.GetDb();
            edit = null;
        }

        public AddEditNotification(Notification notification) : this()
        {
            LoadNotification(notification);
        }
        
        public AddEditNotification(string id) : this()
        {
            LoadNotification(id);
        }

        private async Task LoadNotification(string id)
        {
            var notifyData =  db.Collection("NotificationInfo").Document(id);
            var data = await notifyData.GetSnapshotAsync();
            var a = new Notification
            {
                id = data.Id,
                msg_notification = data.GetValue<string>("Msg"),
                created = data.GetValue<string>("created"),
                updated = data.GetValue<string>("updated")
            };
            LoadNotification(a);
        }

        private void LoadNotification(Notification notify)
        {
            edit = notify;
            msg.Text = notify.msg_notification;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var now = DateTime.UtcNow.ToString("o");
            var data = new Dictionary<string, object>
            {
                { "Msg" , msg.Text },
                { "created", edit == null ?  now : edit.created},
                { "updated", now }
            };

            var collection = db.Collection("NotificationInfo");
            if (edit != null)
            {
                collection.Document(edit.id).SetAsync(data, SetOptions.MergeAll);
                MessageBox.Show("Successfully updated");
            }
            else
            {
                collection.AddAsync(data);
                MessageBox.Show("Successfully Added");
            }
        }
    }
}
