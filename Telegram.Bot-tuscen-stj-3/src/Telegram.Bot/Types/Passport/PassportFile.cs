



using Telegram.Bot.Serialization;
// ReSharper disable once CheckNamespace

namespace Telegram.Bot.Types.Passport;

/// <summary>
/// This object represents a file uploaded to Telegram Passport. Currently all Telegram Passport files are in JPEG format when decrypted and don't exceed 10MB.
/// </summary>
public class PassportFile : FileBase
{
    /// <summary>
    /// DateTime when the file was uploaded
    /// </summary>
    [JsonRequired]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    [JsonConverter(typeof(UnixDateTimeConverter))]
    public DateTime FileDate { get; set; }
}
