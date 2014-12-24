using LeagueSharp;
using LeagueSharp.Common;
using Pantheon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Puckingpant
{
    internal class PotionManager
    {
        private enum PotionType
        {
            Health, Mana
        };

        private class Potion
        {
            public string Name
            {
                get;
                set;
            }
            public int MinCharges
            {
                get;
                set;
            }
            public LeagueSharp.ItemId ItemId
            {
                get;
                set;
            }
            public int Priority
            {
                get;
                set;
            }
            public List<PotionType> TypeList
            {
                get;
                set;
            }
        }

        private List<Potion> _potions;

        public PotionManager()
        {
            _potions = new List<Potion>
            {
                new Potion
                {
                    Name = "ItemCrystalFlask",
                    MinCharges = 1,
                    ItemId = (LeagueSharp.ItemId) 2041,
                    Priority = 1,
                    TypeList = new List<PotionType> {PotionType.Health, PotionType.Mana}
                },
                new Potion
                {
                    Name = "RegenerationPotion",
                    MinCharges = 0,
                    ItemId = (LeagueSharp.ItemId) 2003,
                    Priority = 2,
                    TypeList = new List<PotionType> {PotionType.Health}
                },
                new Potion
                {
                    Name = "ItemMiniRegenPotion",
                    MinCharges = 0,
                    ItemId = (LeagueSharp.ItemId) 2010,
                    Priority = 4,
                    TypeList = new List<PotionType> {PotionType.Health, PotionType.Mana}
                },
                new Potion
                {
                    Name = "FlaskOfCrystalWater",
                    MinCharges = 0,
                    ItemId = (LeagueSharp.ItemId) 2004,
                    Priority = 3,
                    TypeList = new List<PotionType> {PotionType.Mana}
                }
            };
            Load();
        }

        private void Load()
        {
            _potions = _potions.OrderBy(x => x.Priority).ToList();
            Program.MenuExtras.AddSubMenu(new Menu("Potion Manager", "PotionManager"));

            Program.MenuExtras.SubMenu("PotionManager").AddSubMenu(new Menu("Health", "Health"));
            Program.MenuExtras.SubMenu("PotionManager").SubMenu("Health").AddItem(new MenuItem("HealthPotion", "Use Health Potion").SetValue(true));
            Program.MenuExtras.SubMenu("PotionManager").SubMenu("Health").AddItem(new MenuItem("HealthPercent", "HP Trigger Percent").SetValue(new Slider(30)));

            Program.MenuExtras.SubMenu("PotionManager").AddSubMenu(new Menu("Mana", "Mana"));
            Program.MenuExtras.SubMenu("PotionManager").SubMenu("Mana").AddItem(new MenuItem("ManaPotion", "Use Mana Potion").SetValue(true));
            Program.MenuExtras.SubMenu("PotionManager").SubMenu("Mana").AddItem(new MenuItem("ManaPercent", "MP Trigger Percent").SetValue(new Slider(30)));

            Game.OnGameUpdate += OnGameUpdate;
        }

        private void OnGameUpdate(EventArgs args)
        {
            if (Utility.InShopRange()) return;

            try
            {
                if (Program.MenuExtras.Item("HealthPotion").GetValue<bool>())
                {
                    if (GetPlayerHealthPercentage() <= Program.MenuExtras.Item("HealthPercent").GetValue<Slider>().Value)
                    {
                        var healthSlot = GetPotionSlot(PotionType.Health);
                        if (!IsBuffActive(PotionType.Health))
                            healthSlot.UseItem();
                    }
                }
                if (Program.MenuExtras.Item("ManaPotion").GetValue<bool>())
                {
                    if (GetPlayerManaPercentage() <= Program.MenuExtras.Item("ManaPercent").GetValue<Slider>().Value)
                    {
                        var manaSlot = GetPotionSlot(PotionType.Mana);
                        if (!IsBuffActive(PotionType.Mana))
                            manaSlot.UseItem();
                    }
                }
            }

            catch (Exception)
            {

            }
        }

        private InventorySlot GetPotionSlot(PotionType type)
        {
            return (from potion in _potions
                    where potion.TypeList.Contains(type)
                    from item in ObjectManager.Player.InventoryItems
                    where item.Id == potion.ItemId && item.Charges >= potion.MinCharges
                    select item).FirstOrDefault();
        }

        private bool IsBuffActive(PotionType type)
        {
            return (from potion in _potions
                    where potion.TypeList.Contains(type)
                    from buff in ObjectManager.Player.Buffs
                    where buff.Name == potion.Name && buff.IsActive
                    select potion).Any();
        }

        private static float GetPlayerHealthPercentage()
        {
            return ObjectManager.Player.Health * 100 / ObjectManager.Player.MaxHealth;
        }

        private static float GetPlayerManaPercentage()
        {
            return ObjectManager.Player.Mana * 100 / ObjectManager.Player.MaxMana;
        }
    }
}
