﻿using RimWorld;
<<<<<<< HEAD
=======
using System;
>>>>>>> fallback-texture
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace DontShaveYourHead
{
<<<<<<< HEAD
    public static class HairUtility
    {
        private enum Coverage
        {
            None,
            Jaw,
            UpperHead,
            FullHead
        }

        private static Dictionary<Coverage, BodyPartGroupDef> headDefs = new Dictionary<Coverage, BodyPartGroupDef>()
        {
            { Coverage.Jaw, BodyPartGroupDefOfDSYH.Teeth },
            { Coverage.UpperHead, BodyPartGroupDefOf.UpperHead },
            { Coverage.FullHead, BodyPartGroupDefOf.FullHead }
        };

        public static Material GetHairMatFor(Pawn pawn, Rot4 facing)
        {
            // Find maximum coverage of non-mask headwear
            var maxCoverage = getMaxCoverage(pawn);

            // Set hair graphics to headgear-appropriate texture
            var texPath = pawn.story.hairDef.texPath;

            // Set path appendage
            if (maxCoverage != Coverage.None)
            {
                string pathAppendString = maxCoverage.ToString();

                // Check if the path exists
                var newTexPath = texPath + "/" + pathAppendString;
                if (!ContentFinder<Texture2D>.Get(newTexPath + "_south", false))
                {
#if DEBUG
                    Log.Warning("DSYH :: could not find texture at " + texPath);
#endif
                    if (pathAppendString != "Jaw") texPath = HairDefOf.Shaved.texPath;
                }
                else
                {
                    texPath = newTexPath;
                }

            }

            return GraphicDatabase.Get<Graphic_Multi>(texPath, ShaderDatabase.Cutout, Vector2.one, pawn.story.hairColor).MatAt(facing); // Set new graphic
        }

        private static Coverage getMaxCoverage(Pawn pawn)
        {
            var maxCoverage = Coverage.None;

            //dubs bad hygeine clears apparelGraphics when washing, so only check for coverage if the pawn's headgear is actually rendered
            if (pawn.Drawer.renderer.graphics.apparelGraphics.Any())
            {
                //flattening body part groups, and joining to head defs
                var bodyPartGroups = from apparel in pawn.apparel.WornApparel.Where(c => !c.def.apparel.hatRenderedFrontOfFace)
                                     from bodyPartGroup in apparel.def.apparel.bodyPartGroups
                                     join headDef in headDefs on bodyPartGroup equals headDef.Value
                                     select headDef;

                maxCoverage = bodyPartGroups.DefaultIfEmpty().Max(b => b.Key); //finding the max head def coverage
            }

            return maxCoverage;
        }

        /*private enum Coverage
        {
            None,
            Jaw,
            UpperHead,
            FullHead
        }

        public static Material GetHairMatFor(Pawn pawn, Rot4 facing)
        {
        // Define coverage-appropriate path
        var pathAppendString = "";

            // Find maximum coverage of non-mask headwear
            var maxCoverage = Coverage.None;
            foreach (var cur in pawn.apparel.WornApparel)
            {
                if (!cur.def.apparel.hatRenderedFrontOfFace)
                {
                    var coveredGroups = cur.def.apparel.bodyPartGroups;
                    var curCoverage = Coverage.None;

                    // Find highest current max coverage
                    if (coveredGroups.Contains(BodyPartGroupDefOf.FullHead))
                    {
                        curCoverage = Coverage.FullHead;
                    }
                    else if (coveredGroups.Contains(BodyPartGroupDefOf.UpperHead))
                    {
                        curCoverage = Coverage.UpperHead;
                    }
                    else if (coveredGroups.Contains(BodyPartGroupDefOfDSYH.Teeth))
                    {
                        curCoverage = Coverage.Jaw;
                    }

                    // Compare to stored max coverage
                    if (maxCoverage < curCoverage)
                    {
                        maxCoverage = curCoverage;
                    }
                }
            }

            // Set path appendage
            if (maxCoverage != Coverage.None)
            {
                pathAppendString = maxCoverage.ToString();
            }

            // Set hair graphics to headgear-appropriate texture
            var texPath = pawn.story.hairDef.texPath;
            if (!pathAppendString.NullOrEmpty())
            {
                // Check if the path exists
                var newTexPath = texPath + "/" + pathAppendString;
                if (!ContentFinder<Texture2D>.Get(newTexPath + "_south", false))
                {
#if DEBUG
                    Log.Warning("DSYH :: could not find texture at " + texPath);
#endif
                    if (pathAppendString != "Jaw") texPath = HairDefOf.Shaved.texPath;
                }
                else
                {
                    texPath = newTexPath;
                }
            }
            return GraphicDatabase.Get<Graphic_Multi>(texPath, ShaderDatabase.Cutout, Vector2.one, pawn.story.hairColor).MatAt(facing); // Set new graphic
        }*/
    }
}
=======
	public static class HairUtility
	{
		private enum Coverage
		{
			None,
			Jaw,
			UpperHead,
			FullHead
		}


		private static Dictionary<string, string> textureCache = new Dictionary<string, string>();

		private static Dictionary<Coverage, BodyPartGroupDef> headDefs = new Dictionary<Coverage, BodyPartGroupDef>()
		{
			{ Coverage.Jaw, BodyPartGroupDefOfDSYH.Teeth },
			{ Coverage.UpperHead, BodyPartGroupDefOf.UpperHead },
			{ Coverage.FullHead, BodyPartGroupDefOf.FullHead }
		};

		public static Material GetHairMatFor(Pawn pawn, Rot4 facing)
		{
			// Find maximum coverage of non-mask headwear
			var maxCoverage = getMaxCoverage(pawn);

			string texPath = getTexPath(pawn, maxCoverage);

			return GraphicDatabase.Get<Graphic_Multi>(texPath, ShaderDatabase.Cutout, Vector2.one, pawn.story.hairColor).MatAt(facing); // Set new graphic
		}

		private static string getTexPath(Pawn pawn, Coverage maxCoverage)
		{
			// get current hair path
			var texPath = pawn.story.hairDef.texPath;

			string maxCoverageString = maxCoverage.ToString();

			// if there's something covering the hair
			if (maxCoverage != Coverage.None)
			{
				if (Controller.settings.useFallbackTexture && textureCache.ContainsKey(texPath))
				{
					//get texture from cache
					texPath = textureCache[texPath];
				}
				else
				{
					// Check if custom texture path exists
					if (!ContentFinder<Texture2D>.Get($"{texPath}/{maxCoverageString}_south", false))
					{
						//couldn't find a custom texture, get a semi-random fallback
						if (Controller.settings.useFallbackTexture)
						{
							//get lowest pixel to estimate hair length
							int lowestPixelPercentage = getLowestPixelPercentage(pawn, texPath, Rot4.East);

							//get the fallback hairs for the pixel range
							var fallback = fallbackTextures.Where(d => d.PixelRangePercent.ContainsValue(lowestPixelPercentage)).FirstOrDefault().Textures;

							if (fallback.Any())
							{
								//get hair name from path
								var currentHairName = texPath.Split('/').Last();

								//get the closest fallback
								var closestFallback = fallback.OrderBy(h => h.Name).OrderByDescending(h => h.Name.CompareTo(currentHairName)).First();

								//adding to cache
								if (Controller.settings.logFallback)
									Log.Message($"DSYH: {pawn.Name} | {texPath} | {closestFallback.Path}");

								textureCache.Add(texPath, closestFallback.Path);
								texPath = closestFallback.Path;
							}
							else
							{
								//if can't find a fallback texture, return shaved hair
								return HairDefOf.Shaved.texPath;
							}
						}
						else
						{
							//if can't find a fallback texture, return shaved hair
							return HairDefOf.Shaved.texPath;
						}
					}
				}

				texPath = $"{texPath}/{maxCoverageString}";
			}

			return texPath;
		}

		//get lowest pixel as percentage of height, to account for different hair resolutions
		private static int getLowestPixelPercentage(Pawn pawn, string texPath, Rot4 rot)
		{
			//load the current hair mat
			var material = GraphicDatabase.Get<Graphic_Multi>(texPath, ShaderDatabase.Cutout, Vector2.one, pawn.story.hairColor).MatAt(rot);

			//get current hair texture
			Texture2D hairTexture = getReadableTexture((Texture2D)material.mainTexture);

			double percentage = ((double)getLowestPixel(hairTexture) / (double)hairTexture.height) * 100;
			return (int)percentage;
		}

		private static int getLowestPixel(Texture2D hairTexture)
		{
			//start from bottom row, iterate to top
			for (int y = 0; y < hairTexture.height; y++)
			{
				//get pixels one row at a time
				var pixelsRow = hairTexture.GetPixels(0, y, hairTexture.width, 1);

				if (pixelsRow.Any(c => c.a > 0f))
				{
					//if we find a non clear pixel, return;
					return y;
				}
			}

			return -1;
		}

		private static Coverage getMaxCoverage(Pawn pawn)
		{
			var maxCoverage = Coverage.None;

			//dubs bad hygeine clears apparelGraphics when washing, so only check for coverage if the pawn's headgear is actually rendered
			if (pawn.Drawer.renderer.graphics.apparelGraphics.Any())
			{
				//flattening body part groups, and joining to head defs
				var bodyPartGroups = from apparel in pawn.apparel.WornApparel.Where(c => !c.def.apparel.hatRenderedFrontOfFace)
									 from bodyPartGroup in apparel.def.apparel.bodyPartGroups
									 join headDef in headDefs on bodyPartGroup equals headDef.Value
									 select headDef;

				maxCoverage = bodyPartGroups.DefaultIfEmpty().Max(b => b.Key); //finding the max head def coverage
			}

			return maxCoverage;
		}



		private static Texture2D getReadableTexture(Texture2D texture)
		{
			// Create a temporary RenderTexture of the same size as the texture
			RenderTexture tmp = RenderTexture.GetTemporary(
								texture.width,
								texture.height,
								0,
								RenderTextureFormat.Default,
								RenderTextureReadWrite.Linear);

			// Blit the pixels on texture to the RenderTexture
			Graphics.Blit(texture, tmp);

			// Backup the currently set RenderTexture
			RenderTexture previous = RenderTexture.active;

			// Set the current RenderTexture to the temporary one we created
			RenderTexture.active = tmp;

			// Create a new readable Texture2D to copy the pixels to it
			Texture2D readableTexture = new Texture2D(texture.width, texture.height);

			// Copy the pixels from the RenderTexture to the new Texture
			readableTexture.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
			readableTexture.Apply();

			// Reset the active RenderTexture
			RenderTexture.active = previous;

			// Release the temporary RenderTexture
			RenderTexture.ReleaseTemporary(tmp);

			return readableTexture;
		}


		private struct TexturesForRange
		{
			public Range<int> PixelRangePercent { get; set; }
			public List<CustomTexture> Textures { get; set; }
		}

		private struct CustomTexture
		{
			public string Name { get; set; }
			public string Path { get; set; }

			public CustomTexture(string name, string path)
			{
				this.Name = name;
				this.Path = path;
			}
		}

		private static List<TexturesForRange> fallbackTextures = new List<TexturesForRange>()
		{
			new TexturesForRange() { //short
				PixelRangePercent = new Range<int>(34, 100), //bottom pixel in this range
				Textures = new List<CustomTexture>() {
					new CustomTexture("c", "Things/Pawn/Humanlike/Hairs/Mop"),
					new CustomTexture("f", "Hair/nbi_jamie"),
					new CustomTexture("i", "Things/Pawn/Humanlike/Hairs/Burgundy"),
					new CustomTexture("l", "Hair/nbi_upchuck"),
					new CustomTexture("o", "Hair/SPSFrat"),
					new CustomTexture("r", "Hair/SPSJay"),
					new CustomTexture("u", "Hair/nbi_jeffy"),
					new CustomTexture("z", "Hair/nbi_jack")
				}
			},

			new TexturesForRange() { //medium
				PixelRangePercent = new Range<int>(20, 33), //bottom pixel in this range
				Textures = new List<CustomTexture>() {
					new CustomTexture("c", "Things/Pawn/Humanlike/Hairs/Flowy"),
					new CustomTexture("e", "Hair/ED"),
					new CustomTexture("h", "Hair/KE"),
					new CustomTexture("j", "Hair/nbi_short"),
					new CustomTexture("m", "Hair/nbi_tom"),
					new CustomTexture("o", "Hair/nbi_trent"),
					new CustomTexture("r", "Hair/PA"),
					new CustomTexture("t", "Hair/SPSSlick"),
					new CustomTexture("w", "Things/Pawn/Humanlike/Hairs/Curly"),
					new CustomTexture("z", "Hairs/LittleBird")
				}
			},

			new TexturesForRange() { //long
				PixelRangePercent = new Range<int>(0, 19), //bottom pixel in this range
				Textures = new List<CustomTexture>() {
					new CustomTexture("c", "Hairs/Braid"),
					new CustomTexture("e", "Hairs/ClassyF"),
					new CustomTexture("h", "Hairs/Hasslefree"),
					new CustomTexture("j", "Hairs/Ponytail"),
					new CustomTexture("m", "Hairs/StraightLong"),
					new CustomTexture("o", "Hair/nbi_curly"),
					new CustomTexture("r", "Hair/nbi_mel"),
					new CustomTexture("t", "Hair/nbi_regal"),
					new CustomTexture("w", "Hair/nbi_sandi"),
					new CustomTexture("z", "Hair/nbi_witch")
				}
			}
		};
	}


	/// <summary>The Range class.</summary>
	/// <typeparam name="T">Generic parameter.</typeparam>
	public class Range<T> where T : IComparable<T>
	{
		/// <summary>Minimum value of the range.</summary>
		public T Minimum { get; set; }

		/// <summary>Maximum value of the range.</summary>
		public T Maximum { get; set; }


		/// <summary>Determines if the provided value is inside the range.</summary>
		/// <param name="value">The value to test</param>
		/// <returns>True if the value is inside Range, else false</returns>
		public bool ContainsValue(T value)
		{
			return (this.Minimum.CompareTo(value) <= 0) && (value.CompareTo(this.Maximum) <= 0);
		}


		public Range(T min, T max)
		{
			Minimum = min; Maximum = max;
		}
	}

}
>>>>>>> fallback-texture
