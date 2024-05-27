using System;
using System.IO;
using System.Threading.Tasks;
using Firebase.Auth;
using Firebase.Auth.Provider;
using Firebase.Storage;
using FireSharp.Config;
using FireSharp.Interfaces;

namespace Nurast.Controllers
{
    public class FirebaseService
    {
        private IFirebaseClient client;

        public FirebaseService()
        {
            IFirebaseConfig config = new FireSharp.Config.FirebaseConfig
            {
                AuthSecret = "AIzaSyAv1JK4daiKp8ACY-ePBBeh7GWltUFXkDs",
                BasePath = "https://nursat-815c6-default-rtdb.firebaseio.com/"
            };

            client = new FireSharp.FirebaseClient(config);
        }

        public IFirebaseClient GetClient()
        {
            return client;
        }

        private const string ApiKey = "AIzaSyAv1JK4daiKp8ACY-ePBBeh7GWltUFXkDs";
        private const string AuthEmail = "uzenbek03@mail.ru";
        private const string AuthPassword = "12345678";
        private const string Bucket = "nursat-815c6.appspot.com";

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName)
        {
            try
            {
                var authProvider = new FirebaseAuthProvider(new Firebase.Auth.FirebaseConfig(ApiKey));
                var auth = await authProvider.SignInWithEmailAndPasswordAsync(AuthEmail, AuthPassword);

                var task = new FirebaseStorage(
                    Bucket,
                    new FirebaseStorageOptions
                    {
                        AuthTokenAsyncFactory = () => Task.FromResult(auth.FirebaseToken),
                        ThrowOnCancel = true
                    })
                    .Child("uploads")
                    .Child(fileName)
                    .PutAsync(fileStream);

                task.Progress.ProgressChanged += (s, e) => Console.WriteLine($"Прогресс: {e.Percentage} %");

                var downloadUrl = await task;
                return downloadUrl;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
                return null;
            }
        }
    }
}
