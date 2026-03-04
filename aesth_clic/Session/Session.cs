using aesth_clic.Master.Model;
using aesth_clic.Tenant.Model;
using System;

namespace aesth_clic.Session
{
    public sealed class AppSession
    {
        private static readonly Lazy<AppSession> _instance =
            new Lazy<AppSession>(() => new AppSession());

        public static AppSession Instance => _instance.Value;

        private AppSession() { }

        public Client? CurrentClient { get; private set; }
        public User? CurrentUser { get; private set; }

        public bool IsLoggedIn => CurrentUser != null && CurrentClient != null;

        public event Action? SessionChanged;

        public void Login(Client client, User user)
        {
            CurrentClient = client ?? throw new ArgumentNullException(nameof(client));
            CurrentUser = user ?? throw new ArgumentNullException(nameof(user));

            SessionChanged?.Invoke();
        }

        public void Logout()
        {
            CurrentClient = null;
            CurrentUser = null;

            SessionChanged?.Invoke();
        }
    }
}