using System;
using System.Collections.Generic;
using AlphaGenes;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace NatureFlavourPack;

class HediffComp_GauParasites : HediffComp_Parasites
{
    FloatRange skillRange = new FloatRange(0.25f, 0.90f);
    
    public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
    {
        float severityToTurn = Props.severityToTurn;

        Map map = parent.pawn.Corpse.Map;
        if (map != null && parent.Severity > severityToTurn)
        {
            DoHatch();

            for (int i = 0; i < 20; i++)
            {
                IntVec3 c;
                CellFinder.TryFindRandomReachableCellNearPosition(parent.pawn.Corpse.Position,
                    parent.pawn.Corpse.Position, map, 2,
                    TraverseParms.For(TraverseMode.NoPassClosedDoors), null, null, out c);

                FilthMaker.TryMakeFilth(c, parent.pawn.Corpse.Map, ThingDefOf.Filth_Blood);
            }


            InternalDefOf.Hive_Spawn.PlayOneShot(new TargetInfo(parent.pawn.Corpse.Position, map));
            //this.parent.pawn.Corpse.Destroy();
        }
    }

    public void DoHatch()
    {
        try
        {
            PawnGenerationRequest request;
            if (mother != null && !mother.Dead)
            {
                request = new PawnGenerationRequest(mother.kindDef, mother.Faction, PawnGenerationContext.NonPlayer, -1,
                    forceGenerateNewPawn: false, allowDead: false, allowDowned: true, canGeneratePawnRelations: true,
                    mustBeCapableOfViolence: false, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true,
                    allowPregnant: false, allowFood: true, allowAddictions: true, inhabitant: false,
                    certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: false,
                    worldPawnFactionDoesntMatter: false, 0f, 0f, null, 1f, null, null, null, null, null, null, null,
                    null, null, null, null, null, forceNoIdeo: false, forceNoBackstory: false, forbidAnyTitle: false,
                    forceDead: false, null, null, null, null, null, 0f, DevelopmentalStage.Newborn);
            }
            else
            {
                request = new PawnGenerationRequest(motherDef, motherFaction, PawnGenerationContext.NonPlayer, -1,
                    forceGenerateNewPawn: false, allowDead: false, allowDowned: true, canGeneratePawnRelations: true,
                    mustBeCapableOfViolence: false, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true,
                    allowPregnant: false, allowFood: true, allowAddictions: true, inhabitant: false,
                    certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: false,
                    worldPawnFactionDoesntMatter: false, 0f, 0f, null, 1f, null, null, null, null, null, null, null,
                    null, null, null, null, null, forceNoIdeo: false, forceNoBackstory: false, forbidAnyTitle: false,
                    forceDead: false, null, null, null, null, null, 0f, DevelopmentalStage.Newborn);
            }


            Pawn pawn = PawnGenerator.GeneratePawn(request);
            if (PawnUtility.TrySpawnHatchedOrBornPawn(pawn, parent.pawn.Corpse))
            {
                if (pawn != null)
                {
                    if (mother != null)
                    {
                        if (pawn.playerSettings != null && mother.playerSettings != null)
                        {
                            pawn.playerSettings.AreaRestrictionInPawnCurrentMap =
                                mother.playerSettings.AreaRestrictionInPawnCurrentMap;
                        }

                        if (pawn.RaceProps.IsFlesh)
                        {
                            pawn.relations.AddDirectRelation(PawnRelationDefOf.Parent, mother);
                        }
                    }
                }


                Find.LetterStack.ReceiveLetter("AG_ParasitesHatchedLabel".Translate(pawn.NameShortColored),
                    "AG_ParasitesHatched".Translate(pawn.NameShortColored), LetterDefOf.PositiveEvent,
                    (TargetInfo)pawn);

                if (mother != null)
                {
                    pawn.genes.SetXenotype(mother.genes.Xenotype);
                    
                    foreach (Trait trait in mother.story.traits.allTraits)
                    {
                        pawn.story.traits.GainTrait(trait, true);
                    }
                }

                foreach (Gene gene in parent.pawn.genes?.GenesListForReading ?? [])
                {
                    if (Rand.Value > 0.5f)
                    {
                        pawn.genes.AddGene(gene.def, !endogenes);
                    }
                }
                
                foreach (Trait trait in parent.pawn.story.traits.allTraits)
                {
                    pawn.story.traits.GainTrait(trait, true);
                }

                foreach (SkillRecord skill in parent.pawn.skills.skills)
                {
                    pawn.skills.GetSkill(skill.def).Level = Math.Max(
                        pawn.skills.GetSkill(skill.def).Level, 
                        Mathf.CeilToInt(skill.Level * skillRange.RandomInRange)
                        );
                }

                pawn.ageTracker.AgeBiologicalTicks = pawn.ageTracker.AdultMinAgeTicks - GenDate.TicksPerDay;
            }
            else
            {
                Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
            }
        }
        catch (Exception e)
        {
            ModLog.Error("Unknown error hatching", e);
        }
    }
}