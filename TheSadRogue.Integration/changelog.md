# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

## [Unreleased]

None.

## [1.1.0] - 2025-01-01

### Added
- Project now targets .net 8.0 and .net 9.0 natively
### Changed
- `FOVHandlerBase` now takes a flag that causes `TerrainSeen` and `EntitySeen` to be called on all visible cells
    - This allows canonical support for things like an FOV gradient based on distance from center

## [1.0.0] - 2023-11-11
- Updated to GoRogue 3.0.0-beta08 and SadConsole v10
- Removed support for all platforms prior to .NET 6.0 (these versions are no longer supported by SadConsole)

## [1.0.0-beta04] - 2023-05-23

### Added
- Added `KeybindingsComponent<T>` which enforces that its parent is of a particular type and records its `Parent` field of that type
- `RogueLikeComponentBase` and `RogueLikeComponentBase<T>` now fully support being added only as a SadConsole component (the `Parent` field will now update in this case)

### Changed
- Updated GoRogue version requirements
- Renamed `PlayerKeybindingsComponent` to `KeybindingsComponent`
- `KeybindingsComponent` now records its `Parent` as type `IScreenObject`; this is more accurate anyway since the component uses elements of SadConsole's component structure
- `KeybindingsComponent` no longer needs to be attached to the player entity
- The default `KeybindingsComponent.MotionHandler` implementation does nothing (meant to be overriden if motions are used)
- `RogueLikeComponentBase` now records its `Parent` as type `object` (use `RogueLikeComponentBase<T>` for a type which records its Parent as a more specific type)
- `RogueLikeComponentBase<T>` no longer requires that type T inherit from `IObjectWithComponents`
    - This enables `RogueLikeComponentBase<T>` to be used with custom component systems, and allows them to be added as **only** SadConsole components on objects which don't have GoRogue components

### Removed
- `KeybindingsComponent` (previously `PlayerKeybindingsComponent`) no longer takes the (unused) `motionHandler` parameter in the constructor


## [1.0.0-beta03] - 2023-05-17

### Changed
- Updated GoRogue version requirements
- Provided explicit multi-target option for .NET 7.0

## [1.0.0-beta02] - 2023-04-21

### Changed
- Updated primitives library and GoRogue version requirements
    - See those library's changelogs; spatial maps and a few other things changed namespaces and the API changed slightly
- Usages of `RogueLikeEntity.Moved` should now be replaced by `RogueLikeEntity.PositionChanged` (which is SadConsole's event)

### Fixed
- Fixed a bug in the template's method of calculating FOV where it could use out-of-date TransparencyView values once the player is added to a second map

## [1.0.0-beta01] - 2022-10-07

### Changed
- Updated primitives library minimum version to ensure performance improvements

## [1.0.0-alpha03] - 2021-10-17

### Added
- Added `WalkabilityChanging` event that fires directly _before_ walkability is set to map objects
- Added `TransparencyChanging` event that fires directly _before_ transparancy is set to map objects

### Changed
- Updated minimum required version of GoRogue to 3.0.0-alpha08

### Fixed
- Fixed bug that prevented setting the `IsWalkable` field of map objects while they were part of the map

## [1.0.0-alpha02] - 2021-10-13

### Added
- Added support for having `RogueLikeMap` as the parent type for `RogueLikeComponentBase` and `RogueLikeComponentBase<T>`
- Added constructors to `RogueLikeCell`, `MemoryAwareRogueLikeCell`, and `RogueLikeEntity` that don't take a mandatory position parameter

### Changed
- Updated minimum required version of GoRogue to 3.0.0-alpha07
- `PlayerKeybindingsComponent.MotionHandler` is now a virtual function you can override, instead of an `Action`
- `RogueLikeComponentBase` accepts any object which implements `IObjectWithComponents` as its parent
    - For `RogueLikeEntity` components, you should now use `RogueLikeComponentBase<IGameObject>` or `RogueLikeComponentBase<RogueLikeEntity>` as appropriate
