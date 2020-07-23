namespace ManaballThemAll.Controllers
{
  public class ManaballAction
  {
    public ManaballAction(int drainDamage, int resistedDamage)
    {
      ResistedDamage = resistedDamage;
      DrainDamageOnSpellCaster = drainDamage;
    }

    public int ResistedDamage { get; set; }

    public int DrainDamageOnSpellCaster { get; set; }
  }
}