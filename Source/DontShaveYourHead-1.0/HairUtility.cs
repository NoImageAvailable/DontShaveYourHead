using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace DontShaveYourHead
{
    public static class HairUtility
    {
        private enum Coverage
        {
            None,
            Jaw,
            UpperHead,
            FullHead
        }

        private static Dictionary<Coverage, BodyPartGroupDef> headDefs = new Dictionary<Coverage, BodyPartGroupDef>()
        {
            { Coverage.Jaw, BodyPartGroupDefOfDSYH.Teeth },
            { Coverage.UpperHead, BodyPartGroupDefOf.UpperHead },
            { Coverage.FullHead, BodyPartGroupDefOf.FullHead }
        };

        public static Material GetHairMatFor(Pawn pawn, Rot4 facing)
        {
            // Find maximum coverage of non-mask headwear
            var maxCoverage = getMaxCoverage(pawn);

            // Set hair graphics to headgear-appropriate texture
            var texPath = pawn.story.hairDef.texPath;

            // Set path appendage
            if (maxCoverage != Coverage.None)
            {
                string pathAppendString = maxCoverage.ToString();

                // Check if the path exists
                var newTexPath = texPath + "/" + pathAppendString;
                if (!ContentFinder<Texture2D>.Get(newTexPath + "_south", false))
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

            return GraphicDatabase.Get<Graphic_Multi>(texPath, ShaderDatabase.Cutout, Vector2.one, pawn.story.hairColor).MatAt(facing); // Set new graphic
        }

        private static Coverage getMaxCoverage(Pawn pawn)
        {
            var maxCoverage = Coverage.None;

            //dubs bad hygeine clears apparelGraphics when washing, so only check for coverage if the pawn's headgear is actually rendered
            if (pawn.Drawer.renderer.graphics.apparelGraphics.Any())
            {
                //flattening body part groups, and joining to head defs
                var bodyPartGroups = from apparel in pawn.apparel.WornApparel.Where(c => !c.def.apparel.hatRenderedFrontOfFace)
                                     from bodyPartGroup in apparel.def.apparel.bodyPartGroups
                                     join headDef in headDefs on bodyPartGroup equals headDef.Value
                                     select headDef;

                maxCoverage = bodyPartGroups.DefaultIfEmpty().Max(b => b.Key); //finding the max head def coverage
            }

            return maxCoverage;
        }

        /*private enum Coverage
        {
            None,
            Jaw,
            UpperHead,
            FullHead
        }

        public static Material GetHairMatFor(Pawn pawn, Rot4 facing)
        {
        // Define coverage-appropriate path
        var pathAppendString = "";

            // Find maximum coverage of non-mask headwear
            var maxCoverage = Coverage.None;
            foreach (var cur in pawn.apparel.WornApparel)
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
                if (!ContentFinder<Texture2D>.Get(newTexPath + "_south", false))
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
            return GraphicDatabase.Get<Graphic_Multi>(texPath, ShaderDatabase.Cutout, Vector2.one, pawn.story.hairColor).MatAt(facing); // Set new graphic
        }*/
    }
}