namespace ManaballThemAll.Controllers
{
  public class ManaballAction
  {
    public ManaballAction(int mentalDamage, int physicalDamage, int resistedDamage)
    {
      ResistedDamage = resistedDamage;
      MentalDamageOnSpellCaster = mentalDamage;
      PhysicalDamageOnSpellCaster = physicalDamage;
    }

    public int PhysicalDamageOnSpellCaster { get; set; }

    public int ResistedDamage { get; set; }

    public int MentalDamageOnSpellCaster { get; set; }
  }
}