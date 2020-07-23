using System;
using Microsoft.AspNetCore.Mvc;

namespace ManaballThemAll.Controllers
{
  [ApiController]
  [Route("[controller]")]
  public class ManaballController : ControllerBase
  {
    [HttpPost]
    public ActionResult<ManaballAction> ResistManaballDrainDamage([FromBody]CharacterSpecs character, [FromQuery] int ampUpMagic)
    {
      var rollResult = Roll(character.Magic + character.Tradition);
      var resisted = rollResult.hits;
      var drainDamage = CalculateDrainDamage(5, ampUpMagic, resisted);
      return new ActionResult<ManaballAction>(new ManaballAction(drainDamage, resisted));
    }

    private int CalculateDrainDamage(int baseDrain, int ampUpMagic, int resisted)
    {
      return Math.Max(0, (baseDrain + ampUpMagic*2) - resisted);
    }

    private (int hits, bool isGlitch, bool isCriticalGlitch) Roll(int noOfDices)
    {
      var noOfGlitches = 0;
      var noOfHits = 0;
      for (var i = 0; i < noOfDices; i++)
      {
        var roll = RollOne();
        if (roll.hits) noOfHits++;
        if (roll.isGlitch) noOfGlitches++;
      }

      var isGlitch = noOfGlitches > noOfDices / 2;
      return (hits: noOfHits, isGlitch, isCriticalGlitch: isGlitch && noOfHits == 0);
    }

    private (bool hits, bool isGlitch) RollOne()
    {
      var roll = new Random().Next(1, 6);
      if (roll == 1) return (hits: false, isGlitch: true);
      if (roll >= 5) return (hits: true, isGlitch: false);
      return (hits: false, isGlitch: false);

    }
  }
}