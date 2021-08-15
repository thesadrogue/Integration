using System;
using GoRogue.Components;
using GoRogue.GameFramework;
using GoRogue.Components.ParentAware;
using JetBrains.Annotations;
using SadConsole;
using SadConsole.Components;
using SadConsole.Input;

namespace SadRogue.Integration.Components
{
    /// <summary>
    /// A Component that works with both SadConsole and GoRogue's GameFramework that may be attached to any
    /// IGameObject.
    /// </summary>
    /// <remarks>
    /// You may want to consider <see cref="RogueLikeComponentBase{TParent}"/>
    /// for some additional functionality.
    ///
    /// The intended use is for you to derive from this class and
    /// then override the methods you need. The base class there-
    /// fore has declared empty implementations so that no undo
    /// processing or errors occur.
    /// </remarks>
    [PublicAPI]
    public abstract class RogueLikeComponentBase : RogueLikeComponentBase<IGameObject>
    {

        /// <summary>
        /// Creates a new component.
        /// </summary>
        /// <param name="isUpdate">Whether or not this component performs a task OnUpdate</param>
        /// <param name="isRender">Whether or not this component performs a task OnRender</param>
        /// <param name="isMouse">Whether or not this component listens for the mouse</param>
        /// <param name="isKeyboard">Whether or not this component listens for the keyboard</param>
        /// <param name="sortOrder">The order in which this component is processed</param>
        protected RogueLikeComponentBase(bool isUpdate, bool isRender, bool isMouse, bool isKeyboard, uint sortOrder = 5)
            : base(isUpdate, isRender, isMouse, isKeyboard, sortOrder)
        { }
    }

    /// <summary>
    /// A component that works with both SadConsole's and GoRogue's component system.
    /// It is identical to <see cref="RogueLikeComponentBase"/>, except that it enforces
    /// additional constraints on the type of object that it is attached to.
    /// </summary>
    /// <typeparam name="TParent">Type of object this component must be attached to.</typeparam>
    [PublicAPI]
    public class RogueLikeComponentBase<TParent> : ParentAwareComponentBase<TParent>, IComponent, ISortedComponent
        where TParent : class, IGameObject
    {
        /// <summary>
        /// The Order in which the components are processed. Lower is earlier.
        /// </summary>
        public uint SortOrder { get; }

        /// <summary>
        /// Whether or not this component performs a task OnUpdate
        /// </summary>
        public bool IsUpdate { get; }

        /// <summary>
        /// Whether or not this component performs a task OnRender
        /// </summary>
        public bool IsRender { get; }

        /// <summary>
        /// Whether or not this component listens for mouse input
        /// </summary>
        public bool IsMouse { get; }

        /// <summary>
        /// Whether or not this component listens to the keyboard
        /// </summary>
        public bool IsKeyboard { get; }

        /// <summary>
        /// Creates a new component
        /// </summary>
        /// <param name="isUpdate">Whether or not this component performs a task OnUpdate</param>
        /// <param name="isRender">Whether or not this component performs a task OnRender</param>
        /// <param name="isMouse">Whether or not this component listens for the mouse</param>
        /// <param name="isKeyboard">Whether or not this component listens for the keyboard</param>
        /// <param name="sortOrder">The order in which this component is processed</param>
        protected RogueLikeComponentBase(bool isUpdate, bool isRender, bool isMouse, bool isKeyboard, uint sortOrder = 5)
        {
            IsUpdate = isUpdate;
            IsRender = isRender;
            IsMouse = isMouse;
            SortOrder = sortOrder;
            IsKeyboard = isKeyboard;
        }

        /// <summary>
        /// The method called OnUpdate, if IsUpdate is true.
        /// </summary>
        /// <param name="host">The "Parent" if you're using RogueLikeEntity</param>
        /// <param name="delta">The difference in time between this call and the previous</param>
        public virtual void Update(IScreenObject host, TimeSpan delta) { }

        /// <summary>
        /// The method called OnRender, if IsRender is true.
        /// </summary>
        /// <param name="host">The "Parent" if you're using RogueLikeEntity</param>
        /// <param name="delta">The difference in time between this call and the previous</param>
        public virtual void Render(IScreenObject host, TimeSpan delta) { }

        /// <summary>
        /// Hook up an event handler to run this when the component is added to an entity
        /// </summary>
        /// <param name="host">The "Parent" if you're using RogueLikeEntity</param>
        public virtual void OnAdded(IScreenObject host) { }

        /// <summary>
        /// Hook up an event handler to run this when the component is removed from an entity
        /// </summary>
        /// <param name="host">The "Parent" if you're using RogueLikeEntity</param>
        public virtual void OnRemoved(IScreenObject host) { }

        /// <summary>
        /// Called when IsMouse is true
        /// </summary>
        /// <param name="host">The "Parent" if you're using RogueLikeEntity</param>
        /// <param name="state">The state of the mouse</param>
        /// <param name="handled">Whether or not we are done handling mouse input</param>
        public virtual void ProcessMouse(IScreenObject host, MouseScreenObjectState state, out bool handled) => handled = false;

        /// <summary>
        /// Called when IsKeyboard is true
        /// </summary>
        /// <param name="host">The "Parent" if you're using RogueLikeEntity</param>
        /// <param name="keyboard">The state of the keyboard</param>
        /// <param name="handled">Whether or not we are done handling keyboard input</param>
        public virtual void ProcessKeyboard(IScreenObject host, Keyboard keyboard, out bool handled) => handled = false;

    }
}
