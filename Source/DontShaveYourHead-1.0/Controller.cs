using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using RimWorld;
using Verse;
using UnityEngine;
using Harmony;

namespace DontShaveYourHead
{
    public class Controller : Mod
	{
		public static DontShaveYourHeadSettings settings;
		public static IHairUtility HairUtility;

		public Controller(ModContentPack content) : base(content)
		{
			settings = GetSettings<DontShaveYourHeadSettings>();

			ILogger logger = new Logger_Nothing(); //default logger to log to nothing
			if (settings.LogFallback)
			{
				logger = new Logger(); //if logging fallback texture message, log to rimworld log
			}

			if (settings.UseFallbackTexture)
			{
				HairUtility = new HairUtility_Fallback(logger); //if using fallbacktextures, use a _Fallback instance
			}
			else
			{
				HairUtility = new HairUtility(logger); //otherwise use a default instance
			}

			HarmonyInstance.Create("DontShaveYourHead-Harmony").PatchAll(Assembly.GetExecutingAssembly());
		}


		public override void DoSettingsWindowContents(Rect inRect)
		{
			Listing_Standard listingStandard = new Listing_Standard();
			listingStandard.Begin(inRect);
			listingStandard.CheckboxLabeled("SettingUseFallback".Translate(), ref settings.UseFallbackTexture, "SettingUseFallbackDesc".Translate());
			listingStandard.CheckboxLabeled("SettingLogFallback".Translate(), ref settings.LogFallback, "SettingLogFallbackDesc".Translate());
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
			if (Find.Maps != null)
			{
				foreach (var map in Find.Maps)
				{
					foreach (var pawn in map.mapPawns.AllPawnsSpawned.Where(p => p.IsColonist))
					{
						PortraitsCache.SetDirty(pawn);
					}
				}
			}

			base.WriteSettings();
		}
	}
	public class DontShaveYourHeadSettings : ModSettings
	{
		public bool UseFallbackTexture;
		public bool LogFallback;

		/// <summary>
		/// The part that writes our settings to file. Note that saving is by ref.
		/// </summary>
		public override void ExposeData()
		{
			Scribe_Values.Look(ref UseFallbackTexture, "useFallbackTextures", true);
			Scribe_Values.Look(ref LogFallback, "logFallback", false);
			base.ExposeData();
		}

	}
}
