# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

## [Unreleased]

None.

## [1.0.0-alpha03] - 2021-10-17

## Added
- Added `WalkabilityChanging` event that fires directly _before_ walkability is set to map objects
- Added `TransparencyChanging` event that fires directly _before_ transparancy is set to map objects

## Changed
- Updated minimum required version of GoRogue to 3.0.0-alpha08

## Fixed
- Fixed bug that prevented setting the `IsWalkable` field of map objects while they were part of the map

## [1.0.0-alpha02] - 2021-10-13

## Added
- Added support for having `RogueLikeMap` as the parent type for `RogueLikeComponentBase` and `RogueLikeComponentBase<T>`
- Added constructors to `RogueLikeCell`, `MemoryAwareRogueLikeCell`, and `RogueLikeEntity` that don't take a mandatory position parameter

## Changed
- Updated minimum required version of GoRogue to 3.0.0-alpha07
- `PlayerKeybindingsComponent.MotionHandler` is now a virtual function you can override, instead of an `Action`
- `RogueLikeComponentBase` accepts any object which implements `IObjectWithComponents` as its parent
    - For `RogueLikeEntity` components, you should now use `RogueLikeComponentBase<IGameObject>` or `RogueLikeComponentBase<RogueLikeEntity>` as appropriate