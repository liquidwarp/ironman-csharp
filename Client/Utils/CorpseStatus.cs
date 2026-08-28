namespace IronManClient.Utils;

using System.Collections.Concurrent;
using System.Linq;

public class CorpseStatus {
    
    private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, float>> DamageByVictim = new();
    public static void RecordDamage(string victimId, string attackerId, float damage)
    {
        var attackerDamages = DamageByVictim.GetOrAdd(victimId, _ => new ConcurrentDictionary<string, float>());
        attackerDamages.AddOrUpdate(attackerId, damage, (_, existing) => existing + damage);
    }

    public static bool CanLoot(string victimId, string looterId, float requiredShare = 0.5f)
    {
        if (!DamageByVictim.TryGetValue(victimId, out var attackerDamages))
            return false;

        var totalDamage = attackerDamages.Values.Sum();
        if (totalDamage <= 0f)
            return false;

        if (!attackerDamages.TryGetValue(looterId, out var looterDamage))
            return false;

        return looterDamage / totalDamage >= requiredShare;
    }

    public static void ClearTracker()
    {
        DamageByVictim.Clear();
    }
}
