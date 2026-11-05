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
    // Karte "Burnt Rice"
    //
    // Prozesse hängen im Spiel fest am Item und lassen sich nicht pro Karte schalten.
    // Darum gibt es den gekochten Reistopf in zwei gleich aussehenden Varianten: normal | brennt an.
    // RicePotStateSystem tauscht jeden gekochten Topf in die Variante, die zur Karte passt.
    // Das Anbrennen erledigt danach die eingebaute Spielmechanik (Fortschrittsbalken, Feuergefahr).
    // ("Cold Rice" wurde gestrichen: ohne Gerät zeigt das Spiel keinen Fortschrittsbalken an.)
    // ---------------------------------------------------------------------------------
    internal static class RiceTuning
    {
        public const float BurnTime = 15f;    // so lange darf gar gekochter Reis noch auf dem Herd stehen
        public const int Portions = 5;
    }

    // Gemeinsame Basis aller gekochten Reistöpfe (Optik, Portionen).
    public abstract class RiceCookedPotBase : CustomItem
    {
        protected virtual bool Burns => false;

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


        private GameObject _prefab;
        public override GameObject Prefab => _prefab ??= RicePotVisuals.BuildCookedPot(UniqueNameID, null);

        protected static Item Ref(int id) => (Item)GDOUtils.GetExistingGDO(id);
    }

    public class RiceCookedPotBurning : RiceCookedPotBase
    {
        public override string UniqueNameID => "Rice - Cooked Pot (Burns)";
        protected override bool Burns => true;
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

    // ---------- System ----------
    public class RicePotStateSystem : GameSystemBase, IModSystem
    {
        private EntityQuery _pots;
        private EntityQuery _unlocks;

        // Portionen pro Topf merken: Typwechsel im Spiel (abkühlen, aufwärmen) füllen den Topf sonst wieder voll.
        private readonly Dictionary<Entity, (int ID, int Remaining)> _memory = new Dictionary<Entity, (int, int)>();
        private readonly HashSet<Entity> _seen = new HashSet<Entity>();

        private int _normal, _burns, _burnCard;
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
            _burnCard = GDOUtils.GetCustomGameDataObject<BurntRiceCard>().ID;
            _family = new HashSet<int> { _normal, _burns };
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

            bool burn = false;
            using (var unlocks = _unlocks.ToComponentDataArray<CProgressionUnlock>(Allocator.Temp))
            {
                foreach (var u in unlocks)
                {
                    if (!u.IsActive) continue;
                    if (u.ID == _burnCard) burn = true;
                }
            }
            int wanted = burn ? _burns : _normal;

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
                    if (item.ID != wanted)
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
