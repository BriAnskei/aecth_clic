using aesth_clic.Context;
using aesth_clic.Controller;
using aesth_clic.Data;
using aesth_clic.Master.Controller;
using aesth_clic.Master.Services;
using aesth_clic.Repository;
using aesth_clic.Tenant.Controller;
using aesth_clic.Tenant.Services;
using aesth_clic.Util;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using QuestPDF.Infrastructure;
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
            QuestPDF.Settings.License = LicenseType.Community;
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


            // -- Master
            services.AddSingleton<AuthService>();
            services.AddTransient<AuthController>();


            services.AddScoped<SubscriptionService>();
            services.AddScoped<SubscriptionController>();


            services.AddScoped<TncMasterService>();
            services.AddScoped<TncMasterController>();


            services.AddTransient<aesth_clic.Master.Services.CompanyService>();
            services.AddTransient<aesth_clic.Master.Controller.CompanyController>();


            services.AddTransient<aesth_clic.Master.Services.AdminClientService>();
            services.AddTransient<aesth_clic.Master.Controller.AdminUserController>();

            // --





            // -- Tenant
            services.AddTransient<aesth_clic.Tenant.Services.UserService>();
            services.AddTransient<aesth_clic.Tenant.Controller.UserController>();


            services.AddTransient<TncTenantService>();
            services.AddTransient<TncTenantController>();

            services.AddTransient<PatientService>();
            services.AddTransient<PatientController>();


            services.AddTransient<MenuService>();
            services.AddTransient<MenuController>();


            services.AddTransient<PatientProcedureService>();
            services.AddTransient<PatientProcedureController>();



            services.AddTransient<MedicineService>();
            services.AddTransient<MedicineController>();


            // --

            // Infrastructure
            services.AddSingleton<DbConnectionFactory>();
            services.AddScoped<TransactionManager>();

       
      






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
