using CodeSync.CollabService.OT;
using CodeSync.CollabService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace CodeSync.CollabService.Hubs
{
    [Authorize]
    public class CollabHub : Hub
    {
        private readonly RedisService _redis;
        private readonly OTService _otService;

        public CollabHub(
            RedisService redis,
            OTService otService)
        {
            _redis = redis;
            _otService = otService;
        }

        // User joins SignalR group for session
        public async Task JoinSession(string sessionId)
        {
            var userId = GetUserId();
            var username = GetUsername();

            await Groups.AddToGroupAsync(
                Context.ConnectionId, sessionId);

            // Get current document from Redis
            var document = await _redis.GetDocumentAsync(
                Guid.Parse(sessionId));

            // Send current document to joining user
            await Clients.Caller.SendAsync(
                "InitialDocument", document);

            // Notify others user joined
            await Clients.OthersInGroup(sessionId)
                .SendAsync("UserJoined", userId, username);
        }

        // User leaves SignalR group
        public async Task LeaveSession(string sessionId)
        {
            var userId = GetUserId();
            var username = GetUsername();

            await Groups.RemoveFromGroupAsync(
                Context.ConnectionId, sessionId);

            await Clients.OthersInGroup(sessionId)
                .SendAsync("UserLeft", userId, username);
        }

        // User sends an edit operation
        public async Task SendEdit(
            string sessionId, OTOperation operation)
        {
            var sessionGuid = Guid.Parse(sessionId);

            // Get current document
            var document = await _redis
                .GetDocumentAsync(sessionGuid);

            // Apply operation to document
            var newDocument = _otService
                .Apply(document, operation);

            // Save updated document to Redis
            await _redis.SetDocumentAsync(
                sessionGuid, newDocument);

            // Broadcast operation to ALL users
            // including sender (so all stay in sync)
            await Clients.Group(sessionId)
                .SendAsync("ReceiveEdit", operation);
        }

        // User sends cursor position
        public async Task SendCursor(
            string sessionId,
            int line,
            int col,
            string color)
        {
            var userId = GetUserId();
            var username = GetUsername();
            var sessionGuid = Guid.Parse(sessionId);

            // Save cursor to Redis
            await _redis.SetCursorAsync(
                sessionGuid, userId, line, col);

            // Broadcast to OTHER users only
            await Clients.OthersInGroup(sessionId)
                .SendAsync(
                    "ReceiveCursor",
                    userId, username,
                    line, col, color);
        }

        // On disconnect
        public override async Task OnDisconnectedAsync(
            Exception? exception)
        {
            var userId = GetUserId();
            var username = GetUsername();

            await base.OnDisconnectedAsync(exception);
        }

        private string GetUserId()
            => Context.User?.FindFirst(
                ClaimTypes.NameIdentifier)?.Value ?? "";

        private string GetUsername()
            => Context.User?.FindFirst(
                ClaimTypes.Name)?.Value ?? "Unknown";
    }
}