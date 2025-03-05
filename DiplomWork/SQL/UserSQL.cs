using System.Text;
using Npgsql;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DiplomWork
{
    internal class UserSQL
    {        
        string connectionString ="Host=localhost;Username=postgres;Database=CoffeAvtomat;Include Error Detail=true";
        string sqlSelectExpression = "SELECT Name, TelegramId FROM Users";       
        
        public async Task RegisterUser(ITelegramBotClient botClient, Update update)
        {
            var userId = update.Message.From.Id;
            var userName = update.Message.From.FirstName;
            
            if (!await UserExists(userId))
            {                
                await AddUserToDatabase(userId, userName);                
            }
            else
            {
                return;
            }
        }
        private async Task<bool> UserExists(long telegramId)
        {
            const string sqlQuery = "SELECT COUNT(1) FROM Users WHERE TelegramId = @TelegramId";

            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                await connection.OpenAsync();
                using (NpgsqlCommand command = new NpgsqlCommand(sqlQuery, connection))
                {
                    command.Parameters.AddWithValue("@TelegramId", telegramId);
                    var result = await command.ExecuteScalarAsync();
                    return Convert.ToInt32(result) > 0;
                }
            }
        }
        private async Task AddUserToDatabase(long telegramId, string name)
        {
            const string sqlInsert = "INSERT INTO Users (TelegramId, Name, IsAdmin) VALUES (@TelegramId, @Name, @IsAdmin)";

            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                await connection.OpenAsync();
                using (NpgsqlCommand command = new NpgsqlCommand(sqlInsert, connection))
                {
                    command.Parameters.AddWithValue("@TelegramId", telegramId);
                    command.Parameters.AddWithValue("@Name", name);
                    command.Parameters.AddWithValue("@IsAdmin", false); 
                    await command.ExecuteNonQueryAsync();
                }
            }
        }
        public async Task<bool> IsUserAdmin(long telegramId)
        {            
            const string sqlQuery = "SELECT IsAdmin FROM Users WHERE TelegramId = @TelegramId";

            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                await connection.OpenAsync();
                using (NpgsqlCommand command = new NpgsqlCommand(sqlQuery, connection))
                {
                    command.Parameters.AddWithValue("@TelegramId", telegramId);
                    var result = await command.ExecuteScalarAsync();
                    return result != null && Convert.ToBoolean(result);
                }
            }
        }
        public async Task UpdateUserRating(long telegramId, int points)
        {
            const string sql = @"UPDATE Users 
                        SET Rating = Rating + @Points 
                        WHERE TelegramId = @TelegramId";

            using (var connection = new NpgsqlConnection(connectionString))
            {
                await connection.OpenAsync();
                using (var cmd = new NpgsqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@Points", points);
                    cmd.Parameters.AddWithValue("@TelegramId", telegramId);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }
        public async Task<string> GetTopUsersRating(int limit = 10)
        {
            const string sql = @"SELECT Name, Rating 
                        FROM Users 
                        ORDER BY Rating DESC 
                        LIMIT @Limit";

            using (var connection = new NpgsqlConnection(connectionString))
            {
                await connection.OpenAsync();
                using (var cmd = new NpgsqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@Limit", limit);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        var result = new StringBuilder("🏆 Топ пользователей:\n\n");
                        int position = 1;

                        while (await reader.ReadAsync())
                        {
                            result.AppendLine($"{position}. {reader.GetString(0)} - {reader.GetInt32(1)} баллов");
                            position++;
                        }
                        return result.ToString();
                    }
                }
            }
        }        
    }
}
