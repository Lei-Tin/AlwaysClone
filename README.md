# Always Clone

Always Clone is a Slay the Spire 2 mod that forces the Act 2 Ancient event to be Pael and forces Pael's Growth to appear as option 1.

## What this mod does

- In Act 2 (Hive), the selected Ancient is forced to Pael.
- The map-facing Ancient getter is also forced to Pael, so the map icon/state stays consistent.
- In the Pael event options:
  - If Pael's Growth is already in the 3 shown options, it is moved to option 1.
  - If Pael's Growth is not in the shown 3 options, option 1 is replaced with Pael's Growth.
- Other options remain otherwise unchanged.

## Build and install

1. Configure local paths in `local.props`:
	- `STS2GamePath`
	- `GodotExePath`
2. Build from the project root:
	- `dotnet build`
3. The build copies the mod files into:
	- `$(STS2GamePath)\mods\AlwaysClone`

If the copy step fails with a locked file error, close the game and build again.

## Mod structure

- `ModEntry.cs` initializes Harmony and registers patches.
- `Patches/AlwaysClonePatches.cs` contains the runtime behavior:
  - Force Pael in Act 2.
  - Keep Pael shown on map-facing Ancient lookup.
  - Force Pael's Growth to option 1 in Pael event options.

## Notes

- The project targets `net9.0` and references game assemblies from your local STS2 install.
- This mod is gameplay-affecting by design.
