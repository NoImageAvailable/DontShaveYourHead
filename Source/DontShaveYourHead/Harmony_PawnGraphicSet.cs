using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;
using Harmony;

namespace DontShaveYourHead
{
    [HarmonyPatch(typeof(PawnGraphicSet), "ResolveApparelGraphics")]
    public static class Harmony_PawnGraphicSet_ResolveApparelGraphics
    {
        public static void Postfix(PawnGraphicSet __instance)
        {
            Pawn pawn = __instance.pawn;

            // Define coverage-appropriate path
            string pathAppendString = "";
            if (pawn.apparel.BodyPartGroupIsCovered(BodyPartGroupDefOf.FullHead))
            {
                pathAppendString = "FullHead";
            }
            else if (pawn.apparel.BodyPartGroupIsCovered(BodyPartGroupDefOf.UpperHead))
            {
                pathAppendString = "UpperHead";
            }

            // Set hair graphics to headgear-appropriate texture
            var texPath = pawn.story.hairDef.texPath;
            if (!pathAppendString.NullOrEmpty())
            {
                // Check if the path exists
                texPath += "/" + pathAppendString;
                if (!ContentFinder<Texture2D>.Get(texPath + "_front", false))
                {
#if DEBUG
                    Log.Warning("DSYH :: could not find texture at " + texPath);
#endif
                    texPath = HairDefOf.Shaved.texPath;
                }
            }
            __instance.hairGraphic = GraphicDatabase.Get<Graphic_Multi>(texPath, ShaderDatabase.Cutout, Vector2.one, pawn.story.hairColor); // Set new graphic
        }
    }
}
