

using System;
using System.Text;
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
    static class ArcaneDiscoveries
    {
        static LibraryScriptableObject library => Main.library;
        static BlueprintCharacterClass wizardclass = Main.library.Get<BlueprintCharacterClass>("ba34257984f4c41408ce1dc2004e342e");

        
        internal static void Load()
        {
            // Load  feats
            Main.SafeLoad(CreateImmortality, "Who wants to live for ever?");
            Main.SafeLoad(CreateStaffLikeWand, "Laser pistols, kinda");
            Main.SafeLoad(CreateAlchemicalAffinity, "Wizalchimist");
            Main.SafeLoad(CreateForestBlessing, "Wizardruid");
            Main.SafeLoad(CreateFeralSpeach, "animal noise");
            Main.SafeLoad(CreateResilientIllusions, "Shadows and Light");
            Main.SafeLoad(CreateIdealize, "I am perfect");
            Main.SafeLoad(CreateKnowledgeIsPower, "Quill thrower");
            Main.SafeLoad(CreateOppositionResearch, "Mouhahaha");
            Main.SafeLoad(CreateStewardBeyond, "watching like a watcher");
            Main.SafeLoad(CreateSplitSLot, "Yo!");
            
            //needed patch
            Main.ApplyPatch(typeof(UseCasterLevelWithwand), "enlarge your wand");
            Main.ApplyPatch(typeof(ManageSpellPerDayForSplitSlot), "enlarge your spell slot");
          
        }

        static void CreateSplitSLot()//This method is a complete mess but it works. I'm lazy so I'm not cleaning now, if you're lucky I'll comment it.
        {
            string name = "SplitSlot";
            string Name = "Split Slot";
            string desc = "When you prepare spells you may treat any one of your open spell slots as if it were two spell slots that were two spell levels lower. For all purposes, the two lower-level slots are treated as that lower level.\n";
            string longdesc = "Once per day when you prepare spells, you may treat any one of your open spell slots as if it were two spell slots that were two spell levels lower. For example, a 9th-level Wizard can split a 5th-level slot into two 3rd-level slots, preparing fireball and lightning bolt in those 3rd-level slots. For all purposes, the two lower-level slots are treated as that lower level (so the split 5th-level slot used for a fireball has a DC as if it were in a normal 3rd-level slot). This discovery has no effect on cantrips or 1st and 2nd–level spells. ";
            Sprite[] icons = new Sprite[] {
            Helpers.GetIcon("92681f181b507b34ea87018e8f7a528a"),    
            Helpers.GetIcon("e5dcf71e02e08fc448d9745653845df1"),
            Helpers.GetIcon("c9165b50a298ba346bd1b34f8d9cb9f9"),
            Helpers.GetIcon("5e6fe442c2b4be24f969505cef85f61b"),
            Helpers.GetIcon("541bb8d595532ec419343b7a93cdb449"),
            Helpers.GetIcon("0933849149cfc9244ac05d6a5b57fd80"),
            Helpers.GetIcon("d7d18ce5c24bd324d96173fdc3309646")
            };
            BlueprintBuff[] buffs = new BlueprintBuff[7];
            BlueprintAbility[] abilities = new BlueprintAbility[7];
            BlueprintFeature[] features = new BlueprintFeature[7];
            ContextAction[] RemoveBuff = new ContextAction[7];
            
            StringBuilder[] buildedstring = new StringBuilder[7];
            var Globaleresource = Helpers.CreateAbilityResource(name + "resource", Name + " resource", "", Helpers.getGuid(name + "resource"), icons[0]);
            Globaleresource.SetFixedResource(1);
            var addresource = Helpers.CreateAddAbilityResource(Globaleresource);
            Sprite icon;
            var fx = Helpers.GetFx("c4d861e816edd6f4eab73c55a18fdadd");
            for (int i = 0; i < 7; i++)
            {
                icon = icons[i];
                buildedstring[i] = new StringBuilder();
                buildedstring[i].Append(string.Format("This ability split a {0}th level slot, loosing it for the day, and create two {1}th level spells.", i + 3, i + 1));
                

                buffs[i] = Helpers.CreateBuff(name + "buff" + i, Name, "", Helpers.getGuid(name + "buff" + i), icon, fx, fx,
                    Helpers.Create<ReinitializeSlot>(s => { s.SplitedSlot = i + 3; s.AddedSlot = i + 1; })
                    );
                buffs[i].SetBuffFlags(BuffFlags.StayOnDeath);
                int num = (i + 3);
                abilities[i] = Helpers.CreateAbility(name +"ability"+i , Name + ": Spell Level " +num, desc + buildedstring[i], Helpers.getGuid(name + "ability" + i), icon, AbilityType.Special, CommandType.Free, AbilityRange.Personal, "", "",
                    
                    Helpers.CreateResourceLogic(Globaleresource, true)
                    );
                abilities[i].ResourceAssetIds = new string[] { fx.AssetId };
                features[i] = Helpers.CreateFeature(name + "Feature" + i, Name, desc, Helpers.getGuid(name + "Feature" + i), icon, FeatureGroup.WizardFeat, abilities[i].CreateAddFact());
                features[i].HideInCharacterSheetAndLevelUp = true;
                features[i].HideInUI = true;
                
                
                RemoveBuff[i] = Helpers.Create<ContextActionRemoveBuff>(r => { r.Buff = buffs[i]; r.ToCaster = true; });
                


            }
            
            for (int i = 0; i < 7; i++)
            {
                GameAction[] abilitiesactions = new GameAction[] {Helpers.CreateApplyBuff(buffs[i], Helpers.CreateContextDuration(1, DurationRate.Days), false, false, true, false, true)};
                foreach (ContextAction action in RemoveBuff)
                {
                    if (i != RemoveBuff.IndexOf(action))
                    {
                        abilitiesactions = abilitiesactions.AddToArray(action);
                    }


                }
                abilities[i].AddComponent(Helpers.CreateRunActions(abilitiesactions));
                abilities[i].AddComponent(Helpers.Create<AbilityCasterHasSlot>(s => s.Slot = i + 3));
            }



            var removeresource = Helpers.CreateAbilityResource(name + "removeresource", "", "", Helpers.getGuid(name + "removeresource"), icons[0]);
            removeresource.SetFixedResource(1);
            var removeresourcelogic = Helpers.CreateResourceLogic(removeresource);
            var addremoveresource = Helpers.CreateAddAbilityResource(removeresource);
            addremoveresource.RestoreAmount = true;

            RemoveBuff = RemoveBuff.AddToArray(Helpers.Create<RestoreOtherResource>(c => { c.Resource = Globaleresource; }));
            var removeability = Helpers.CreateAbility(name + "removeABility", "Unsplit Slot", "Immeditly restore your spell slot to their normal state.", Helpers.getGuid(name + "removeABility"), icons[6], AbilityType.Special, CommandType.Free, AbilityRange.Personal,
                "", "",
                Helpers.CreateRunActions(RemoveBuff),
                removeresourcelogic
                
                );
            removeability.CanTargetSelf = true;

            var feat = Helpers.CreateFeature(name + "Feat", "Arcane Discovery: "+Name, longdesc, Helpers.getGuid(name + "Feat"), icons[6], FeatureGroup.WizardFeat,
                removeability.CreateAddFact(), 
                Helpers.PrerequisiteClassLevel(wizardclass, 5),
                addresource,
                addremoveresource
                );
            feat.Groups = feat.Groups.AddToArray(FeatureGroup.Feat);

            for(int i = 0; i< 7; i++)
            {
                

                feat.AddComponent(Helpers.Create<AddFeatureOnClassLevel>(c =>
                {
                    c.BeforeThisLevel = false;
                    c.Class = wizardclass;
                    c.Level = ((i + 3) * 2) - 1;
                    c.Feature = features[i];

                }
                ));
            }

            library.AddFeats(feat);
            library.AddFeats("8c3102c2ff3b69444b139a98521a4899", feat);

        }

        public class AbilityCasterHasSlot: BlueprintComponent, IAbilityCasterChecker
        {
            public bool CorrectCaster(UnitEntityData caster)
            {
                return caster.Descriptor.DemandSpellbook(wizardclass).GetSpellsPerDay(Slot) > 0;
            }

            public string GetReason()
            {
                return LocalizedTexts.Instance.Reasons.OutOfSpellsPerDay;
            }

           public int Slot; 

        }

        public class RestoreOtherResource: ContextAction
        {
            public override string GetCaption()
            {
                return "restore resource";
            }
            public override void RunAction()
            {
                UnitEntityData maybeCaster = base.Context.MaybeCaster;
                if (maybeCaster == null)
                {
                    UberDebug.LogError("Caster is missing", Array.Empty<object>());
                    return;
                }
                maybeCaster.Descriptor.Resources.Restore(Resource, amount);
            }
            public BlueprintAbilityResource Resource;
            public int amount = 1;

        }
        public class ReinitializeSlot : BuffLogic
        {
            public override void OnFactActivate()
            {
                base.Owner.DemandSpellbook(wizardclass).CalcSlotsLimit(AddedSlot, SpellSlotType.Common);
                base.Owner.DemandSpellbook(wizardclass).CalcSlotsLimit(SplitedSlot, SpellSlotType.Common);
            }

            public override void OnFactDeactivate()
            {
                base.Owner.DemandSpellbook(wizardclass).CalcSlotsLimit(AddedSlot, SpellSlotType.Common);
                base.Owner.DemandSpellbook(wizardclass).CalcSlotsLimit(SplitedSlot, SpellSlotType.Common);
            }

            public int SplitedSlot;
            public int AddedSlot;

        }



        static void CreateImmortality()
        {
            var middleage = library.Get<BlueprintFeature>(Helpers.getGuid("MiddleAgeMalusFeature"));
            var oldage = library.Get<BlueprintFeature>(Helpers.getGuid("OldAgeMalusFeature"));
            var venerableage = library.Get<BlueprintFeature>(Helpers.getGuid("VenerableAgeMalusFeature"));
            var icon = Helpers.GetIcon("80a1a388ee938aa4e90d427ce9a7a3e9");

            var immortality = Helpers.CreateFeature("ImmortalityArcaneDiscovery", "Arcane Discovery: Immortality", " You discover a cure for aging, and from this point forward you take no penalty to your physical ability scores from advanced age. If you are already taking such penalties, they are removed at this time. ",
                Helpers.getGuid("ImmortalityArcaneDiscovery"), icon, FeatureGroup.WizardFeat,
                Helpers.PrerequisiteClassLevel(wizardclass, 20),
                Helpers.Create<RemoveFeatureOnApply>(r => r.Feature = middleage),
                Helpers.Create<RemoveFeatureOnApply>(r => r.Feature = oldage),
                Helpers.Create<RemoveFeatureOnApply>(r => r.Feature = venerableage)
                );
            immortality.Groups = immortality.Groups.AddToArray(FeatureGroup.Feat);
            library.AddFeats(immortality);
            library.AddFeats("8c3102c2ff3b69444b139a98521a4899", immortality);




        }

        static void CreateStaffLikeWand()
        {
           

            var prereq = library.TryGet<BlueprintFeature>("46fad72f54a33dc4692d3b62eca7bb78#CraftMagicItems(feat=wand)");
            if (prereq == null)
                prereq = library.Get<BlueprintFeature>("f43ffc8e3f8ad8a43be2d44ad6e27914");//skilfocus umd
            var wandmastery = Helpers.CreateAddMechanics(AddMechanicsFeature.MechanicsFeatureType.WandMastery);



            var icon = Helpers.GetIcon("1b94043744e83494c8a083319e1602f3"); //wandmastery
            var StaffLikeWandFeature = Helpers.CreateFeature("StaffLikeWandArcaneDiscovery", "Arcane Discovery : Staff-Like Wand", "Similar to using a magic staff, you use your own Intelligence score and relevant feats to set the DC for saves against spells you cast from a wand, and you can use your caster level when activating the power of a wand if it’s higher than the caster level of the wand.",
                Helpers.getGuid("StaffLikeWandArcaneDiscovery"), icon, FeatureGroup.WizardFeat,
                Helpers.PrerequisiteFeature(prereq),
                Helpers.PrerequisiteClassLevel(wizardclass, 11),
                wandmastery
                
                );

            StaffLikeWandFeature.Groups = StaffLikeWandFeature.Groups.AddToArray(FeatureGroup.Feat);
            library.AddFeats(StaffLikeWandFeature);
            library.AddFeats("8c3102c2ff3b69444b139a98521a4899", StaffLikeWandFeature);
        }

        static void CreateAlchemicalAffinity()
        {

            

            var icon = Helpers.GetIcon("24afb2c948c731440a3aaf5411904c89"); //Targeted bomb admixture
            var AlchemicalAffinityFeature = Helpers.CreateFeature("AlchemicalAffinityFeature", "Arcane Discovery : Alchemical Affinity", "Whenever you cast a spell that appears on both the wizard and alchemist spell lists, you treat your caster level as 1 higher than normal and the save DC of such spells increases by 1. ",
                Helpers.getGuid("AlchemicalAffinityFeature"), icon, FeatureGroup.WizardFeat,
                Helpers.PrerequisiteClassLevel(wizardclass, 5),
                Helpers.Create<OtherListAffinityLogic>(l => l.otherlist = Helpers.alchemistSpellList)

                );

            AlchemicalAffinityFeature.Groups = AlchemicalAffinityFeature.Groups.AddToArray(FeatureGroup.Feat);
            library.AddFeats(AlchemicalAffinityFeature);
            library.AddFeats("8c3102c2ff3b69444b139a98521a4899", AlchemicalAffinityFeature);
        }

        static void CreateForestBlessing()
        {

            var icon = Helpers.GetIcon("0fd00984a2c0e0a429cf1a911b4ec5ca"); //entangle
            var ForestBlessingFeature = Helpers.CreateFeature("ForestBlessingFeature", "Arcane Discovery : Forest’s Blessing", "You cast any spells that appear on both the wizard and druid spell lists at +1 caster level and with +1 to the save DC. ",
                Helpers.getGuid("ForestBlessingFeature"), icon, FeatureGroup.WizardFeat,
                Helpers.PrerequisiteClassLevel(wizardclass, 5),
                Helpers.Create<OtherListAffinityLogic>(l => l.otherlist = Helpers.druidSpellList)

                );

            ForestBlessingFeature.Groups = ForestBlessingFeature.Groups.AddToArray(FeatureGroup.Feat);
            library.AddFeats(ForestBlessingFeature);
            library.AddFeats("8c3102c2ff3b69444b139a98521a4899", ForestBlessingFeature);
        }

        static void CreateFeralSpeach()
        {
            var icon = Helpers.GetIcon("08df458bd00ba704dab32dd493c61518");
            var animaltype = Main.library.Get<BlueprintFeature>("a95311b3dc996964cbaa30ff9965aaf6");
            var verminetype = Main.library.Get<BlueprintFeature>("09478937695300944a179530664e42ec");
            var fx = new PrefabLink();
            fx.AssetId = "09f795c3900b21b47a1254bcb3f263c8";
            var olddominatebuff = Main.library.Get<BlueprintBuff>("c0f4e1c24c9cd334ca988ed1bd9d201f");
            var dominatebuff = Main.library.CopyAndAdd<BlueprintBuff>("c0f4e1c24c9cd334ca988ed1bd9d201f", "BecomeAllyBuff", Helpers.getGuid("BecomeAllyBuff"));
            var charmbuff = Main.library.Get<BlueprintBuff>("9dc29118addce3d48ae9b92be953b5b4");

            dominatebuff.ComponentsArray = Array.Empty<BlueprintComponent>();
            dominatebuff.AddComponent(olddominatebuff.GetComponent<ChangeFaction>());
            dominatebuff.SetBuffFlags(BuffFlags.StayOnDeath);
            

            var immunebuff = Helpers.CreateBuff("feralspeechimmunebuff", "", "", Helpers.getGuid("feralspeachimmunebuff"), icon,null,null);
            immunebuff.SetBuffFlags(BuffFlags.HiddenInUi | BuffFlags.StayOnDeath);
           

            var ability = Helpers.CreateAbility("FeralSpeechPersuadeAbility", "Persuade Animal", "As a stantard Action you can try to persuade an animal to make him friendlier to you. You must succeed at a persuasion" +
                "skill check (DC = 15 + Target Level + Target Wisdom Modifier + 15 if it is an animal companion). If you succeed by more 10 the animal is dominated by you, if you simply succeed the animal is charmed by you." +
                "Weither you succeed or fail, the creature is immune to this ability for a day.",
                Helpers.getGuid("FeralSpeechPersuadeAbility"),
                icon,
                AbilityType.Supernatural,
                CommandType.Standard,
                AbilityRange.Medium,
                "",
                "",
                Helpers.CreateRunActions(Helpers.CreateConditional(Helpers.CreateConditionsCheckerAnd(Helpers.Create<ContextConditionIsEnemy>(), Helpers.Create<ContextConditionIsInCombat>()),
                //if true:
                Helpers.Create<Persuade>(b => { b.Buff = charmbuff; b.GreaterBuff = dominatebuff; })
                ), 
                Helpers.CreateApplyBuff(immunebuff, Helpers.CreateContextDuration(1, DurationRate.Days), false, false, false, false, false)
                ),
                Helpers.Create<AbilitySpawnFx>(f => { f.PrefabLink = fx; f.Anchor = AbilitySpawnFxAnchor.ClickedTarget;}),
                Helpers.Create<AbilityTargetHasConditionOrBuff>(b => { b.Buffs = new BlueprintBuff[] { immunebuff, charmbuff, dominatebuff }; b.Not = true; }),
                Helpers.Create<AbilityTargetNotSelf>(),
                Helpers.Create<AbilityTargetIsPartyMember>(p => p.Not = true),
                Helpers.Create<AbilityTargetHasFact>(f => f.CheckedFacts = new BlueprintUnitFact[] {animaltype} )
                );
            ability.CanTargetEnemies = true;
            ability.CanTargetFriends = false;
            ability.CanTargetPoint = false;
            ability.CanTargetSelf = false;
            ability.EffectOnEnemy = AbilityEffectOnUnit.Harmful;
            ability.Animation = Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle.Directional;
            ability.ResourceAssetIds = new string[] { fx.AssetId };

            var abilityupgrade = Helpers.CreateAbility("FeralSpeechPersuadeUpgradeAbility", "Persuade Animal & Vermine", "As a stantard Action you can try to persuade an animal or a vermine to make it friendlier to you. You must succeed at a persuasion" +
                "skill check (DC = 15 + Target Level + Target Wisdom Modifier + 15 if it is an animal companion). If you succeed by more 10 the creature is dominated by you, if you simply succeed the creature is charmed by you." +
                "Weither you succeed or fail, the creature is immune to this ability for a day.",
                Helpers.getGuid("FeralSpeechPersuadeUpgradeAbility"),
                icon,
                AbilityType.Supernatural,
                CommandType.Standard,
                AbilityRange.Medium,
                "",
                "",
                Helpers.CreateRunActions(Helpers.CreateConditional(Helpers.CreateConditionsCheckerAnd(Helpers.Create<ContextConditionIsEnemy>(), Helpers.Create<ContextConditionIsInCombat>()),
                //if true:
                Helpers.Create<Persuade>(b => { b.Buff = charmbuff; b.GreaterBuff = dominatebuff; })
                ),
                Helpers.CreateApplyBuff(immunebuff, Helpers.CreateContextDuration(1, DurationRate.Days), false, false, false, false, false)
                ),
                Helpers.Create<AbilitySpawnFx>(f => { f.PrefabLink = fx; f.Anchor = AbilitySpawnFxAnchor.ClickedTarget; }),
                Helpers.Create<AbilityTargetHasConditionOrBuff>(b => { b.Buffs = new BlueprintBuff[] { immunebuff, charmbuff, dominatebuff }; b.Not = true; }),
                Helpers.Create<AbilityTargetNotSelf>(),
                Helpers.Create<AbilityTargetIsPartyMember>(p => p.Not = true),
                Helpers.Create<AbilityTargetHasFact>(f => f.CheckedFacts = new BlueprintUnitFact[] { animaltype, verminetype })
                );
            abilityupgrade.CanTargetEnemies = true;
            abilityupgrade.CanTargetFriends = false;
            abilityupgrade.CanTargetPoint = false;
            abilityupgrade.CanTargetSelf = false;
            abilityupgrade.EffectOnEnemy = AbilityEffectOnUnit.Harmful;
            abilityupgrade.Animation = Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle.Directional;
            abilityupgrade.ResourceAssetIds = new string[] { fx.AssetId };

            var upgrade = Helpers.CreateFeature("FeralSpeachUpgradeFeature", "Arcane Discovery : Feral Speech", " You gain the supernatural ability to speak with and understand the response of any animal as if using speak with animals, this allow you to make persuasion check to persuade animal to work for you. You can make yourself understood as far as your voice carries. When you reach 12th level, you can also use this ability to communicate with vermin.",
                Helpers.getGuid("FeralSpeechUpgradeFeature"), icon, FeatureGroup.WizardFeat,
                abilityupgrade.CreateAddFact()
                );
            upgrade.Groups = upgrade.Groups.AddToArray(FeatureGroup.Feat);
            var feat = Helpers.CreateFeature("FeralSpeachFeature", "Arcane Discovery : Feral Speech", " You gain the supernatural ability to speak with and understand the response of any animal as if using speak with animals, this allow you to make persuasion check to persuade animal to work for you. You can make yourself understood as far as your voice carries. When you reach 12th level, you can also use this ability to communicate with vermin.", 
                Helpers.getGuid("FeralSpeechFeature"), icon, FeatureGroup.WizardFeat,
                Helpers.PrerequisiteClassLevel(wizardclass, 5),
                ability.CreateAddFact(),
                Helpers.PrerequisiteNoFeature(upgrade),
                Helpers.Create<AddFeatureOnClassLevel>(c => 
                {
                    c.Class = Helpers.GetClass("ba34257984f4c41408ce1dc2004e342e");
                    c.Level = 12;
                    c.Feature = upgrade;
                })

                );
            feat.Groups = feat.Groups.AddToArray(FeatureGroup.Feat);

            var deletecompo = Helpers.Create<RemoveFeatureOnApply>(r => { r.Feature = feat; });
            upgrade.AddComponent(deletecompo);
            

            library.AddFeats(feat);
            library.AddFeats("8c3102c2ff3b69444b139a98521a4899", feat);

        }

        static void CreateResilientIllusions()
        {
            var icon = Helpers.GetIcon("237427308e48c3341b3d532b9d3a001f");
            var buff = Helpers.CreateBuff("ResilientIllusionBuff", "", "", Helpers.getGuid("ResilientIllusionBuff"), icon, null, null,
                Helpers.Create<ResilentIllusionBuffMechanics>());
            buff.SetBuffFlags(BuffFlags.HiddenInUi);
            
            var feat = Helpers.CreateFeature("ResilentIllusionsFeature", "Arcane Discovery : Resilient Illusions", "Anytime a creature tries to disbelieve one of your illusion effects, make a caster level check. Treat the illusion’s save DC as its normal DC or the result of the caster level check, whichever is higher.",
                Helpers.getGuid("ResilentIllusionsFeature"), icon, FeatureGroup.WizardFeat,
                Helpers.PrerequisiteClassLevel(wizardclass, 5),
                Helpers.Create<ApplyBuffOnSpellSchoolCast>(b => { b.Buff = buff; b.School = SpellSchool.Illusion; b.duration = 1; })
                );
            feat.Groups = feat.Groups.AddToArray(FeatureGroup.Feat);
            library.AddFeats(feat);
            library.AddFeats("8c3102c2ff3b69444b139a98521a4899", feat);

        }

        static void CreateIdealize()
        {
            var icon = Helpers.GetIcon("df2a0ba6b6dcecf429cbb80a56fee5cf");

            var buff = Helpers.CreateBuff("IdealizeBuff", "", "", Helpers.getGuid("IdealizeBuff"), icon, null, null,
                Helpers.Create<IdealizeBuffMechanics>());
            buff.SetBuffFlags(BuffFlags.HiddenInUi);

            var compo = Helpers.Create<AddBuffOnApplyingSpell>( c =>
            {
                var content = new AddBuffOnApplyingSpell.SpellConditionAndBuff()
                {
                    Buff = buff,
                    School = SpellSchool.Transmutation,
                    Duration = Helpers.CreateContextDuration(1, DurationRate.Rounds)
                };
                c.Buffs = new AddBuffOnApplyingSpell.SpellConditionAndBuff[]{content};
                c.OnEffectApplied = false;
            });
            
            var feat = Helpers.CreateFeature("IdealizeFeature", "Arcane Discovery : Idealize", "When a transmutation spell you cast grants an enhancement bonus to an ability score, that bonus increases by 2. At 20th level, the bonus increases by 4.",
                Helpers.getGuid("IdealizeFeature"), icon, FeatureGroup.WizardFeat,
                Helpers.PrerequisiteClassLevel(wizardclass, 10),
                compo,
                Helpers.Create<ApplyBuffOnSpellSchoolCast>(a => { a.Buff = buff; a.School = SpellSchool.Transmutation; a.duration = 1; }),
                Helpers.Create<BuffFixerForParty>()
                );
            feat.Groups = feat.Groups.AddToArray(FeatureGroup.Feat);
            library.AddFeats(feat);
            library.AddFeats("8c3102c2ff3b69444b139a98521a4899", feat);
        }

        static void CreateOppositionResearch()
        {
            var oppositionschoolselection = library.Get<BlueprintFeatureSelection>("6c29030e9fea36949877c43a6f94ff31");
            var oppositionlist = oppositionschoolselection.AllFeatures;
            var icon = Helpers.GetIcon("68a23a419b330de45b4c3789649b5b41");
            BlueprintFeature[] ResearchList = new BlueprintFeature[] { };
            foreach (BlueprintFeature school in oppositionlist)
            {
                SpellSchool spellschool = school.GetComponent<AddOppositionSchool>().School;

                var feat = Helpers.CreateFeature(school.name + "ResearchFeature","Research "+school.Name,school.Description, Helpers.getGuid(school.name + "ResearchFeature"),school.Icon, FeatureGroup.WizardFeat,
                    Helpers.Create<RemoveFeatureOnApply>(f => f.Feature = school),
                    Helpers.Create<RemoveOppositionSchool>(s => s.School = spellschool),
                    Helpers.PrerequisiteFeature(school)
                    );
                ResearchList = ResearchList.AddToArray(feat);
            }
            var noFeature = Helpers.CreateFeature("OppositionResearchNoFeature", "Opposition Research", "", Helpers.getGuid("OppositionResearchNoFeature"), icon, FeatureGroup.WizardFeat);
            noFeature.HideInCharacterSheetAndLevelUp = true;
            noFeature.HideInUI = true;
            var featselection = Helpers.CreateFeatureSelection("OppositionResearchSelection2", "Arcane Discovery : Opposition Research", "Select one Wizard opposition school; preparing spells of this school now only requires one spell slot of the appropriate level instead of two, and you no longer have the –4 Spellcraft penalty for crafting items from that school.\nNOTE: Feat from The Lost Grimoire", Helpers.getGuid("OppositionResearchSelection"), icon, FeatureGroup.WizardFeat,
                                Helpers.PrerequisiteClassLevel(wizardclass, 9),
                                Helpers.PrerequisiteFeaturesFromList(oppositionlist, true),
                                Helpers.Create<PrerequisiteNoFeature>(f => f.Feature = noFeature),
                                Helpers.Create<AddFeatureOnApply>(a => a.Feature = noFeature)
                                );
            featselection.SetFeatures(ResearchList);
            
            library.AddFeats(featselection);
            library.AddFeats("8c3102c2ff3b69444b139a98521a4899", featselection);
        }
        static void CreateKnowledgeIsPower()
        {
            var icon = Helpers.GetIcon("1f01a098d737ec6419aedc4e7ad61fdd");

            var feat = Helpers.CreateFeature("KnowledgeisPowerFeature", "Arcane Discovery : Knowledge is Power", "Your understanding of physical forces gives you power over them. You add your Intelligence modifier on combat maneuver checks and to your CMD. You also add your Intelligence modifier on Strength checks to break or lift objects.",
                Helpers.getGuid("KnowledgeisPowerFeature"), icon, FeatureGroup.WizardFeat,
                Helpers.PrerequisiteClassLevel(wizardclass, 1),
                Helpers.Create<AddContextStatBonus>(s => { s.Stat = StatType.AdditionalCMB; s.Descriptor = ModifierDescriptor.UntypedStackable; s.Multiplier = 1; s.Value = Helpers.CreateContextValue(AbilityRankType.StatBonus); }),
                Helpers.Create<AddContextStatBonus>(s => { s.Stat = StatType.AdditionalCMD; s.Descriptor = ModifierDescriptor.UntypedStackable; s.Multiplier = 1; s.Value = Helpers.CreateContextValue(AbilityRankType.StatBonus); }),                
                Helpers.Create<AbilityScoreCheckBonus>(s => { s.Stat = StatType.Strength; s.Descriptor = ModifierDescriptor.UntypedStackable; s.Bonus = Helpers.CreateContextValue(AbilityRankType.StatBonus); }),
                Helpers.CreateContextRankConfig(ContextRankBaseValueType.StatBonus, ContextRankProgression.AsIs,AbilityRankType.StatBonus, null, null, 0, 0, false, StatType.Intelligence), 
                Helpers.Create<RecalculateOnStatChange>(r => r.Stat = StatType.Intelligence)
                );
            feat.Groups = feat.Groups.AddToArray(FeatureGroup.Feat);
            library.AddFeats(feat);
            library.AddFeats("8c3102c2ff3b69444b139a98521a4899", feat);
        }

        static void CreateStewardBeyond()
        {
            var dispel = library.Get<BlueprintAbility>("92681f181b507b34ea87018e8f7a528a");
            var icon = dispel.Icon;
            var fx = new PrefabLink();
            fx.AssetId = "3eaac4c26129c2848ab8482706a420b2";
            var name = "StewardOfTheGreatBeyond";

            var effectbuff = Helpers.CreateBuff(name+"EffectBuff", "plop","", Helpers.getGuid(name+"EffectBuff"), icon, null, null, 
                Helpers.Create<CounterSummonSpell>(a => a.type = CounterSummonSpell.source.feat)
                );
            
            effectbuff.SetBuffFlags(BuffFlags.HiddenInUi);
            
            var area = Helpers.Create<BlueprintAbilityAreaEffect>(a => a.name = name+"Area");
            area.AffectEnemies = true;
            area.AggroEnemies = false;
            area.IgnoreSleepingUnits = false;
            area.Fx = fx;
            area.Shape = AreaEffectShape.Cylinder;
            area.Size = 30.Feet();
            area.SpellResistance = false;
            var areacompos = new BlueprintComponent[] {
                Helpers.CreateAreaEffectRunAction(
                    //Enter
                    Helpers.CreateConditional(Helpers.Create<ContextConditionIsEnemy>(),
                        Helpers.CreateApplyBuff(effectbuff, Helpers.CreateContextDuration(1, DurationRate.Rounds), false, false, false, true, true),null
                    ),
                    //exit 
                    Helpers.CreateConditional(Helpers.CreateConditionsCheckerAnd(Helpers.CreateConditionHasFact(effectbuff), Helpers.Create<ContextConditionIsEnemy>()),
                        Helpers.Create<ContextActionRemoveBuff>(r => r.Buff = effectbuff)), 
                    //move
                    null,
                    Helpers.CreateConditional(Helpers.Create<ContextConditionIsEnemy>(),
                        Helpers.CreateApplyBuff(effectbuff, Helpers.CreateContextDuration(1, DurationRate.Rounds), false, false, false, true, true),null
                    )

                    )
            };
            area.SetComponents(areacompos);
            library.AddAsset(area, Helpers.getGuid(area.name));

            var buff = Helpers.CreateBuff(name+"Buff", "Steward of the Great Beyond", "Whenever a creature attempts to summon a creature within 30 feet of you, you may attempt to block the effect",Helpers.getGuid(name+"Buff"), icon, null, null,
                Helpers.Create<AddAreaEffect>(a => {
                    a.AreaEffect = area;
                    
                }));
            var ressource = Helpers.CreateAbilityResource(name+"Resource", "","", Helpers.getGuid(name + "Resource"), icon);
            ressource.SetIncreasedByLevelStartPlusDivStep(1, 15, 1, 5, 1, 1, 0, new BlueprintCharacterClass[] { wizardclass }, null);
            var ability = Helpers.CreateActivatableAbility(name+"Ability", "Steward of the Great Beyond", "Whenever a creature attempts to use a teleportation effect or summon a creature within 30 feet of you, you may attempt to block the effect.Make an opposed caster level check(1d20 + caster level) as an immediate action.If the check succeeds, the spell or effect fails and is wasted; otherwise, it is unaffected.",
                Helpers.getGuid(name+"Ability"), icon, buff, AbilityActivationType.Immediately,CommandType.Free, null, 
                Helpers.CreateActivatableResourceLogic(ressource, ActivatableAbilityResourceLogic.ResourceSpendType.Never)
                );
            
            var feat = Helpers.CreateFeature(name+"Feature", "Arcane Discovery : Steward of the Great Beyond", "Whenever a creature attempts to use a teleportation effect or summon a creature within 30 feet of you, you may attempt to block the effect. Make an opposed caster level check (1d20 + caster level) as an immediate action. If the check succeeds, the spell or effect fails and is wasted; otherwise, it is unaffected. You can use this ability once per day plus one additional time for every 5 wizard levels you possess beyond 10th.", Helpers.getGuid(name+"Feature"), icon,
                FeatureGroup.WizardFeat,
                ability.CreateAddFact(),
                Helpers.PrerequisiteClassLevel(wizardclass, 9),
                ressource.CreateAddAbilityResource()
                );

            feat.Groups = feat.Groups.AddToArray(FeatureGroup.Feat);
            library.AddFeats(feat);
            library.AddFeats("8c3102c2ff3b69444b139a98521a4899", feat);



        }

       

        public class CounterSummonSpell: BuffLogic, IInitiatorRulebookHandler<RuleCastSpell>
        {
            public void OnEventAboutToTrigger(RuleCastSpell evt)
            {
                
                bool flag = false;
                switch (type)
                {
                    case source.feat:
                        flag = evt.Spell.Blueprint.SpellDescriptor.HasFlag(SpellDescriptor.Summoning) ? true : false;
                        break;
                    case source.spell:
                        flag = true;
                        break;
                }
                
                if (evt.Initiator.IsInCombat && Game.Instance.CurrentMode != GameModeType.Cutscene && !Buff.Context.MaybeCaster.CombatState.HasCooldownForCommand(CommandType.Swift) && flag)
                {
                    var CasterCheck = 0;
                    var BuffCheck = 0;
                    var buffroll = RulebookEvent.Dice.D20;
                    var Casterroll = RulebookEvent.Dice.D20;
                    var SpellCL = evt.Context.Params.CasterLevel;
                    var buffCL = 0;

                    var difficulty = 10+ evt.Spell.SpellLevel;

                    if (Buff.Context.MaybeCaster.Descriptor.HasFact(Improved) && Buff.Context.SpellLevel > evt.Spell.SpellLevel)
                    {
                        SpellCL = 0;
                        difficulty = 0;
                    }


                    if (type == source.feat)
                    {
                        buffCL = Buff.Context.MaybeCaster.Descriptor.GetSpellbook(wizardclass).CasterLevel;
                        CasterCheck = SpellCL + Casterroll;
                        BuffCheck = buffCL + buffroll;
                    }
                    else if (type == source.spell)
                    {

                        buffCL = Buff.Context.Params.CasterLevel;
                        CasterCheck = SpellCL + difficulty;
                        if (Teamwork == 2)
                            bonus += 2;
                        buffroll = RulebookEvent.Dice.D20;
                        BuffCheck = buffCL + buffroll + bonus;
                    }

                    if (BuffCheck > CasterCheck)
                    {
                        var caseimproved = difficulty == 0 ? "(improved counterspell)" : "";
                        evt.SpellFailureChance = 100;
                        var message = new Kingmaker.UI.Log.LogDataManager.LogItemData(evt.Initiator.CharacterName + "'s " + evt.Spell.Name + " blocked by " + Buff.Context.MaybeCaster.CharacterName + "\nCaster Level Check = " + BuffCheck + " vs " + CasterCheck+caseimproved, Kingmaker.Blueprints.Root.Strings.GameLog.GameLogStrings.Instance.DefaultColor, null, Kingmaker.UI.Log.PrefixIcon.LeftArrow);
                        Game.Instance.UI.BattleLogManager.LogView.AddLogEntry(message);
                    }
                    else
                    {
                        
                        var message = new Kingmaker.UI.Log.LogDataManager.LogItemData(Buff.Context.MaybeCaster.CharacterName+" failed to block "+ evt.Initiator.CharacterName + "'s " + evt.Spell.Name + "\nCaster Level Check = " + BuffCheck + " vs " + CasterCheck, Kingmaker.Blueprints.Root.Strings.GameLog.GameLogStrings.Instance.DefaultColor, null, Kingmaker.UI.Log.PrefixIcon.LeftArrow);
                        Game.Instance.UI.BattleLogManager.LogView.AddLogEntry(message);

                    }
                    Buff.Context.MaybeCaster.CombatState.Cooldown.SwiftAction = 6f;

                    if(type == source.feat)
                    { 
                    Buff.Context.MaybeCaster.Descriptor.Resources.Spend(library.Get<BlueprintAbilityResource>(Helpers.getGuid("StewardOfTheGreatBeyondResource")), 1);
                    
                    if (Buff.Context.MaybeCaster.Descriptor.Resources.GetResourceAmount(library.Get<BlueprintAbilityResource>(Helpers.getGuid("StewardOfTheGreatBeyondResource"))) < 1)
                        Buff.Context.MaybeCaster.Descriptor.ActivatableAbilities.GetFact(library.Get<BlueprintActivatableAbility>(Helpers.getGuid("StewardOfTheGreatBeyondAbility"))).Deactivate();
                    }
                    if (type == source.spell)
                        Buff.Remove();
                }
            }

            public void OnEventDidTrigger(RuleCastSpell evt)
            { }
            public int Teamwork = 0;
            public BlueprintFeature Improved = library.Get<BlueprintFeature>(Helpers.getGuid("ImprovedCounterspellfeature"));
            public source type;
            public int bonus = 0;
            public enum source { feat, spell };
        }
        public class RemoveOppositionSchool : OwnedGameLogicComponent<UnitDescriptor>
        {
            public override void OnFactActivate()
            {
                base.Owner.DemandSpellbook(wizardclass).OppositionSchools.Remove(this.School);
            }

            
          
            public SpellSchool School;
        }

        public class BuffFixerForParty: OwnedGameLogicComponent<UnitDescriptor>
        {
           
            public override void PostLoad()
            {
                Bonus = Fact.MaybeContext.MaybeOwner.Descriptor.GetSpellbook(wizardclass).CasterLevel >= 20 ? 4 : 2;
                var party = Kingmaker.Game.Instance.Player.ControllableCharacters;
                foreach (UnitEntityData Char in party)
                {

                    var buffs = Char.Descriptor.Buffs.Enumerable.ToList();
                    foreach (Buff buff in buffs)
                    {

                        if (buff.Context.MaybeCaster == Fact.MaybeContext.MaybeOwner && buff.Context.SourceAbility != null && buff.Context.SourceAbility.School == SpellSchool.Transmutation && buff.Context.SourceAbility.IsSpell)
                        {

                            var buffcompos = buff.Components;
                            foreach (GameLogicComponent component in buffcompos)
                            {

                                if (component.GetType() == typeof(AddStatBonus))
                                {
                                    statBonus = statBonus.AddToArray((AddStatBonus)component);

                                }

                                if (component.GetType() == typeof(AddContextStatBonus))
                                {
                                    contextStatBonus = contextStatBonus.AddToArray((AddContextStatBonus)component);

                                }

                                if (component.GetType() == typeof(AddStatBonusAbilityValue))
                                {
                                    statBonusAbility = statBonusAbility.AddToArray((AddStatBonusAbilityValue)component);

                                }
                            }

                            foreach (AddStatBonus StatBonus in statBonus)
                            {

                                if (StatBonus.Descriptor == ModifierDescriptor.Enhancement && (StatBonus.Stat == StatType.Strength || StatBonus.Stat == StatType.Dexterity || StatBonus.Stat == StatType.Constitution || StatBonus.Stat == StatType.Intelligence || StatBonus.Stat == StatType.Wisdom || StatBonus.Stat == StatType.Charisma))
                                {
                                    StatBonus.Value += Bonus;

                                }
                            }
                            foreach (AddContextStatBonus StatBonus in contextStatBonus)
                            {
                                if (StatBonus.Descriptor == ModifierDescriptor.Enhancement && (StatBonus.Stat == StatType.Strength || StatBonus.Stat == StatType.Dexterity || StatBonus.Stat == StatType.Constitution || StatBonus.Stat == StatType.Intelligence || StatBonus.Stat == StatType.Wisdom || StatBonus.Stat == StatType.Charisma))
                                {
                                    StatBonus.Value.Value += Bonus;
                                }
                            }
                            foreach (AddStatBonusAbilityValue StatBonus in statBonusAbility)
                            {
                                if (StatBonus.Descriptor == ModifierDescriptor.Enhancement && (StatBonus.Stat == StatType.Strength || StatBonus.Stat == StatType.Dexterity || StatBonus.Stat == StatType.Constitution || StatBonus.Stat == StatType.Intelligence || StatBonus.Stat == StatType.Wisdom || StatBonus.Stat == StatType.Charisma))
                                {
                                    StatBonus.Value.Value += Bonus;
                                }
                            }
                            statBonusAbility = new AddStatBonusAbilityValue[] { };
                            contextStatBonus = new AddContextStatBonus[] { };
                            statBonus = new AddStatBonus[] { };
                        }

                    }

                }

            }

            public void OnPostLoad()
            {

            }

            public override void OnTurnOn()
            {
                
                

            }
            public AddStatBonusAbilityValue[] statBonusAbility = new AddStatBonusAbilityValue[] { };
            public AddContextStatBonus[] contextStatBonus = new AddContextStatBonus[] { };
            public AddStatBonus[] statBonus = new AddStatBonus[] { };
            public int Bonus;

        }
        public class IdealizeBuffMechanics : BuffLogic, IInitiatorRulebookHandler<RuleApplyBuff>
        {
            public bool Check;
            public void OnEventAboutToTrigger(RuleApplyBuff evt)
            {
                if (evt.Reason.Caster != null && Buff.Context.MaybeCaster == evt.Reason.Caster && evt.Reason.Ability.Blueprint.IsSpell && evt.Reason.Ability.Blueprint.School == SpellSchool.Transmutation)
                {

                    Check = true;
                    Bonus = evt.Context.Params.CasterLevel >= 20 ? 4 : 2;

                    var buffcompos = evt.Blueprint.ComponentsArray;
                    foreach (BlueprintComponent component in buffcompos)
                    {
                        if (component.GetType() == typeof(AddStatBonus))
                        {
                            statBonus = statBonus.AddToArray((AddStatBonus)component);
                        }

                        if (component.GetType() == typeof(AddContextStatBonus))
                        {
                            contextStatBonus = contextStatBonus.AddToArray((AddContextStatBonus)component);
                        }

                        if (component.GetType() == typeof(AddStatBonusAbilityValue))
                        {
                            statBonusAbility = statBonusAbility.AddToArray((AddStatBonusAbilityValue)component);
                        }
                    }

                    foreach (AddStatBonus StatBonus in statBonus)
                    {
                        if (StatBonus.Descriptor == ModifierDescriptor.Enhancement && (StatBonus.Stat == StatType.Strength || StatBonus.Stat == StatType.Dexterity || StatBonus.Stat == StatType.Constitution || StatBonus.Stat == StatType.Intelligence || StatBonus.Stat == StatType.Wisdom || StatBonus.Stat == StatType.Charisma))
                        {
                            StatBonus.Value += Bonus;
                        }
                    }
                    foreach (AddContextStatBonus StatBonus in contextStatBonus)
                    {
                        if (StatBonus.Descriptor == ModifierDescriptor.Enhancement && (StatBonus.Stat == StatType.Strength || StatBonus.Stat == StatType.Dexterity || StatBonus.Stat == StatType.Constitution || StatBonus.Stat == StatType.Intelligence || StatBonus.Stat == StatType.Wisdom || StatBonus.Stat == StatType.Charisma))
                        {
                            StatBonus.Value.Value += Bonus;
                        }
                    }
                    foreach (AddStatBonusAbilityValue StatBonus in statBonusAbility)
                    {
                        if (StatBonus.Descriptor == ModifierDescriptor.Enhancement && (StatBonus.Stat == StatType.Strength || StatBonus.Stat == StatType.Dexterity || StatBonus.Stat == StatType.Constitution || StatBonus.Stat == StatType.Intelligence || StatBonus.Stat == StatType.Wisdom || StatBonus.Stat == StatType.Charisma))
                        {
                            StatBonus.Value.Value += Bonus;
                        }
                    }
                }
            }

            public void OnEventDidTrigger(RuleApplyBuff evt)
            {
                if (Check)
                {
                 foreach (AddStatBonus StatBonus in statBonus)
                    {
                        if (StatBonus.Descriptor == ModifierDescriptor.Enhancement && (StatBonus.Stat == StatType.Strength || StatBonus.Stat == StatType.Dexterity || StatBonus.Stat == StatType.Constitution || StatBonus.Stat == StatType.Intelligence || StatBonus.Stat == StatType.Wisdom || StatBonus.Stat == StatType.Charisma))
                        {
                            StatBonus.Value -= Bonus;
                        }
                    }
                    foreach (AddContextStatBonus StatBonus in contextStatBonus)
                    {
                        if (StatBonus.Descriptor == ModifierDescriptor.Enhancement && (StatBonus.Stat == StatType.Strength || StatBonus.Stat == StatType.Dexterity || StatBonus.Stat == StatType.Constitution || StatBonus.Stat == StatType.Intelligence || StatBonus.Stat == StatType.Wisdom || StatBonus.Stat == StatType.Charisma))
                        {
                            StatBonus.Value.Value -= Bonus;
                        }
                    }
                    foreach (AddStatBonusAbilityValue StatBonus in statBonusAbility)
                    {
                        if (StatBonus.Descriptor == ModifierDescriptor.Enhancement && (StatBonus.Stat == StatType.Strength || StatBonus.Stat == StatType.Dexterity || StatBonus.Stat == StatType.Constitution || StatBonus.Stat == StatType.Intelligence || StatBonus.Stat == StatType.Wisdom || StatBonus.Stat == StatType.Charisma))
                        {
                            StatBonus.Value.Value -= Bonus;
                        }
                    }
                    
                }
                Bonus = 0;
                Check = false;
            }
            
            public AddStatBonusAbilityValue[] statBonusAbility = new AddStatBonusAbilityValue[] { };
            public AddContextStatBonus[] contextStatBonus = new AddContextStatBonus[] { };
            public AddStatBonus[] statBonus = new AddStatBonus[] { };
            public int Bonus;

        }

        public class ResilentIllusionBuffMechanics : BuffLogic, IInitiatorRulebookHandler<RuleSavingThrow>
        {
           
            public void OnEventAboutToTrigger(RuleSavingThrow evt)
            {

                    if (evt.Reason.Caster != null && Buff.Context.MaybeCaster == evt.Reason.Caster && evt.Type == SavingThrowType.Will && evt.Reason.Ability.Blueprint.School == SpellSchool.Illusion && evt.Reason.Ability.Blueprint.IsSpell)
                    {
                        var CasterLevel = evt.Reason.Context.Params.CasterLevel;
                        var roll = RulebookEvent.Dice.D20;
                        var check = CasterLevel + roll;
                        if (evt.DifficultyClass < check)
                        {
                            Helpers.SetField(evt, "DifficultyClass", check);
                        }

                    }

            }

            public void OnEventDidTrigger(RuleSavingThrow evt)
            {
                
            }
          
        }

        public class ApplyBuffOnSpellSchoolCast : RuleInitiatorLogicComponent<RuleSpellResistanceCheck>
        {
            public override void OnEventAboutToTrigger(RuleSpellResistanceCheck evt)
            {
                if (evt.Ability.School == School && evt.Ability.IsSpell)
                {
                    Buff buff = evt.Target.Descriptor.AddBuff(this.Buff, evt.Context.MaybeCaster, new TimeSpan?(duration.Rounds().Seconds));
                }
            }
            public override void OnEventDidTrigger(RuleSpellResistanceCheck evt)
            {

            }
            public int duration;
            public SpellSchool School;
            public BlueprintBuff Buff;

        }


        public class OtherListAffinityLogic : RuleInitiatorLogicComponent<RuleCalculateAbilityParams>
        {
            
            public override void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
            {
                if (evt.Spell.IsInSpellList(Helpers.wizardSpellList) && evt.Spell.IsInSpellList(otherlist))
                {
                    
                    evt.AddBonusCasterLevel(1);
                    evt.AddBonusDC(1);                    
                }
            }


            public override void OnEventDidTrigger(RuleCalculateAbilityParams evt)
            {

            }

            public BlueprintSpellList otherlist;

        }

        public class Persuade : ContextAction
        {
           
            public override string GetCaption()
            {
                return "Persuade target";
            }

            
            public override void RunAction()
            {
                MechanicsContext.Data data = ElementsContext.GetData<MechanicsContext.Data>();
                MechanicsContext mechanicsContext = (data != null) ? data.Context : null;
                UnitEntityData unitEntityData = (mechanicsContext != null) ? mechanicsContext.MaybeCaster : null;
                if (unitEntityData == null || !base.Target.IsUnit)
                {
                    UberDebug.LogError(this, "Unable to apply buff: no context found", Array.Empty<object>());
                    return;
                }
                var devotion = 0;
                if (base.Target.Unit.Descriptor.Progression.IsArchetype(Companion))
                    devotion = 15;

                int DC = 15 + base.Target.Unit.Descriptor.Progression.CharacterLevel + base.Target.Unit.Stats.Wisdom.Bonus + devotion;
                ModifiableValue.Modifier modifier = null;
                try
                {
                    
                    RuleSkillCheck ruleSkillCheck = mechanicsContext.TriggerRule<RuleSkillCheck>(new RuleSkillCheck(unitEntityData, StatType.CheckDiplomacy, DC));
                    if (ruleSkillCheck.IsPassed)
                    {
                        int quality = ruleSkillCheck.RollResult - DC;
                        int duration = (1 + quality/ 5)*10 ;
                        

                       if (quality>= 10)
                        {
                            Buff buff = base.Target.Unit.Descriptor.AddBuff(this.GreaterBuff, mechanicsContext, new TimeSpan?(duration.Rounds().Seconds));
                        }
                       else
                        {
                            Buff buff = base.Target.Unit.Descriptor.AddBuff(this.Buff, mechanicsContext, new TimeSpan?(duration.Rounds().Seconds));
                        }
                    }



                }
                finally
                {
                    if (modifier != null)
                    {
                        modifier.Remove();
                    }
                }
            }

            BlueprintArchetype Companion = library.Get<BlueprintArchetype>("9f8a232fbe435a9458bf64c3024d7bee");
            public BlueprintBuff Buff;
            public BlueprintBuff GreaterBuff;

            
        }

        [HarmonyPatch(typeof(Spellbook))]
        [Harmony12.HarmonyPatch("GetSpellsPerDay")]
        private static class ManageSpellPerDayForSplitSlot
        {

            static void Postfix(Spellbook __instance, ref int __result, int spellLevel)
            {
                if (spellLevel > 0 && spellLevel < 8)
                {
                    int buffid = spellLevel - 1;
                    var Buff = library.Get<BlueprintBuff>(Helpers.getGuid("SplitSlotbuff" + buffid));
                    if (Buff != null && __instance.Owner.Buffs.HasFact(Buff))
                    {
                        __result += 2;
                    }

                }
                if (spellLevel > 2)
                {

                    int buffid = spellLevel - 3;
                    var Buff = library.Get<BlueprintBuff>(Helpers.getGuid("SplitSlotbuff" + buffid));
                    if (Buff != null && __instance.Owner.Buffs.HasFact(Buff))
                    {
                        __result -= 1;

                    }
                }



            }


        }



        [HarmonyPatch(typeof(AbilityData))]
        [Harmony12.HarmonyPatch("GetParamsFromItem")]
        private static class UseCasterLevelWithwand
        {

            static void Postfix(AbilityData __instance, AbilityParams __result, BlueprintItemEquipment item)
            {
                var Stafflikewandfeature = Main.library.Get<BlueprintFeature>(Helpers.getGuid("StaffLikeWandArcaneDiscovery"));
                BlueprintItemEquipmentUsable Source = item as BlueprintItemEquipmentUsable;
                var Sourcetype = Source != null ? Source.Type : UsableItemType.Other;
                if (Sourcetype == UsableItemType.Wand && __instance.Caster.HasFact(Stafflikewandfeature))
               {
                    if(__result.CasterLevel < __instance.Caster.GetSpellbook(wizardclass).CasterLevel)
                    __result.CasterLevel = __instance.Caster.GetSpellbook(wizardclass).CasterLevel;
                }


            }


        }

       

    }
}

