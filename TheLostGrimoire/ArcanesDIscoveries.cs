

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
using Kingmaker.Designers.Mechanics.Buffs;
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
            //needed patch
            Main.ApplyPatch(typeof(UseCasterLevelWithwand), "enlarge your wand");

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
                Helpers.Create<ApplyBuffOnSpellSchoolCast>(b => { b.Buff = buff; b.School = SpellSchool.Illusion; })
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
                Helpers.Create<ApplyBuffOnSpellSchoolCast>(a => { a.Buff = buff; a.School = SpellSchool.Transmutation; }),
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
            var featselection = Helpers.CreateFeatureSelection("OppositionResearchSelection", "Arcane Discovery : Opposition Research", "Select one Wizard opposition school; preparing spells of this school now only requires one spell slot of the appropriate level instead of two, and you no longer have the –4 Spellcraft penalty for crafting items from that school.\nNOTE: Feat from The Lost Grimoire", Helpers.getGuid("OppositionResearchSelection"), icon, FeatureGroup.WizardFeat,
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
                Helpers.Create<AbilityScoreCheckBonus>(s => { s.Stat = StatType.AdditionalCMD; s.Descriptor = ModifierDescriptor.UntypedStackable; s.Bonus = Helpers.CreateContextValue(AbilityRankType.StatBonus); }),
                Helpers.CreateContextRankConfig(ContextRankBaseValueType.StatBonus, ContextRankProgression.AsIs,AbilityRankType.StatBonus, null, null, 0, 0, false, StatType.Intelligence), 
                Helpers.Create<RecalculateOnStatChange>(r => r.Stat = StatType.Intelligence)
                );
            feat.Groups = feat.Groups.AddToArray(FeatureGroup.Feat);
            library.AddFeats(feat);
            library.AddFeats("8c3102c2ff3b69444b139a98521a4899", feat);
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
                Bonus = Fact.MaybeContext.MaybeOwner.Descriptor.GetSpellbook(wizardclass).CasterLevel == 20 ? 4 : 2;
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
                Log.Write("Entering OnPostload");

            }

            public override void OnTurnOn()
            {
                Log.Write("Starting TurnOn");
                
                

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
                    Bonus = evt.Context.Params.CasterLevel == 20 ? 4 : 2;

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
                    Buff buff = evt.Target.Descriptor.AddBuff(this.Buff, evt.Context.MaybeCaster, new TimeSpan?(1.Rounds().Seconds));
                }
            }
            public override void OnEventDidTrigger(RuleSpellResistanceCheck evt)
            {

            }
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

        [HarmonyPatch(typeof(AbilityData))]
        [Harmony12.HarmonyPatch("GetParamsFromItem")]
        private static class UseCasterLevelWithwand
        {

            static void Postfix(AbilityData __instance, AbilityParams __result)
            {
                var Stafflikewandfeature = Main.library.Get<BlueprintFeature>(Helpers.getGuid("StaffLikeWandArcaneDiscovery"));

              if (__instance.SourceItemUsableBlueprint.Type == UsableItemType.Wand && __instance.Caster.HasFact(Stafflikewandfeature))
               {
                    if(__result.CasterLevel < __instance.Caster.GetSpellbook(wizardclass).CasterLevel)
                    __result.CasterLevel = __instance.Caster.GetSpellbook(wizardclass).CasterLevel;
                }


            }


        }

    }
}

