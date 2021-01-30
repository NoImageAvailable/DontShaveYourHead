using Harmony;
using RimWorld;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;

namespace DontShaveYourHead
{
	public class Controller : Mod
	{
		public static DontShaveYourHeadSettings settings;
		public static IHairUtility HairUtility;

		public Controller(ModContentPack content) : base(content)
		{
			settings = GetSettings<DontShaveYourHeadSettings>();

			//either logs to rimworld log or just comsume log message based on LogDebug setting
			ILogger logger = settings.LogDebugMessage ? ((ILogger)new Logger()) : ((ILogger)new Logger_Nothing());

			//if using fallback textures, use the fallback implementation
			HairUtility = settings.UseFallbackTextures ? new HairUtility_Fallback(logger) : new HairUtility(logger);

			HarmonyInstance.Create("DontShaveYourHead-Harmony").PatchAll(Assembly.GetExecutingAssembly());
		}


		public override void DoSettingsWindowContents(Rect inRect)
		{
			Listing_Standard listingStandard = new Listing_Standard();
			listingStandard.Begin(inRect);
			listingStandard.CheckboxLabeled("SettingUseFallbackTextures".Translate(), ref settings.UseFallbackTextures, "SettingUseFallbackTexturesDesc".Translate());
			listingStandard.CheckboxLabeled("SettingLogDebugMessage".Translate(), ref settings.LogDebugMessage, "SettingLogDebugMessageDesc".Translate());
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
			if (Find.Maps != null)
			{
				//when settings get written re-render portraits
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
		public bool UseFallbackTextures;
		public bool LogDebugMessage;

		/// <summary>
		/// The part that writes our settings to file. Note that saving is by ref.
		/// </summary>
		public override void ExposeData()
		{
			Scribe_Values.Look(ref UseFallbackTextures, "useFallbackTextures", true);
			Scribe_Values.Look(ref LogDebugMessage, "logDebugMessage", true);
			base.ExposeData();
		}

	}
}
