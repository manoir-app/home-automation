using Home.Common.Model;
using Home.Graph.Common;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Home.Graph.Services
{
    /// <summary>
    /// Service de gestion des utilisateurs
    /// </summary>
    public class UserService
    {
        private readonly IMongoCollection<User> _usersCollection;

        public UserService()
        {
            var mongoDb = MongoDbHelper.GetClient<User>();
            _usersCollection = mongoDb.Database.GetCollection<User>("users");
        }

        /// <summary>
        /// Récupère tous les utilisateurs principaux
        /// </summary>
        public List<User> GetMainUsers()
        {
            return _usersCollection
                .Find(u => u.IsMain == true)
                .ToList();
        }

        /// <summary>
        /// Récupère les utilisateurs actuellement présents
        /// </summary>
        public List<User> GetPresentUsers()
        {
            var allUsers = GetMainUsers();
            var presentUsers = new List<User>();

            foreach (var user in allUsers)
            {
                var presence = GetUserPresence(user.Id);
                if (presence?.IsPresent == true)
                {
                    presentUsers.Add(user);
                }
            }

            return presentUsers;
        }

        /// <summary>
        /// Récupère un utilisateur par son ID
        /// </summary>
        public User GetUserById(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return null;

            return _usersCollection
                .Find(u => u.Id == userId)
                .FirstOrDefault();
        }

        /// <summary>
        /// Récupère le statut de présence d'un utilisateur
        /// </summary>
        public PresenceData GetUserPresence(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return null;

            var user = GetUserById(userId);
            return user?.Presence;
        }

        /// <summary>
        /// Envoie une notification à un utilisateur
        /// </summary>
        public async Task<bool> SendNotificationAsync(string userId, UserNotification notification)
        {
            if (string.IsNullOrEmpty(userId) || notification == null)
                return false;

            try
            {
                notification.UserId = userId;
                notification.Id = notification.Id ?? Guid.NewGuid().ToString();
                notification.Date = notification.Date == default ? DateTimeOffset.Now : notification.Date;

                // Sauvegarder la notification
                var mongoClient = MongoDbHelper.GetClient<UserNotification>();
                var notificationsCollection = mongoClient.Database.GetCollection<UserNotification>("usernotifications");
                await notificationsCollection.InsertOneAsync(notification);

                // Envoyer via SignalR/Mobile si nécessaire
                // TODO: Implémenter l'envoi push mobile
                
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Log("user-service", "send-notification-error", $"Erreur: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Récupère les TODOs d'un utilisateur
        /// </summary>
        public List<TodoItem> GetUserTodos(string userId, string status = null, int? limit = null)
        {
            if (string.IsNullOrEmpty(userId))
                return new List<TodoItem>();

            try
            {
                var mongoClient = MongoDbHelper.GetClient<TodoItem>();
                var todosCollection = mongoClient.Database.GetCollection<TodoItem>("todoitems");

                var filter = Builders<TodoItem>.Filter.Eq(t => t.UserId, userId);

                if (!string.IsNullOrEmpty(status))
                {
                    TodoItemStatus statusEnum;
                    if (Enum.TryParse<TodoItemStatus>(status, true, out statusEnum))
                    {
                        var statusFilter = Builders<TodoItem>.Filter.Eq(t => t.Status, statusEnum);
                        filter = Builders<TodoItem>.Filter.And(filter, statusFilter);
                    }
                }

                var query = todosCollection.Find(filter).SortByDescending(t => t.DueDate);

                if (limit.HasValue && limit.Value > 0)
                {
                    query = query.Limit(limit.Value);
                }

                return query.ToList();
            }
            catch (Exception ex)
            {
                LogHelper.Log("user-service", "get-todos-error", $"Erreur: {ex.Message}");
                return new List<TodoItem>();
            }
        }
    }
}
