using KitchenData;
using KitchenLib.Customs;
using KitchenLib.References;
using KitchenLib.Utils;
using MyChickenMod.Custom.Rice;
using UnityEngine;

namespace MyChickenMod.Custom.Portions
{
    // Eigene Portions-Items für Reis und Brokkoli aus unseren Töpfen.
    // Vorher waren das die Wok-Zutaten des Spiels ("Rice - Container Cooked",
    // "Broccoli - Chopped Container Cooked"). Folgen davon:
    //  - Reis + Brokkoli auf einem Teller wurde zum Wok-Gericht -> kein Chicken mehr möglich
    //  - in normalen Wok-Runs ließen sich unsere Töpfe als Abkürzung nutzen
    // Optik bleibt gleich (Modelle aus dem Spiel).

    public class RicePortion : CustomItem
    {
        public override string UniqueNameID => "Rice - Portion";
        public override ItemCategory ItemCategory => ItemCategory.Generic;
        public override string ColourBlindTag => "Ri";

        private GameObject _prefab;
        public override GameObject Prefab
        {
            get
            {
                if (_prefab == null)
                {
                    _prefab = PrefabUtil.CreateRoot("Rice Portion");
                    var rice = PrefabUtil.CloneVisual(Ref(ItemReferences.RiceContainerCooked).Prefab, _prefab.transform,
                        "Rice", Vector3.zero, 1.1f);
                    RicePot.RemoveBowl(rice);
                }
                return _prefab;
            }
        }

        private static Item Ref(int id) => (Item)GDOUtils.GetExistingGDO(id);
    }

    public class SteamedBroccoliPortion : CustomItem
    {
        public override string UniqueNameID => "Broccoli - Steamed Portion";
        public override ItemCategory ItemCategory => ItemCategory.Generic;
        public override string ColourBlindTag => "B";

        private GameObject _prefab;
        public override GameObject Prefab
        {
            get
            {
                if (_prefab == null)
                {
                    _prefab = PrefabUtil.CreateRoot("Steamed Broccoli Portion");
                    PrefabUtil.CloneVisual(Ref(ItemReferences.BroccoliChopped).Prefab, _prefab.transform,
                        "Broccoli", Vector3.zero, 0.8f);
                }
                return _prefab;
            }
        }

        private static Item Ref(int id) => (Item)GDOUtils.GetExistingGDO(id);
    }

    // Unsichtbare Markierung: wird von der Karte "Low Carb" freigeschaltet und von ChickenOrderPatch
    // abgefragt. Kommt in keinem Gericht vor und wird nie bestellt.
    public class LowCarbMarker : CustomItem
    {
        public override string UniqueNameID => "Low Carb Marker";
        private GameObject _prefab;
        public override GameObject Prefab => _prefab ??= PrefabUtil.CreateRoot("Low Carb Marker");
    }
}
