using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using GoRogue;
using GoRogue.GameFramework;
using JetBrains.Annotations;
using SadConsole;
using SadConsole.Input;
using SadRogue.Integration.Components;
using SadRogue.Primitives;

namespace SadRogue.Integration.Keybindings
{
    /// <summary>
    /// A keybindings component for mapping player inputs to actions.
    /// </summary>
    /// <remarks>
    /// This keybindings component is designed to provide a fairly robust input handling system that can be used to
    /// map keybindings to actions, and process them during the game loop.  It must be attached to a player entity
    /// in order to interact directly with that entity.
    ///
    /// Keybindings may be supplied simply as SadConsole Keys enumeration values, or as InputKey structures.  InputKeys
    /// provide built-in support for typical "modifier" keys, including shift, control, and alt.
    ///
    /// The component supports two main types of bindings:
    ///     1. Actions - An action simply maps a keybinding to a function to call when that input key is pressed.
    ///     2. Motions - A motion instead maps a keybinding to a Direction.  When the keybinding is pressed, the Direction
    ///        the key is mapped to is passed to the <see cref="MotionHandler"/> function, which will handle it.  This can
    ///        serve as a convenient way to implement movement, look-around, and other direction-centric input.
    ///
    /// For Motions, a default motion handler is provided which will simply attempt to move the Parent object of this
    /// component in the direction generated.If you need to do something more complex than simply attempt to move in a
    /// direction, you may provide your own <see cref="MotionHandler"/>.
    ///
    /// The component also has a number of configuration parameters that affect how keybindings are interpreted, which
    /// may be useful to tailor it to your use case.
    ///
    /// First is the <see cref="ExactMatches"/> flag.  This defaults to true, and in this mode it will only interpret
    /// a keybinding as pressed if the modifiers currently pressed match the ones in the keybinding EXACTLY.  For example,
    /// in this mode, a keybinding of Ctrl + A, will only be activated if the user presses Ctrl + A, not Ctrl + Shift + A,
    /// Ctrl + Alt + A, or any other combination.
    ///
    /// When <see cref="ExactMatches"/> is set to false, modifier keys on keybindings are treated differently.  Modifier
    /// keys listed in the keybinding are required, however extra modifier keys are allowed, so long as there is not
    /// another registered binding that matches specifically.  In this mode, if we have the following keybindings
    /// registered in the component:
    ///     1. Ctrl + A
    ///     2. Ctrl + Alt + A
    ///     3. A
    /// Then if the user presses Ctrl + A,  Ctrl + Shift + A, or any combination of Ctrl and other modifiers NOT
    /// including Alt, keybinding 1 will be selected.  Pressing Ctrl + Alt + A or Ctrl + Alt + Shift + A will select
    /// keybinding 2, and simply pressing A, or Alt + A will select keybinding 3.
    ///
    /// Non-exact matching can be useful if you wish to ignore modifier states unless they are relevant, OR if you
    /// wish to handle modifier keys manually within a given keybinding's handler.
    /// </remarks>
    [PublicAPI]
    public class PlayerKeybindingsComponent : RogueLikeComponentBase
    {
        #region Commonly Used Motion Schemes
        /// <summary>
        /// Motions that map the arrow keys to appropriate movement directions.
        /// </summary>
        public static readonly IEnumerable<(InputKey binding, Direction direction)> ArrowMotions = new[]
        {
            ((InputKey)Keys.Up, Direction.Up),
            (Keys.Right, Direction.Right),
            (Keys.Down, Direction.Down),
            (Keys.Left, Direction.Left)
        };

        /// <summary>
        /// Motions that map the WASD keys to appropriate movement directions.
        /// </summary>
        public static readonly IEnumerable<(InputKey binding, Direction direction)> WasdMotions = new[]
        {
            ((InputKey)Keys.W, Direction.Up),
            (Keys.D, Direction.Right),
            (Keys.S, Direction.Down),
            (Keys.A, Direction.Left)
        };

        /// <summary>
        /// Motions that map NumPad 8, 6, 2, and 4 to the appropriate cardinal movement directions.
        /// </summary>
        public static readonly IEnumerable<(InputKey binding, Direction direction)> NumPadCardinalMotions = new[]
        {
            ((InputKey)Keys.NumPad8, Direction.Up),
            (Keys.NumPad6, Direction.Right),
            (Keys.NumPad2, Direction.Down),
            (Keys.NumPad4, Direction.Left)
        };

