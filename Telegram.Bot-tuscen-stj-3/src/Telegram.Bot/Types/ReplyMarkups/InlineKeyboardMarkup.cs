using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Telegram.Bot.Types.ReplyMarkups;

/// <summary>
/// This object represents an inline keyboard that appears right next to the <see cref="Message"/> it belongs to.
/// </summary>
/// <remarks>
/// Note: This will only work in Telegram versions released after 9 April, 2016. Older clients will display
/// <i>unsupported message</i>.
/// </remarks>

public class InlineKeyboardMarkup : IReplyMarkup
{
    /// <summary>
    /// Array of <see cref="InlineKeyboardButton"/> rows, each represented by an Array of
    /// <see cref="InlineKeyboardButton"/>.
    /// </summary>
    [JsonRequired]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public required IEnumerable<IEnumerable<InlineKeyboardButton>> InlineKeyboard { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="InlineKeyboardMarkup"/>
    /// </summary>
    public InlineKeyboardMarkup()
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="InlineKeyboardMarkup"/> class with only one keyboard button
    /// </summary>
    /// <param name="inlineKeyboardButton">Keyboard button</param>
    [SetsRequiredMembers]
    public InlineKeyboardMarkup(InlineKeyboardButton inlineKeyboardButton)
        : this([inlineKeyboardButton])
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="InlineKeyboardMarkup"/> class with a one-row keyboard
    /// </summary>
    /// <param name="inlineKeyboardRow">The inline keyboard row</param>
    [SetsRequiredMembers]
    public InlineKeyboardMarkup(IEnumerable<InlineKeyboardButton> inlineKeyboardRow)
        : this([inlineKeyboardRow])
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="InlineKeyboardMarkup"/> class.
    /// </summary>
    /// <param name="inlineKeyboard">The inline keyboard.</param>
    [JsonConstructor]
    [SetsRequiredMembers]
    public InlineKeyboardMarkup(IEnumerable<IEnumerable<InlineKeyboardButton>> inlineKeyboard) =>
        InlineKeyboard = inlineKeyboard;

    /// <summary>
    /// Generate an empty inline keyboard markup
    /// </summary>
    /// <returns>Empty inline keyboard markup</returns>
    public static InlineKeyboardMarkup Empty() =>
        new(Array.Empty<InlineKeyboardButton[]>());

    /// <summary>
    /// Generate an inline keyboard markup with one button
    /// </summary>
    /// <param name="button">Inline keyboard button</param>
    [return: NotNullIfNotNull(nameof(button))]
    public static implicit operator InlineKeyboardMarkup?(InlineKeyboardButton? button) =>
        button is null ? default : new(button);

    /// <summary>
    /// Generate an inline keyboard markup with one button
    /// </summary>
    /// <param name="buttonText">Text of the button</param>
    [return: NotNullIfNotNull(nameof(buttonText))]
    public static implicit operator InlineKeyboardMarkup?(string? buttonText) =>
        buttonText is null ? default : new(buttonText!);

    /// <summary>
    /// Generate an inline keyboard markup from multiple buttons
    /// </summary>
    /// <param name="inlineKeyboard">Keyboard buttons</param>
    [return: NotNullIfNotNull(nameof(inlineKeyboard))]
    public static implicit operator InlineKeyboardMarkup?(IEnumerable<InlineKeyboardButton>[]? inlineKeyboard) =>
        inlineKeyboard is null ? default : new(inlineKeyboard);

    /// <summary>
    /// Generate an inline keyboard markup from multiple buttons on 1 row
    /// </summary>
    /// <param name="inlineKeyboard">Keyboard buttons</param>
    [return: NotNullIfNotNull(nameof(inlineKeyboard))]
    public static implicit operator InlineKeyboardMarkup?(InlineKeyboardButton[]? inlineKeyboard) =>
        inlineKeyboard is null ? default : new(inlineKeyboard);
}
