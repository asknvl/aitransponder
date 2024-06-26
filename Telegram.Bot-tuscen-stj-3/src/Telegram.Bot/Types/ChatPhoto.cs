namespace Telegram.Bot.Types;

/// <summary>
/// Collection of fileIds of profile pictures of a chat.
/// </summary>
public class ChatPhoto
{
    /// <summary>
    /// File identifier of small (160x160) chat photo. This FileId can be used only for photo download and only
    /// for as long as the photo is not changed.
    /// </summary>
    [JsonRequired]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public string SmallFileId { get; set; } = default!;

    /// <summary>
    /// Unique file identifier of small (160x160) chat photo, which is supposed to be the same over time and for
    /// different bots. Can't be used to download or reuse the file.
    /// </summary>
    [JsonRequired]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public string SmallFileUniqueId { get; set; } = default!;

    /// <summary>
    /// File identifier of big (640x640) chat photo. This FileId can be used only for photo download and only for
    /// as long as the photo is not changed.
    /// </summary>
    [JsonRequired]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public string BigFileId { get; set; } = default!;

    /// <summary>
    /// Unique file identifier of big (640x640) chat photo, which is supposed to be the same over time and for
    /// different bots. Can't be used to download or reuse the file.
    /// </summary>
    [JsonRequired]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public string BigFileUniqueId { get; set; } = default!;
}
