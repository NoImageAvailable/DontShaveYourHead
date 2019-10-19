using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Reflection.Emit;
using RimWorld;
using Verse;
using UnityEngine;
using Harmony;

namespace DontShaveYourHead
{
    [HarmonyPatch(typeof(PawnRenderer), "RenderPawnInternal", new Type[] { typeof(Vector3), typeof(float), typeof(bool), typeof(Rot4), typeof(Rot4), typeof(RotDrawMode), typeof(bool), typeof(bool) })]
    public static class Harmony_PawnRenderer
    {
        private const float DrawOffset = 0.03515625f;

        // Reroute of hair DrawMeshNowOrLater call. Replaces normal hair mat with our own
        public static void DrawHairReroute(Mesh mesh, Vector3 loc, Quaternion quat, Material mat, bool isPortrait, Pawn pawn, Rot4 facing)
        {
            // Check if we need to recalculate hair
            if (!isPortrait || !Prefs.HatsOnlyOnMap)
            {
                mat = HairUtility.GetHairMatFor(pawn, facing);
            }
            GenDraw.DrawMeshNowOrLater(mesh, loc, quat, mat, isPortrait);
        }


        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            const float hatOffset = 0.03125f;
            var writing = false;

            foreach (var code in instructions)
            {
                // Apply draw offset for hat rendering
                if (code.opcode == OpCodes.Ldc_R4 && (float)code.operand == hatOffset)
                {
                    yield return new CodeInstruction(OpCodes.Ldc_R4, DrawOffset + hatOffset);
                }
                // Skip hide hair flag
                else if (code.opcode == OpCodes.Ldloc_S && code.operand is LocalBuilder b && b.LocalIndex == 13)
                {
                    var newCode = new CodeInstruction(OpCodes.Ldc_I4_0) { labels = code.labels };
                    yield return newCode;
                }
                // Replace hair draw mesh call
                else if (code.opcode == OpCodes.Callvirt && code.operand == AccessTools.Method(typeof(PawnGraphicSet), nameof(PawnGraphicSet.HairMatAt)))
                {
                    writing = true;
                    yield return code;
                }
                else if (writing && code.opcode == OpCodes.Call && code.operand == AccessTools.Method(typeof(GenDraw), nameof(GenDraw.DrawMeshNowOrLater)))
                {
                    writing = false;

                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(PawnRenderer), "pawn"));
                    yield return new CodeInstruction(OpCodes.Ldarg, 5);
                    yield return new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(Harmony_PawnRenderer), nameof(DrawHairReroute)));
                }
                // Default
                else
                {
                    yield return code;
                }
            }
            /*
            var codes = instructions.ToList();

            // look for headGraphic block
            var idxHGfx = codes.FirstIndexOf(ci => ci.operand == typeof(PawnGraphicSet).GetField(nameof(PawnGraphicSet.headGraphic)));
            if (idxHGfx == -1)
            {
                Log.Error("Could not find headGraphics block start");
                return codes;
            }
            var postHeadGraphicsLabel = (Label)codes[idxHGfx + 1].operand;
            var idxHGfxEnd = codes.FirstIndexOf(ci => ci.labels.Any(l => l == postHeadGraphicsLabel));
            if (idxHGfxEnd == -1)
            {
                Log.Error("Could not find headGraphics block end");
                return codes;
            }

            // head block contents atart at idxHGfx +2 , and at idxHGfxEnd -1

            var idxbodyDrawType = -1;
            for (var i = idxHGfx + 2; i < idxHGfxEnd - 1; i++)
            {
                if (codes[i].opcode == OpCodes.Ldarg_S && ((byte)6).Equals(codes[i].operand))
                {
                    idxbodyDrawType = i;
                }
            }
            if (idxbodyDrawType == -1)
            {
                Log.Error("Could not find bodyDrawType reference");
                return codes;
            }

            codes[idxbodyDrawType - 2].opcode = OpCodes.Ldc_I4_0;        // 2nd use of bodyDrawType has <c>flag</c> we want to skip check of
            codes[idxbodyDrawType - 2].operand = null;

            // Get IL code of hatRenderedFrontOfFace to find hat render block
            var hatBlockIndex = codes.FirstIndexOf(c => c.operand == typeof(ApparelProperties).GetField(nameof(ApparelProperties.hatRenderedFrontOfFace)));

            // Find next DrawMeshNowOrLaterCall as it is responsible for drawing non-mask hats
            for (var i = hatBlockIndex; i < codes.Count; i++)
            {
                var code = codes[i];
                if (code.operand == typeof(GenDraw).GetMethod(nameof(GenDraw.DrawMeshNowOrLater)))
                {
                    code.operand = typeof(Harmony_PawnRenderer).GetMethod(nameof(Harmony_PawnRenderer.DrawHatNowOrLaterShifted));   // Reroute to custom method
                    break;
                }
            }

            // Find hair DrawMeshNowOrLaterCall to reroute
            var foundHairMatAt = false;
            var headFacingCode = new CodeInstruction(OpCodes.Nop); // Code that pushes headFacing argument
            for (var i = hatBlockIndex; i < codes.Count; i++)
            {
                // Find hair block
                var code = codes[i];

                if (code.operand == typeof(PawnGraphicSet).GetMethod(nameof(PawnGraphicSet.HairMatAt)))
                {
                    foundHairMatAt = true;
                    headFacingCode = codes[i - 1];
                }
                // Find and replace hair draw call
                if (foundHairMatAt
                    && code.operand == typeof(GenDraw).GetMethod(nameof(GenDraw.DrawMeshNowOrLater)))
                {
                    // Reroute draw call
                    code.operand = typeof(Harmony_PawnRenderer).GetMethod(nameof(DrawHairReroute));

                    // Insert argument calls
                    var newCodes = new List<CodeInstruction>
                    {
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Ldfld, typeof(PawnRenderer).GetField("pawn", BindingFlags.NonPublic | BindingFlags.Instance)),
                        new CodeInstruction(headFacingCode.opcode, headFacingCode.operand)
                    };

                    codes.InsertRange(i, newCodes);
                    break;
                }
            }

            if (!foundHairMatAt)
            {
                Log.Error("DSYH :: Failed to find HairMatAt call");
            }

            return codes;*/
        }
    }
}
