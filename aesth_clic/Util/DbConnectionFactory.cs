using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aesth_clic.Data
{
    internal class DbConnectionFactory
    {
      private readonly string connectionString =
      "Server=localhost;Database=aesth_clic;User ID=root;Password=;Port=3306;";

        public MySqlConnection GetConnection()
        {
            return new MySqlConnection(connectionString);
        }

    }
}
