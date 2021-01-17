# Don't Shave Your Head

This mod allows pawn hair to show when wearing headwear. Headwear that doesn't cover the upper head (e.g. sunshades, goggles, etc.) displays the normal hair texture. Covering headwear will have the texture replaced with custom textures that fit exactly under the headwear, thus allowing to display hear at all times without any clipping. Works with all headwear, vanilla and modded automatically.

## Supported hair mods

Currently supported hair mods include:
- vanilla (default) hairstyles,
- Rimsenal Hair Pack,
- Spoons Hair Mod,
- Nackblad Inc Rimhair (both old and new).
- Vanilla Hair Expanded

## How it works

The mod has 2 methods for replacing the hair texture:
	explicit custom texture, 
	and semi-random choice where no explicit texture exists

For explicit custom textures, the mod places texture sets called "FullHead" and "UpperHead" within a folder structure equal to the hair's texPath, e.g. the textures for the vanilla Afro would be placed in `Things/Pawn/Humanlike/Hairs/Afro/`. The mod automatically detects properly placed and named textures and will switch to the FullHead/UpperHead texture if the respective BodyPartGroup is covered.

If a texture can't be found (i.e. non-supported hairstyle mod), it will choose a semi-random texture from the explicit custom textures, which are grouped into short, medium, and long styles. The mod calculates the approximate length of the original hair, then chooses one with similar length (so an original 'short' hairstyle doesn't suddenly grow a ponytail).

The 2nd method can be disabled, in which case unsupported hairstyles will work like vanilla.

To make a hair mod compatible all that is needed is for the texture to be in the appropriate place, no def editing necessary.