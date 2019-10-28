using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Recommendations;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.ElementsSystem;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.Items;
using Kingmaker;
using Kingmaker.GameModes;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.ResourceLinks;
using Kingmaker.UnitLogic.Class.LevelUp.Actions;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.Abilities.Components.Base;
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
using UnityEngine;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;
namespace thelostgrimoire
{
    class Archetype
    {
        static LibraryScriptableObject library => Main.library;
        static BlueprintCharacterClass wizardclass = Main.library.Get<BlueprintCharacterClass>("ba34257984f4c41408ce1dc2004e342e");
        static BlueprintSpellList wizardlist = Main.library.Get<BlueprintSpellList>("ba0401fdeb4062f40a7aa95b6f07fe89");
        static BlueprintArchetype thassilonian = Main.library.Get<BlueprintArchetype>("55a8ce15e30d71547a44c69bb2e8a84f");
        internal static void Load()
        {
            // Load  feats
            Main.SafeLoad(CreateSpellBinder, "Mage Domain");


            //needed patch

        }

        static void CreateSpellBinder()
        {
            //needed thing
            var icon = Helpers.GetIcon("2fb5e65bd57caa943b45ee32d825e9b9");
            var Arcanebondselection = library.Get<BlueprintFeatureSelection>("03a1781486ba98043afddaabf6b7d8ff");
            string name = "SpellBinderFalseArchetype";
            string Name = "Spellbinder : Spell Bond";
            var spellspecialisation = library.Get<BlueprintParametrizedFeature>("f327a765a4353d04f872482ef3e48c35");
            var levlspellX = spellspecialisation.BlueprintParameterVariants;//Spell list from spellspecialisation

            //Starting progression
            var SpellBinderProgression = Helpers.CreateProgression(name + "Progression", "", "", Helpers.getGuid(name + "Progression"), icon, FeatureGroup.WizardFeat);
            SpellBinderProgression.IsClassFeature = true;
            SpellBinderProgression.Classes = SpellBinderProgression.Classes.AddToArray(wizardclass);
            List<LevelEntry> addFeatures = new List<LevelEntry>();

            //Starting Bond Selection
            var BondSelection = Helpers.CreateFeatureSelection(name + "Selection", Name, "PlaceHolder", Helpers.getGuid(name + "Selection"), icon, FeatureGroup.WizardFeat, Helpers.Create<PrerequisiteNoArchetype>(n => { n.CharacterClass = wizardclass; n.Archetype = thassilonian; }));

            //Initialise Array of Parametrized feature
            BlueprintParametrizedFeature[] spellbondwiz = new BlueprintParametrizedFeature[]{null, null, null, null, null, null, null, null, null };
                        
            //Loop creating parametrized feature
            for (int i = 1; i <10; i++)
            {
                
                //Creating parametrized feature
                spellbondwiz[i-1] = Helpers.CreateParametrizedFeature(name+i, Name, "Place Holder", Helpers.getGuid(name+i), icon, FeatureGroup.WizardFeat, FeatureParameterType.LearnSpell,
                Helpers.PrerequisiteFeature(Helpers.elf),
                Helpers.PrerequisiteClassLevel(wizardclass, i == 1? 1: (i*2-1) ),
                Helpers.Create<SpontaneousCastingParametrized>(l => l.SpellLevel = i));

                // For spell level 1
                if (i ==1)
                {
                    var nothass = Helpers.Create<PrerequisiteNoArchetype>(n => { n.CharacterClass = wizardclass; n.Archetype = thassilonian; });
                    var progcompo = Helpers.Create<AddFeatureOnApply>(f => f.Feature = SpellBinderProgression);
                    spellbondwiz[0].AddComponents(progcompo, nothass);
                }
                // Adding no self component
                var noself = Helpers.PrerequisiteNoFeature(spellbondwiz[i - 1]);
                spellbondwiz[i - 1].AddComponent(noself);
                // Seting option right
                spellbondwiz[i - 1].BlueprintParameterVariants = levlspellX;// Spell list = Spellspecialisation
                spellbondwiz[i - 1].IsClassFeature = true;
                spellbondwiz[i - 1].Ranks = 1;
                spellbondwiz[i - 1].SpecificSpellLevel = true;
                spellbondwiz[i - 1].SpellLevel = i;
                spellbondwiz[i - 1].SpellcasterClass = wizardclass;
                spellbondwiz[i - 1].SpellList = wizardlist;
                spellbondwiz[i - 1].HideNotAvailibleInUI = true;

                //CReating level entry for progression
                addFeatures.Add(Helpers.LevelEntry(i == 1 ? 1 : (i * 2 - 1), BondSelection));

            }
            
            //Adding the level entry
            SpellBinderProgression.LevelEntries = SpellBinderProgression.LevelEntries.AddToArray(addFeatures);


           
            //
            BondSelection.Features = BondSelection.Features.AddRangeToArray(spellbondwiz);
            BondSelection.AllFeatures = BondSelection.AllFeatures.AddRangeToArray(spellbondwiz);
            BondSelection.HideNotAvailibleInUI = true;

            Arcanebondselection.Features = Arcanebondselection.Features.AddToArray(spellbondwiz[0]);
            Arcanebondselection.AllFeatures = Arcanebondselection.AllFeatures.AddToArray(spellbondwiz[0]);
        }
     

