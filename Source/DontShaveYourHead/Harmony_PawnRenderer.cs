using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;
using RimWorld;
using Verse;
using UnityEngine;
using Harmony;

namespace DontShaveYourHead
{
    [HarmonyPatch(typeof(PawnRenderer), "RenderPawnInternal", new Type[] { typeof(Vector3), typeof(Quaternion), typeof(bool), typeof(Rot4), typeof(Rot4), typeof(RotDrawMode), typeof(bool), typeof(bool) })]
    public static class Harmony_PawnRenderer
    {
        public static void DrawNowOrLaterShifted(Mesh mesh, Vector3 loc, Quaternion quat, Material mat, bool drawNow)
        {
            loc.y += 0.001f;
            GenDraw.DrawMeshNowOrLater(mesh, loc, quat, mat, drawNow);
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();

            // look for headGraphic block
            int idxHGfx = codes.FirstIndexOf(ci => ci.operand == typeof(PawnGraphicSet).GetField(nameof(PawnGraphicSet.headGraphic)));
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
            for (int i = idxHGfx + 2; i < idxHGfxEnd - 1; i++)
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

            var idxDrawMeshNowOrLater = -1;
            for (int i = idxbodyDrawType; i > idxHGfx; i--)
            {
                if (codes[i].operand == typeof(GenDraw).GetMethod(nameof(GenDraw.DrawMeshNowOrLater)))
                {
                    idxDrawMeshNowOrLater = i;
                    break;
                }
            }
            if (idxDrawMeshNowOrLater == -1)
            {
                Log.Error("Could not find DrawMeshNowOrLater reference");
                return codes;
            }

            codes[idxbodyDrawType - 2].opcode = OpCodes.Ldc_I4_0;        // 2nd use of bodyDrawType has <c>flag</c> we want to skip check of
            codes[idxbodyDrawType - 2].operand = null;
            codes[idxDrawMeshNowOrLater].operand = typeof(Harmony_PawnRenderer).GetMethod(nameof(Harmony_PawnRenderer.DrawNowOrLaterShifted));
            

            return codes;
        }
    }
}
