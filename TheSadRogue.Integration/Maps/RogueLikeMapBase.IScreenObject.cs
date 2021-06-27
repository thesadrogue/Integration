﻿// SadConsole does not use nullable reference types.  Since we are copying the implementation from SadConsole,
// we will disable it for this file as well to ensure as much parity as possible.
#nullable disable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.Serialization;
using SadConsole;
using SadConsole.Components;
using SadConsole.Entities;
using SadConsole.Input;
using SadRogue.Primitives;

namespace SadRogue.Integration.Maps
{
    [DataContract]
    public partial class RogueLikeMapBase
    {
        [DataMember(Name = "Children")]
        private IScreenObject[] _childrenSerialized;

        [DataMember(Name = "Components")]
        private IComponent[] _componentsSerialized;

        [DataMember(Name = "Position")]
        private Point _position;

        private IScreenObject _parentObject;
        private bool _isVisible = true;
        private bool _isEnabled = true;
        private bool _isFocused;


        /// <inheritdoc/>
        public event EventHandler<ValueChangedEventArgs<IScreenObject>> ParentChanged;

        /// <inheritdoc/>
        public event EventHandler<ValueChangedEventArgs<Point>> PositionChanged;

        /// <inheritdoc/>
        public event EventHandler VisibleChanged;

        /// <inheritdoc/>
        public event EventHandler EnabledChanged;

        /// <inheritdoc/>
        public event EventHandler FocusLost;

        /// <inheritdoc/>
        public event EventHandler Focused;

        /// <summary>
        /// A filtered list from <see cref="SadComponents"/> where <see cref="IComponent.IsUpdate"/> is <see langword="true"/>.
        /// </summary>
        protected List<IComponent> ComponentsUpdate;

        /// <summary>
        /// A filtered list from <see cref="SadComponents"/> where <see cref="IComponent.IsRender"/> is <see langword="true"/>.
        /// </summary>
        protected List<IComponent> ComponentsRender;

        /// <summary>
        /// A filtered list from <see cref="SadComponents"/> where <see cref="IComponent.IsMouse"/> is <see langword="true"/>.
        /// </summary>
        protected List<IComponent> ComponentsMouse;

        /// <summary>
        /// A filtered list from <see cref="SadComponents"/> where <see cref="IComponent.IsKeyboard"/> is <see langword="true"/>.
        /// </summary>
        protected List<IComponent> ComponentsKeyboard;

        /// <summary>
        /// A filtered list from <see cref="SadComponents"/> that is not set for update, render, mouse, or keyboard.
        /// </summary>
        protected List<IComponent> ComponentsEmpty;

        /// <inheritdoc/>
        public ObservableCollection<IComponent> SadComponents { get; protected set; }

        /// <inheritdoc/>
        public ScreenObjectCollection Children { get; protected set; }

        /// <inheritdoc/>
        public IScreenObject Parent
        {
            get => _parentObject;
            set
            {
                if (value == this) throw new Exception("Cannot set parent to itself.");
                if (_parentObject == value) return;

                if (_parentObject == null)
                {
                    _parentObject = value;
                    _parentObject.Children.Add(this);
                    OnParentChanged(null, _parentObject);
                }
                else
                {
                    IScreenObject oldParent = _parentObject;
                    _parentObject = null;
                    oldParent.Children.Remove(this);
                    _parentObject = value;

                    _parentObject?.Children.Add(this);
                    OnParentChanged(oldParent, _parentObject);
                }
            }
        }

        /// <inheritdoc/>
        public Point Position
        {
            get => _position;
            set
            {
                if (_position == value) return;

                Point oldPoint = _position;
                _position = value;
                OnPositionChanged(oldPoint, _position);
            }
        }

        /// <inheritdoc/>
        public Point AbsolutePosition { get; protected set; }

        /// <inheritdoc/>
        [DataMember]
        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                if (_isVisible == value) return;

