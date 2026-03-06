using MySqlConnector;

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





// users table after db creation for client
//CREATE TABLE users (
//    id INT AUTO_INCREMENT PRIMARY KEY,
//    name VARCHAR(255),
//    email VARCHAR(255) UNIQUE,
//    password VARCHAR(255),
//    role ENUM('admin','doctor','receptionist','pharmacist'),
//    status ENUM('active','inactive') DEFAULT 'active',
//    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
//);