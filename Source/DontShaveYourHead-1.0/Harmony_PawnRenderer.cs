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
        private const float YOffset_PostHead = 0.03515625f;
        private const float YOffset_OnHead = 0.03125f;

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
            var writing = false;

            foreach (var code in instructions)
            {
                // Apply draw offset for hat rendering
                if (code.opcode == OpCodes.Ldc_R4 && Math.Abs((float)code.operand - YOffset_OnHead) < 0.0000001f)
                {
                    yield return new CodeInstruction(OpCodes.Ldc_R4, YOffset_OnHead + YOffset_PostHead);
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

                     // reset offset for hair rendering
                    //loading loc1 and subtracting the offset
                    yield return new CodeInstruction(OpCodes.Ldloca_S, 12);
                    yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(Vector3), "y"));
                    yield return new CodeInstruction(OpCodes.Dup);
                    yield return new CodeInstruction(OpCodes.Ldind_R4);
                    yield return new CodeInstruction(OpCodes.Ldc_R4, YOffset_PostHead);
                    yield return new CodeInstruction(OpCodes.Sub);
                    yield return new CodeInstruction(OpCodes.Stind_R4);

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
           
        }
    }
}
