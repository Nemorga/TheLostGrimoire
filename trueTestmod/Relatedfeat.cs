// Copyright (c) 2019 Jennifer Messerly
// This code is licensed under MIT license (see LICENSE for details)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
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
    static class RelatedFeat
    {
        static LibraryScriptableObject library => Main.library;



        internal static void Load()
        {
            // Load  feats
            Main.SafeLoad(CreateOlderFeat, "More aged hero");




        }


        static void CreateOlderFeat()
        {
            var icon1 = Helpers.GetIcon("82fbb68796a4e6d4a8b79cf3f14600b7");
            var icon2 = Helpers.GetIcon("82fbb68796a4e6d4a8b79cf3f14600b7");
            var icon3 = Helpers.GetIcon("82fbb68796a4e6d4a8b79cf3f14600b7");
            var icon4 = Helpers.GetIcon("82fbb68796a4e6d4a8b79cf3f14600b7");

            var MiddleAgeMalus = Helpers.CreateFeature("MiddleAgeMalusFeature", "Old Age", "", Helpers.getGuid("MiddleAgeMalusFeature"), null, FeatureGroup.None,
                Helpers.CreateAddStatBonus(StatType.Strength, -1, ModifierDescriptor.Racial),
                Helpers.CreateAddStatBonus(StatType.Dexterity, -1, ModifierDescriptor.Racial),
                Helpers.CreateAddStatBonus(StatType.Constitution, -1, ModifierDescriptor.Racial)
                );
            MiddleAgeMalus.HideInUI = true;

            var OldAgeMalus = Helpers.CreateFeature("OldAgeMalusFeature", "Old Age", "", Helpers.getGuid("OldAgeMalusFeature"), null, FeatureGroup.None,
                Helpers.CreateAddStatBonus(StatType.Strength, -3, ModifierDescriptor.Racial),
                Helpers.CreateAddStatBonus(StatType.Dexterity, -3, ModifierDescriptor.Racial),
                Helpers.CreateAddStatBonus(StatType.Constitution, -3, ModifierDescriptor.Racial)
                );
            OldAgeMalus.HideInUI = true;

            var VenerableAgeMalus = Helpers.CreateFeature("VenerableAgeMalusFeature", "Old Age", "", Helpers.getGuid("VenerableAgeMalusFeature"), null, FeatureGroup.None,
                Helpers.CreateAddStatBonus(StatType.Strength, -6, ModifierDescriptor.Racial),
                Helpers.CreateAddStatBonus(StatType.Dexterity, -6, ModifierDescriptor.Racial),
                Helpers.CreateAddStatBonus(StatType.Constitution, -6, ModifierDescriptor.Racial)
                );
            VenerableAgeMalus.HideInUI = true;

            var MiddleAgeBonus = Helpers.CreateFeature("MiddleAgeBonusFeature", "Old Age", "", Helpers.getGuid("MiddleAgeBonusFeature"), null, FeatureGroup.None,
                Helpers.CreateAddStatBonus(StatType.Intelligence, 1, ModifierDescriptor.Racial),
                Helpers.CreateAddStatBonus(StatType.Wisdom, 1, ModifierDescriptor.Racial),
                Helpers.CreateAddStatBonus(StatType.Charisma, 1, ModifierDescriptor.Racial)
                );
            MiddleAgeBonus.HideInUI = true;

            var OldAgeBonus = Helpers.CreateFeature("OldAgeBonusFeature", "Old Age", "", Helpers.getGuid("OldAgeBonusFeature"), null, FeatureGroup.None,
                Helpers.CreateAddStatBonus(StatType.Intelligence, 2, ModifierDescriptor.Racial),
                Helpers.CreateAddStatBonus(StatType.Wisdom, 2, ModifierDescriptor.Racial),
                Helpers.CreateAddStatBonus(StatType.Charisma, 2, ModifierDescriptor.Racial)
                );
            OldAgeBonus.HideInUI = true;

            var VenerableAgeBonus = Helpers.CreateFeature("VenerableAgeBonusFeature", "Old Age", "", Helpers.getGuid("VenerableAgeBonusFeature"), null, FeatureGroup.None,
                Helpers.CreateAddStatBonus(StatType.Intelligence, 3, ModifierDescriptor.Racial),
                Helpers.CreateAddStatBonus(StatType.Wisdom, 3, ModifierDescriptor.Racial),
                Helpers.CreateAddStatBonus(StatType.Charisma, 3, ModifierDescriptor.Racial)
                );
            VenerableAgeBonus.HideInUI = true;


            var MiddleAgeFeature = Helpers.CreateFeature("MiddleAgeFeature", "Middle Age", "You are older than the majority of heroes and have reached middle age. You suffer from an inherent penalty of -1 to your Strength, Dexterity and Constitution score, while you also benefit from an inherent bonus of +1 to your Inteligence, Wisdom and Charisma score.",
                Helpers.getGuid("MiddleAgeFeature"), icon1, FeatureGroup.Feat, 
                Helpers.Create<AddFeatureOnApply>(f => f.Feature = MiddleAgeBonus),
                Helpers.Create<AddFeatureOnApply>(f => f.Feature = MiddleAgeMalus)
                );
            var OldAgeFeature = Helpers.CreateFeature("OldAgeFeature", "Old Age", "You are older than the majority of heroes and have reached old age. You suffer from an inherent penalty of -3 to your Strength, Dexterity and Constitution score, while you also benefit from an inherent bonus of +2 to your Inteligence, Wisdom and Charisma score.",
                Helpers.getGuid("OldAgeFeature"), icon2, FeatureGroup.Feat,
                Helpers.Create<AddFeatureOnApply>(f => f.Feature = OldAgeBonus),
                Helpers.Create<AddFeatureOnApply>(f => f.Feature = OldAgeMalus)
                );
            var VenerableAgeFeature = Helpers.CreateFeature("VenerableAgeFeature", "Venerable", "You are far older than the majority of heroes and have reached a venerable age. You suffer from an inherent penalty of -6 to your Strength, Dexterity and Constitution score, while you also benefit from an inherent bonus of +3 to your Inteligence, Wisdom and Charisma score.",
                Helpers.getGuid("VenerableAgeFeature"), icon3, FeatureGroup.Feat,
                Helpers.Create<AddFeatureOnApply>(f => f.Feature = VenerableAgeBonus),
                Helpers.Create<AddFeatureOnApply>(f => f.Feature = VenerableAgeMalus)
                );

            var MoreAgedFeatureSelection = Helpers.CreateFeatureSelection("MoreAgedFeatureSelection", "Older Hero", "You are older than most heroes and it impacts your base abilities score. Choose one of the following option: Middle Age, Old Age or Venerable.",
                Helpers.getGuid("MoreAgedFeatureSelection"), icon4, FeatureGroup.Feat,
                Helpers.PrerequisiteNoFeature(VenerableAgeFeature, true),
                Helpers.PrerequisiteNoFeature(OldAgeFeature, true),
                Helpers.PrerequisiteNoFeature(MiddleAgeFeature, true),
                Helpers.Create<PrerequisiteNoMoreCharacterLevel>(p => p.Level = 1)
                
                );
            MoreAgedFeatureSelection.SetFeatures(MiddleAgeFeature, OldAgeFeature, VenerableAgeFeature);

            library.AddFeats(MoreAgedFeatureSelection);
        }

    }

    public class PrerequisiteNoMoreCharacterLevel : Prerequisite
    {

        public override bool Check(FeatureSelectionState selectionState, UnitDescriptor unit, LevelUpState state)
        {
            return unit.Progression.CharacterLevel <= this.Level;
        }


        public override string GetUIText()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(string.Format("{0}: {1}", UIStrings.Instance.Tooltips.CharacterLevel, "Only at "+ this.Level+"st level"));
            return stringBuilder.ToString();
        }


        public int Level;

    }

}

