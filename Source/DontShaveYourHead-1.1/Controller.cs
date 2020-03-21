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
        public Controller(ModContentPack content) : base(content)
        {
            new Harmony("DontShaveYourHead-Harmony").PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
