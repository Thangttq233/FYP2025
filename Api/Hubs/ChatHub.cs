using FYP2025.Application.Services.ChatService;
using FYP2025.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace FYP2025.Api.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ChatHub(IChatService chatService, UserManager<ApplicationUser> userManager)
        {
            _chatService = chatService;
            _userManager = userManager;
        }

        public async Task SendMessage(string recipientUserId, string message)
        {
            var senderId = Context.UserIdentifier;
            var sender = await _userManager.FindByIdAsync(senderId);
            var recipient = await _userManager.FindByIdAsync(recipientUserId);

            if (recipient == null || sender == null)
            {
                return;
            }

            var conversation = await _chatService.GetOrCreateConversationAsync(senderId, recipientUserId);
            var savedMessage = await _chatService.SaveMessageAsync(conversation.Id, senderId, message);

            var messageDto = new 
            {
                conversationId = conversation.Id,
                senderId,
                senderName = sender.FullName,
                content = savedMessage.Content,
                timestamp = savedMessage.Timestamp
            };

            await Clients.User(recipientUserId).SendAsync("ReceiveMessage", messageDto);
            await Clients.User(senderId).SendAsync("ReceiveMessage", messageDto);
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(System.Exception exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}
