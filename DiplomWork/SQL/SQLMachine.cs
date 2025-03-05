using Npgsql;

namespace DiplomWork
{
    internal class SQLMachine
    {
        private static readonly HttpClient httpClient = new HttpClient();
        string connectionStringForCoffeAutomat = "Host=localhost;Username=postgres;Database=CoffeAvtomat;Include Error Detail=true";                
        string sqlExpression = "INSERT INTO CoffeAvtomat (Name, Latitude, Longitude, Adres, PhotoURL) VALUES (@Name, @Latitude, @Longitude, @Adres, @PhotoURL)";
        string sqlSelectExpression = "SELECT name, Latitude, Longitude, PhotoURL FROM PendingCoffeAvtomat";
        string sqlInsertExpression = "INSERT INTO PendingCoffeAvtomat (name, latitude,longitude, adres, photoURL,isapproved) VALUES (@name, @latitude, @longitude, @adres, @photoURL,@isapproved)";
        private YandexGeocodingService geocodingService;
        public SQLMachine()
        {                     
            geocodingService= new YandexGeocodingService();
        }        
        public async Task AddCoffeMachine(Location location, string name, string photoData)
        {
            string latitudeString = location.Latitude.ToString("G", System.Globalization.CultureInfo.InvariantCulture);
            string longitudeString = location.Longitude.ToString("G", System.Globalization.CultureInfo.InvariantCulture);
            var adres = await geocodingService.GetAddressFromCoordinates(latitudeString, longitudeString);
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionStringForCoffeAutomat))
                {
                    await connection.OpenAsync();
                    using (NpgsqlCommand command = new NpgsqlCommand(sqlExpression, connection))
                    {
                        command.Parameters.AddWithValue("name", name);
                        command.Parameters.AddWithValue("latitude", location.Latitude);
                        command.Parameters.AddWithValue("longitude", location.Longitude);
                        command.Parameters.AddWithValue("adres", adres);
                        command.Parameters.AddWithValue("photoURL", photoData);
                        command.Parameters.AddWithValue("IsApproved", false);
                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (NpgsqlException ex)
            {
                Console.WriteLine($"Ошибка при добавлении кофе-автомата: {ex.Message}");
            }
        }
        public async Task<List<CoffeAvtomatModderation>> GetPendingMachines()
        {
            List<CoffeAvtomatModderation> pendingMachines = new List<CoffeAvtomatModderation>();

            string sqlSelectPending = "SELECT Id, Name, Latitude, Longitude, Adres, PhotoURL, IsApproved " +
                             "FROM coffeavtomat " +
                             "WHERE IsApproved = FALSE";

            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionStringForCoffeAutomat))
                {
                    await connection.OpenAsync();
                    using (NpgsqlCommand command = new NpgsqlCommand(sqlSelectPending, connection))
                    {
                        using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                CoffeAvtomatModderation pendingMachine = new CoffeAvtomatModderation
                                {
                                    Id = (int)reader["Id"],
                                    Name = reader["Name"]?.ToString() ?? "Неизвестно",
                                    Adres = reader["Adres"]?.ToString() ?? string.Empty,
                                    Latitude = (double)(reader["Latitude"] != DBNull.Value ? Convert.ToDouble(reader["Latitude"]) : 0.0),
                                    Longitude = (double)(reader["Longitude"] != DBNull.Value ? Convert.ToDouble(reader["Longitude"]) : 0.0),
                                    PhotoURL = reader["PhotoURL"]?.ToString() ?? string.Empty,
                                    IsApproved = reader["IsApproved"] != DBNull.Value && (bool)reader["IsApproved"]
                                };

                                pendingMachines.Add(pendingMachine);
                            }
                        }
                    }
                }
            }
            catch (NpgsqlException ex)
            {
                Console.WriteLine($"Ошибка при получении ожидающих кофе-автоматов: {ex.Message}");
            }

            return pendingMachines;
        }
        public async Task ApproveMachine(int id)
        {
            string sqlApprove = "UPDATE coffeavtomat SET IsApproved = TRUE WHERE Id = @Id";

            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionStringForCoffeAutomat))
                {
                    await connection.OpenAsync();
                    using (NpgsqlCommand command = new NpgsqlCommand(sqlApprove, connection))
                    {
                        command.Parameters.AddWithValue("Id", id);
                        int rowsAffected = await command.ExecuteNonQueryAsync();
                        Console.WriteLine($"Одобрено записей: {rowsAffected}");
                    }
                }
            }
            catch (NpgsqlException ex)
            {
                Console.WriteLine($"Ошибка при одобрении кофе-автомата: {ex.Message}");
            }
        }

        public async Task RejectMachine(int id)
        {
            string sqlReject = "DELETE FROM coffeavtomat WHERE Id = @Id";

            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionStringForCoffeAutomat))
                {
                    await connection.OpenAsync();
                    using (NpgsqlCommand command = new NpgsqlCommand(sqlReject, connection))
                    {
                        command.Parameters.AddWithValue("Id", id);
                        int rowsAffected = await command.ExecuteNonQueryAsync();
                        Console.WriteLine($"Удалено записей: {rowsAffected}");
                    }
                }
            }
            catch (NpgsqlException ex)
            {
                Console.WriteLine($"Ошибка при отказе от кофе-автомата: {ex.Message}");
            }
        }
        public async Task<CofeAvtomat?> FindNearestCofeMachine(Location userLocation)
        {
            CofeAvtomat? nearestMachine = null;
            double nearestDistance = double.MaxValue;

            string sqlSelectExpression = "SELECT Name, Latitude, Longitude, PhotoURL FROM coffeavtomat WHERE IsApproved = TRUE";

            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionStringForCoffeAutomat))
                {
                    await connection.OpenAsync();
                    using (NpgsqlCommand command = new NpgsqlCommand(sqlSelectExpression, connection))
                    {
                        using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                string name = reader["name"].ToString();
                                double latitude = Convert.ToDouble(reader["Latitude"]);
                                double longitude = Convert.ToDouble(reader["Longitude"]);
                                string photoUrl = reader["PhotoURL"] != DBNull.Value ? reader["PhotoURL"].ToString() : string.Empty;

                                double distance = CalculateDistance(userLocation.Latitude, userLocation.Longitude, latitude, longitude);

                                if (distance < nearestDistance)
                                {
                                    nearestDistance = distance;
                                    nearestMachine = new CofeAvtomat(name, new Location(latitude, longitude), photoUrl);
                                }
                            }
                        }
                    }
                }
            }
            catch (NpgsqlException ex)
            {
                Console.WriteLine($"Ошибка при поиске кофе-автоматов: {ex.Message}");
            }

            return nearestMachine;
        }
        public async Task<bool> CheckIfMachineExists(double latitude, double longitude)
        {
            const string sqlQuery = @"
        SELECT COUNT(*) 
        FROM coffeavtomat
            WHERE ROUND(Longitude::numeric, 3) = ROUND(@Longitude::numeric, 3)
                  AND ROUND(Latitude::numeric, 3) = ROUND(@Latitude::numeric, 3);";

            using (var connection = new NpgsqlConnection(connectionStringForCoffeAutomat))
            {
                await connection.OpenAsync();
                using (var cmd = new NpgsqlCommand(sqlQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@Longitude", longitude);
                    cmd.Parameters.AddWithValue("@Latitude", latitude);

                    var result = await cmd.ExecuteScalarAsync();
                    return Convert.ToInt32(result) > 0;
                }
            }
        }
        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double RADIUS_OF_EARTH_KM = 6371;

            double dLat = DegreesToRadians(lat2 - lat1);
            double dLon = DegreesToRadians(lon2 - lon1);

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return RADIUS_OF_EARTH_KM * c;
        }

        private double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }        
    }
}
