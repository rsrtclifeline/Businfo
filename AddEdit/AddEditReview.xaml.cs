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

namespace Businfo.AddEdit
{
    /// <summary>
    /// Interaction logic for AddEditReview.xaml
    /// </summary>
    public partial class AddEditReview : Window
    {
        FirestoreDb db;
        Review edit;
        CollectionReference collection;
        public AddEditReview()
        {
            InitializeComponent();
            db = DBIniter.GetDb();
            collection = db.Collection("ReviewInfo");
        }

        public AddEditReview(Review r) : this()
        {
            LoadReview(r);
        }

        public AddEditReview(string id) : this()
        {
            LoadReview(id);
        }

        public async Task LoadReview(string id)
        {
            var data = collection.Document(id);
            var field = await data.GetSnapshotAsync();
            var dic = field.ToDictionary();
            var a = new Review
            {
                id = field.Id,
                BusID = dic["BusID"].ToString(),
                ReviewMsg = dic["msg"].ToString(),
                seen = dic["status"].ToString()
            };

            LoadReview(a);
        }



        private void LoadReview(Review r)
        {
            edit = r;
            txtBusID.Text = r.BusID;
            txtReviewMsg.Text = r.ReviewMsg;
        }

        private void btnRemoveReview_Click(object sender, RoutedEventArgs e)
        {
            collection.Document(edit.id).DeleteAsync();
        }

        private void btnMarkAsSeen_Click(object sender, RoutedEventArgs e)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>
            {
                { "status", "seen" }
            };
            collection.Document(edit.id).SetAsync(dic, SetOptions.MergeAll);
            this.Close();
        }
    }

    public class Review
    {
        public string id { get; set; }            // Firestore document ID
        public string BusID { get; set; }
        public string ReviewMsg { get; set; }
        public string seen { get; set; }             // Mark as seen

        public Review Clone()
        {
            return new Review
            {
                id = this.id,
                BusID = this.BusID,
                ReviewMsg = this.ReviewMsg,
                seen = this.seen
            };
        }

    }

}
