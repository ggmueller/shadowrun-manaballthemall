using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ManaballThemAll.Controllers
{
  [ApiController]
  [Route("[controller]")]
  public class ManaballController : ControllerBase
  {

    public struct RollResult
    {
      public int hits;
      public bool isGlitch;
      public bool isCriticalGlitch;
      public int[] rolls;
    }

    [HttpPost("cast")]
    public ActionResult<ManaballCastResult> CastManaball([FromBody] CastManaball manaball)
    {
      var edgeResult = CalculateEdgeForCombatAction(manaball.Caster.Magic + manaball.Caster.Tradition,
        manaball.Target.DefenseRating);
      var hitResult = Roll(manaball.Caster.Magic + manaball.Caster.Sorcery);
      var evasionResult = Roll(manaball.Target.Willpower + manaball.Target.Intuition);

      if (hitResult.hits >= evasionResult.hits)
      {
        var damage = CalculateDamageOnTarget(hitResult.hits, evasionResult.hits, manaball.AmpUp);
        return Damage(damage, hitResult, evasionResult, edgeResult);
      }
      else
        return NoDamage(hitResult, evasionResult, edgeResult);
     
    }

    private 
      ManaballCastResult.EdgeResult CalculateEdgeForCombatAction(int attackRating, int defenseRating)
    {
      if (attackRating >= defenseRating + 4) return new ManaballCastResult.EdgeResult(gainedEdgeForAttacker: 1, gainedEdgeForDefender: 0);
      if (defenseRating >= attackRating + 4) return new ManaballCastResult.EdgeResult(gainedEdgeForAttacker: 0, gainedEdgeForDefender: 1);
      return new ManaballCastResult.EdgeResult(0, 0);
    }

    private ActionResult<ManaballCastResult> NoDamage(RollResult hitResult, RollResult evasionResult, ManaballCastResult.EdgeResult edgeResult)
    {
      return new ActionResult<ManaballCastResult>(new ManaballCastResult(damageToTarget: 0,
        hitResult.isGlitch, hitResult.isCriticalGlitch, 
        evasionResult.isGlitch, evasionResult.isCriticalGlitch,
        edgeResult,
        hitResult.rolls,
        evasionResult.rolls,
        new Dictionary<string, int>{{"hits", hitResult.hits}, {"evasion", evasionResult.hits}}));
    }

    private ActionResult<ManaballCastResult> Damage(int damage, RollResult hitResult, RollResult evasionResult, ManaballCastResult.EdgeResult edgeResult)
    {
      return new ActionResult<ManaballCastResult>(new ManaballCastResult(damageToTarget: damage,
        glitchForCaster: hitResult.isGlitch, criticalGlitchForCaster: hitResult.isCriticalGlitch, glitchForTarget: evasionResult.isGlitch,
        criticalGlitchForTarget: evasionResult.isCriticalGlitch, 
        edge: edgeResult,
        hitResult.rolls,
        evasionResult.rolls,
        results: new Dictionary<string, int>{{"hits", hitResult.hits}, {"evasion", evasionResult.hits}}));
    }

    private int CalculateDamageOnTarget(int noOfHits, int noOfEvasions, int ampUps)
    {
      Debug.Assert(noOfHits >= noOfEvasions);
      return noOfHits - noOfEvasions + ampUps;
    }

    [HttpPost("resist")]
    public ActionResult<ManaballResistResult> ResistManaballDrainDamage([FromBody]CharacterSpecs character, [FromQuery] int ampUpMagic, [FromQuery] int increaseArea)
    {
      var rollResult = Roll(character.Magic + character.Tradition);
      var resisted = rollResult.hits;
      var drainDamage = CalculateDrainDamage(5, ampUpMagic, increaseArea, resisted, character);
      return new ActionResult<ManaballResistResult>(new ManaballResistResult(drainDamage.mentalDamage, drainDamage.physicalDamage, resisted, rollResult.rolls));
    }

    private (int mentalDamage, int physicalDamage) CalculateDrainDamage(int baseDrain, int ampUpMagic, int increaseArea,
      int resisted, CharacterSpecs character)
    {
      var drainDamage = (baseDrain + ampUpMagic*2 + increaseArea) - resisted;
      return drainDamage <= character.Magic ? 
        (mentalDamage: Math.Max(0, drainDamage), physicalDamage: 0) 
        : (mentalDamage: 0, physicalDamage: drainDamage);
    }

    private RollResult Roll(int noOfDices)
    {
      var noOfGlitches = 0;
      var noOfHits = 0;
      var rolls = new int[noOfDices];
      for (var i = 0; i < noOfDices; i++)
      {
        var roll = RollOne();
        rolls[i] = roll.roll;
        if (roll.hits) noOfHits++;
        if (roll.isGlitch) noOfGlitches++;
      }

      var isGlitch = noOfGlitches > noOfDices / 2;
      Array.Sort(rolls, (m, n) => n - m); //Descending
      return new RollResult{hits = noOfHits, isGlitch = isGlitch, isCriticalGlitch = isGlitch && noOfHits == 0, rolls = rolls};
    }

    private (bool hits, bool isGlitch, int roll) RollOne()
    {
      var roll = new Random().Next(1, 7);
      if (roll == 1) return (hits: false, isGlitch: true, roll);
      if (roll >= 5) return (hits: true, isGlitch: false, roll);
      return (hits: false, isGlitch: false, roll);

    }
  }

  public class ManaballCastResult
  {
    public class EdgeResult
    {
      public EdgeResult(int gainedEdgeForAttacker, int gainedEdgeForDefender)
      {
        GainedEdgeForAttacker = gainedEdgeForAttacker;
        GainedEdgeForDefender = gainedEdgeForDefender;
      }
      
      public int GainedEdgeForAttacker { get; set; }
      public int GainedEdgeForDefender { get; set; }
    }

    public ManaballCastResult(int damageToTarget, bool glitchForCaster, bool criticalGlitchForCaster,
      bool glitchForTarget, bool criticalGlitchForTarget, EdgeResult edge, int[] hitResultRolls,
      int[] evasionResultRolls,
      Dictionary<string, int> results)
    {
      DamageToTarget = damageToTarget;
      GlitchForCaster = glitchForCaster;
      GlitchForTarget = glitchForTarget;
      CriticalGlitchForTarget = criticalGlitchForTarget;
      CriticalGlitchForCaster = criticalGlitchForCaster;
      Results = results;
      
      Edge = edge;
      HitResultRolls = hitResultRolls;
      EvasionResultRolls = evasionResultRolls;
    }


    public int DamageToTarget { get; set; }
    public bool GlitchForCaster { get; set; }
    public bool GlitchForTarget { get; set; }
    public bool CriticalGlitchForTarget { get; set; }
    public bool CriticalGlitchForCaster { get; set; }

    public EdgeResult Edge { get; set; }
    public int[] HitResultRolls { get; set; }
    public int[] EvasionResultRolls { get; set; }

    Dictionary<string, int> Results { get; set; }
  }

  public class CastManaball
  {
    public int AmpUp { get; set; }
    public CharacterSpecs Caster { get; set; }
    public CharacterSpecs Target { get; set; }
    
    
  }
}