# Don't Shave Your Head

This mod allows pawn hair to show when wearing headwear. Headwear that doesn't cover the upper head (e.g. sunshades, goggles, etc.) displays the normal hair texture. Covering headwear will have the texture replaced with custom textures that fit exactly under the headwear, thus allowing to display hear at all times without any clipping. Works with all headwear, vanilla and modded automatically.

## Supported hair mods

Currently supported hair mods include:
- NackbladInc Rimhair
- Spoon's Hair Mod

## How it works

The mod places texture sets called "FullHead" and "UpperHead" within a folder structure equal to the hair's texPath, e.g. the textures for the vanilla Afro would be placed in `Things/Pawn/Humanlike/Hairs/Afro/`. The mod automatically detects properly placed and named textures and will switch to the FullHead/UpperHead texture if the respective BodyPartGroup is covered. If a texture can't be found it will default to Shaved hair, meaning vanilla style hiding of hair.

To make a hair mod compatible all that is needed is for the texture to be in the appropriate place, no def editing necessary.