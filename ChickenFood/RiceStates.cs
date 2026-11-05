using System.Collections.Generic;
using Kitchen;
using KitchenData;
using KitchenLib.Customs;
using KitchenLib.References;
using KitchenLib.Utils;
using KitchenMods;
using MyChickenMod.Custom.Chicken;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace MyChickenMod.Custom.Rice
{
    // ---------------------------------------------------------------------------------
    // Karten "Burnt Rice" und "Cold Rice"
    //
    // Prozesse hängen im Spiel fest am Item und lassen sich nicht pro Karte schalten.
    // Darum gibt es den gekochten Reistopf in vier gleich aussehenden Varianten:
    //   normal | brennt an | kühlt ab | beides
    // RicePotStateSystem tauscht jeden gekochten Topf in die Variante, die zu den aktiven
    // Karten passt. Anbrennen/Abkühlen erledigt danach die eingebaute Spielmechanik
    // (Fortschrittsbalken, Feuergefahr auf dem Herd, Herd stoppt das Abkühlen).
    // ---------------------------------------------------------------------------------
    internal static class RiceTuning
    {
        public const float BurnTime = 15f;    // so lange darf gar gekochter Reis noch auf dem Herd stehen
        public const float CoolTime = 25f;    // so lange bleibt er abseits vom Herd warm
        public const float ReheatTime = 3f;   // kalten Reis kurz auf den Herd -> wieder warm
        public const int Portions = 5;
    }

    // Gemeinsame Basis aller gekochten Reistöpfe (Optik, Portionen).
    public abstract class RiceCookedPotBase : CustomItem
    {
        protected virtual bool Burns => false;
        protected virtual bool Cools => false;

        public override ItemStorage ItemStorageFlags => ItemStorage.None;
        public override Item SplitSubItem => (Item)GDOUtils.GetCustomGameDataObject<MyChickenMod.Custom.Portions.RicePortion>().GameDataObject;
        public override int SplitCount => RiceTuning.Portions;
        public override List<Item> SplitDepletedItems => new List<Item> { Ref(ItemReferences.Pot) };
        public override Item DisposesTo => Ref(ItemReferences.Pot);
        public override string ColourBlindTag => "R";

        public override List<Item.ItemProcess> Processes => Burns
            ? new List<Item.ItemProcess>
            {
                new Item.ItemProcess
                {
                    Process = (Process)GDOUtils.GetExistingGDO(ProcessReferences.Cook),
                    Duration = RiceTuning.BurnTime,
                    IsBad = true,
                    Result = (Item)GDOUtils.GetCustomGameDataObject<RiceBurntPot>().GameDataObject
                }
            }
            : new List<Item.ItemProcess>();

        public override Item.ItemProcess AutomaticItemProcess => Cools
            ? new Item.ItemProcess
            {
                Process = (Process)GDOUtils.GetCustomGameDataObject<RiceCoolDownProcess>().GameDataObject,
                Duration = RiceTuning.CoolTime,
                IsBad = true,
                Result = (Item)GDOUtils.GetCustomGameDataObject<RiceColdPot>().GameDataObject
            }
            : default;

        private GameObject _prefab;
        public override GameObject Prefab => _prefab ??= RicePotVisuals.BuildCookedPot(UniqueNameID, null);

        protected static Item Ref(int id) => (Item)GDOUtils.GetExistingGDO(id);
    }

    public class RiceCookedPotBurning : RiceCookedPotBase
    {
        public override string UniqueNameID => "Rice - Cooked Pot (Burns)";
        protected override bool Burns => true;
    }

    public class RiceCookedPotCooling : RiceCookedPotBase
    {
        public override string UniqueNameID => "Rice - Cooked Pot (Cools)";
        protected override bool Cools => true;
    }

    public class RiceCookedPotBurningCooling : RiceCookedPotBase
    {
        public override string UniqueNameID => "Rice - Cooked Pot (Burns, Cools)";
        protected override bool Burns => true;
        protected override bool Cools => true;
    }

    // Kalter Reis: Portionen bleiben im Topf, lassen sich aber erst nach dem Aufwärmen entnehmen.
    public class RiceColdPot : CustomItem
    {
        public override string UniqueNameID => "Rice - Cold Pot";
        public override ItemStorage ItemStorageFlags => ItemStorage.None;
        public override Item SplitSubItem => (Item)GDOUtils.GetCustomGameDataObject<MyChickenMod.Custom.Portions.RicePortion>().GameDataObject;
        public override int SplitCount => RiceTuning.Portions;
        public override bool PreventExplicitSplit => true;
        public override List<Item> SplitDepletedItems => new List<Item> { Ref(ItemReferences.Pot) };
        public override Item DisposesTo => Ref(ItemReferences.Pot);
        public override string ColourBlindTag => "Rc";

        // Aufwärmen ergibt den normalen Topf; das System macht daraus wieder die passende Variante.
        public override List<Item.ItemProcess> Processes => new List<Item.ItemProcess>
        {
            new Item.ItemProcess
            {
                Process = (Process)GDOUtils.GetExistingGDO(ProcessReferences.Cook),
                Duration = RiceTuning.ReheatTime,
                Result = (Item)GDOUtils.GetCustomGameDataObject<RiceCookedPot>().GameDataObject
            }
        };

        private GameObject _prefab;
        public override GameObject Prefab => _prefab ??= RicePotVisuals.BuildCookedPot("Rice Cold Pot", new Color(0.72f, 0.85f, 1f));

        private static Item Ref(int id) => (Item)GDOUtils.GetExistingGDO(id);
    }

    // Angebrannt: nur noch wegwerfen (Mülleimer gibt den Topf zurück).
    public class RiceBurntPot : CustomItem
    {
        public override string UniqueNameID => "Rice - Burnt Pot";
        public override ItemStorage ItemStorageFlags => ItemStorage.None;
        public override Item DisposesTo => Ref(ItemReferences.Pot);
        public override string ColourBlindTag => "Rb";

        private GameObject _prefab;
        public override GameObject Prefab
        {
            get
            {
                if (_prefab == null)
                {
                    _prefab = PrefabUtil.CreateRoot("Rice Burnt Pot");
                    var pot = Ref(ItemReferences.Pot).Prefab;
                    PrefabUtil.CloneVisual(pot.transform.Find("Pot").gameObject, _prefab.transform, "Pot", Vector3.zero);
                    PrefabUtil.CloneVisual(Ref(ItemReferences.BurnedFood).Prefab, _prefab.transform, "Burnt",
                        new Vector3(0f, 0.15f, 0f), 0.9f);
                }
                return _prefab;
            }
        }

        private static Item Ref(int id) => (Item)GDOUtils.GetExistingGDO(id);
    }

    // Eigener Prozess "Abkühlen": läuft automatisch, ein Herd (Kochen) hebt ihn auf.
    public class RiceCoolDownProcess : CustomProcess
    {
        public override string UniqueNameID => "Rice - Cool Down";
        public override Process IsCounteractedBy => (Process)GDOUtils.GetExistingGDO(ProcessReferences.Cook);
        public override List<(Locale, ProcessInfo)> InfoList => new List<(Locale, ProcessInfo)>
        {
            (Locale.English, new ProcessInfo { Name = "Cool Down", Icon = "<sprite name=\"cook\">" })
        };
    }

    internal static class RicePotVisuals
    {
        private static readonly Vector3 RiceFull = new Vector3(0f, 0.15f, 0f);
        private static readonly Vector3 RiceEmpty = new Vector3(0f, -0.05f, 0f);

        // Topf mit gekochtem Reis, der beim Portionieren absinkt (wie Suppe). tint != null -> Reis einfärben.
        public static GameObject BuildCookedPot(string name, Color? tint)
        {
            var root = PrefabUtil.CreateRoot(name);
            var pot = ((Item)GDOUtils.GetExistingGDO(ItemReferences.Pot)).Prefab;
            PrefabUtil.CloneVisual(pot.transform.Find("Pot").gameObject, root.transform, "Pot", Vector3.zero);
            var rice = PrefabUtil.CloneVisual(((Item)GDOUtils.GetExistingGDO(ItemReferences.RiceContainerCooked)).Prefab,
                root.transform, "Rice", RiceFull, 1.4f);
            RicePot.RemoveBowl(rice);
            if (tint.HasValue) Tint(rice, tint.Value);

            var view = root.AddComponent<PositionSplittableView>();
            ReflectionUtils.GetField<PositionSplittableView>("FullPosition").SetValue(view, RiceFull);
            ReflectionUtils.GetField<PositionSplittableView>("EmptyPosition").SetValue(view, RiceEmpty);
            ReflectionUtils.GetField<PositionSplittableView>("Objects").SetValue(view, new List<GameObject> { rice });
            return root;
        }

        private static void Tint(GameObject go, Color colour)
        {
            foreach (var r in go.GetComponentsInChildren<MeshRenderer>(true))
            {
                var mats = r.sharedMaterials;
                for (int i = 0; i < mats.Length; i++)
                {
                    if (mats[i] == null) continue;
                    var m = new Material(mats[i]);
                    if (m.HasProperty("_Color0")) m.SetColor("_Color0", m.GetColor("_Color0") * colour);
                    if (m.HasProperty("_Color")) m.SetColor("_Color", m.GetColor("_Color") * colour);
                    mats[i] = m;
                }
                r.sharedMaterials = mats;
            }
        }
    }

    // ---------- Karten ----------
    public abstract class RiceDifficultyCard : CustomUnlockCard
    {
        public override UnlockGroup UnlockGroup => UnlockGroup.Generic;
        public override CardType CardType => CardType.Default;
        public override bool IsUnlockable => true;
        public override List<Unlock> HardcodedRequirements => new List<Unlock> { Refs.BaseDish };
        public override List<UnlockEffect> Effects => new List<UnlockEffect>();
    }

    public class BurntRiceCard : RiceDifficultyCard
    {
        public override string UniqueNameID => "ChickenBurntRice_Card";
        public override List<(Locale, UnlockInfo)> InfoList => new List<(Locale, UnlockInfo)>
        {
            (Locale.English, new UnlockInfo
            {
                Name = "Burnt Rice",
                Description = "Cooked rice burns if it stays on the hob too long",
                FlavourText = "Smells like Sunday at grandma's."
            })
        };
    }

    public class ColdRiceCard : RiceDifficultyCard
    {
        public override string UniqueNameID => "ChickenColdRice_Card";
        public override List<(Locale, UnlockInfo)> InfoList => new List<(Locale, UnlockInfo)>
        {
            (Locale.English, new UnlockInfo
            {
                Name = "Cold Rice",
                Description = "Cooked rice cools down away from the hob and must be reheated before portioning",
                FlavourText = "Nobody likes cold rice."
            })
        };
    }

    // ---------- System ----------
    public class RicePotStateSystem : GameSystemBase, IModSystem
    {
        private EntityQuery _pots;
        private EntityQuery _unlocks;

        // Portionen pro Topf merken: Typwechsel im Spiel (abkühlen, aufwärmen) füllen den Topf sonst wieder voll.
        private readonly Dictionary<Entity, (int ID, int Remaining)> _memory = new Dictionary<Entity, (int, int)>();
        private readonly HashSet<Entity> _seen = new HashSet<Entity>();

        private int _normal, _burns, _cools, _both, _cold, _burnCard, _coldCard;
        private HashSet<int> _family;

        protected override void Initialise()
        {
            base.Initialise();
            _pots = GetEntityQuery(typeof(CItem), typeof(CSplittableItem));
            _unlocks = GetEntityQuery(typeof(CProgressionUnlock));
        }

        private bool EnsureIds()
        {
            if (_family != null) return true;
            var normal = GDOUtils.GetCustomGameDataObject<RiceCookedPot>();
            if (normal?.GameDataObject == null) return false;
            _normal = normal.ID;
            _burns = GDOUtils.GetCustomGameDataObject<RiceCookedPotBurning>().ID;
            _cools = GDOUtils.GetCustomGameDataObject<RiceCookedPotCooling>().ID;
            _both = GDOUtils.GetCustomGameDataObject<RiceCookedPotBurningCooling>().ID;
            _cold = GDOUtils.GetCustomGameDataObject<RiceColdPot>().ID;
            _burnCard = GDOUtils.GetCustomGameDataObject<BurntRiceCard>().ID;
            _coldCard = GDOUtils.GetCustomGameDataObject<ColdRiceCard>().ID;
            _family = new HashSet<int> { _normal, _burns, _cools, _both, _cold };
            return true;
        }

        protected override void OnUpdate()
        {
            if (_pots.IsEmpty)
            {
                _memory.Clear();
                return;
            }
            if (!EnsureIds()) return;

            bool burn = false, cool = false;
            using (var unlocks = _unlocks.ToComponentDataArray<CProgressionUnlock>(Allocator.Temp))
            {
                foreach (var u in unlocks)
                {
                    if (!u.IsActive) continue;
                    if (u.ID == _burnCard) burn = true;
                    else if (u.ID == _coldCard) cool = true;
                }
            }
            int wanted = burn ? (cool ? _both : _burns) : (cool ? _cools : _normal);

            _seen.Clear();
            var ctx = new EntityContext(EntityManager);
            using (var entities = _pots.ToEntityArray(Allocator.Temp))
            {
                foreach (var e in entities)
                {
                    var item = EntityManager.GetComponentData<CItem>(e);
                    if (!_family.Contains(item.ID)) continue;
                    _seen.Add(e);
                    var split = EntityManager.GetComponentData<CSplittableItem>(e);

                    // Typ wurde vom Spiel gewechselt -> Portionen von vorher wiederherstellen.
                    if (_memory.TryGetValue(e, out var before) && before.ID != item.ID && split.RemainingCount != before.Remaining)
                    {
                        split.RemainingCount = Mathf.Clamp(before.Remaining, 0, split.TotalCount);
                        EntityManager.SetComponentData(e, split);
                    }

                    // Kalter Reis bleibt kalt, bis er aufgewärmt wurde. Sonst: passende Variante.
                    if (item.ID != _cold && item.ID != wanted)
                    {
                        int remaining = split.RemainingCount;
                        ctx.ChangeItemType(e, wanted, false, 0, false);
                        if (EntityManager.HasComponent<CSplittableItem>(e))
                        {
                            split = EntityManager.GetComponentData<CSplittableItem>(e);
                            split.RemainingCount = Mathf.Clamp(remaining, 0, split.TotalCount);
                            EntityManager.SetComponentData(e, split);
                        }
                        item.ID = wanted;
                    }

                    _memory[e] = (item.ID, split.RemainingCount);
                }
            }

            if (_memory.Count > _seen.Count)
            {
                var stale = new List<Entity>();
                foreach (var key in _memory.Keys)
                    if (!_seen.Contains(key)) stale.Add(key);
                foreach (var key in stale) _memory.Remove(key);
            }
        }
    }
}
