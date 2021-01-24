using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DontShaveYourHead
{
	public class FallbackTextureListDef : Def
	{
		public Range bottomPixelRange;
		public List<string> texturePathList;
	}

	public class Range
	{
		public int start;
		public int end;
	}
}
