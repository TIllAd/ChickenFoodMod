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
        private static int _plain, _shredded, _rice, _broccoli, _marker, _bulkingMarker;

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
            _bulkingMarker = GDOUtils.GetCustomGameDataObject<BulkingMarker>().ID;
            return true;
        }

        public static bool IsChickenPlate(int group) => EnsureIds() && (group == _plain || group == _shredded);

        // Freischaltpflichtiger Platz, dessen Zutat auch in einem Pflicht-Platz steckt (= zweites Chicken).
        private static bool IsExtraPortionSet(ItemGroup.ItemSet set, ItemGroup group) =>
            set.RequiresUnlock && set.Items.Count > 0 && set.Items.All(i => i != null &&
                group.DerivedSets.Any(o => !o.RequiresUnlock && o.Items.Any(x => x != null && x.ID == i.ID)));

        // Anteil der Bestellungen je Karte (schließen sich gegenseitig aus, Rest = normal).
        public const float BulkingShare = 0.25f;
        public const float LowCarbShare = 0.25f;

        public static ItemList Build(ItemGroup group, HashSet<int> available)
        {
            // Erst die Art der Bestellung würfeln: Bulking (2x Chicken + Reis), Low Carb (Brokkoli statt Reis) oder normal.
            float roll = Random.value;
            bool bulking = available.Contains(_bulkingMarker) && roll < BulkingShare;
            bool lowCarb = !bulking && available.Contains(_marker) && roll >= BulkingShare && roll < BulkingShare + LowCarbShare;

            ItemList result = default;
            foreach (var set in group.DerivedSets)
            {
                // Beilagen-Platz: Reis, bei Low Carb Brokkoli.
                if (set.Items.Any(i => i != null && i.ID == _rice))
                {
                    result.Add(lowCarb ? _broccoli : _rice);
                    continue;
                }

                // Zweiter Chicken-Platz: nur bei Bulking (Chicken ist über den Pflicht-Platz immer freigeschaltet).
                if (IsExtraPortionSet(set, group))
                {
                    if (bulking) result.Add(set.Items[0].ID);
                    continue;
                }

                // Alles andere (Toppings) wie im Spiel.
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
