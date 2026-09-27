using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DisplayMagician.Contracts;
using NLog;
using Newtonsoft.Json;
using MessageStoreDocument = DisplayMagician.Messaging.MessageStoreDocument;
using StoredMessage = DisplayMagician.Messaging.LocalMessage;

namespace DisplayMagician.UserAgent.Messaging;

internal sealed class UserMessageStore
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly string _messagesDirectory;
    private readonly string _indexPath;
    private readonly object _syncRoot = new();

    public UserMessageStore(string userDataPath)
    {
        _messagesDirectory = Path.Combine(userDataPath ?? throw new ArgumentNullException(nameof(userDataPath)), "Messages");
        _indexPath = Path.Combine(_messagesDirectory, "MessagesIndex.json");
    }

    public MessageListResult GetMessages()
    {
        lock (_syncRoot)
        {
            MessageStoreDocument store = LoadStore();
            MessageView[] messages = store.Messages
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
    }

    public bool SetReadState(IEnumerable<string>? messageIds, bool isRead)
    {
        HashSet<string> ids = new((messageIds ?? Array.Empty<string>()).Where(id => !string.IsNullOrWhiteSpace(id)), StringComparer.OrdinalIgnoreCase);
        if (ids.Count == 0)
        {
            return false;
        }

        lock (_syncRoot)
        {
            MessageStoreDocument store = LoadStore();
            bool changed = false;
            foreach (StoredMessage message in store.Messages.Where(message => ids.Contains(message.Id)))
            {
                if (message.IsRead == isRead)
                {
                    continue;
                }

                message.IsRead = isRead;
                changed = true;
            }

            if (changed)
            {
                SaveStore(store);
            }

            return changed;
        }
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

    private MessageStoreDocument LoadStore()
    {
        if (!File.Exists(_indexPath))
        {
            return new MessageStoreDocument();
        }

        try
        {
            string json = File.ReadAllText(_indexPath);
            return JsonConvert.DeserializeObject<MessageStoreDocument>(json) ?? new MessageStoreDocument();
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is JsonException)
        {
            Logger.Warn(ex, "UserMessageStore/LoadStore: Could not read message store (path={0}).", _indexPath);
            return new MessageStoreDocument();
        }
    }

    private void SaveStore(MessageStoreDocument store)
    {
        try
        {
            Directory.CreateDirectory(_messagesDirectory);
            string temporaryPath = Path.Combine(_messagesDirectory, $".MessagesIndex.{Guid.NewGuid():N}.tmp");
            File.WriteAllText(temporaryPath, JsonConvert.SerializeObject(store, Formatting.Indented));
            if (File.Exists(_indexPath))
            {
                File.Replace(temporaryPath, _indexPath, null);
            }
            else
            {
                File.Move(temporaryPath, _indexPath);
            }
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
        {
            Logger.Warn(ex, "UserMessageStore/SaveStore: Could not persist message store (path={0}).", _indexPath);
        }
    }

}
