using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using KitchenData;
using KitchenLib.Utils;
using MyChickenMod.Custom.Chicken;
using MyChickenMod.Custom.Portions;
using UnityEngine;

namespace MyChickenMod
{
    // Bestellungen für unsere Teller selbst würfeln.
    //
    // Grund: "Brokkoli statt Reis" (Low Carb) und das Brokkoli-Topping sind dieselbe Portion aus
    // demselben Topf. Das Spiel schaltet Zutaten aber pro Teller frei, nicht pro Platz auf dem Teller -
    // mit nur einer der beiden Karten käme sonst auch die andere Bestellung vor.
    // Lösung: Die Low-Carb-Karte schaltet eine unsichtbare Markierung (LowCarbMarker) frei. Hier wird
    // für den Beilagen-Platz nur dann Brokkoli gewürfelt, wenn die Markierung freigeschaltet ist.
    // Alle anderen Plätze laufen exakt wie im Spiel (ItemSetView.GetRandomConfiguration).
    [HarmonyPatch(typeof(ItemSetView), nameof(ItemSetView.GetRandomConfiguration))]
    internal static class ChickenOrderPatch
    {
        private static bool Prefix(int group, HashSet<int> available_ingredients, bool allow_any, ref ItemList __result)
        {
            if (allow_any || available_ingredients == null) return true;
            if (!ChickenOrders.IsChickenPlate(group)) return true;
            if (!GameData.Main.TryGet<ItemGroup>(group, out var itemGroup)) return true;
            __result = ChickenOrders.Build(itemGroup, available_ingredients);
            return false;
        }
    }

    internal static class ChickenOrders
    {
        private static int _plain, _shredded, _rice, _broccoli, _marker;

        private static bool EnsureIds()
        {
            if (_plain != 0) return true;
            var plain = GDOUtils.GetCustomGameDataObject<MyChicken_Plated>();
            if (plain?.GameDataObject == null) return false;
            _plain = plain.ID;
            _shredded = GDOUtils.GetCustomGameDataObject<ShreddedChicken_Plated>().ID;
            _rice = GDOUtils.GetCustomGameDataObject<RicePortion>().ID;
            _broccoli = GDOUtils.GetCustomGameDataObject<SteamedBroccoliPortion>().ID;
            _marker = GDOUtils.GetCustomGameDataObject<LowCarbMarker>().ID;
            return true;
        }

        public static bool IsChickenPlate(int group) => EnsureIds() && (group == _plain || group == _shredded);

        public static ItemList Build(ItemGroup group, HashSet<int> available)
        {
            ItemList result = default;
            foreach (var set in group.DerivedSets)
            {
                // Beilagen-Platz: genau eins von Reis / (mit Low Carb) Brokkoli.
                if (set.Items.Any(i => i != null && i.ID == _rice))
                {
                    var choices = new List<int> { _rice };
                    if (available.Contains(_marker)) choices.Add(_broccoli);
                    result.Add(choices[Random.Range(0, choices.Count)]);
                    continue;
                }

                // Alles andere wie im Spiel.
                int num = Random.Range(set.Min, set.Max + 1);
                foreach (var item in set.Items
                             .Where(i => i != null && (!set.RequiresUnlock || available.Contains(i.ID)))
                             .OrderBy(_ => Random.value))
                {
                    if (num <= 0) break;
                    result.Add(item.ID);
                    num--;
                }
            }
            return result;
        }
    }
}
