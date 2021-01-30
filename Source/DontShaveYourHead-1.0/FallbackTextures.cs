using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace DontShaveYourHead
{
	public class FallbackTextures
	{
		public Range BottomPixelRange { get; set; }
		public List<FallbackTexture> Textures { get; set; }

		public static List<FallbackTextures> CreateFallbackTexturesList(IEnumerable<FallbackTextureListDef> defs)
		{
			List<FallbackTextures> fallbackTextures = new List<FallbackTextures>();

			foreach (var def in defs)
			{
				fallbackTextures.Add(new FallbackTextures()
				{
					BottomPixelRange = def.bottomPixelRange,
					Textures = def.texturePathList.Select(path => new FallbackTexture()
					{
						Hash = MD5Utility.CreateMD5String(path), //creating a hash to use for comparison
						Path = path
					}).ToList()
				});
			}

			return fallbackTextures;
		}
		public FallbackTexture GetClostestFallbackTexture(Pawn pawn, string texPath)
		{
			//create hash from current hair path to compare against
			var currentHairMD5 = MD5Utility.CreateMD5String(texPath.Split('/').Last());

			//compares the hashes of the texture paths to retreive a semi-random-but-deterministic texture, so it should pick the same texture each time.
			var closestFallback = this.Textures.OrderBy(h => h.Hash).OrderByDescending(h => h.Hash.CompareTo(currentHairMD5)).First();
			closestFallback.OriginalPath = texPath;

			return closestFallback;
		}

		public class FallbackTexture
		{
			public string OriginalPath { get; set; }
			public string Hash { get; set; }
			public string Path { get; set; }

			public FallbackTexture() { }
		}
	}
}
