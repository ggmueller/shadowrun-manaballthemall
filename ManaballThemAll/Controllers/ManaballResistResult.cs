namespace ManaballThemAll.Controllers
{
  public class ManaballResistResult
  {
    public ManaballResistResult(int stunDamage, int physicalDamage, int resistedDamage, int[] rollResult)
    {
      ResistedDamage = resistedDamage;
      StunDamageOnSpellCaster = stunDamage;
      PhysicalDamageOnSpellCaster = physicalDamage;
      RollResult = rollResult;
    }

    public int PhysicalDamageOnSpellCaster { get; set; }

    public int ResistedDamage { get; set; }

    public int StunDamageOnSpellCaster { get; set; }
    public int[] RollResult { get; set; }
  }
}