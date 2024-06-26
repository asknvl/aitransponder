namespace Telegram.Bot.Types.ReplyMarkups;

/// <summary>
/// Upon receiving a message with this object, Telegram clients will remove the current custom keyboard and display
/// the default letter-keyboard. By default, custom keyboards are displayed until a new keyboard is sent by a bot.
/// An exception is made for one-time keyboards that are hidden immediately after the user presses a button
/// (see <see cref="ReplyKeyboardMarkup"/>).
/// </summary>
public class ReplyKeyboardRemove : ReplyMarkupBase
{
    /// <summary>
    /// Requests clients to remove the custom keyboard (user will not be able to summon this keyboard; if you want to
    /// hide the keyboard from sight but keep it accessible, use '<see cref="ReplyKeyboardMarkup.OneTimeKeyboard"/>'
    /// in <see cref="ReplyKeyboardMarkup"/>)
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public bool RemoveKeyboard => true;
}
