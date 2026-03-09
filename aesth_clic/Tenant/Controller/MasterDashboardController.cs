using aesth_clic.Tenant.DTO;
using aesth_clic.Tenant.Services;
using System.Threading.Tasks;

namespace aesth_clic.Tenant.Controller
{
    public class MasterDashboardController
    {
        private readonly MasterDashboardService _dashboardService;

        public MasterDashboardController(MasterDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        public async Task<DashboardDto> GetDashboardDataAsync()
        {
            return await _dashboardService.GetDashboardDataAsync();
        }
    }
}