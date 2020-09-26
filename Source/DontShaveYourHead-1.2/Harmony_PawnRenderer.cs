using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Reflection.Emit;
using RimWorld;
using Verse;
using UnityEngine;
using HarmonyLib;

namespace DontShaveYourHead
{

	[HarmonyPatch(typeof(PawnRenderer), "RenderPawnInternal", new Type[] { typeof(Vector3), typeof(float), typeof(bool), typeof(Rot4), typeof(Rot4), typeof(RotDrawMode), typeof(bool), typeof(bool), typeof(bool) })]
	public static class Harmony_PawnRenderer
	{
		const float YOffset_OnHead = 0.03061225f;
		const float YOffset_PostHead = 0.03367347f;

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
					// if IL code loads YOffset_OnHead value, return YOffset_OnHead + YOffset_PostHead
					yield return new CodeInstruction(OpCodes.Ldc_R4, YOffset_OnHead + YOffset_PostHead);
				}
				// Skip hide hair flag (bool flag...)
				else if (code.opcode == OpCodes.Ldloc_S && code.operand is LocalBuilder b && b.LocalIndex == 14)
				{
					yield return new CodeInstruction(OpCodes.Ldc_I4_0) { labels = code.labels };
				}
				//help isolate the DrawMeshNowOrLater we want to replace i.e. the DrawMeshNowOrLater call after HairMatAt_NewTemp
				else if (code.opcode == OpCodes.Callvirt && (MethodInfo)code.operand == AccessTools.Method(typeof(PawnGraphicSet), nameof(PawnGraphicSet.HairMatAt_NewTemp)))
				{
					writing = true;
					yield return code;
				}
				// reset offset for hair rendering
				else if (writing && code.opcode == OpCodes.Ldloc_S && code.operand is LocalBuilder b2 && b2.LocalIndex == 13)
				{
					//loading loc1 and subtracting the offset from loc1's y field
					yield return new CodeInstruction(OpCodes.Ldloca_S, 13); //load loc1
					yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(Vector3), "y")); //load field y
					yield return new CodeInstruction(OpCodes.Dup); //copy the y value
					yield return new CodeInstruction(OpCodes.Ldind_R4); //push y value onto eval stack 
					yield return new CodeInstruction(OpCodes.Ldc_R4, YOffset_PostHead);  //push YOffset_PostHead value onto eval stack
					yield return new CodeInstruction(OpCodes.Sub); //subtract the values
					yield return new CodeInstruction(OpCodes.Stind_R4); //store the value

					yield return code;
				}
				// Replace DrawMeshNowOrLater call after HairMatAt_NewTemp
				else if (writing && code.opcode == OpCodes.Call && (MethodInfo)code.operand == AccessTools.Method(typeof(GenDraw), nameof(GenDraw.DrawMeshNowOrLater)))
				{
					writing = false;

					//load extra DrawMeshNowOrLater arguments
					//Mesh mesh, Vector3 loc, Quaternion quat, Material mat, bool isPortrait are already loaded at this stage
					yield return new CodeInstruction(OpCodes.Ldarg_0); //load 'this'
					yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(PawnRenderer), "pawn")); // load this.pawn
					yield return new CodeInstruction(OpCodes.Ldarg, 5); //load RenderPawnInternal argument 5 i.e. Rot4 headFacing

					//reroute DrawMeshNowOrLater to DrawHairReroute
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