        /// <summary>
        /// Motions that map NumPad 9, 3, 1, and 7 to the appropriate diagonal movement directions.
        /// </summary>
        public static readonly IEnumerable<(InputKey binding, Direction direction)> NumPadDiagonalMotions = new[]
        {
            ((InputKey)Keys.NumPad9, Direction.UpRight),
            (Keys.NumPad3, Direction.DownRight),
            (Keys.NumPad1, Direction.DownLeft),
            (Keys.NumPad7, Direction.UpLeft)
        };

        /// <summary>
        /// Motions that map the NumPad keys to the appropriate movement (8-way) movement directions.
        /// </summary>
        /// <remarks>
        /// This is simply a combination of <see cref="NumPadCardinalMotions"/> and <see cref="NumPadDiagonalMotions"/>.
        /// </remarks>
        public static readonly IEnumerable<(InputKey binding, Direction direction)> NumPadAllMotions =
            NumPadCardinalMotions.Concat(NumPadDiagonalMotions).ToArray();
        #endregion

        private readonly Dictionary<Keys, List<(InputKey binding, Action action)>> _actions;
        /// <summary>
        /// A mapping of keybindings to an action to be performed.
        /// </summary>
        public ReadOnlyDictionary<Keys, List<(InputKey binding, Action action)>> Actions => _actions.AsReadOnly();

        private readonly Dictionary<Keys, List<(InputKey binding, Direction direction)>> _motions;
        /// <summary>
        /// A mapping of keybindings to a direction to generate, which will be passed to the <see cref="MotionHandler"/>.
        /// Useful for handling move-type actions.
        /// </summary>
        public ReadOnlyDictionary<Keys, List<(InputKey binding, Direction direction)>> Motions => _motions.AsReadOnly();


        private Action<Direction> _motionHandler;

        /// <summary>
        /// Function to call in order to handle motions generated from bindings set up in <see cref="Motions"/>.
        /// </summary>
        [AllowNull]
        public Action<Direction> MotionHandler
        {
            get => _motionHandler;
            set => _motionHandler = value ?? DefaultMotionHandler;
        }

        /// <summary>
        /// Dictates how the controls component will resolve keypresses.  Defaults to true.  See class description
        /// for details on this configuration option.
        /// </summary>
        public bool ExactMatches;

        /// <summary>
        /// Creates a new component that maps keybindings to various forms of actions.
        /// </summary>
        /// <param name="motionHandler">
        /// The function to use for handling any keybindings in <see cref="Motions"/>.  If set to null, a default handler
        /// will be used that simply sets the parent object's Position if it can move in the selected direction.
        /// </param>
        /// <param name="sortOrder">Sort order for the component.</param>
        public PlayerKeybindingsComponent(Action<Direction>? motionHandler = null, uint sortOrder = 5U)
            : base(false, false, false, true, sortOrder)
        {
            ExactMatches = true;

            // Initialize dictionaries
            _actions = new Dictionary<Keys, List<(InputKey, Action)>>();
            _motions = new Dictionary<Keys, List<(InputKey binding, Direction direction)>>();

            // Add motion handler, or default if one was not specified
            _motionHandler = motionHandler ?? DefaultMotionHandler;
        }

        /// <summary>
        /// Adds a new Action that binds the given key to the given action, overriding any existing Action bindings
        /// for that key.
        /// </summary>
        /// <param name="binding">The keybinding to bind.</param>
        /// <param name="action">The action to perform when said binding is pressed.</param>
        public void SetAction(InputKey binding, Action action)
            => SetActions((binding, action));

        /// <summary>
        /// Adds Actions that set the given keys to the corresponding action.  Any existing Action keybindings for these
        /// InputKeys are overriden.
        /// </summary>
        /// <param name="actions">Actions to add.</param>
        public void SetActions(params (InputKey binding, Action action)[] actions)
            => SetActions((IEnumerable<(InputKey binding, Action action)>)actions);

        /// <summary>
        /// Adds Actions that set the given keys to the corresponding action.  Any existing Action keybindings for these
        /// InputKeys are overriden.
        /// </summary>
        /// <param name="actions">Actions to add.</param>
        public void SetActions(IEnumerable<(InputKey binding, Action action)> actions)
        {
            foreach (var action in actions)
            {
                if (!_actions.ContainsKey(action.binding.Key))
                    _actions[action.binding.Key] = new List<(InputKey binding, Action action)>();

                InsertOrdered(_actions[action.binding.Key], action);
            }
        }

        /// <summary>
        /// Removes the binding for an Action.
        /// </summary>
        /// <param name="binding">The keybinding to remove.</param>
        public void RemoveAction(InputKey binding)
        {
            var idx = _actions[binding.Key].FindIndex(tuple => tuple.binding.Matches(binding));
            if (idx == -1) throw new ArgumentException("Specified Action could not be found.", nameof(binding));

            _actions[binding.Key].RemoveAt(idx);

            if (_actions[binding.Key].Count == 0)
                _actions.Remove(binding.Key);
        }

