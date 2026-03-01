using aesth_clic.Context;
using aesth_clic.Controller;
using aesth_clic.Data;
using aesth_clic.Repository;

using aesth_clic.Tenant.Uti;
using aesth_clic.Util;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using System;
using System.Runtime.Versioning;

namespace aesth_clic
{
    public partial class App : Application
    {
        private Window? _window = null;
        public Window? MainWindow => _window; // ← expose it
        public static IServiceProvider Services { get; private set; } = null!;

        public App()
        {
            InitializeComponent();
            Services = ConfigureServices();
        }

        private static ServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();



            string masterConnection =
      "Server=localhost\\SQLEXPRESS;Database=aesthic_clic;Trusted_Connection=True;TrustServerCertificate=True;";
            // connect master
            services.AddDbContext<MasterDbContext>(options =>
     options.UseSqlServer(
         masterConnection,
         sqlOptions =>

         {
             sqlOptions.EnableRetryOnFailure(
                 maxRetryCount: 5,
                 maxRetryDelay: TimeSpan.FromSeconds(10),
                 errorNumbersToAdd: null);
         }));


            services.AddSingleton<TenantDbContextFactory>();




            services.AddTransient<aesth_clic.Master.Services.CompanyService>();
            services.AddTransient<aesth_clic.Master.Controller.CompanyController>();



            // Infrastructure
            services.AddSingleton<DbConnectionFactory>();
            services.AddScoped<TransactionManager>();

            // Repositories
            //  -- users, auth
            //services.AddTransient<UserRepository>();
            services.AddTransient<CompanyRepository>();
            services.AddTransient<AccountStatusRepository>();
            services.AddTransient<PaymentRepository>();

            // Services
            //services.AddTransient<AuthService>();
            //services.AddTransient<UserService>();
            services.AddTransient<Services.SuperAdminServices.CompanyService>();

            // Controllers
            //services.AddTransient<AuthController>();
            //services.AddTransient<UserController_superAdmin>();
            services.AddTransient<CompanyController>();

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
