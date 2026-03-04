using aesth_clic.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aesth_clic.Session
{
    public abstract class TenantServiceBase
    {
        protected TenantDbContext CreateTenantDb()
        {
            var client = AppSession.Instance.CurrentClient
                         ?? throw new Exception("No active client session.");
            return new TenantDbContextFactory().Create(client.DbName);
        }
    }
}
