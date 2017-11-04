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
        private enum Coverage
        {
            None,
            Jaw,
            UpperHead,
            FullHead
        }

        public static void Postfix(PawnGraphicSet __instance)
        {
            Pawn pawn = __instance.pawn;

            // Define coverage-appropriate path
            string pathAppendString = "";

            // Find maximum coverage of non-mask headwear
            var maxCoverage = Coverage.None;
            foreach(Apparel cur in pawn.apparel.WornApparel)
            {
                if (!cur.def.apparel.hatRenderedFrontOfFace)
                {
                    var coveredGroups = cur.def.apparel.bodyPartGroups;
                    var curCoverage = Coverage.None;

                    // Find highest current max coverage
                    if (coveredGroups.Contains(BodyPartGroupDefOf.FullHead))
                    {
                        curCoverage = Coverage.FullHead;
                    }
                    else if (coveredGroups.Contains(BodyPartGroupDefOf.UpperHead))
                    {
                        curCoverage = Coverage.UpperHead;
                    }
                    else if (coveredGroups.Contains(BodyPartGroupDefOfDSYH.Teeth))
                    {
                        curCoverage = Coverage.Jaw;
                    }

                    // Compare to stored max coverage
                    if (maxCoverage < curCoverage)
                    {
                        maxCoverage = curCoverage;
                    }
                }
            }

            // Set path appendage
            if (maxCoverage != Coverage.None)
            {
                pathAppendString = maxCoverage.ToString();
            }

            // Set hair graphics to headgear-appropriate texture
            var texPath = pawn.story.hairDef.texPath;
            if (!pathAppendString.NullOrEmpty())
            {
                // Check if the path exists
                var newTexPath = texPath + "/" + pathAppendString;
                if (!ContentFinder<Texture2D>.Get(newTexPath + "_front", false))
                {
#if DEBUG
                    Log.Warning("DSYH :: could not find texture at " + texPath);
#endif
                    if (pathAppendString != "Jaw") texPath = HairDefOf.Shaved.texPath;
                }
                else
                {
                    texPath = newTexPath;
                }
            }
            __instance.hairGraphic = GraphicDatabase.Get<Graphic_Multi>(texPath, ShaderDatabase.Cutout, Vector2.one, pawn.story.hairColor); // Set new graphic
        }
    }
}
