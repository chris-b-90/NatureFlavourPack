using System;
using System.Reflection;
using Verse;
using UnityEngine;
using HarmonyLib;
using Verse.AI;

namespace NatureFlavourPack;

public class NatureFlavourPackMod : Mod
{
    public NatureFlavourPackMod(ModContentPack content) : base(content)
    {
        ModLog.Log("Loading NatureFlavourPackMod");
#if DEBUG
        Harmony.DEBUG = true;
#endif
        Harmony harmony = new Harmony("mss.NatureFlavourPackMod.main");
        harmony.PatchAll();
    }
}