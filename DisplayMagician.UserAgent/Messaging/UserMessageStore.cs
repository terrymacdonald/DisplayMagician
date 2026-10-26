using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DisplayMagician.Contracts;
using NLog;
using StoredMessage = DisplayMagician.Messaging.LocalMessage;

namespace DisplayMagician.UserAgent.Messaging;

internal sealed class UserMessageStore
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly string _messagesDirectory;
    private readonly DisplayMagician.Messaging.MessageSyncService _messageSyncService;

    public UserMessageStore(string userDataPath, DisplayMagician.Messaging.MessageSyncService messageSyncService)
    {
        _messagesDirectory = Path.Combine(userDataPath ?? throw new ArgumentNullException(nameof(userDataPath)), "Messages");
        _messageSyncService = messageSyncService ?? throw new ArgumentNullException(nameof(messageSyncService));
    }

    public MessageListResult GetMessages()
    {
        MessageView[] messages = _messageSyncService.GetMessages()
            .OrderByDescending(message => message.ReceivedUtc)
            .ThenByDescending(message => message.PublishedUtc ?? DateTime.MinValue)
            .Select(CreateView)
            .ToArray();
        return new MessageListResult
        {
            Messages = messages,
            UnreadCount = messages.Count(message => !message.IsRead)
        };
    }

    public bool SetReadState(IEnumerable<string>? messageIds, bool isRead)
    {
        HashSet<string> ids = new((messageIds ?? Array.Empty<string>()).Where(id => !string.IsNullOrWhiteSpace(id)), StringComparer.OrdinalIgnoreCase);
        if (ids.Count == 0)
        {
            return false;
        }

        return _messageSyncService.SetReadState(ids, isRead);
    }

    private MessageView CreateView(StoredMessage message)
    {
        return new MessageView
        {
            Id = message.Id,
            Title = message.Title,
            Content = ReadContent(message),
            Format = message.Format,
            PublishedUtc = message.PublishedUtc,
            ReceivedUtc = message.ReceivedUtc,
            IsRead = message.IsRead,
            ShowOnStartup = message.ShowOnStartup,
            IsFaulty = message.IsFaulty,
            Kind = message.Kind,
            ReleaseVersion = message.ReleaseVersion,
            ReleaseChannel = message.ReleaseChannel,
            UpdateAction = message.UpdateAction
        };
    }

    private string ReadContent(StoredMessage message)
    {
        if (string.IsNullOrWhiteSpace(message.MarkdownFileName) || !string.Equals(Path.GetFileName(message.MarkdownFileName), message.MarkdownFileName, StringComparison.Ordinal))
        {
            return string.Empty;
        }

        string contentPath = Path.Combine(_messagesDirectory, message.MarkdownFileName);
        try
        {
            return File.Exists(contentPath) ? File.ReadAllText(contentPath) : string.Empty;
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
        {
            Logger.Warn(ex, "UserMessageStore/ReadContent: Could not read message content (messageId={0}, path={1}).", message.Id, contentPath);
            return string.Empty;
        }
    }

}
