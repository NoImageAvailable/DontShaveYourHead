using RimWorld;
using UnityEngine;
using Verse;
using System.Collections.Generic;
using System.Linq;
using System;

namespace DontShaveYourHead
{
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

			// Set hair graphics to headgear-appropriate texture
			var texPath = pawn.story.hairDef.texPath;

			// Set path appendage
			if (maxCoverage != Coverage.None)
			{
				string maxCoverageString = maxCoverage.ToString();

				//ids for cache
				string textureId = $"{texPath}_{maxCoverageString}_{facing.ToStringWord()}";
				if (!textureCache.ContainsKey(textureId))
				{
					// Check if the path exists
					var newTexPath = texPath + "/" + maxCoverageString;

					if (!ContentFinder<Texture2D>.Get(newTexPath + "_south", false))
					{
						//couldn't find a custom texture, get a semi-random default custom

						//load the current hair
						var material = GraphicDatabase.Get<Graphic_Multi>(texPath, ShaderDatabase.Cutout, Vector2.one, pawn.story.hairColor).MatAt(Rot4.South);

						//get current hair texture
						Texture2D hairTexture = getReadableTexture((Texture2D)material.mainTexture);

						//find the lowest pixel, to get hair length
						int lowestPixel = -1;

						//start from bottom row, iterate to top
						for (int y = 0; y < hairTexture.height; y++)
						{
							//get pixels one row at a time
							var pixelsRow = hairTexture.GetPixels(0, y, hairTexture.width, 1);

							if (pixelsRow.Any(c => c != Color.clear))
							{
								//if we find a non clear pixel, set lowestPixel and break;
								lowestPixel = y;
								break;
							}
						}

						//get the default customs for the pixel range
						var customHairs = defaultCustomHairs.Where(d => d.PixelRange.ContainsValue(lowestPixel)).FirstOrDefault().Hairs;

						//get just the hair name
						var currentHairName = texPath.Split('/').Last();

						//get the closest default custom name
						var getClosestDefault = customHairs.OrderByDescending(h => h.Name.CompareTo(currentHairName)).First();

						//check for facial hair?
						newTexPath = getClosestDefault.Path + "/" + maxCoverageString;


						//if (maxCoverageString != "Jaw")
						//newTexPath = HairDefOf.Shaved.texPath;

						textureCache.Add(textureId, newTexPath);
						texPath = newTexPath;
					}
					else
					{
						texPath = newTexPath;
					}
				}
				else
				{
					//get original texture from cache
					texPath = textureCache[textureId];
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


		private struct DefaultCustomHair
		{
			public Range<int> PixelRange { get; set; }
			public List<CustomHair> Hairs { get; set; }
		}

		private struct CustomHair
		{
			public string Name { get; set; }
			public string Path { get; set; }

			public CustomHair(string name, string path)
			{
				this.Name = name;
				this.Path = path;
			}
		}

		private static List<DefaultCustomHair> defaultCustomHairs = new List<DefaultCustomHair>()
		{
			new DefaultCustomHair() {
				PixelRange = new Range<int>(128, 128),
				Hairs = new List<CustomHair>()
				{
					new CustomHair("", "")
				}
			},

			new DefaultCustomHair() { //short
				PixelRange = new Range<int>(50, 128),
				Hairs = new List<CustomHair>() {
					new CustomHair("AniEmo", "Hairs/AniEmo"),
					new CustomHair("Neat", "Hairs/Neat"),
					new CustomHair("Vintage", "Hairs/Vintage"),
					new CustomHair("ED", "Hair/ED"),
					new CustomHair("jack", "Hair/nbi_jack"),
					new CustomHair("jeffy", "Hair/nbi_jeffy"),
					new CustomHair("panda", "Hair/nbi_panda"),
					new CustomHair("tom", "Hair/nbi_tom"),
					new CustomHair("trent", "Hair/nbi_trent"),
					new CustomHair("Rei", "Hair/RimNGE_Rei"),
					new CustomHair("Frat", "Hair/SPSFrat"),
					new CustomHair("Jay", "Hair/SPSJay"),
					new CustomHair("Leon", "Hair/SPSLeon"),
					new CustomHair("Lulu", "Hair/SPSLulu"),
					new CustomHair("Usagi", "Hair/SPSUsagi"),
					new CustomHair("VC", "Hair/VC"),
					new CustomHair("Bob", "Things/Pawn/Humanlike/Hairs/Bob"),
					new CustomHair("Burgundy", "Things/Pawn/Humanlike/Hairs/Burgundy"),
					new CustomHair("Mess", "Things/Pawn/Humanlike/Hairs/Mess"),
					new CustomHair("Mop", "Things/Pawn/Humanlike/Hairs/Mop")
				}
			},

			new DefaultCustomHair() { //medium
				PixelRange = new Range<int>(20, 49),
				Hairs = new List<CustomHair>() {
					new CustomHair("FemShort_back", "Hairs/FemShort_back"),
					new CustomHair("StraightShort", "Hairs/StraightShort"),
					new CustomHair("WavyShort", "Hairs/WavyShort"),
					new CustomHair("CA", "Hair/CA"),
					new CustomHair("KE", "Hair/KE"),
					new CustomHair("nbi_cleo", "Hair/nbi_cleo"),
					new CustomHair("nbi_marcie", "Hair/nbi_marcie"),
					new CustomHair("nbi_tiffany", "Hair/nbi_tiffany"),
					new CustomHair("OA", "Hair/OA"),
					new CustomHair("PA", "Hair/PA"),
					new CustomHair("SM", "Hair/SM"),
					new CustomHair("Flora", "Hair/SPSFlora"),
					new CustomHair("Kat", "Hair/SPSKat"),
					new CustomHair("Slick", "Hair/SPSSlick"),
					new CustomHair("TS", "Hair/TS"),
					new CustomHair("Xeva", "Hair/Xeva"),
					new CustomHair("XT", "Hair/XT"),
					new CustomHair("Curly", "Things/Pawn/Humanlike/Hairs/Curly"),
					new CustomHair("Flowy", "Things/Pawn/Humanlike/Hairs/Flowy"),
					new CustomHair("Long", "Things/Pawn/Humanlike/Hairs/Long"),
					new CustomHair("Wavy", "Things/Pawn/Humanlike/Hairs/Wavy")
				}
			},

			new DefaultCustomHair() { //long
				PixelRange = new Range<int>(0, 19),
				Hairs = new List<CustomHair>() {
					new CustomHair("ClassyF", "Hairs/ClassyF"),
					new CustomHair("Hasslefree", "Hairs/Hasslefree"),
					new CustomHair("Ponytail", "Hairs/Ponytail"),
					new CustomHair("StraightLong", "Hairs/StraightLong"),
					new CustomHair("MA", "Hair/MA"),
					new CustomHair("curly", "Hair/nbi_curly"),
					new CustomHair("quinn", "Hair/nbi_quinn"),
					new CustomHair("sandi", "Hair/nbi_sandi"),
					new CustomHair("Asuka", "Hair/RimNGE_Asuka"),
					new CustomHair("Princess", "Things/Pawn/Humanlike/Hairs/Princess")
				}
			}
		};
	}
	public static class Extensions
	{
		public static T[] Slice<T>(this T[] source, int index, int length)
		{
			T[] slice = new T[length];
			Array.Copy(source, index, slice, 0, length);
			return slice;
		}
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