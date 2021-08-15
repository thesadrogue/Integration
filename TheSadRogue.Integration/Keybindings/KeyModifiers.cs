using JetBrains.Annotations;

namespace SadRogue.Integration.Keybindings
{
    /// <summary>
    /// Modifiers for key commands.
    /// </summary>
    [PublicAPI]
    public enum KeyModifiers
    {
        /// <summary>
        /// Represents no key modifiers.
        /// </summary>
        None,
        /// <summary>
        /// Represents the left shift key.
        /// </summary>
        LeftShift,
        /// <summary>
        /// Represents the right shift key.
        /// </summary>
        RightShift,
        /// <summary>
        /// Represents either the left or right shift key.
        /// </summary>
        Shift,
        /// <summary>
        /// Represents the left control key.
        /// </summary>
        LeftCtrl,
        /// <summary>
        /// Represents the right control key.
        /// </summary>
        RightCtrl,
        /// <summary>
        /// Represents either the left or right ctrl key.
        /// </summary>
        Ctrl,
        /// <summary>
        /// Represents the left alt key.
        /// </summary>
        LeftAlt,
        /// <summary>
        /// Represents the right alt key.
        /// </summary>
        RightAlt,
        /// <summary>
        /// Represents either the left or right alt key.
        /// </summary>
        Alt
    };
}
