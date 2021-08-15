using System;
using JetBrains.Annotations;
using SadConsole.Input;
using SadRogue.Primitives;

namespace SadRogue.Integration.Keybindings
{
    /// <summary>
    /// Represents an input state that can be bound to an action.
    /// </summary>
    [PublicAPI]
    public readonly struct InputKey : IEquatable<InputKey>, IMatchable<InputKey>
    {
        /// <summary>
        /// Key for the input state.
        /// </summary>
        public readonly Keys Key;

        /// <summary>
        /// Any modifiers related to shift (left-shift, right-shift, or either) that are associated with this control.
        /// </summary>
        public readonly KeyModifiers Shift;

        /// <summary>
        /// Any modifiers related to control (left-ctrl, right-ctrl, or either) that are associated with this control.
        /// </summary>
        public readonly KeyModifiers Ctrl;

        /// <summary>
        /// Any modifiers related to alt (left-alt, right-alt, or either) that are associated with this control.
        /// </summary>
        public readonly KeyModifiers Alt;


        /// <summary>
        /// Creates a new input state.
        /// </summary>
        /// <param name="key">Key for the input state.</param>
        /// <param name="modifiers">Any required modifiers for the input state (SHIFT, CTRL, etc.)</param>
        public InputKey(Keys key, params KeyModifiers[] modifiers)
        {
            Key = key;

            Shift = KeyModifiers.None;
            Ctrl = KeyModifiers.None;
            Alt = KeyModifiers.None;
            foreach (var modifier in modifiers)
            {
                switch (modifier)
                {
                    case KeyModifiers.LeftAlt:
                    case KeyModifiers.RightAlt:
                    case KeyModifiers.Alt:
                        if (Alt != KeyModifiers.None)
                            throw new ArgumentException("Only one Alt modifier is allowed.", nameof(modifiers));
                        Alt = modifier;
                        break;
                    case KeyModifiers.LeftCtrl:
                    case KeyModifiers.RightCtrl:
                    case KeyModifiers.Ctrl:
                        if (Ctrl != KeyModifiers.None)
                            throw new ArgumentException("Only one Ctrl modifier is allowed.", nameof(modifiers));
                        Ctrl = modifier;
                        break;
                    case KeyModifiers.RightShift:
                    case KeyModifiers.LeftShift:
                    case KeyModifiers.Shift:
                        if (Shift != KeyModifiers.None)
                            throw new ArgumentException("Only one Shift modifier is allowed.", nameof(modifiers));
                        Shift = modifier;
                        break;
                }
            }
        }

        /// <summary>
        /// Implicitly converts a Keys enumeration value to an input state (with no modifiers), for convenience.
        /// </summary>
        /// <param name="key">Key to convert.</param>
        /// <returns>An input state representing the key given with no modifiers.</returns>
        public static implicit operator InputKey(Keys key) => new InputKey(key);

        /// <summary>
        /// True if the given keyboard state meets the modifier conditions for this input state.
        /// </summary>
        /// <remarks>
        /// This function ensures that precisely the modifiers the input state specifies are true, and NO others.
        /// </remarks>
        /// <param name="keyboard">Keyboard state to check.</param>
        /// <param name="exactState">Whether to check for an exact state (true), or simply check that all required modifier keys (non-exclusive) are pressed.</param>
        /// <returns>True if the keyboard state meets the modifier conditions for this input state; false otherwise.</returns>
        public bool ModiferConditionsMet(Keyboard keyboard, bool exactState)
        {
            if (exactState)
            {
                // Check ctrl state
                if (!MatchesExactState(keyboard, Ctrl, Keys.LeftControl, Keys.RightControl, KeyModifiers.LeftCtrl,
                    KeyModifiers.RightCtrl))
                    return false;

                // Check alt state
                if (!MatchesExactState(keyboard, Alt, Keys.LeftAlt, Keys.RightAlt, KeyModifiers.LeftAlt,
                    KeyModifiers.RightAlt))
                    return false;

                // Check shift state
                return MatchesExactState(keyboard, Shift, Keys.LeftShift, Keys.RightShift, KeyModifiers.LeftShift,
                    KeyModifiers.RightShift);
            }

            // Check ctrl state
            if (!MatchesState(keyboard, Ctrl, Keys.LeftControl, Keys.RightControl, KeyModifiers.LeftCtrl,
                KeyModifiers.RightCtrl))
                return false;

            // Check alt state
            if (!MatchesState(keyboard, Alt, Keys.LeftAlt, Keys.RightAlt, KeyModifiers.LeftAlt,
                KeyModifiers.RightAlt))
                return false;

            // Check shift state
            return MatchesState(keyboard, Shift, Keys.LeftShift, Keys.RightShift, KeyModifiers.LeftShift,
                KeyModifiers.RightShift);
        }

        /// <summary>
        /// True if the input state given precisely matches the current one in key and modifiers; false otherwise.
        /// </summary>
        /// <param name="other"/>
        /// <returns/>
        public bool Equals(InputKey other) => Matches(other);

        /// <summary>
        /// True if the given object is an input state that precisely matches the current one in key and modifiers;
        /// false otherwise.
        /// </summary>
        /// <param name="obj"/>
        /// <returns/>
        public override bool Equals(object? obj) => obj is InputKey m && Matches(m);

        /// <summary>
        /// True if the input state given precisely matches the current one in key and modifiers; false otherwise.
        /// </summary>
        /// <param name="other"/>
        /// <returns/>
        public bool Matches(InputKey other)
            => Key == other.Key && Ctrl == other.Ctrl && Alt == other.Alt && Shift == other.Shift;

        /// <inheritdoc />
        public override int GetHashCode() => HashCode.Combine(Key, Ctrl, Alt, Shift);

        /// <summary>
        /// True if the two input states specified precisely match in key and modifiers; false otherwise.
        /// </summary>
        /// <param name="lhs"/>
        /// <param name="rhs"/>
        /// <returns/>
        public static bool operator==(InputKey lhs, InputKey rhs) => lhs.Matches(rhs);

        /// <summary>
        /// True if the two input states differ at all in key or modifiers; false if they are equivalent.
        /// </summary>
        /// <param name="lhs"/>
        /// <param name="rhs"/>
        /// <returns/>
        public static bool operator!=(InputKey lhs, InputKey rhs) => !lhs.Matches(rhs);

        private static bool MatchesExactState(Keyboard keyboard, KeyModifiers state, Keys keyLeft, Keys keyRight, KeyModifiers left,
                                  KeyModifiers right)
        {
            // Check key state
            bool leftDown = keyboard.IsKeyDown(keyLeft);
            bool rightDown = keyboard.IsKeyDown(keyRight);

            if (state == KeyModifiers.None) return !(leftDown || rightDown);
            if (state == left) return leftDown && !rightDown;
            if (state == right) return rightDown && !leftDown;

            // Invariants enforced by class allow us to assume this means "either left or right".
            return leftDown && !rightDown || rightDown && !leftDown;
        }

        private static bool MatchesState(Keyboard keyboard, KeyModifiers state, Keys keyLeft, Keys keyRight, KeyModifiers left,
                                              KeyModifiers right)
        {
            if (state == KeyModifiers.None) return true;
            if (state == left) return keyboard.IsKeyDown(keyLeft);
            if (state == right) return keyboard.IsKeyDown(keyRight);

            // Invariants enforced by class allow us to assume this means "either left or right".
            return keyboard.IsKeyDown(keyLeft) || keyboard.IsKeyDown(keyRight);
        }
    }
}
