// Copyright (c) 2019 Jennifer Messerly
// This code is licensed under MIT license (see LICENSE for details)

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
    static class ArcaneDiscoveries
    {
        static LibraryScriptableObject library => Main.library;
        static BlueprintCharacterClass wizardclass = Main.library.Get<BlueprintCharacterClass>("ba34257984f4c41408ce1dc2004e342e");


        internal static void Load()
        {
            // Load  feats
            Main.SafeLoad(CreateImmortality, "Who wants to live for ever?");
            Main.SafeLoad(CreateStaffLikeWand, "Laser pistols, kinda");



        }


        static void CreateImmortality()
        {
            var middleage = library.Get<BlueprintFeature>(Helpers.getGuid("MiddleAgeMalusFeature"));
            var oldage = library.Get<BlueprintFeature>(Helpers.getGuid("OldAgeMalusFeature"));
            var venerableage = library.Get<BlueprintFeature>(Helpers.getGuid("VenerableAgeMalusFeature"));
            var icon = Helpers.GetIcon("80a1a388ee938aa4e90d427ce9a7a3e9");

            var immortality = Helpers.CreateFeature("ImmortalityArcaneDiscovery", "Arcane Discovery: Immortality", " You discover a cure for aging, and from this point forward you take no penalty to your physical ability scores from advanced age. If you are already taking such penalties, they are removed at this time. ",
                Helpers.getGuid("ImmortalityArcaneDiscovery"), icon, FeatureGroup.WizardFeat,
                Helpers.PrerequisiteClassLevel(wizardclass, 1),
                Helpers.Create<RemoveFeatureOnApply>(r => r.Feature = middleage),
                Helpers.Create<RemoveFeatureOnApply>(r => r.Feature = oldage),
                Helpers.Create<RemoveFeatureOnApply>(r => r.Feature = venerableage)
                );
            immortality.Groups = immortality.Groups.AddToArray(FeatureGroup.Feat);
            library.AddFeats(immortality);




        }

        static void CreateStaffLikeWand()
        {
            //var prereq = Helpers.Create<BlueprintFeature>("f43ffc8e3f8ad8a43be2d44ad6e27914");//skilfocus umd

            var prereq = library.TryGet<BlueprintFeature>("46fad72f54a33dc4692d3b62eca7bb78#CraftMagicItems(feat=wand)");
            if (prereq == null)
                prereq = library.Get<BlueprintFeature>("f43ffc8e3f8ad8a43be2d44ad6e27914");//skilfocus umd

            var icon = Helpers.GetIcon("1b94043744e83494c8a083319e1602f3"); //wandmastery
            var StaffLikeWandFeature = Helpers.CreateFeature("StaffLikeWandArcaneDiscovery", "Arcane Discovery : Staff-Like Wand", "Similar to using a magic staff, you use your own Intelligence score and relevant feats to set the DC for saves against spells you cast from a wand, and you can use your caster level when activating the power of a wand if it’s higher than the caster level of the wand.",
                Helpers.getGuid("StaffLikeWandArcaneDiscovery"), icon, FeatureGroup.WizardFeat,
                Helpers.PrerequisiteFeature(prereq),
                Helpers.PrerequisiteClassLevel(wizardclass, 5),
                Helpers.Create<StafflikewandMechanic>()
                );

            StaffLikeWandFeature.Groups = StaffLikeWandFeature.Groups.AddToArray(FeatureGroup.Feat);
            library.AddFeats(StaffLikeWandFeature);
        }

        public class StafflikewandMechanic : OwnedGameLogicComponent<UnitDescriptor>, IInitiatorRulebookHandler<RuleCastSpell>, IRulebookHandler<RuleCastSpell>, IInitiatorRulebookSubscriber
        {
            public void OnEventAboutToTrigger(RuleCastSpell evt)
            {
                if (!(evt.Spell.SourceItemUsableBlueprint == null) && evt.Spell.SourceItemUsableBlueprint.Type == UsableItemType.Wand)
                {
                    var wand = evt.Spell.SourceItemUsableBlueprint;
                    var caster = evt.Spell.Caster;
                    var spell = evt.Spell; 
                    //@@Nemorga : If balance issue, add condition for it to work only with wizars spell or even only spell in spellbook
                    if (wand.CasterLevel< caster.GetSpellbook(wizardclass).CasterLevel)
                    wand.CasterLevel = caster.GetSpellbook(wizardclass).CasterLevel;
                    if(wand.DC < 10 + spell.SpellLevel + caster.Stats.Intelligence.Bonus)
                    wand.DC = 10 + spell.SpellLevel + caster.Stats.Intelligence.Bonus;
                }
            }


            public void OnEventDidTrigger(RuleCastSpell evt)
            {

            }


        }
    }
}

