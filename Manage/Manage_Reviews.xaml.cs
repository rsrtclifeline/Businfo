using Businfo.AddEdit;
using Businfo.Helper;
using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for Manage_Reviews.xaml
    /// </summary>
    public partial class Manage_Reviews : Window
    {
        FirestoreDb db;
        CollectionReference collection;
        FirestoreChangeListener listener;
        ObservableCollection<Review> reviews = new ObservableCollection<Review>();
        public Manage_Reviews()
        {
            InitializeComponent();
            db = DBIniter.GetDb();
            collection = this.db.Collection("ReviewInfo");
            reviewGrid.ItemsSource = reviews;
            LoadData();
        }

        private void LoadData()
        {
            listener = collection.Listen(snapshot =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    reviews.Clear();
                    foreach (var doc in snapshot.Documents)
                    {
                        var review = new Review
                        {
                            id = doc.Id,
                            BusID = doc.GetValue<string>("BusID"),
                            ReviewMsg = doc.GetValue<string>("msg"),
                            seen = doc.GetValue<string>("status")
                        };
                        reviews.Add(review);
                    }
                });
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e)// view
        {
            if (reviewGrid.SelectedItem is Review r)
            {
                AddEditReview review = new AddEditReview(r);
                review.ShowDialog();
            }
        }

        protected override async void OnClosed(EventArgs e)
        {
            if (listener != null)
            {
                await listener.StopAsync();
            }
            base.OnClosed(e);
        }
    }
}
