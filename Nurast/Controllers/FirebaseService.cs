using FireSharp.Config;
using FireSharp.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Nurast.Controllers
{
    public class FirebaseService
    {
        private IFirebaseClient client;

        public FirebaseService()
        {
            IFirebaseConfig config = new FirebaseConfig
            {
                AuthSecret = "AIzaSyBarOt-_sHpsn2ul4hhJrjQDwcA8rTnYUs",
                BasePath = "https://nursat-app-default-rtdb.firebaseio.com/"
            };

            client = new FireSharp.FirebaseClient(config);
        }

        public IFirebaseClient GetClient()
        {
            return client;
        }
    }
}
