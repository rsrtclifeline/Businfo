using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Businfo.Helper
{
    public static class DBIniter
    {
        private static FirestoreDb? db;
        static DBIniter()
        {
            
        }
        public static FirestoreDb? GetDb()
        {
            return db;
        }
    }
}