        /// <summary>
        /// Adds a new Motion that binds the given key to the given direction, overriding any existing Motion binding
        /// for that key.
        /// </summary>
        /// <param name="binding">Keybinding to bind.</param>
        /// <param name="direction">Direction to generate when keybinding is pressed.</param>
        public void SetMotion(InputKey binding, Direction direction)
            => SetMotions((binding, direction));

        /// <summary>
        /// Adds Motions that set the given keys to the corresponding directions.  Any existing Motion keybindings for
        /// these InputKeys are overriden.
        /// </summary>
        /// <param name="motions">Motions to add.</param>
        public void SetMotions(IEnumerable<(InputKey binding, Direction direction)> motions)
        {
            foreach (var motion in motions)
            {
                if (!_motions.ContainsKey(motion.binding.Key))
                    _motions[motion.binding.Key] = new List<(InputKey binding, Direction direction)>();

                InsertOrdered(_motions[motion.binding.Key], motion);
            }
        }

        /// <summary>
        /// Adds Motions that set the given keys to the corresponding directions.  Any existing Motion keybindings for
        /// these InputKeys are overriden.
        /// </summary>
        /// <param name="motions">Motions to set.</param>
        public void SetMotions(params (InputKey binding, Direction direction)[] motions)
            => SetMotions((IEnumerable<(InputKey binding, Direction direction)>)motions);

        /// <summary>
        /// Removes the binding for a Motion.
        /// </summary>
        /// <param name="binding">The keybinding to remove.</param>
        public void RemoveMotion(InputKey binding)
        {
            var idx = _motions[binding.Key].FindIndex(tuple => tuple.binding.Matches(binding));
            if (idx == -1) throw new ArgumentException("Specified command could not be found.", nameof(binding));

            _motions[binding.Key].RemoveAt(idx);

            if (_motions[binding.Key].Count == 0)
                _motions.Remove(binding.Key);
        }

        /// <inheritdoc />
        public override void ProcessKeyboard(IScreenObject host, Keyboard keyboard, out bool handled)
        {
            // Map any keys pressed to appropriate key bindings
            foreach (var asciiKey in keyboard.KeysPressed)
            {
                // Check for motions
                if (_motions.ContainsKey(asciiKey.Key))
                {
                    foreach (var (binding, direction) in _motions[asciiKey.Key])
                        if (binding.ModiferConditionsMet(keyboard, ExactMatches))
                        {
                            MotionHandler(direction);
                            handled = true;
                            return;
                        }
                }

                // Check for actions
                if (_actions.ContainsKey(asciiKey.Key))
                {
                    foreach (var (binding, action) in _actions[asciiKey.Key])
                        if (binding.ModiferConditionsMet(keyboard, ExactMatches))
                        {
                            action();
                            handled = true;
                            return;
                        }
                }
            }

            handled = false;
        }

        private void DefaultMotionHandler(Direction direction)
        {
            if(Parent!.CanMoveIn(direction))
                Parent!.Position += direction;
        }

        private static void InsertOrdered<T>(List<(InputKey control, T obj)> list, (InputKey control, T obj) binding)
        {
            int idx = list.Count;
            bool foundExistingBinding = false;

            for(int i = 0; i < list.Count; i++)
            {
                var item = list[i];
                int bindingKeyModCount = CountModifiers(ref binding.control);
                int itemKeyModCount = CountModifiers(ref item.control);

                // The insertion point is further down the list
                if (bindingKeyModCount > itemKeyModCount)
                    continue;

                // The insertion point is further down the list, but we should check for duplicates.
                if (bindingKeyModCount == itemKeyModCount)
                {
                    // If we found a match, flag it, record the index, and end the search.  Otherwise the insertion
                    // point is further down the list.
                    if (binding.control.Matches(item.control))
                        foundExistingBinding = true;
                    else
                        continue;
                }

                idx = i;
                break;
            }

            // If we found an existing binding for this key, just replace it; otherwise insert a new one
            // at the proper position.
            if (foundExistingBinding)
                list[idx] = binding;
            else
                list.Insert(idx, binding);
        }

        private static int CountModifiers(ref InputKey control)
        {
            int count = 0;
            if (control.Shift != KeyModifiers.None) count++;
            if (control.Alt != KeyModifiers.None) count++;
            if (control.Ctrl != KeyModifiers.None) count++;

            return count;
        }
    }
}
