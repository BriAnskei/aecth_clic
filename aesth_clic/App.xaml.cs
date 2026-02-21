using aesth_clic.Controller;
using aesth_clic.Data;
using aesth_clic.Repository;
using aesth_clic.Services.AuthServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using System;
using System.Runtime.Versioning;



namespace aesth_clic
{
    public partial class App : Application
    {
        private Window? _window = null;
        public Window? MainWindow => _window;  // ← expose it
        public static IServiceProvider Services { get; private set; } = null!;
        public App()
        {
            InitializeComponent();
            Services = ConfigureServices();
        }


        private static ServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            // Infrastructure
            services.AddSingleton<DbConnectionFactory>();

            // Repositories
            //  -- users, auth
            services.AddTransient<UserRepository>();
            services.AddTransient<CompanyRepository>();
            services.AddTransient<CompanyModuleRepository>();

            // Services
            services.AddTransient<AuthService>();

            // Controllers
            services.AddTransient<AuthController>();

            return services.BuildServiceProvider();
        }


        [SupportedOSPlatform("windows10.0.17763.0")]
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            _window = new MainWindow();
            _window.Activate();


        }
    }
}