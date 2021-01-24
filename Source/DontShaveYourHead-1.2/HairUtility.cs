using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace DontShaveYourHead
{
	public interface IHairUtility
	{
		Material GetHairMatFor(Pawn pawn, Rot4 facing);
	}


	public class HairUtility : IHairUtility
	{
		protected readonly ILogger logger;
		protected Dictionary<Enums.Coverage, ICoverageType> headCoverages;
		protected bool init;

		public HairUtility(ILogger logger)
		{
			this.logger = logger;
		}

		protected virtual void Init()
		{
			if (!this.init) //if init hasn't been run yet
			{
				//setting defs here, since defs aren't loaded when Controller is called
				this.headCoverages = new Dictionary<Enums.Coverage, ICoverageType>()
				{
					{ Enums.Coverage.None, new CoverageType.None() },
					{ Enums.Coverage.Jaw, new CoverageType.CoveredType(Enums.Coverage.Jaw) },
					{ Enums.Coverage.UpperHead, new CoverageType.CoveredType(Enums.Coverage.UpperHead) },
					{ Enums.Coverage.FullHead, new CoverageType.CoveredType(Enums.Coverage.FullHead) }
				};
			}
		}

		public Material GetHairMatFor(Pawn pawn, Rot4 facing)
		{
			this.Init(); //initialize some variables

			// Find maximum coverage of non-mask headwear
			var maxCoverage = this.getMaxCoverage(pawn);

			string texPath = maxCoverage.GetTexPath(pawn);

			return GraphicDatabase.Get<Graphic_Multi>(texPath, ShaderDatabase.Cutout, Vector2.one, pawn.story.hairColor).MatAt(facing); // Set new graphic
		}

		//gets how much of the head the hat/helmet is covering
		private ICoverageType getMaxCoverage(Pawn pawn)
		{
			var maxCoverage = this.headCoverages[Enums.Coverage.None];

			//dubs bad hygeine clears apparelGraphics when washing, so only check for coverage if the pawn's headgear is actually rendered
			if (pawn.Drawer.renderer.graphics.apparelGraphics.Any())
			{
				//flattening body part groups, and returning coverage type based on defextension
				var coverages = from apparel in pawn.apparel.WornApparel.Where(a => !a.def.apparel.hatRenderedFrontOfFace)
									 from bodyPartGroup in apparel.def.apparel.bodyPartGroups
									 select this.headCoverages[bodyPartGroup.GetModExtension<BodyPartGroupDefExtension>().Coverage];

				//get the max coverage
				maxCoverage = coverages.OrderByDescending(c => c.Coverage).FirstOrDefault();
			}

			return maxCoverage;
		}
	}

	public class HairUtility_Fallback : HairUtility
	{
		private Dictionary<string, string> cachedTextures = new Dictionary<string, string>();
		private List<FallbackTextures> fallbackTexturesList;
		public HairUtility_Fallback(ILogger logger) : base(logger) { }

		protected override void Init()
		{
			if (!this.init) //if init hasn't been run yet
			{
				//setting defs here, since defs aren't loaded when Controller is called
				this.fallbackTexturesList = FallbackTextures.CreateFallbackTexturesList(DefDatabase<FallbackTextureListDef>.AllDefs);

				this.headCoverages = new Dictionary<Enums.Coverage, ICoverageType>()
				{
					{ Enums.Coverage.None, new CoverageType.None() },
					{ Enums.Coverage.Jaw, new CoverageType.CoveredType_Fallback(Enums.Coverage.Jaw, this.cachedTextures, this.fallbackTexturesList, this.logger) },
					{ Enums.Coverage.UpperHead, new CoverageType.CoveredType_Fallback(Enums.Coverage.UpperHead, this.cachedTextures, this.fallbackTexturesList, this.logger) },
					{ Enums.Coverage.FullHead, new CoverageType.CoveredType_Fallback(Enums.Coverage.FullHead, this.cachedTextures, this.fallbackTexturesList, this.logger) }
				};

			}
		}
		
	}

}
