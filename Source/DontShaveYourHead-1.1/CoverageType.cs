using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace DontShaveYourHead
{
	public interface ICoverageType
	{
		Enums.Coverage Coverage { get; set; }
		string GetTexPath(Pawn pawn);
	}

	public static class CoverageType
	{
		public class None : ICoverageType
		{
			public Enums.Coverage Coverage { get; set; }

			public None()
			{
				this.Coverage = Enums.Coverage.None;
			}
			public string GetTexPath(Pawn pawn)
			{
				//nothing covering hair, return normal hair
				return pawn.story.hairDef.texPath;
			}
		}

		public class CoveredType : ICoverageType
		{
			public Enums.Coverage Coverage { get; set; }
			public CoveredType(Enums.Coverage coverage)
			{
				this.Coverage = coverage;
			}
			public string GetTexPath(Pawn pawn)
			{
				// Check if custom texture path exists
				if (!ContentFinder<Texture2D>.Get($"{pawn.story.hairDef.texPath}/{this.Coverage}_south", false))
				{
					//if no custom texture return shaved
					return HairDefOf.Shaved.texPath;
				}

				return $"{pawn.story.hairDef.texPath}/{this.Coverage}";
			}
		}

		public class CoveredType_Fallback : ICoverageType
		{
			public Enums.Coverage Coverage { get; set; }
			private readonly Dictionary<string, string> cachedTextures;
			private readonly List<FallbackTextures> fallbackTexturesList;
			private readonly ILogger logger;

			public CoveredType_Fallback(Enums.Coverage coverage, Dictionary<string, string> cachedTextures, List<FallbackTextures> fallbackTexturesList, ILogger logger)
			{
				this.Coverage = coverage;
				this.cachedTextures = cachedTextures;
				this.fallbackTexturesList = fallbackTexturesList;
				this.logger = logger;
			}

			public string GetTexPath(Pawn pawn)
			{
				// get current hair path
				var texPath = pawn.story.hairDef.texPath;

				if (this.cachedTextures.ContainsKey(texPath))
				{
					//get texture from cache if it already exists
					texPath = this.cachedTextures[texPath];
					this.logger.LogMessage($"{pawn.Name} | retreived from cache | {texPath}");
				}
				else
				{
					// Check if custom texture path exists
					if (!ContentFinder<Texture2D>.Get($"{texPath}/{this.Coverage}_south", false))//couldn't find a custom texture, get a semi-random fallback
					{
						//get lowest pixel to estimate hair length
						int bottomPixel = TextureUtility.GetBottomPixelPercentage(pawn, texPath, Rot4.East);

						//get the fallback textures for the pixel range
						var textures = this.fallbackTexturesList.Where(ft => bottomPixel >= ft.BottomPixelRange.start && bottomPixel <= ft.BottomPixelRange.end).FirstOrDefault();

						if (textures != null)
						{
							FallbackTextures.FallbackTexture closestFallback = textures.GetClostestFallbackTexture(pawn, texPath);

							this.logger.LogMessage($"{pawn.Name} | {bottomPixel} | {closestFallback.OriginalPath} | {closestFallback.Path}");

							//adding to the cache so we don't have to do the lookup again
							this.cachedTextures.Add(texPath, closestFallback.Path);

							texPath = closestFallback.Path;
						}
						else
						{
							//if can't find a fallback texture, return shaved hair
							return HairDefOf.Shaved.texPath;
						}
					}
				}

				return $"{texPath}/{this.Coverage}";
			}
		}
	}
}