                _isVisible = value;
                OnVisibleChanged();
            }
        }

        /// <inheritdoc/>
        [DataMember]
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (_isEnabled == value) return;

                _isEnabled = value;
                OnEnabledChanged();
            }
        }

        /// <inheritdoc/>
        public bool IsFocused
        {
            get => _isFocused;
            set
            {
                if ((_isFocused && value) || (!_isFocused && !value)) return;

                _isFocused = value;

                if (value)
                {
                    switch (FocusedMode)
                    {
                        case FocusBehavior.Set:
                            GameHost.Instance.FocusedScreenObjects.Set(this);
                            break;
                        case FocusBehavior.Push:
                            GameHost.Instance.FocusedScreenObjects.Push(this);
                            break;
                    }

                    Focused?.Invoke(this, EventArgs.Empty);
                    OnFocused();
                }
                else
                {
                    if (GameHost.Instance.FocusedScreenObjects.ScreenObject == this && FocusedMode != FocusBehavior.None)
                        GameHost.Instance.FocusedScreenObjects.Pop(this);

                    FocusLost?.Invoke(this, EventArgs.Empty);
                    OnFocusLost();
                }
            }
        }

        /// <inheritdoc/>
        [DataMember]
        public FocusBehavior FocusedMode { get; set; } = FocusBehavior.Set;

        /// <inheritdoc/>
        [DataMember]
        public bool IsExclusiveMouse { get; set; }
        /// <inheritdoc/>
        [DataMember]
        public bool UseKeyboard { get; set; }

        /// <inheritdoc/>
        [DataMember]
        public bool UseMouse { get; set; }

        /// <inheritdoc/>
        public virtual void Render(TimeSpan delta)
        {
            if (!IsVisible) return;

            foreach (IComponent component in ComponentsRender.ToArray())
                component.Render(this, delta);

            foreach (IScreenObject child in new List<IScreenObject>(Children))
                child.Render(delta);
        }

        /// <inheritdoc/>
        public virtual void Update(TimeSpan delta)
        {
            if (!IsEnabled) return;

            // Update entities in map
            foreach (var entity in Entities.Items)
            {
                // Guaranteed to succeed since all must be RoguelikeEntities
                var scEntity = (Entity)entity;
                scEntity.Update(delta);
            }

            foreach (IComponent component in ComponentsUpdate.ToArray())
                component.Update(this, delta);

            foreach (IScreenObject child in new List<IScreenObject>(Children))
                child.Update(delta);
        }

        /// <inheritdoc/>
        public virtual bool ProcessKeyboard(Keyboard keyboard)
        {
            if (!UseKeyboard) return false;

            foreach (var component in ComponentsKeyboard.ToArray())
            {
                component.ProcessKeyboard(this, keyboard, out bool isHandled);

                if (isHandled)
                    return true;
            }

            return false;
        }

        /// <inheritdoc/>
        public virtual bool ProcessMouse(MouseScreenObjectState state)
        {
            if (!IsVisible)
                return false;

            foreach (var component in ComponentsMouse.ToArray())
            {
                component.ProcessMouse(this, state, out bool isHandled);

                if (isHandled)
                    return true;
            }

            if (!UseMouse)
                return false;

            return false;
        }

        /// <inheritdoc/>
        public virtual void LostMouse(MouseScreenObjectState state) { }

        /// <inheritdoc/>
        public virtual void OnFocusLost() { }

        /// <inheritdoc/>
        public virtual void OnFocused() { }

        /// <inheritdoc/>
        public IEnumerable<TComponent> GetSadComponents<TComponent>()
            where TComponent : class, IComponent
        {
            foreach (IComponent component in SadComponents)
            {
                if (component is TComponent)
                    yield return (TComponent)component;
            }
        }

        /// <inheritdoc/>
        public TComponent GetSadComponent<TComponent>()
            where TComponent : class, IComponent
        {
            foreach (IComponent component in SadComponents)
            {
                if (component is TComponent)
                    return (TComponent)component;
            }

            return null;
        }

        /// <summary>
        /// Called when a component is added to the <see cref="SadComponents"/> collection.
        /// </summary>
        /// <param name="component">The component added.</param>
        protected virtual void SadComponentAdded(IComponent component) { }

        /// <summary>
        /// Called when a component is removed from the <see cref="SadComponents"/> collection.
        /// </summary>
        /// <param name="component">The component removed.</param>
        protected virtual void SadComponentRemoved(IComponent component) { }

        /// <inheritdoc/>
        public bool HasSadComponent<TComponent>(out TComponent component)
            where TComponent: class, IComponent
        {
            foreach (IComponent comp in SadComponents)
            {
                if (comp is TComponent)
                {
                    component = (TComponent)comp;
                    return true;
                }
            }

            component = null;
            return false;
        }


        private void Components_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (object item in e.NewItems)
                    {
                        FilterAddItem((IComponent)item);
                        ((IComponent)item).OnAdded(this);
                        SadComponentAdded((IComponent)item);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (object item in e.OldItems)
                    {
                        FilterRemoveItem((IComponent)item);
                        ((IComponent)item).OnRemoved(this);
                        SadComponentRemoved((IComponent)item);
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    foreach (object item in e.NewItems)
                    {
                        FilterAddItem((IComponent)item);
                        ((IComponent)item).OnAdded(this);
                        SadComponentAdded((IComponent)item);
                    }
                    foreach (object item in e.OldItems)
                    {
                        FilterRemoveItem((IComponent)item);
                        ((IComponent)item).OnRemoved(this);
                        SadComponentRemoved((IComponent)item);
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Reset:
                    List<IComponent> items = new List<IComponent>(ComponentsRender.Count + ComponentsUpdate.Count + ComponentsKeyboard.Count + ComponentsMouse.Count);

                    while (ComponentsRender.Count != 0)
                    {
                        ComponentsRender[0].OnRemoved(this);

                        if (!items.Contains(ComponentsRender[0]))
                            items.Add(ComponentsRender[0]);

                        FilterRemoveItem(ComponentsRender[0]);
                    }
                    while (ComponentsUpdate.Count != 0)
                    {
                        ComponentsUpdate[0].OnRemoved(this);

                        if (!items.Contains(ComponentsUpdate[0]))
                            items.Add(ComponentsUpdate[0]);

                        FilterRemoveItem(ComponentsUpdate[0]);
                    }
                    while (ComponentsKeyboard.Count != 0)
                    {
                        ComponentsKeyboard[0].OnRemoved(this);

                        if (!items.Contains(ComponentsKeyboard[0]))
                            items.Add(ComponentsKeyboard[0]);

                        FilterRemoveItem(ComponentsKeyboard[0]);
                    }
                    while (ComponentsMouse.Count != 0)
                    {
                        ComponentsMouse[0].OnRemoved(this);

                        if (!items.Contains(ComponentsMouse[0]))
                            items.Add(ComponentsMouse[0]);

                        FilterRemoveItem(ComponentsMouse[0]);
                    }
                    while (ComponentsEmpty.Count != 0)
                    {
                        ComponentsEmpty[0].OnRemoved(this);

                        if (!items.Contains(ComponentsEmpty[0]))
                            items.Add(ComponentsEmpty[0]);

                        FilterRemoveItem(ComponentsEmpty[0]);
                    }

                    foreach (IComponent item in items)
                        SadComponentRemoved(item);

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            void FilterAddItem(IComponent component)
            {
                if (component.IsRender)
                {
                    if (!ComponentsRender.Contains(component))
                        ComponentsRender.Add(component);
                }

                if (component.IsUpdate)
                {
                    if (!ComponentsUpdate.Contains(component))
                        ComponentsUpdate.Add(component);
                }

                if (component.IsKeyboard)
                {
                    if (!ComponentsKeyboard.Contains(component))
                        ComponentsKeyboard.Add(component);
                }

                if (component.IsMouse)
                {
                    if (!ComponentsMouse.Contains(component))
                        ComponentsMouse.Add(component);
                }

                if (!component.IsRender && !component.IsUpdate && !component.IsKeyboard && !component.IsMouse)
                {
                    if (!ComponentsEmpty.Contains(component))
                        ComponentsEmpty.Add(component);
                }

                ComponentsRender.Sort(CompareComponent);
                ComponentsUpdate.Sort(CompareComponent);
                ComponentsKeyboard.Sort(CompareComponent);
                ComponentsMouse.Sort(CompareComponent);
            }

            void FilterRemoveItem(IComponent component)
            {
                if (component.IsRender)
                {
                    if (ComponentsRender.Contains(component))
                        ComponentsRender.Remove(component);
                }

                if (component.IsUpdate)
                {
                    if (ComponentsUpdate.Contains(component))
                        ComponentsUpdate.Remove(component);
                }

                if (component.IsKeyboard)
                {
                    if (ComponentsKeyboard.Contains(component))
                        ComponentsKeyboard.Remove(component);
                }

                if (component.IsMouse)
                {
                    if (ComponentsMouse.Contains(component))
                        ComponentsMouse.Remove(component);
                }

                if (!component.IsRender && !component.IsUpdate && !component.IsKeyboard && !component.IsMouse)
                {
                    if (!ComponentsEmpty.Contains(component))
                        ComponentsEmpty.Remove(component);
                }

                ComponentsRender.Sort(CompareComponent);
                ComponentsUpdate.Sort(CompareComponent);
                ComponentsKeyboard.Sort(CompareComponent);
                ComponentsMouse.Sort(CompareComponent);
            }
        }

        /// <summary>
        /// Raises the <see cref="ParentChanged"/> event.
        /// </summary>
        /// <param name="oldParent">The previous parent.</param>
        /// <param name="newParent">The new parent.</param>
        protected virtual void OnParentChanged(IScreenObject oldParent, IScreenObject newParent)
        {
            UpdateAbsolutePosition();
            ParentChanged?.Invoke(this, new ValueChangedEventArgs<IScreenObject>(oldParent, newParent));
        }

        /// <summary>
        /// Raises the <see cref="PositionChanged"/> event.
        /// </summary>
        /// <param name="oldPosition">The previous position.</param>
        /// <param name="newPosition">The new position.</param>
        protected virtual void OnPositionChanged(Point oldPosition, Point newPosition)
        {
            UpdateAbsolutePosition();
            PositionChanged?.Invoke(this, new ValueChangedEventArgs<Point>(oldPosition, newPosition));
        }

        /// <summary>
        /// Called when the visibility of the object changes.
        /// </summary>
        protected virtual void OnVisibleChanged() =>
            VisibleChanged?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Called when the paused status of the object changes.
        /// </summary>
        protected virtual void OnEnabledChanged() =>
            EnabledChanged?.Invoke(this, EventArgs.Empty);

        /// <inheritdoc/>
        public virtual void UpdateAbsolutePosition()
        {
            AbsolutePosition = Position + (Parent?.AbsolutePosition ?? new Point(0, 0));

            foreach (IScreenObject child in Children)
                child.UpdateAbsolutePosition();
        }

        /// <summary>
        /// Sorts the components based on the <see cref="IComponent.SortOrder"/> value.
        /// </summary>
        public void SortComponents()
        {
            ComponentsRender.Sort(CompareComponent);
            ComponentsUpdate.Sort(CompareComponent);
            ComponentsKeyboard.Sort(CompareComponent);
            ComponentsMouse.Sort(CompareComponent);
        }

        static int CompareComponent(IComponent left, IComponent right)
        {
            if (left.SortOrder > right.SortOrder)
                return 1;

            if (left.SortOrder < right.SortOrder)
                return -1;

            return 0;
        }

        /// <summary>
        /// Returns the value "ScreenObject".
        /// </summary>
        /// <returns>The string "ScreenObject".</returns>
        public override string ToString() =>
            "ScreenObject";

        /// <summary>
        /// Nothing.
        /// </summary>
        /// <param name="context">Nothing.</param>
        [OnSerializing]
        protected void OnSerializingMethod(StreamingContext context)
        {
            _childrenSerialized = Children.ToArray();
            _componentsSerialized = SadComponents.ToArray();
        }

        [OnSerialized]
        // ReSharper disable once UnusedMember.Local
        // ReSharper disable once UnusedParameter.Local
        private void OnSerialized(StreamingContext context)
        {
            _childrenSerialized = null;
            _componentsSerialized = null;
        }

        [OnDeserialized]
        // ReSharper disable once UnusedMember.Local
        // ReSharper disable once UnusedParameter.Local
        private void OnDeserialized(StreamingContext context)
        {
            foreach (IScreenObject item in _childrenSerialized)
                Children.Add(item);

            foreach (IComponent item in _componentsSerialized)
                SadComponents.Add(item);

            _componentsSerialized = null;
            _childrenSerialized = null;

            UpdateAbsolutePosition();
        }
    }
    /*
    public abstract partial class RogueLikeMapBase
    {
        /// <summary>
        /// The IScreenObject acting as the object for the IScreenObject forwarder implementation.
        /// </summary>
        protected IScreenObject BackingObject { get; set; }

        /// <inheritdoc/>
        public virtual void Render(TimeSpan delta) => BackingObject.Render(delta);

        /// <inheritdoc/>
        public virtual void OnFocused() => BackingObject.OnFocused();

        /// <inheritdoc/>
        public virtual void OnFocusLost() => BackingObject.OnFocusLost();

        /// <inheritdoc/>
        TComponent IScreenObject.GetSadComponent<TComponent>() => BackingObject.GetSadComponent<TComponent>();

        /// <inheritdoc/>
        IEnumerable<TComponent> IScreenObject.GetSadComponents<TComponent>() => BackingObject.GetSadComponents<TComponent>();

        /// <inheritdoc/>
        bool IScreenObject.HasSadComponent<TComponent>(out TComponent component) => BackingObject.HasSadComponent(out component);

        /// <inheritdoc/>
        public virtual bool ProcessKeyboard(Keyboard keyboard) => BackingObject.ProcessKeyboard(keyboard);

        /// <inheritdoc/>
        bool IScreenObject.ProcessMouse(MouseScreenObjectState state)
        {
            // Transformation is required to ensure that backing objects which are surfaces are handled correctly.
            // See https://github.com/thesadrogue/TheSadRogue.Integration/issues/47.
            state = new MouseScreenObjectState(BackingObject, state.Mouse.Clone());
            return ProcessMouse(state);
        }

        /// <summary>
        /// Overridable implementation of IScreenObject's ProcessMouse which is guaranteed to receive a mouse state
        /// appropriate for the map implementation.
        /// </summary>
        /// <param name="state"/>
        /// <returns/>
        protected virtual bool ProcessMouse(MouseScreenObjectState state) => BackingObject.ProcessMouse(state);

        /// <inheritdoc/>
        public virtual void LostMouse(MouseScreenObjectState state) => BackingObject.LostMouse(state);

        /// <summary>
        /// Calls Update for all entities, then Updates all SadComponents and Children. Only processes if IsEnabled is
        /// true.
        /// </summary>
        /// <param name="delta">Time since last update.</param>
        public virtual void Update(TimeSpan delta)
        {
            if (!IsEnabled) return;

            foreach (var entity in Entities.Items)
            {
                // Guaranteed to succeed since all must be RoguelikeEntities
                var scEntity = (Entity)entity;
                scEntity.Update(delta);
            }

            BackingObject.Update(delta);
        }

        /// <inheritdoc/>
        public virtual void UpdateAbsolutePosition() => BackingObject.UpdateAbsolutePosition();

        /// <inheritdoc/>
        public FocusBehavior FocusedMode
        {
            get => BackingObject.FocusedMode;
            set => BackingObject.FocusedMode = value;
        }

        /// <inheritdoc/>
        public Point AbsolutePosition => BackingObject.AbsolutePosition;

        /// <inheritdoc/>
        public ScreenObjectCollection Children => BackingObject.Children;

        /// <inheritdoc/>
        public ObservableCollection<IComponent> SadComponents => BackingObject.SadComponents;

        /// <inheritdoc/>
        public bool IsEnabled
        {
            get => BackingObject.IsEnabled;
            set => BackingObject.IsEnabled = value;
        }

        /// <inheritdoc/>
        public bool IsExclusiveMouse
        {
            get => BackingObject.IsExclusiveMouse;
            set => BackingObject.IsExclusiveMouse = value;
        }

        /// <inheritdoc/>
        public bool IsFocused
        {
            get => BackingObject.IsFocused;
            set => BackingObject.IsFocused = value;
        }

        /// <inheritdoc/>
        public bool IsVisible
        {
            get => BackingObject.IsVisible;
            set => BackingObject.IsVisible = value;
        }

        /// <inheritdoc/>
        public IScreenObject Parent
        {
            get => BackingObject.Parent;
            set => BackingObject.Parent = value;
        }

        /// <inheritdoc/>
        public Point Position
        {
            get => BackingObject.Position;
            set => BackingObject.Position = value;
        }

        /// <inheritdoc/>
        public bool UseKeyboard
        {
            get => BackingObject.UseKeyboard;
            set => BackingObject.UseKeyboard = value;
        }

        /// <inheritdoc/>
        public bool UseMouse
        {
            get => BackingObject.UseMouse;
            set => BackingObject.UseMouse = value;
        }

        /// <inheritdoc/>
        public event EventHandler EnabledChanged
        {
            add => BackingObject.EnabledChanged += value;
            remove => BackingObject.EnabledChanged -= value;
        }

        /// <inheritdoc/>
        public event EventHandler<ValueChangedEventArgs<IScreenObject>> ParentChanged
        {
            add => BackingObject.ParentChanged += value;
            remove => BackingObject.ParentChanged -= value;
        }

        /// <inheritdoc/>
        public event EventHandler<ValueChangedEventArgs<Point>> PositionChanged
        {
            add => BackingObject.PositionChanged += value;
            remove => BackingObject.PositionChanged -= value;
        }

        /// <inheritdoc/>
        public event EventHandler VisibleChanged
        {
            add => BackingObject.VisibleChanged += value;
            remove => BackingObject.VisibleChanged -= value;
        }

        /// <inheritdoc/>
        public event EventHandler FocusLost
        {
            add => BackingObject.FocusLost += value;
            remove => BackingObject.FocusLost -= value;
        }

        /// <inheritdoc/>
        public event EventHandler Focused
        {
            add => BackingObject.Focused += value;
            remove => BackingObject.Focused -= value;
        }
    }
    */
}
