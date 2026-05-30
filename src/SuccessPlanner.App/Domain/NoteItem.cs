namespace SuccessPlanner.App.Domain;

public sealed class NoteItem
{
    private readonly List<string> _tags = [];

    private NoteItem(
        Guid id,
        NoteOwnerType ownerType,
        Guid? ownerId,
        string text,
        DateTimeOffset createdAt)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Id cannot be empty.", nameof(id));
        }

        ValidateOwner(ownerType, ownerId);

        Id = id;
        OwnerType = ownerType;
        OwnerId = ownerId;
        Text = NormalizeRequired(text, nameof(text));
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public Guid Id { get; }

    public NoteOwnerType OwnerType { get; private set; }

    public Guid? OwnerId { get; private set; }

    public string Text { get; private set; }

    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public bool IsPinned { get; private set; }

    public bool IsReviewHighlight { get; private set; }

    public IReadOnlyList<string> Tags => _tags;

    public static NoteItem Capture(string text)
    {
        return new NoteItem(Guid.NewGuid(), NoteOwnerType.Inbox, null, text, DateTimeOffset.Now);
    }

    public static NoteItem Create(NoteOwnerType ownerType, Guid? ownerId, string text)
    {
        return new NoteItem(Guid.NewGuid(), ownerType, ownerId, text, DateTimeOffset.Now);
    }

    public static NoteItem Rehydrate(
        Guid id,
        NoteOwnerType ownerType,
        Guid? ownerId,
        string text,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt,
        bool isPinned = false,
        bool isReviewHighlight = false,
        IEnumerable<string>? tags = null)
    {
        NoteItem item = new(id, ownerType, ownerId, text, createdAt)
        {
            UpdatedAt = updatedAt,
            IsPinned = isPinned,
            IsReviewHighlight = isReviewHighlight
        };

        if (tags is not null)
        {
            foreach (string tag in tags)
            {
                item.AddTag(tag);
            }

            item.UpdatedAt = updatedAt;
        }

        return item;
    }

    public void MoveTo(NoteOwnerType ownerType, Guid? ownerId)
    {
        ValidateOwner(ownerType, ownerId);

        OwnerType = ownerType;
        OwnerId = ownerId;
        Touch();
    }

    public void UpdateText(string text)
    {
        Text = NormalizeRequired(text, nameof(text));
        Touch();
    }

    public void AppendText(string text)
    {
        string normalized = NormalizeRequired(text, nameof(text));
        Text = string.IsNullOrWhiteSpace(Text) ? normalized : $"{Text}{Environment.NewLine}{normalized}";
        Touch();
    }

    public void Pin()
    {
        IsPinned = true;
        Touch();
    }

    public void Unpin()
    {
        IsPinned = false;
        Touch();
    }

    public void MarkReviewHighlight()
    {
        IsReviewHighlight = true;
        AddTag("Review");
        Touch();
    }

    public void ClearReviewHighlight()
    {
        IsReviewHighlight = false;
        RemoveTag("Review");
        Touch();
    }

    public void AddTag(string tag)
    {
        string normalized = NormalizeRequired(tag, nameof(tag));
        if (!_tags.Contains(normalized, StringComparer.OrdinalIgnoreCase))
        {
            _tags.Add(normalized);
            Touch();
        }
    }

    public void RemoveTag(string tag)
    {
        int removed = _tags.RemoveAll(existing => string.Equals(existing, tag, StringComparison.OrdinalIgnoreCase));
        if (removed > 0)
        {
            Touch();
        }
    }

    private void Touch()
    {
        UpdatedAt = DateTimeOffset.Now;
    }

    private static void ValidateOwner(NoteOwnerType ownerType, Guid? ownerId)
    {
        if (!Enum.IsDefined(typeof(NoteOwnerType), ownerType))
        {
            throw new ArgumentOutOfRangeException(nameof(ownerType), "Owner type is not valid.");
        }

        if (ownerType == NoteOwnerType.Inbox)
        {
            return;
        }

        if (ownerId is null || ownerId == Guid.Empty)
        {
            throw new ArgumentException("Owner id cannot be empty for attached notes.", nameof(ownerId));
        }
    }

    private static string NormalizeRequired(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value cannot be blank.", parameterName);
        }

        return value.Trim();
    }
}
