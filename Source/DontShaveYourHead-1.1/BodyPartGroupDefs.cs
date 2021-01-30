using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace DontShaveYourHead
{
    public static class BodyPartGroupDefOfDSYH
    {
        public static BodyPartGroupDef Teeth;
    }
    public class BodyPartGroupDefExtension : DefModExtension
    {
        public Enums.Coverage Coverage;
    }

}
