using aesth_clic.Master.Dto.Dashboard;
using aesth_clic.Master.Services;
using System.Threading.Tasks;

namespace aesth_clic.Master.Controller
{
    public sealed class DashboardController(DashboardService dashboardService)
    {
        private readonly DashboardService _dashboardService = dashboardService;

        public Task<SuperAdminDashboardDto> GetSuperAdminDashboardAsync()
            => _dashboardService.GetSuperAdminDashboardAsync();
    }
}