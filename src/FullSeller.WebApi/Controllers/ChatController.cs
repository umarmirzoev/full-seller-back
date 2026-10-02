using FullSeller.Domain.Entities;
using FullSeller.Domain.Enums;
using FullSeller.Domain.Interfaces;
using FullSeller.WebApi.Auth;
using FullSeller.WebApi.Contracts.Chat;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FullSeller.WebApi.Controllers;

[ApiController]
[Route("api/chat")]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly IChatRepository _chat;

    public ChatController(IChatRepository chat)
    {
        _chat = chat;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ChatMessageDto>>> GetHistory(CancellationToken ct)
    {
        var messages = await _chat.GetHistoryAsync(User.GetUserId(), ct);
        await _chat.MarkReadAsync(User.GetUserId(), ct);
        return Ok(messages.Select(ToDto).ToList());
    }

    [HttpPost]
    public async Task<ActionResult<ChatMessageDto>> SendMessage([FromBody] SendMessageRequest request, CancellationToken ct)
    {
        var message = new ChatMessage
        {
            UserId = User.GetUserId(),
            SenderType = ChatSenderType.User,
            Text = request.Text,
            AttachmentUrl = request.AttachmentUrl,
        };
        message.Id = await _chat.AddMessageAsync(message, ct);
        return Ok(ToDto(message));
    }

    private static ChatMessageDto ToDto(ChatMessage m) => new(m.Id, m.SenderType, m.Text, m.AttachmentUrl, m.SentAt, m.IsRead);
}