        public class SpontaneousCastingParametrized : ParametrizedFeatureComponent
        {
            



            public override void OnFactActivate()
            {
                Log.Write("Activate");
                BlueprintAbility[] SpellSpontaneous = { null, null, null, null, null, null, null, null, null, null };

                BlueprintAbility blueprintAbility = (!(base.Param != null)) ? null : (base.Param.Value.Blueprint as BlueprintAbility);
                
                if(blueprintAbility.HasVariants)
                {
                    var Hasvariant = blueprintAbility.Variants;
                    foreach(BlueprintAbility variant in Hasvariant)
                    {
                        
                        var variantcopy = library.TryGet<BlueprintAbility>(Helpers.getGuid("SpellBinder" + variant.name + base.Owner.Unit.UniqueId)); //copy exist already ?
                        if(variantcopy == null)
                        {
                            var variantcopyfirst = library.CopyAndAdd<BlueprintAbility>(variant.AssetGuid, "SpellBinder" + variant.name + base.Owner.Unit.UniqueId, Helpers.getGuid("SpellBinder" + variant.name + base.Owner.Unit.UniqueId));
                            variantcopyfirst.ActionType = CommandType.Standard;
                            Helpers.SetField(variantcopyfirst, "m_IsFullRoundAction", true);
                            variantcopy = variantcopyfirst;
                        }
                        BlueprintAbility[] SpellSpontaneousmany = { null, null, null, null, null, null, null, null, null, null };
                        SpellSpontaneousmany[SpellLevel] = variantcopy;
                        Owner.DemandSpellbook(wizardclass).AddSpellConversionList(SpellSpontaneousmany);
                    }

                }
                else
                {
                    var copy = library.TryGet<BlueprintAbility>(Helpers.getGuid("SpellBinder" + blueprintAbility.name + base.Owner.Unit.UniqueId));
                    
                    if (copy == null)
                    {
                        var copyfirst = library.CopyAndAdd<BlueprintAbility>(blueprintAbility.AssetGuid, "SpellBinder" + blueprintAbility.name + base.Owner.Unit.UniqueId, Helpers.getGuid("SpellBinder" + blueprintAbility.name + base.Owner.Unit.UniqueId));
                        copyfirst.ActionType = CommandType.Standard;
                        Helpers.SetField(copyfirst, "m_IsFullRoundAction", true);
                        copy = copyfirst;
                    }
                    SpellSpontaneous[SpellLevel] = copy;
                    Owner.DemandSpellbook(wizardclass).AddSpellConversionList(SpellSpontaneous);
                }

                
                if (!Owner.DemandSpellbook(wizardclass).IsKnown(blueprintAbility))
                {
                    
                    Owner.DemandSpellbook(wizardclass).AddKnown(SpellLevel, blueprintAbility);
                    
                }
            }
            public override void OnFactDeactivate()
            {
                Log.Write("Deactivate");
                
            }




            public int SpellLevel;

        }

    

    }
}
