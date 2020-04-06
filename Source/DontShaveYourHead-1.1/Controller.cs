using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using RimWorld;
using Verse;
using UnityEngine;
using HarmonyLib;

namespace DontShaveYourHead
{
	public class Controller : Mod
	{
		public static DontShaveYourHeadSettings settings;
		public Controller(ModContentPack content) : base(content)
		{
			settings = GetSettings<DontShaveYourHeadSettings>();

			new Harmony("DontShaveYourHead-Harmony").PatchAll(Assembly.GetExecutingAssembly());
		}


		public override void DoSettingsWindowContents(Rect inRect)
		{
			Listing_Standard listingStandard = new Listing_Standard();
			listingStandard.Begin(inRect);
			listingStandard.CheckboxLabeled("SettingUseFallback".Translate(), ref settings.useFallbackTexture, "SettingUseFallbackDesc".Translate());
			listingStandard.CheckboxLabeled("SettingLogFallback".Translate(), ref settings.logFallback, "SettingLogFallbackDesc".Translate());
			listingStandard.End();
			base.DoSettingsWindowContents(inRect);
		}

		/// <summary>
		/// Override SettingsCategory to show up in the list of settings.
		/// Using .Translate() is optional, but does allow for localisation.
		/// </summary>
		/// <returns>The (translated) mod name.</returns>
		public override string SettingsCategory()
		{
			return "DSYHTitle".Translate();
		}

		public override void WriteSettings()
		{
			//when settings get written re-render portraits
			foreach (var map in Find.Maps)
			{
				foreach (var pawn in map.mapPawns.AllPawnsSpawned.Where(p => p.IsColonist))
				{
					PortraitsCache.SetDirty(pawn);
				}
			}

			base.WriteSettings();
		}
	}

	public class DontShaveYourHeadSettings : ModSettings
	{
		public bool useFallbackTexture;
		public bool logFallback;

		/// <summary>
		/// The part that writes our settings to file. Note that saving is by ref.
		/// </summary>
		public override void ExposeData()
		{
			Scribe_Values.Look(ref useFallbackTexture, "useFallbackTextures");
			Scribe_Values.Look(ref logFallback, "logFallback");
			base.ExposeData();
		}

	}
}
