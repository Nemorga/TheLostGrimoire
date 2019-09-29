using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Designers.Mechanics.EquipmentEnchants;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Designers;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.Designers.Mechanics.Recommendations;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.ElementsSystem;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.Items;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;

using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Class.LevelUp;

using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.Controllers.Projectiles;
using Harmony12;
using Newtonsoft.Json;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;

namespace thelostgrimoire
{
    class Fixandchange
    {
        static LibraryScriptableObject library => Main.library;



        internal static void Load()
        {
            // Load  Change
            Main.SafeLoad(AmuletofDyingWisdomChange, "Modify the enchant on Amulet of dying wisdom to acount for Immortality");

            //Load fix
            Main.SafeLoad(FixMissingDescriptorOnMagicMissile, "Add the force descriptor to Magic missile");
            Main.SafeLoad(FixMissingDescriptorOnMagicBateringBlast, "Add the force descriptor to BateringBLast");


        }


        static void FixMissingDescriptorOnMagicMissile()
        {
       
        }

        static void FixMissingDescriptorOnMagicBateringBlast()
        {

        }

        static void AmuletofDyingWisdomChange()
        {
            var immortality = library.Get<BlueprintFeature>(Helpers.getGuid("ImmortalityArcaneDiscovery"));
            var middleage = library.Get<BlueprintFeature>(Helpers.getGuid("MiddleAgeFeature"));
            var oldage = library.Get<BlueprintFeature>(Helpers.getGuid("OldAgeFeature"));
            var venerableage = library.Get<BlueprintFeature>(Helpers.getGuid("VenerableAgeFeature"));
            var DyingWisdomEnchant = library.Get<BlueprintEquipmentEnchantment>("84b63c1516d208247bceec28d6b6b38d");
            
            DyingWisdomEnchant.ComponentsArray = Array.Empty<BlueprintComponent>();
            var newcomponents = new BlueprintComponent[] {Helpers.Create<HandleImmortality>(p => p.Stat = StatType.Strength), 
                Helpers.Create<HandleImmortality>(p => p.Stat = StatType.Dexterity), 
                Helpers.Create<HandleImmortality>(p => p.Stat = StatType.Constitution),
                Helpers.Create<HandleImmortality>(p => p.Stat = StatType.Intelligence),
                Helpers.Create<HandleImmortality>(p => p.Stat = StatType.Wisdom),
                Helpers.Create<HandleImmortality>(p => p.Stat = StatType.Charisma)
            };

            DyingWisdomEnchant.AddComponents(newcomponents);

            var testitem = library.Get<BlueprintItemEquipmentNeck>("c4c7997fb14c9ee47a384d76fbc4512a");
            testitem.Enchantments.Clear();
            testitem.Enchantments.Add(DyingWisdomEnchant);
                
                

        }

        public class HandleImmortality : ItemEnchantmentLogic

        {

            
            public override void OnTurnOn()
            {
                var immortality = library.Get<BlueprintFeature>(Helpers.getGuid("ImmortalityArcaneDiscovery"));
                var middleage = library.Get<BlueprintFeature>(Helpers.getGuid("MiddleAgeFeature"));
                var oldage = library.Get<BlueprintFeature>(Helpers.getGuid("OldAgeFeature"));
                var venerableage = library.Get<BlueprintFeature>(Helpers.getGuid("VenerableAgeFeature"));
                ModifiableValue stat = base.Owner.Wielder.Stats.GetStat(this.Stat);
                if (stat != null)
                {

                    if (base.Owner.Wielder.HasFact(middleage))
                    {
                        if (base.Owner.Wielder.HasFact(immortality))
                        {
                            if(Stat == StatType.Strength || Stat == StatType.Dexterity || Stat == StatType.Constitution)
                                this.m_Modifier = stat.AddItemModifier(0, this, base.Owner, ModifierDescriptor.Profane);

                            if (Stat == StatType.Intelligence || Stat == StatType.Wisdom || Stat == StatType.Charisma)
                                this.m_Modifier = stat.AddItemModifier(1, this, base.Owner, ModifierDescriptor.Profane);
                            return;
                        }

                        else
                        {
                            if (Stat == StatType.Strength || Stat == StatType.Dexterity || Stat == StatType.Constitution)
                                this.m_Modifier = stat.AddItemModifier(-2, this, base.Owner, ModifierDescriptor.Profane);

                            if (Stat == StatType.Intelligence || Stat == StatType.Wisdom || Stat == StatType.Charisma)
                                this.m_Modifier = stat.AddItemModifier(1, this, base.Owner, ModifierDescriptor.Profane);
                            return;
                        }


                    }
                    if (base.Owner.Wielder.HasFact(oldage) || base.Owner.Wielder.HasFact(venerableage))
                    { 
                        this.m_Modifier = stat.AddItemModifier(0, this, base.Owner, ModifierDescriptor.Profane);
                        return;
                    }
                    
                    if (base.Owner.Wielder.HasFact(immortality))
                    {
                        if (Stat == StatType.Strength || Stat == StatType.Dexterity || Stat == StatType.Constitution)
                            this.m_Modifier = stat.AddItemModifier(0, this, base.Owner, ModifierDescriptor.Profane);

                        if (Stat == StatType.Intelligence || Stat == StatType.Wisdom || Stat == StatType.Charisma)
                            this.m_Modifier = stat.AddItemModifier(2, this, base.Owner, ModifierDescriptor.Profane);
                        return;
                    }
                        
                    else
                    {
                        if (Stat == StatType.Strength || Stat == StatType.Dexterity || Stat == StatType.Constitution)
                            this.m_Modifier = stat.AddItemModifier(-3, this, base.Owner, ModifierDescriptor.Profane);

                        if (Stat == StatType.Intelligence || Stat == StatType.Wisdom || Stat == StatType.Charisma)
                            this.m_Modifier = stat.AddItemModifier(2, this, base.Owner, ModifierDescriptor.Profane);
                       
                    }
                }
            }

            public override void OnTurnOff()
            {
                if (this.m_Modifier != null)
                {
                    this.m_Modifier.Remove();
                }
                this.m_Modifier = null;
            }

            
            

            
            public StatType Stat;
            private ModifiableValue.Modifier m_Modifier;

        }

    }
}
