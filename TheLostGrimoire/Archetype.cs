using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Validation;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Designers.Mechanics.EquipmentEnchants;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Designers.EventConditionActionSystem.Conditions;
using Kingmaker.Designers.EventConditionActionSystem.Evaluators;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Designers;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.Controllers.Combat;
using Kingmaker.Controllers;
using Kingmaker.Controllers.Rest;
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
using Kingmaker.Kingdom;
using Kingmaker.Kingdom.Blueprints;
using Kingmaker.Kingdom.Actions;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.ResourceLinks;
using Kingmaker.UnitLogic.Class.LevelUp.Actions;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;

using Kingmaker.UI.Tooltip;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.Groups;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.Visual.Particles;
using Kingmaker.Controllers.Projectiles;
using Harmony12;
using Newtonsoft.Json;
using UnityEngine;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UI.ActionBar;
using Kingmaker.UI.UnitSettings;
using Kingmaker.UI.Constructor;

namespace thelostgrimoire
{
    class Archetype
    {
        static LibraryScriptableObject library => Main.library;
        static BlueprintCharacterClass wizardclass = Main.library.Get<BlueprintCharacterClass>("ba34257984f4c41408ce1dc2004e342e");
        static BlueprintArchetype thassilonian = Main.library.Get<BlueprintArchetype>("55a8ce15e30d71547a44c69bb2e8a84f");
        static BlueprintFeatureSelection Arcanebondselection = library.Get<BlueprintFeatureSelection>("03a1781486ba98043afddaabf6b7d8ff");
        static BlueprintFeatureSelection SpecialistSchoolSelection = library.Get<BlueprintFeatureSelection>("5f838049069f1ac4d804ce0862ab5110");
        static BlueprintSpellbook Wizardbook = library.Get<BlueprintSpellbook>("5a38c9ac8607890409fcb8f6342da6f4");
        static BlueprintFeature BondedItem = library.Get<BlueprintFeature>("2fb5e65bd57caa943b45ee32d825e9b9");
        static BlueprintFeatureSelection wizardfeats = library.Get<BlueprintFeatureSelection>("8c3102c2ff3b69444b139a98521a4899");
        static BlueprintFeature OppositionSchool = library.Get<BlueprintFeatureSelection>("6c29030e9fea36949877c43a6f94ff31");
        static string Guid(string name) => Helpers.getGuid(name);
        static Sprite GetIcon(string id) => Helpers.GetIcon(id);
        static BlueprintAbility GetAbility(string id) => library.Get<BlueprintAbility>(id);
        static BlueprintFeature GetFeat(string id) => library.Get<BlueprintFeature>(id);
        static BlueprintBuff GetBuff(string id) => library.Get<BlueprintBuff>(id);
        static BlueprintAbility Ability(String name, String displayName,
            String description, Sprite icon, AbilityRange range, CommandType actionType, AbilityType type= AbilityType.SpellLike, string var = "",
            String duration = "", String savingThrow = "",
            params BlueprintComponent[] components) => Helpers.CreateAbility(name + var + "Ability", displayName, description, Guid(name + var + "Spell"), icon, type , actionType, range, duration, savingThrow, components);
        static BlueprintBuff buff(String name, String displayName, String description, Sprite icon, BuffFlags flags, PrefabLink fxOnStart = null, PrefabLink FxOnRemove = null, StackingType stack = StackingType.Replace,
            params BlueprintComponent[] components)
        {
            BlueprintBuff buff = Helpers.CreateBuff(name + "Buff", displayName, description, Guid(name + "Buff"), icon, fxOnStart, FxOnRemove, components);
            buff.Stacking = stack;
            buff.SetBuffFlags(flags);

            return buff;
        }
        static BlueprintBuff tokenbuff(string name) => buff(name + "Token", "", "", wizardclass.Icon, BuffFlags.HiddenInUi, null, null, StackingType.Replace);

        //Specialist progression 
        static BlueprintProgression AbjurationProgression = library.Get<BlueprintProgression>("c451fde0aec46454091b70384ea91989");
        static BlueprintProgression ConjurationProgression = library.Get<BlueprintProgression>("567801abe990faf4080df566fadcd038");
        static BlueprintProgression DivinationProgression = library.Get<BlueprintProgression>("d7d18ce5c24bd324d96173fdc3309646");
        static BlueprintProgression EnchantmentProgression = library.Get<BlueprintProgression>("252363458703f144788af49ef04d0803");
        static BlueprintProgression EvocationProgression = library.Get<BlueprintProgression>("f8019b7724d72a241a97157bc37f1c3b");
        static BlueprintProgression IllusionProgression = library.Get<BlueprintProgression>("24d5402c0c1de48468b563f6174c6256");
        static BlueprintProgression NecromancyProgression = library.Get<BlueprintProgression>("e9450978cc9feeb468fb8ee3a90607e3");
        static BlueprintProgression TransmutationProgression = library.Get<BlueprintProgression>("b6a604dab356ac34788abf4ad79449ec");

        //Specialist Base Feature
        static BlueprintFeature AbjurationBaseFeature = library.Get<BlueprintFeature>("30f20e6f850519b48aa59e8c0ff66ae9");
        static BlueprintFeature ConjurationBaseFeature = library.Get<BlueprintFeature>("cee0f7edbd874a042952ee150f878b84");
        static BlueprintFeature DivinationBaseFeature = library.Get<BlueprintFeature>("54d21b3221ea82a4d90d5a91b7872f3d");
        static BlueprintFeature EnchantementBaseFeature = library.Get<BlueprintFeature>("9e4c4799735ae9c45964e9113107ef02");
        static BlueprintFeature EvocationBaseFeature = library.Get<BlueprintFeature>("c46512b796216b64899f26301241e4e6");
        static BlueprintFeature IllusionBaseFeature = library.Get<BlueprintFeature>("9be5e050244352d43a1cb50aad8d548f");
        static BlueprintFeature NecromancyBaseFeature = library.Get<BlueprintFeature>("927707dce06627d4f880c90b5575125f");
        static BlueprintFeature TransmurationBaseFeature = library.Get<BlueprintFeature>("c459c8200e666ef4c990873d3e501b91");



        //Wizard Spelllist (Base+ thassilonian)
        static BlueprintSpellList wizardlist = Main.library.Get<BlueprintSpellList>("ba0401fdeb4062f40a7aa95b6f07fe89");
        static BlueprintSpellList ThassAbjurationList = library.Get<BlueprintSpellList>("280dd5167ccafe449a33fbe93c7a875e");
        static BlueprintSpellList ThassConjurationList = library.Get<BlueprintSpellList>("5b154578f228c174bac546b6c29886ce");
        static BlueprintSpellList ThassEnchantmentList = library.Get<BlueprintSpellList>("ac551db78c1baa34eb8edca088be13cb");
        static BlueprintSpellList ThassEvocationList = library.Get<BlueprintSpellList>("17c0bfe5b7c8ac3449da655cdcaed4e7");
        static BlueprintSpellList ThassIllusionList = library.Get<BlueprintSpellList>("c311aed33deb7a346ab715baef4a0572");
        static BlueprintSpellList ThassNecromancyList = library.Get<BlueprintSpellList>("5c08349132cb6b04181797f58ccf38ae");
        static BlueprintSpellList ThassTransmutationList = library.Get<BlueprintSpellList>("f3a8f76b1d030a64084355ba3eea369a");

        //Thassilonian school selection
        static BlueprintFeature ThassAbju = library.Get<BlueprintFeature>("15c681d5a76c1a742abe2760376ddf6d");//ThassilonianAbjurationFeature
        static BlueprintFeature ThassConj = library.Get<BlueprintFeature>("1a258cd8e93461a4ab011c73a2c43dac");//ThassilonianConjurationFeature
        static BlueprintFeature ThassEnch = library.Get<BlueprintFeature>("e1ebc61a71c55054991863a5f6f6d2c2");//ThassilonianEnchantmentFeature
        static BlueprintFeature ThassEvoc = library.Get<BlueprintFeature>("5e33543285d1c3d49b55282cf466bef3");//ThassilonianEvocationFeature
        static BlueprintFeature ThassIllu = library.Get<BlueprintFeature>("aa271e69902044b47a8e62c4e58a9dcb");//ThassilonianIllusionFeature
        static BlueprintFeature ThassNecr = library.Get<BlueprintFeature>("fb343ede45ca1a84496c91c190a847ff");//ThassilonianNecromancyFeature
        static BlueprintFeature ThassTran = library.Get<BlueprintFeature>("dd163630abbdace4e85284c55d269867");//ThassilonianTransmutationFeature

        //Array of thing needed
        static BlueprintFeature[] SpecialistBaseFeatures = new BlueprintFeature[] { AbjurationBaseFeature, ConjurationBaseFeature, DivinationBaseFeature, EnchantementBaseFeature, EvocationBaseFeature, IllusionBaseFeature, NecromancyBaseFeature, TransmurationBaseFeature };
        static BlueprintProgression[] SpecialistProgression = new BlueprintProgression[] { AbjurationProgression, ConjurationProgression, DivinationProgression, EnchantmentProgression, EvocationProgression, IllusionProgression, NecromancyProgression, TransmutationProgression };
        static BlueprintFeature[] WizFeatureList = new BlueprintFeature[] { null, ThassAbju, ThassConj, ThassEnch, ThassEvoc, ThassIllu, ThassNecr, ThassTran };//School slection option for Thass
        static BlueprintSpellList[] spellLists = new BlueprintSpellList[] { wizardlist, ThassAbjurationList, ThassConjurationList, ThassEnchantmentList, ThassEvocationList, ThassIllusionList, ThassNecromancyList, ThassTransmutationList };//Array of base spelllist

        //creature type
        static BlueprintFeature Undead = library.Get<BlueprintFeature>("734a29b693e9ec346ba2951b27987e33");
        static BlueprintFeature Construct = library.Get<BlueprintFeature>("fd389783027d63343b4a5634bd81645f");
        static BlueprintFeature Dragon = library.Get<BlueprintFeature>("455ac88e22f55804ab87c2467deff1d6");
        static BlueprintFeature Plant = library.Get<BlueprintFeature>("706e61781d692a042b35941f14bc41c5");
        static BlueprintFeature Vermine = library.Get<BlueprintFeature>("09478937695300944a179530664e42ec");
        public static BlueprintAbility LesserCounterSpell;
        public static BlueprintAbility CounterSpell;
        public static BlueprintAbility GreaterCounterSpell;
        internal static void Load()
        {
            // Load  feats
            Main.SafeLoad(CreateSpellBinder, "Mage Domain");
            Main.SafeLoad(CreatePoleiheiraAdherent, "Travel Far");
            Main.SafeLoad(AddTurnbasedInterface, "Take your turn");
            Main.SafeLoad(CreatePhantasmSubSchool, "All your wildest dream");
            Main.SafeLoad(CreateLifeSubSchool, "Live or die");
            Main.SafeLoad(CreateTeleportationSubSchool, "Not there");
            Main.SafeLoad(CreateAdmixtureSubSchool, "Many flavor of destruction");
            Main.SafeLoad(CreateEnhancedmentSubSchool, "Be stronk");
            Main.SafeLoad(CreateProphecySubschool, "It was written");
            Main.SafeLoad(CreateCounterSpellSubSchool, "No you don't!");
            //needed patch
            Main.ApplyPatch(typeof(AddCounterSpellConversion), "Counter all the day");
            Main.ApplyPatch(typeof(HandleDisruptionCheck), "No you don't, for real!!!");
            Main.ApplyPatch(typeof(ShowCasterNameOnBuffToolTip), "Anonymous buff killer");
            Main.ApplyPatch(typeof(MakeCreatureFlankedByBuff), "you're surounded");
        }
        static void CreateCounterSpellSubSchool()
        {
            string name = "CounterSpellSchool";
            string Name = "Focused School -- CounterSpell";
            string lvl1 = "Disruption";
            string lvl1Name = "Disruption (Su)";
            string lvl1Desc = " At 1st level, you gain the ability to disrupt spellcasting with a touch. As a melee touch attack, you can place a disruptive field around the target. While the field is in place, the target must make a concentration check to cast any spell or to use a spell-like ability in addition to any other required concentration checks. The DC of this check is equal to 15 + twice the spell’s level. If the check is failed, the target’s spell is wasted. This field lasts for a number of rounds equal to 1/2 your wizard level (minimum 1). You can use this ability a number of times per day equal to 3 + your Intelligence modifier.";

            string lvl6 = "CounterspellMastery";
            string Lvl6bName = "Counterspell Mastery (Su)";
            string Lvl6Dessc = "At 6th level, you gain Improved Counterspell as a bonus feat. You may cast a counterspell spell once per day as a Free Action (instead of a FullRound action), you still need to keep a swift action for the actual counterspelling on your target's round.You can use this ability once per day at 6th level, plus one additional time per day for every 4 levels beyond 6th.";
            Sprite SchoolIcon = AbjurationProgression.Icon;
            SchoolUtility.wizardressource BaseResource = SchoolUtility.GetWizardBaseResource(SpellSchool.Abjuration);

            BlueprintBuff DisrutpionBuff = buff(lvl1, lvl1Name, lvl1Desc, SchoolIcon, BuffFlags.StayOnDeath);

            BlueprintAbility Disruption = Ability(lvl1, lvl1Name, lvl1Desc, SchoolIcon, AbilityRange.Touch, CommandType.Standard, AbilityType.Supernatural, "", "A number of rounds equal to 1/2 your wizard level", "",
                Helpers.CreateDeliverTouch(),
                Helpers.CreateRunActions(Helpers.CreateApplyBuff(DisrutpionBuff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default)), false, false)),
                Helpers.CreateContextRankConfig(progression: ContextRankProgression.Div2)
                );
            Disruption.Animation = Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle.Touch;
            Disruption.CanTargetEnemies = true;
            Disruption.CanTargetFriends = true;

            BlueprintAbility DisruptionCast = Ability(lvl1+"Cast", lvl1Name, lvl1Desc, SchoolIcon, AbilityRange.Touch, CommandType.Standard, AbilityType.Supernatural, "", "A number of rounds equal to 1/2 your wizard level", "",
                Helpers.CreateStickyTouch(Disruption),
                BaseResource.logic
                );
            DisruptionCast.Animation = Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle.Touch;
            DisruptionCast.CanTargetEnemies = true;
            DisruptionCast.CanTargetFriends = true;
            BlueprintAbility Mastery = Ability(lvl6, Lvl6bName, Lvl6Dessc, SchoolIcon, AbilityRange.Medium, CommandType.Free, AbilityType.Supernatural, "", "Instantaneous", "",
                Helpers.Create<CounterSpellMastery>()
                );
            Mastery.CanTargetEnemies = true;
            Mastery.CanTargetFriends = true;
            Mastery.Hidden = true;
            Mastery.Animation = Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle.Directional;

            AbjurationBaseFeature.AddComponent(Mastery.CreateAddFact());
        }
        public class CounterSpellMastery : AbilityApplyEffect, IAbilityAvailabilityProvider, IAbilityParameterRequirement, IAbilityVisibilityProvider
        {
            public bool RequireSpellSlot
            {
                get
                {
                    return true;
                }
            }

            public bool RequireSpellbook
            {

                get
                {
                    return false;
                }
            }

            public bool RequireSpellLevel
            {
                get
                {
                    return false;
                }
            }



            public override void Apply(AbilityExecutionContext context, TargetWrapper target)
            {
                if (context.Ability.ParamSpellSlot == null || context.Ability.ParamSpellSlot.Spell == null)
                {
                    UberDebug.LogError(context.AbilityBlueprint, string.Format("Target spell is missing: {0}", context.AbilityBlueprint), Array.Empty<object>());
                    return;
                }
                if (context.Ability.ParamSpellSlot.Spell.Spellbook == null)
                {
                    UberDebug.LogError(context.AbilityBlueprint, string.Format("Spellbook is missing: {0}", context.AbilityBlueprint), Array.Empty<object>());
                    return;
                }
                
                if (target == null || target.Unit == null)
                {
                    UberDebug.LogError(context.AbilityBlueprint, "Can't use spell: target is missing", Array.Empty<object>());
                    return;
                }
                
                Rulebook.Trigger(new RuleCastSpell(context.Ability.ParamSpellSlot.Spell, target));
                context.Ability.ParamSpellSlot.Spell.Spend();

            }
            public string GetReason()
            {
                return string.Empty;
            }
            public bool IsAvailableFor(AbilityData ability)
            {
                SpellSlot slot = ability.ParamSpellSlot;
                AbilityData abilitydata = (slot != null) ? slot.Spell : null;
                BlueprintAbility spellblueprint = (abilitydata != null) ? abilitydata.Blueprint : null;
                
                return spellblueprint != null && abilities.Contains(spellblueprint) && slot != null && slot.Available;
            }
            public bool IsAbilityVisible(AbilityData spell)
            {
                return IsAvailableFor(spell);
            }
            
            public static BlueprintAbility[] abilities = new BlueprintAbility[] { LesserCounterSpell, CounterSpell, GreaterCounterSpell };

        }
        static void CreateProphecySubschool()
        {
            string name = "ProphecySchool";
            string Name = "Focused School -- Prophecy";
            string lvl1 = "InspiringPrediction";
            string lvl1Name = "Inspiring Prediction(Su)";
            string lvl1Desc = "A number of times per day equal to 3 + your Intelligence modifier, you can predict an ally’s success, bolstering others’ resolve. As a swift action, you can shout an inspiring prediction, granting each ally within 50 feet who can hear you a +4 luck bonus on her next attack roll, saving throw, or skill check.";

            string lvl1b = "TheProphecy";
            string Lvl1bName = "In Accordance with the Prophecy (Su)";
            string Lvl1bDessc = "A number of times per day equal to your Intelligence modifier, you can publicly declare that your next spell is guided by prophecy as a Free Action. When you do, the next spell you cast has a 20% chance of fizzling (1–20 on a d%). If the spell does not fail, treat the spell as if it had been modified by the Empower Spell feat, even if you do not have that feat. At 12th level, the chance that the spell fizzles is reduced to 15% (1–15 on a d%). At 16th level, the chance is reduced to 10% (1–10 on a d%).";

            BlueprintAbility DivinerFortune = library.Get<BlueprintAbility>("0997652c1d8eb164caae8a462401a25d");

            string basedesc = DivinerFortune.Name + ": " + DivinerFortune.Description +
                "\n" + lvl1Name + ": " + lvl1Desc;
            string vardesc = basedesc + "\n" + Lvl1bName + ": " + Lvl1bDessc;

            Sprite SchoolIcon = DivinationProgression.Icon;
            Sprite CombatCasting = Helpers.GetIcon("06964d468fde1dc4aa71a92ea04d930d");
            Sprite Shout = Helpers.GetIcon("f09453607e683784c8fca646eec49162");

           
            

            PrefabLink CommonDivination = Helpers.GetFx("c388856d0e8855f429a83ccba67944ba");
            //INSPIRING PREDICTION
            var Predictionressource = SchoolUtility.CreateWizardResource(lvl1 + "Resource", SchoolIcon, true);

            var PredictionBuff = Helpers.CreateBuff(lvl1+"Buff", lvl1Name, lvl1Desc, Guid(lvl1+"Buff"), Shout, CommonDivination, null,  
                Helpers.Create<PredictionBuffBonus>()
                );
            var PredictionAbility = Helpers.CreateAbility(lvl1 + "Ability", lvl1Name, lvl1Desc, Guid(lvl1 + "Ability"), Shout, AbilityType.Supernatural, CommandType.Swift, AbilityRange.Personal, "1 round", "",
                Helpers.CreateRunActions(Helpers.CreateApplyBuff(PredictionBuff, Helpers.CreateContextDuration(), false, false, false, false, true)),
                Helpers.CreateAbilityTargetsAround(50.Feet(), TargetType.Ally),
                Predictionressource.logic
                );
            PredictionAbility.EffectOnAlly = AbilityEffectOnUnit.Helpful;
            PredictionAbility.Animation = Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle.Omni;
            PredictionAbility.HasFastAnimation = false;
            PredictionAbility.EffectOnEnemy = AbilityEffectOnUnit.None;
            PredictionAbility.ResourceAssetIds = new string[] { CommonDivination.AssetId};
            //IN ACCORDANCE WITH THE PROPHECY
            var prophecyresource = SchoolUtility.CreateWizardResource(lvl1b + "Resource", SchoolIcon, false);
            prophecyresource.resource.SetIncreasedByStat(0, StatType.Intelligence);

            var ProphecyBuff = Helpers.CreateBuff(lvl1b+"Buff", Lvl1bName, Lvl1bDessc, Guid(lvl1b + "Buff"), CombatCasting, CommonDivination, null, 
                Helpers.Create<ProphecyBuff>()
                );
            var ProphecyAbility = Helpers.CreateAbility(lvl1b + "Ability", Lvl1bName, Lvl1bDessc, Guid(lvl1b + "Ability"), CombatCasting, AbilityType.Supernatural, CommandType.Free, AbilityRange.Personal, "", "",
                Helpers.CreateRunActions(Helpers.CreateApplyBuff(ProphecyBuff, Helpers.CreateContextDuration(), false, false, true, true)),
                prophecyresource.logic
                );
            ProphecyAbility.CanTargetSelf = true;
            ProphecyAbility.Animation = Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle.Self;
            ProphecyAbility.ResourceAssetIds = new string[] { CommonDivination.AssetId };
            var ProphecyFeature = SchoolUtility.CreateFeature(name + "BaseFeature", Name, vardesc, SchoolIcon, 1, false, false, 
                prophecyresource.add, 
                Predictionressource.add, 
                SchoolUtility.GetWizardBaseResource(SpellSchool.Divination).add,
                ProphecyAbility.CreateAddFact(),
                PredictionAbility.CreateAddFact(),
                DivinerFortune.CreateAddFact(), 
                Helpers.Create<ReplaceAbilitiesStat>(c=> { c.Ability = new BlueprintAbility[]{ DivinerFortune}; c.Stat = StatType.Intelligence;}),
                SchoolUtility.SpeciaListComponent(SpellSchool.Divination)
                );
            var TokenFeature = Helpers.CreateFeature(name+"TokenFeature", "In Accordance with the Prophecy : Spell Failure Chance decrease", "On 12th, 16th and 20th level, the spell failure chance induced by In Accordance with the Prophecy is decreased by 5% to minimum if 5% at level 20", Guid(name + "TokenFeature"), CombatCasting, FeatureGroup.None);
            TokenFeature.HideInCharacterSheetAndLevelUp = true;

            var progression = SchoolUtility.CreateSchoolVariantProgression(DivinationProgression, name, Name, vardesc, true,SchoolUtility.BuildLevelEntry(
                (1, ProphecyFeature),
                (1, OppositionSchool), 
                (1, OppositionSchool), 
                (12, TokenFeature), 
                (16, TokenFeature), 
                (20, TokenFeature)
                ));

            var OppositionDivination = library.Get<BlueprintFeature>("09595544116fe5349953f939aeba7611");
            OppositionDivination.AddComponent(Helpers.PrerequisiteNoFeature(progression));

        }
        public class ProphecyBuff : BuffLogic, IInitiatorRulebookHandler<RuleCalculateAbilityParams>, IRulebookHandler<RuleCalculateAbilityParams>, IInitiatorRulebookHandler<RuleCastSpell>, IRulebookHandler<RuleCastSpell>, IInitiatorRulebookSubscriber
        {
           
            public int Fail {
                get
                {
                   int lvl = Owner.Progression.GetClassLevel(wizardclass);
                    return lvl >= 20 ? 5 : lvl > 15 ? 10 : lvl > 11 ? 15 : 20;
                }
            }

            public  void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
            {
                if (evt.Spell != null && evt.Spellbook != null && evt.Spell.Type == AbilityType.Spell)
                {
                    evt.AddMetamagic(Metamagic.Empower);
                    Applied = true;
                    
                }
            }

            public void OnEventAboutToTrigger(RuleCastSpell evt)
            {
                if (Applied)
                {
                    evt.SpellFailureChance += Fail;
                    Applied = false;
                    Buff.Remove();

                }

            }

            public  void OnEventDidTrigger(RuleCalculateAbilityParams evt)
            {
            }


            public void OnEventDidTrigger(RuleCastSpell evt)
            {
            }
            
            public bool Applied = false; 

        }
        public class PredictionBuffBonus : BuffLogic, IInitiatorRulebookHandler<RuleAttackRoll>, IInitiatorRulebookHandler<RuleSavingThrow>, IInitiatorRulebookHandler<RuleSkillCheck>,
            IInitiatorRulebookSubscriber, IRulebookHandler<RuleAttackRoll>, IRulebookHandler<RuleSavingThrow>, IRulebookHandler<RuleSkillCheck>
        {
            public void OnEventAboutToTrigger(RuleAttackRoll evt)
            {
                evt.AddTemporaryModifier(evt.Initiator.Stats.AdditionalAttackBonus.AddModifier(4, this, ModifierDescriptor.Luck));
            }

            public void OnEventAboutToTrigger(RuleSavingThrow evt)
            {
                evt.AddTemporaryModifier(evt.Initiator.Stats.GetStat(evt.StatType).AddModifier(4, this, ModifierDescriptor.Luck));
            }

            public void OnEventAboutToTrigger(RuleSkillCheck evt)
            {
                evt.AddTemporaryModifier(evt.Initiator.Stats.GetStat(evt.StatType).AddModifier(4, this, ModifierDescriptor.Luck));
            }

            public void OnEventDidTrigger(RuleAttackRoll evt)
            {
                Buff.Remove();
            }

            public void OnEventDidTrigger(RuleSavingThrow evt)
            {
                Buff.Remove();

            }

            public void OnEventDidTrigger(RuleSkillCheck evt)
            {
                Buff.Remove();

            }



        }




        static void CreateEnhancedmentSubSchool()
        {
            string name = "EnhancementSchool";
            string Name = "Focused School -- Enhancement";
            string lvl1 = "Augment";
            string lvl1Name = "Augment (Sp)";
            string lvl1Desc = "As a standard action, you can touch a creature and grant it either a +2 enhancement bonus to a single ability score of your choice or a +1 bonus to natural armor that " +
                "stacks with any natural armor the creature might possess. At 10th level, the enhancement bonus to one ability score increases to +4. " +
                "The natural armor bonus increases by +1 for every five wizard levels you possess, to a maximum of +5 at 20th level. " +
                "This augmentation lasts a number of rounds equal to 1/2 your wizard level (minimum 1 round). " +
                "ou can use this ability a number of times per day equal to 3 + your Intelligence modifier.";
            string Lvl1abilitiesdesc = "As a standard action, you can touch a creature and grant it a {0} bonus to {1} for a number of rounds equal to 1/2 your wizard level (minimum 1 round).";

            string lvl8 = "PerfectionofSelf";
            string Lvl8Name = "Perfection of Self (Su)";
            string Lvl8Dessc = "At 8th level, as a swift action you can grant yourself an enhancement bonus to a single ability score equal to 1/2 your wizard level (maximum +10) for one round." +
                " You may use this ability for a number of times per day equal to your wizard level.";
            string Lvl8abilitiesdesc = "As a swift action you can grant yourself an enhancement bonus to {0} equal to 1/2 your wizard level for one round.";

            BlueprintFeature PhysicalEnhancement = library.Get<BlueprintFeature>("93919f8ce64dc5a4cbf058a486a44a1b");

            string basedesc = PhysicalEnhancement.Name+": "+ PhysicalEnhancement.Description +
                "\n" + lvl1Name + ": " + lvl1Desc;
            string vardesc = basedesc + "\n" + Lvl8Name + ": " + Lvl8Dessc;

            Sprite SchoolIcon = TransmutationProgression.Icon;

            // Getting all the needed stuff to create the abilities in loop
            Sprite Lvl1Icon = Helpers.GetIcon("c60969e7f264e6d4b84a1499fdcf9039"); //Enlarge Person 
            Sprite Lvl8Icon = Helpers.GetIcon("4e0e9aba6447d514f88eff1464cc4763"); //Reduce person
            Sprite[] Icons = new Sprite[] {
                Helpers.GetIcon("4c3d08935262b6544ae97599b3a9556d"),
                Helpers.GetIcon("de7a025d48ad5da4991e7d3c682cf69d"),
                Helpers.GetIcon("a900628aea19aa74aad0ece0e65d091a"),
                Helpers.GetIcon("ae4d3ad6a8fda1542acf2e9bbc13d113"),
                Helpers.GetIcon("f0455c9295b53904f9e02fc571dd2ce1"),
                Helpers.GetIcon("446f7bf201dc1934f96ac0a26e324803"),
                Helpers.GetIcon("5b77d7cc65b8ab74688e74a37fc2f553")

            };//BullStrength, Cat's Grace, Bear Endurance, Fox cunning, Owl wisdom, Eagle splendor, Barkskin 
            PrefabLink TransmutationBuff = Helpers.GetFx("352469f228a3b1f4cb269c7ab0409b8e");
            StatType[] Stats = new StatType[] { StatType.Strength, StatType.Dexterity, StatType.Constitution, StatType.Intelligence, StatType.Wisdom, StatType.Charisma, StatType.AC };



            //CREATING AUGMENT 
            BlueprintAbility[] AugmentVariants = new BlueprintAbility[7];
            BlueprintBuff[] AugmentBuff = new BlueprintBuff[7];

            var AugmentAbility = Helpers.CreateAbility(lvl1 + "BaseAbility", lvl1Name, lvl1Desc, Guid(lvl1+"BaseAbility"), Lvl1Icon, AbilityType.SpellLike, CommandType.Standard, AbilityRange.Touch, "1/2 Wizard level", "");
            var Baseresource = SchoolUtility.GetWizardBaseResource(SpellSchool.Transmutation);
            for (int i = 0; i< AugmentVariants.Length; i++)
            {
                string FormatedDesc = "";
                if (i < 6)
                {
                    FormatedDesc = string.Format(Lvl1abilitiesdesc, "+2 (or +4 if you are lvl 10)", Stats[i].ToString());

                    AugmentBuff[i] = Helpers.CreateBuff(lvl1 + Stats[i].ToString() + "buff", lvl1Name + ": " + Stats[i].ToString(), FormatedDesc, Guid(lvl1 + Stats[i].ToString() + "buff"), Icons[i], TransmutationBuff, null,
                        Helpers.CreateAddContextStatBonus(Stats[i], ModifierDescriptor.Enhancement, ContextValueType.Rank),
                        Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, ContextRankProgression.Custom, min: 2, max: 4,
                        classes: new BlueprintCharacterClass[] { wizardclass }, customProgression: new (int, int)[] { (9, 2), (20, 4) })
                        );


                }
                else
                {
                    FormatedDesc = string.Format(Lvl1abilitiesdesc, "+ 1 per 5 wizard level", "Natural Armor");

                    AugmentBuff[i] = Helpers.CreateBuff(lvl1 + "Natural" + Stats[i].ToString() + "buff", lvl1Name + ": Natural " + Stats[i].ToString(), FormatedDesc , Guid(lvl1 + "Natural" + Stats[i].ToString() + "buff"), Icons[i], TransmutationBuff, null,
                        Helpers.CreateAddContextStatBonus(Stats[i], ModifierDescriptor.NaturalArmor, ContextValueType.Rank),
                        Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, ContextRankProgression.OnePlusDivStep, min: 1, max: 5, startLevel: 1, stepLevel: 5,
                        classes: new BlueprintCharacterClass[] { wizardclass })
                        );
                }
                AugmentVariants[i] = Helpers.CreateAbility(lvl1 + Stats[i].ToString() + "Variant", lvl1Name + ": " + Stats[i].ToString(), FormatedDesc, Guid(lvl1 + Stats[i].ToString() + "Variant"), Icons[i],
                    AbilityType.SpellLike, CommandType.Standard, AbilityRange.Touch, "1/2 Wizard level", "", 
                    Helpers.CreateRunActions(Helpers.CreateApplyBuff(AugmentBuff[i], Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.StatBonus)), true)),
                    Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, ContextRankProgression.Div2, AbilityRankType.StatBonus, 1, 10, classes: new BlueprintCharacterClass[] { wizardclass }),
                    Baseresource.logic
                    );
                AugmentVariants[i].CanTargetFriends = true;
                AugmentVariants[i].CanTargetSelf = true;
                AugmentVariants[i].CanTargetEnemies = true;
                AugmentVariants[i].EffectOnAlly = AbilityEffectOnUnit.Helpful;
                AugmentVariants[i].EffectOnEnemy = AbilityEffectOnUnit.Helpful;
                AugmentVariants[i].Animation = Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle.Touch;


            }
            AugmentAbility.AddComponent(AugmentAbility.CreateAbilityVariants(AugmentVariants));
            AugmentAbility.AddComponents(Baseresource.logic);
            AugmentAbility.ResourceAssetIds = new string[] { TransmutationBuff.AssetId};
           
            //PERFECTION OF SELF (PoF)
            BlueprintAbility[] PoFVariants = new BlueprintAbility[6];
            BlueprintBuff[] PoFBuff = new BlueprintBuff[6];
            var GreaterResource = SchoolUtility.CopySchoolresource("bf214cd0561aebb43a789ff83f12928b");
            for (int i = 0; i < PoFVariants.Length; i++)
            {
                string FormatDesc = string.Format(Lvl8abilitiesdesc, Stats[i].ToString());
                PoFBuff[i] = Helpers.CreateBuff(lvl8 + Stats[i].ToString() + "Buff", Lvl8Name + ": " + Stats[i].ToString(), FormatDesc, Guid(lvl8 + Stats[i].ToString() + "Buff"), Icons[i], TransmutationBuff, null,
                    Helpers.CreateAddContextStatBonus(Stats[i], ModifierDescriptor.Enhancement, ContextValueType.Rank),
                    Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, ContextRankProgression.Div2, classes: new BlueprintCharacterClass[] { wizardclass })
                    );
                PoFVariants[i] = Helpers.CreateAbility(lvl8 + Stats[i].ToString() + "Variant", Lvl8Name + ": " + Stats[i].ToString(), FormatDesc, Guid(lvl8 + Stats[i].ToString() + "Variant"), Icons[i],
                    AbilityType.Supernatural, CommandType.Swift, AbilityRange.Personal, "1 round", "",  
                    Helpers.CreateRunActions(Helpers.CreateApplyBuff(PoFBuff[i], Helpers.CreateContextDuration(1, DurationRate.Rounds), false, false, true, false, false)),
                    GreaterResource.logic
                    );
                PoFVariants[i].Animation = Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle.SelfTouch;
                PoFVariants[i].CanTargetSelf = true;
            }
            var PoFAbility = Helpers.CreateAbility(lvl8 + "BaseAbility", Lvl8Name, Lvl8Dessc, Guid(lvl8 + "BaseAbility"), Lvl8Icon, AbilityType.SpellLike, CommandType.Swift, AbilityRange.Personal, "1 round", "");
            PoFAbility.AddComponent(PoFAbility.CreateAbilityVariants(PoFVariants));
            PoFAbility.AddComponent(GreaterResource.logic);
            PoFAbility.ResourceAssetIds = new string[] { TransmutationBuff.AssetId};

            //FEATURE AND PROGRESSION

            var BaseFeature = SchoolUtility.CreateFeature(name + "BaseFeature", Name, basedesc, TransmutationProgression.Icon, 1, false, false,
                Baseresource.add,
                PhysicalEnhancement.CreateAddFact(),
                AugmentAbility.CreateAddFact(),
                SchoolUtility.SpeciaListComponent(SpellSchool.Transmutation)
                );

            var GreatFeature = SchoolUtility.CreateFeature(name + "GreaterFeature", Lvl8Name, Lvl8Dessc, Lvl8Icon, 1, false, false, 
                GreaterResource.add, 
                PoFAbility.CreateAddFact()
                );
            var capstone = library.Get<BlueprintFeature>("6aa7d3496cd68e643adcd439a7306caa");

            var schoolprogression = SchoolUtility.CreateSchoolVariantProgression(TransmutationProgression, name + "Progression", Name, vardesc, true,
                SchoolUtility.BuildLevelEntry(
                    (1, BaseFeature),
                    (1, OppositionSchool), 
                    (1, OppositionSchool), 
                    (8, GreatFeature), 
                    (20, capstone)
                    ));

            var TransOpposition = library.Get<BlueprintFeature>("fc519612a3c604446888bb345bca5234");
            TransOpposition.AddComponent(Helpers.PrerequisiteNoFeature(schoolprogression));


        }


        static void CreateAdmixtureSubSchool()
        {
            string name = "AdmixtureSchool";
            string Name = "Focused School -- Admixture";
            string lvl1 = "VersatileEvocation";
            string lvl1Name = "Versatile Evocation (Su)";
            string lvl1Desc = " When you cast an evocation spell that does acid, cold, electricity, or fire damage, you may change the damage dealt " +
                "to one of the other four energy types. This changes the descriptor of the spell to match the new energy type. " +
                "Any non-damaging effects remain unchanged. You can use this ability a number of times per day equal to 3 + your Intelligence modifier.";

            string lvl8 = "ElementalManipulation";
            string Lvl8Name = "Elemental Manipulation (Su)";
            string Lvl8Dessc = "At 8th level, you can emit a 30-foot aura that transforms magical energy gathered by your allies. Choose an energy type from acid, cold, electricity, and fire, any magical source of energy of this type casted by one of your allies is altered to the chosen energy type. This includes supernatural effects and spell-like ability as well as a Kinetiscist's blast. You can use this ability for a number of rounds per day equal to half your wizard level. The rounds do not need to be consecutive.";

            string basedesc = "Intense Spells: Whenever you cast an evocation spell that deals hit point damage, add 1/2 your wizard level to the damage (minimum +1). " +
                "This bonus only applies once to a spell, not once per missile or ray, and cannot be split between multiple missiles or rays. This damage is of the same " +
                "type as the spell. At 20th level, whenever you cast an evocation spell, you can roll twice to penetrate a creature's spell resistance and take the " +
                "better result." +
                "\n" + lvl1Name + ": " + lvl1Desc;
            string vardesc = basedesc + "\n" + Lvl8Name + ": " + Lvl8Dessc;
            BlueprintActivatableAbility ColdAbility = library.CopyAndAdd<BlueprintActivatableAbility>("dd484f0706325de40aee5dba15fbce45", lvl1 + "ColdAbility", Guid(lvl1 + "ColdAbility"));
            BlueprintActivatableAbility FireAbility = library.CopyAndAdd<BlueprintActivatableAbility>("924dfcd481c0be54c959c2846b3fb7da", lvl1 + "FireAbility", Guid(lvl1 + "FireAbility"));
            BlueprintActivatableAbility AcidAbility = library.CopyAndAdd<BlueprintActivatableAbility>("94ce51ed666fc8d42830aa9fe48897f9", lvl1 + "AcidAbility", Guid(lvl1 + "AcidAbility"));
            BlueprintActivatableAbility ElecAbility = library.CopyAndAdd<BlueprintActivatableAbility>("5f6315dfeb74a564f96f460d72f7206c", lvl1 + "ElecAbility", Guid(lvl1 + "ElecAbility"));

            //VERSATILE EVOVCATION
            var Baseresource = SchoolUtility.GetWizardBaseResource(SpellSchool.Evocation);
            Baseresource.ToggleLogic.SpendType = ActivatableAbilityResourceLogic.ResourceSpendType.Judgment;

            //Creating each element ability

            //------fire
            //----------Buff
            string FireVersatility = "When you cast an evocation spell that does elemental damage, you will change the damage dealt to Fire. This changes the descriptor of the spell but any non-damaging effects remain unchanged.";
            var firebuff = Helpers.CreateBuff(lvl1 + "FireBuff", "Fire Versatility", FireVersatility, Guid(lvl1 + "FireBuff"), FireAbility.Icon, Helpers.GetFx("ac9eb9c722a6074488c7b120b6839efc"), null,
                Helpers.Create<AdmixtureChangeElementalDamage>(a => {
                    a.Element = DamageEnergyType.Fire;
                    a.RestrictedSchool = SpellSchool.Evocation;
                    a.RestrictSchool = true;
                    a.resource = Baseresource.resource;
                }));
            //---------Setting up actibatable ability
            FireAbility.Buff = firebuff;
            FireAbility.DeactivateImmediately = true;
            FireAbility.ActivationType = AbilityActivationType.WithUnitCommand;
            FireAbility.SetNameDescription(lvl1Name, FireVersatility);
            FireAbility.AddComponent(Baseresource.ToggleLogic);
            Helpers.SetField(FireAbility, "m_ActivateWithUnitCommand", CommandType.Free);

            //------Cold
            //----------Buff
            string ColdVersatility = "When you cast an evocation spell that does elemental damage, you will change the damage dealt to Cold. This changes the descriptor of the spell but any non-damaging effects remain unchanged.";
            var Coldbuff = Helpers.CreateBuff(lvl1 + "ColdBuff", "Cold Versatility", ColdVersatility, Guid(lvl1 + "ColdBuff"), ColdAbility.Icon, Helpers.GetFx("6b0e6932d794790419a7a606b95be2c7"), null,
                Helpers.Create<AdmixtureChangeElementalDamage>(a => {
                    a.Element = DamageEnergyType.Cold;
                    a.RestrictedSchool = SpellSchool.Evocation;
                    a.RestrictSchool = true;
                    a.resource = Baseresource.resource;
                }));
            //---------Setting up actibatable ability
            ColdAbility.Buff = Coldbuff;
            ColdAbility.DeactivateImmediately = true;
            ColdAbility.ActivationType = AbilityActivationType.WithUnitCommand;
            ColdAbility.SetNameDescription(lvl1Name, ColdVersatility);
            ColdAbility.AddComponent(Baseresource.ToggleLogic);
            Helpers.SetField(ColdAbility, "m_ActivateWithUnitCommand", CommandType.Free);

            //------Electricity
            //----------Buff
            string ElecVersatility = "When you cast an evocation spell that does elemental damage, you will change the damage dealt to Electricity. This changes the descriptor of the spell but any non-damaging effects remain unchanged.";
            var Elecbuff = Helpers.CreateBuff(lvl1 + "ElecBuff", "Electricity Versatility", ElecVersatility, Guid(lvl1 + "ElecBuff"), ElecAbility.Icon, Helpers.GetFx("db5ef84b0b43f8e4aa44a5721c4d2df6"), null,
                Helpers.Create<AdmixtureChangeElementalDamage>(a => {
                    a.Element = DamageEnergyType.Electricity;
                    a.RestrictedSchool = SpellSchool.Evocation;
                    a.RestrictSchool = true;
                    a.resource = Baseresource.resource;
                }));
            //---------Setting up actibatable ability
            ElecAbility.Buff = Elecbuff;
            ElecAbility.DeactivateImmediately = true;
            ElecAbility.ActivationType = AbilityActivationType.WithUnitCommand;
            ElecAbility.SetNameDescription(lvl1Name, ElecVersatility);
            ElecAbility.AddComponent(Baseresource.ToggleLogic);
            Helpers.SetField(ElecAbility, "m_ActivateWithUnitCommand", CommandType.Free);

            //------Acid
            //----------Buff
            string AcidVersatility = "When you cast an evocation spell that does elemental damage, you will change the damage dealt to Acid. This changes the descriptor of the spell but any non-damaging effects remain unchanged.";
            var Acidbuff = Helpers.CreateBuff(lvl1 + "AcidBuff", "Acid Versatility", AcidVersatility, Guid(lvl1 + "AcidBuff"), AcidAbility.Icon, Helpers.GetFx("f49dc9f7b7c816240b42ef957083be21"), null,
                Helpers.Create<AdmixtureChangeElementalDamage>(a => {
                    a.Element = DamageEnergyType.Acid;
                    a.RestrictedSchool = SpellSchool.Evocation;
                    a.RestrictSchool = true;
                    a.resource = Baseresource.resource;
                }));
            //---------Setting up actibatable ability
            AcidAbility.Buff = Acidbuff;
            AcidAbility.DeactivateImmediately = true;
            AcidAbility.ActivationType = AbilityActivationType.WithUnitCommand;
            AcidAbility.SetNameDescription(lvl1Name, AcidVersatility);
            AcidAbility.AddComponent(Baseresource.ToggleLogic);
            Helpers.SetField(AcidAbility, "m_ActivateWithUnitCommand", CommandType.Free);

            //Creating and adding an exclusive activation componnent to mimic the group mechanic of Activatable ABility
            BlueprintActivatableAbility[] Group = new BlueprintActivatableAbility[] { AcidAbility, ColdAbility, ElecAbility, FireAbility };

            var AcidExcluCompo = Helpers.Create<FalseGroupMechanic>(f => f.Group = new BlueprintActivatableAbility[] { ColdAbility, ElecAbility, FireAbility });
            Acidbuff.AddComponent(AcidExcluCompo);

            var ColdExcluCompo = Helpers.Create<FalseGroupMechanic>(f => f.Group = new BlueprintActivatableAbility[] { AcidAbility, ElecAbility, FireAbility });
            Coldbuff.AddComponent(ColdExcluCompo);

            var ElecExcluCompo = Helpers.Create<FalseGroupMechanic>(f => f.Group = new BlueprintActivatableAbility[] { AcidAbility, ColdAbility, FireAbility });
            Elecbuff.AddComponent(ElecExcluCompo);

            var FireExcluCompo = Helpers.Create<FalseGroupMechanic>(f => f.Group = new BlueprintActivatableAbility[] {AcidAbility, ColdAbility, ElecAbility});
            firebuff.AddComponent(FireExcluCompo);

            //ELEMENTAL MANIPULATION
            var Greaterresource = SchoolUtility.CreateWizardResource("AdmixtureGreaterResource", EvocationProgression.Icon, false);
            Greaterresource.resource.SetIncreasedByLevelStartPlusDivStep(0, 2, 1, 2, 1, 1, 0, new BlueprintCharacterClass[] { wizardclass }); //SetIncreasedByLevel(0, 1, new BlueprintCharacterClass[] { wizardclass }, null);
            var EvocFx = Helpers.GetFx("dfc59904273f7ee49ab00e5278d86e16");

            var ProtectionVsEnergyCommu = library.Get<BlueprintAbility>("76a629d019275b94184a1a8733cac45e");
            var AcidIcon = ProtectionVsEnergyCommu.Variants[0].Icon;
            var ColdIcon = ProtectionVsEnergyCommu.Variants[1].Icon;
            var ElecIcon = ProtectionVsEnergyCommu.Variants[2].Icon;
            var FireIcon = ProtectionVsEnergyCommu.Variants[3].Icon;
            //Creating each element ability
            string ElemManipDesc = " As a Standard Action, you can emit a 30-foot aura : any magical energy created by your allies inside this aura is altered to {0}.";
            var FireManipulationAbility = CreateElementalManipulation(lvl8, "Fire", string.Format(ElemManipDesc, "Fire"), FireIcon, DamageEnergyType.Fire, EvocFx, Helpers.GetFx("ac9eb9c722a6074488c7b120b6839efc"),
                Helpers.GetFx("379c602be99d76644a8fb70ad0645f94"), Greaterresource);
            var AcidManipulationAbility = CreateElementalManipulation(lvl8, "Acid", string.Format(ElemManipDesc, "Acid"), AcidIcon, DamageEnergyType.Acid, EvocFx, Helpers.GetFx("f49dc9f7b7c816240b42ef957083be21"),
                Helpers.GetFx("61b82291a449add469bd953239fdd64c"), Greaterresource);
            var ColdManipulationAbility = CreateElementalManipulation(lvl8, "Cold", string.Format(ElemManipDesc, "Cold"), ColdIcon, DamageEnergyType.Cold, EvocFx, Helpers.GetFx("6b0e6932d794790419a7a606b95be2c7"),
                Helpers.GetFx("d266e80a8c99a3d4299bcfef13896108"), Greaterresource);
            var ElecManipulationAbility = CreateElementalManipulation(lvl8, "Electricity", string.Format(ElemManipDesc, "Electricity"), ElecIcon, DamageEnergyType.Electricity, EvocFx, Helpers.GetFx("db5ef84b0b43f8e4aa44a5721c4d2df6"),
                Helpers.GetFx("c946053d3cb4bd04bacdfd42ab8a70a7"), Greaterresource);

            var AcidManipulationExclucompo = Helpers.Create<FalseGroupMechanic>(f => f.Group = new BlueprintActivatableAbility[] { ColdManipulationAbility, ElecManipulationAbility, FireManipulationAbility });
            var ColdManipulationExclucompo = Helpers.Create<FalseGroupMechanic>(f => f.Group = new BlueprintActivatableAbility[] { AcidManipulationAbility, ElecManipulationAbility, FireManipulationAbility });
            var ElecManipulationExclucompo = Helpers.Create<FalseGroupMechanic>(f => f.Group = new BlueprintActivatableAbility[] { AcidManipulationAbility, ColdManipulationAbility, FireManipulationAbility });
            var FireManipulationExclucompo = Helpers.Create<FalseGroupMechanic>(f => f.Group = new BlueprintActivatableAbility[] { AcidManipulationAbility, ColdManipulationAbility, ElecManipulationAbility});

            AcidManipulationAbility.Buff.AddComponent(AcidManipulationExclucompo);
            ColdManipulationAbility.Buff.AddComponent(ColdManipulationExclucompo);
            ElecManipulationAbility.Buff.AddComponent(ElecManipulationExclucompo);
            FireManipulationAbility.Buff.AddComponent(FireManipulationExclucompo);

            var Basefeature = SchoolUtility.CreateFeature(lvl1 + "BaseFeature", "Admixture Focused School", basedesc, EvocationBaseFeature.Icon, 1, false, false,
                Baseresource.add,
                Helpers.Create<IntenseSpells>(c => c.Wizard = wizardclass),
                SchoolUtility.SpeciaListComponent(SpellSchool.Evocation),
                Helpers.CreateAddFacts(FireAbility, AcidAbility, ColdAbility, ElecAbility)
                );
            var GreaterFeature = SchoolUtility.CreateFeature(lvl8 + "Feature", Lvl8Name, Lvl8Dessc, Helpers.GetIcon("0340fe43f35e7a448981b646c638c83d"), 1, false, false,  
                Greaterresource.add, 
                Helpers.CreateAddFacts(AcidManipulationAbility, ColdManipulationAbility, ElecManipulationAbility, FireManipulationAbility)
                );

            var AdmixtureProgression = SchoolUtility.CreateSchoolVariantProgression(EvocationProgression, name, Name, vardesc, true, 
                SchoolUtility.BuildLevelEntry(
                    (1, Basefeature),
                    (1, OppositionSchool), 
                    (1, OppositionSchool), 
                    (8, GreaterFeature)
                    ));
            var EvocOpposition = library.Get<BlueprintFeature>("c3724cfbe98875f4a9f6d1aabd4011a6");
            EvocOpposition.AddComponent(Helpers.PrerequisiteNoFeature(AdmixtureProgression));
        }
        public static BlueprintActivatableAbility CreateElementalManipulation(string basename, string ElementName, string Description, Sprite Icon, DamageEnergyType Element, PrefabLink Aurafx, PrefabLink EffectBuffFx, PrefabLink AuraBuffFx, SchoolUtility.wizardressource ResourceWrapper)

        {
            
            //----------Buff
            
            
            var Manipulationbuff = Helpers.CreateBuff(basename +ElementName+ "Buff", ElementName+" Manipulation", Description, Guid(basename + ElementName + "Buff"), Icon, EffectBuffFx, null,
                Helpers.Create<AdmixtureChangeElementalDamage>(a => {
                    a.Element = Element;
                }));
            //---------Creating Aura
            var Area = Helpers.Create<BlueprintAbilityAreaEffect>(a => {
                a.name = basename + ElementName+"Aura";
                a.AffectEnemies = false;
                a.AggroEnemies = false;
                a.Fx = Aurafx;
                a.Shape = AreaEffectShape.Cylinder;
                a.Size = 30.Feet();
                a.SetComponents(
                Helpers.Create<AbilityAreaEffectBuff>(c => {
                    c.Condition = Helpers.CreateConditionsCheckerAnd(Helpers.Create<ContextConditionIsAlly>()
                        );
                    c.Buff = Manipulationbuff;
                }));
            });
            library.AddAsset(Area, Helpers.getGuid(Area.name));

            //------- Buff for Aura
            var ManipulationAuraBuff = Helpers.CreateBuff(basename + ElementName+"AuraBuff", "", "", Guid(basename + ElementName + "AuraBuff"), Icon,AuraBuffFx, null,
                 Helpers.Create<AddAreaEffect>(a => a.AreaEffect = Area)
                 );
            ManipulationAuraBuff.SetBuffFlags(BuffFlags.HiddenInUi);


            var ManipulationAbility = Helpers.CreateActivatableAbility(basename + ElementName+"ActivatbleAbility", "Elemental Manipulation: "+ElementName, Description, Guid(basename + ElementName + "ActivatbleAbility"), Icon, ManipulationAuraBuff, AbilityActivationType.WithUnitCommand, CommandType.Standard, null,
               ResourceWrapper.ToggleLogic);
            ManipulationAbility.DeactivateImmediately = true;
            ManipulationAbility.DeactivateIfOwnerUnconscious = true;
            

            return ManipulationAbility;

        } 
       

        public class FalseGroupMechanic : BuffLogic
        {
            public override void OnTurnOn()
            {
                ActivatableAbility[] array = Owner.ActivatableAbilities.Enumerable.ToArray();
                foreach (ActivatableAbility ability in array)
                {
                    if (Group.Contains(ability.Blueprint))
                    {
                        if (ability.IsOn)
                        {
                            ability.IsOn = false;
                        }
                    }
                }
            }
            public BlueprintActivatableAbility[] Group;
        }

        public class AdmixtureChangeElementalDamage : OwnedGameLogicComponent<UnitDescriptor>, IInitiatorRulebookHandler<RuleCastSpell>,
            IInitiatorRulebookHandler<RuleCalculateDamage>, IRulebookHandler<RuleCastSpell>, IInitiatorRulebookSubscriber, IRulebookHandler<RuleCalculateDamage>
        {

            public bool ConditionForExecution(MechanicsContext context, bool AtCastEvent = true)
            {
                bool IsOk = true;
                if (AtCastEvent && RestrictElement && !context.SpellDescriptor.HasFlag(ElementToSpellDescriptor(RestrictedElement)))
                    IsOk = false;
                if (RestrictSchool && (context.SpellSchool != RestrictedSchool || !context.SourceAbility.IsSpell))
                    IsOk = false;
                if (!context.SpellDescriptor.HasAnyFlag(SpellDescriptor.Acid | SpellDescriptor.Cold | SpellDescriptor.Fire | SpellDescriptor.Electricity))
                    IsOk = false;
                if (context.SourceAbility.Type == AbilityType.Extraordinary)
                    IsOk = false;
                return IsOk;

            }
            public bool CheckEnergy(DamageEnergyType damagetype)
            {
                bool IsOk = true;
                if (RestrictElement && damagetype != RestrictedElement)
                    IsOk = false;
                if (damagetype != DamageEnergyType.Acid && damagetype != DamageEnergyType.Cold && damagetype != DamageEnergyType.Electricity && damagetype != DamageEnergyType.Fire)
                    IsOk = false;
                return IsOk;
            }

            public void OnEventAboutToTrigger(RuleCastSpell evt)
            {
            }

            public void OnEventDidTrigger(RuleCastSpell evt)
            {

                if (!ConditionForExecution(evt.Context))
                    return;

                if (resource != null && Owner.Resources.GetResourceAmount(resource) < 1)
                {
                    Fact.Deactivate();
                    return;
                }
                if (resource != null && Owner.Resources.GetResourceAmount(resource) > 0)
                    Owner.Resources.Spend(resource, 1);
                AbilityExecutionContext context = evt.Context;



                context.RemoveSpellDescriptor(SpellDescriptor.Fire);
                context.RemoveSpellDescriptor(SpellDescriptor.Cold);
                context.RemoveSpellDescriptor(SpellDescriptor.Acid);
                context.RemoveSpellDescriptor(SpellDescriptor.Electricity);
                context.AddSpellDescriptor(ElementToSpellDescriptor(this.Element));
            }

            public void OnEventAboutToTrigger(RuleCalculateDamage evt)
            {

                var ability = evt.Reason.Context.SourceAbility;
                if (ability == null || !ConditionForExecution(evt.Reason.Context, false))
                    return;


                foreach (BaseDamage baseDamage in evt.DamageBundle)
                {
                    EnergyDamage energyDamage = baseDamage as EnergyDamage;
                    if (energyDamage != null)
                    {
                        if (CheckEnergy(energyDamage.EnergyType))
                            energyDamage.ReplaceEnergy(this.Element);
                    }
                }
            }

            public void OnEventDidTrigger(RuleCalculateDamage evt)
            {
            }
            private static SpellDescriptor ElementToSpellDescriptor(DamageEnergyType element)
            {
                switch (element)
                {
                    case DamageEnergyType.Fire:
                        return SpellDescriptor.Fire;
                    case DamageEnergyType.Cold:
                        return SpellDescriptor.Cold;
                    case DamageEnergyType.Electricity:
                        return SpellDescriptor.Electricity;
                    case DamageEnergyType.Acid:
                        return SpellDescriptor.Acid;
                }
                return SpellDescriptor.Fire;
            }

            public DamageEnergyType Element;
            public bool RestrictElement;
            //[ShowIf("RestrictElement")]
            public DamageEnergyType RestrictedElement;
            public bool RestrictSchool;
            //[ShowIf("RestrictSchool")]
            public SpellSchool RestrictedSchool;
            public BlueprintAbilityResource resource;

        }

        static void CreateTeleportationSubSchool()
        {
            string name = "TeleportationSchool";
            string Name = "Focused School -- Teleportation";
            string basedesc = "Whenever you cast a conjuration (summoning) spell, increase the duration by a number of rounds equal to 1/2 your wizard level (minimum 1). This increase is not doubled by Extend Spell.";

            string TPname = "ShiftAbility";
            string TPName = "Shift";
            string TPfeat = "ShiftFeature";
            string TPdesc = "At 1st level, you can teleport to a nearby space as a swift action as if using dimension door. This movement does not provoke an attack of opportunity. You must be able to see the space that you are moving into." +
                " You cannot take other creatures with you when you use this ability (except for familiars). You can move 5 feet for every two wizard levels you possess (minimum 5 feet). " +
                "You can use this ability a number of times per day equal to 3 + your Intelligence modifier.";

            BlueprintFeature ConjurationBase = library.Get<BlueprintFeature>("cee0f7edbd874a042952ee150f878b84");
            BlueprintFeature ConjurationGreat = library.Get<BlueprintFeature>("71293f6177954334697ca44a9ddf7090");
            BlueprintAbility TravelAbility = library.Get<BlueprintAbility>("867e6fd88d089c442be7cdd49f05a88e");
            string vardesc = "Summoner Smile: " + basedesc + "\nShift(Su): " + TPdesc + "\n" + ConjurationGreat.Name + ": " + ConjurationGreat.Description;

            var TPresource = SchoolUtility.GetWizardBaseResource(SpellSchool.Conjuration);


            var ability = library.CopyAndAdd<BlueprintAbility>(TravelAbility, TPname, Helpers.getGuid(TPname));
            ability.SetNameDescription(TPName, TPdesc);
            ability.ActionType = CommandType.Swift;
            ability.Range = AbilityRange.Custom;
            ability.CustomRange = 5.Feet();
            ability.Type = AbilityType.Supernatural;
            ability.ReplaceComponent<AbilityResourceLogic>(r => r.RequiredResource = TPresource.resource);

            var Teleportfeat = Helpers.CreateFeature(TPfeat, TPName, TPdesc, Helpers.getGuid(TPfeat), ability.Icon, FeatureGroup.Domain, ability.CreateAddFact());
            Teleportfeat.ReapplyOnLevelUp = false;

            BlueprintComponent[] addcompos = new BlueprintComponent[10];
            BlueprintFeature[] featurebylevel = new BlueprintFeature[10];
            BlueprintAbility[] copies = new BlueprintAbility[10];
            for (int i = 0; i < 9; i++)
            {
                //create a copy with the right range
                copies[i] = library.CopyAndAdd<BlueprintAbility>(ability, TPname + i, Helpers.getGuid(TPname + i));
                copies[i].CustomRange = (5 + (5 * (i + 1))).Feet();

                //create feature to replace the ability
                featurebylevel[i] = Helpers.CreateFeature("Update" + TPname + i, "Shift Range Increase", "At level 4 and every other level thereafter the range of the Shift ability increase by 5ft.\n Shift's Range is " + (5 + 5 * (i + 1)) + " feet.", Helpers.getGuid("Update" + TPname + i), ability.Icon, FeatureGroup.Domain,

                    copies[i].CreateAddFact()
                    );
                featurebylevel[i].HideInUI = false;
                featurebylevel[i].AddComponent(Helpers.Create<RemoveFeatureOnApply>(c => c.Feature = i == 0 ? (BlueprintUnitFact)Teleportfeat : (BlueprintUnitFact)featurebylevel[i - 1]));
                //create the add feature component


            }
            var TPbasefeature = SchoolUtility.CreateFeature("TeleportationBaseFeature", "Summoner's Charm", basedesc, ConjurationBase.Icon, 1, false, false,
                SchoolUtility.SpeciaListComponent(SpellSchool.Conjuration), TPresource.add,
                Helpers.Create<AddClassLevelToSummonDuration>(s =>
                {
                    s.CharacterClass = wizardclass;
                    s.Half = true;

                }));

            var TPSchoolVariant = SchoolUtility.CreateSchoolVariantProgression(ConjurationProgression, name, Name, vardesc, true,
                SchoolUtility.BuildLevelEntry(
                    (1, TPbasefeature),
                    (1, Teleportfeat),
                    (1, OppositionSchool),
                    (1, OppositionSchool),
                    (4, featurebylevel[0]),
                    (6, featurebylevel[1]),
                    (8, featurebylevel[2]),
                    (8, ConjurationGreat),
                    (10, featurebylevel[3]),
                    (12, featurebylevel[4]),
                    (14, featurebylevel[5]),
                    (16, featurebylevel[6]),
                    (18, featurebylevel[7]),
                    (20, featurebylevel[8])


                    ));
            UIGroup shiftuigroup = new UIGroup();
            shiftuigroup.Features.Add(Teleportfeat);
            shiftuigroup.Features.AddRange(featurebylevel);

            TPSchoolVariant.UIGroups = TPSchoolVariant.UIGroups.AddToArray(shiftuigroup);
            var oppositionconjur = library.Get<BlueprintFeature>("ca4a0d68c0408d74bb83ade784ebeb0d");
            oppositionconjur.AddComponent(Helpers.PrerequisiteNoFeature(TPSchoolVariant));





        }
        static void CreateLifeSubSchool()
        {
            string name = "LifeSchool";
            string basedisplay = "Healing Grace and Share Essence";

            string Gracename = "HealingGrace";
            string gracedisplay = "Healing Grace";
            string gracedesc = "Whenever you cast a spell, you gain a \"healing pool\" for an amount equal to the level of the spell." +
                "Then, for a short time, you may use a free action to heal creatures targeted by the spell for a total of 1 point of damage healed per point of your healing pool. " +
                "If you assign any of the healing to an undead creature, it instead takes 1 point of damage for each point assigned. Any point remaining in your healing pool after this first target has been affected will be automatically assigned to another valid target." +
                "If you cast another spell while you still have point in your healing pool, they will be automatically expended and creature will be healed or damaged accordingly." +
                "At 11th level, the healing pool total gained when casting a spell is equal to 2 points per level of the spell. " +
                "At 20th level, the total increase to 3 points  per level of the spell.";
            string gracetargetdesc = "This ability allows you to spend your healing pool on a valid target with a free action. The target will be healed immediatly (or damaged if it is undead) by an amount equal to the total of the healing pool." +
                "If this total would exceed the target's missing hit point (or hit point left, in case of damage), another valid target will be affected by the remaining points in the pool, and so on until the pool is empty or there is no more valid targets." +
                "A valid target is any creature on which you casted a spell recently or that was in the area of effect of one of your spells.  ";

            string gracetoggledesc = "You can activate this abiity to automatically use Healing Grace on a valid target very shortly after casting a spell. This target will be the ally wich has taken the most damage or the undead ennemy with he most hp left, wichever is higher among the valid target.";

            string sharename = "ShareEssence";
            string sharedisplay = "Share Essence";
            string sharedesc = "As a standard action, you can share your vital energy with a living creature that you touch. You take 1d6 points of nonlethal damage + 1 for every two wizard levels you possess. " +
                "You cannot take an amount of nonlethal damage equal to or greater than your current hit point total; any excess is prevented. " +
                "The recipient gains a number of temporary hit points equal to the amount of damage you received (prevented damage is not counted). " +
                "These temporary hit points disappear 1 hour later. You may not use this ability to grant yourself temporary hit points. " +
                "You can use this ability a number of times per day equal to 3 + your Intelligence modifier. This ability has no effect if you are immune to nonlethal damage.";

            var necrogreater = library.Get<BlueprintFeature>("82371e899df830e4bb955429d89b755c");
            string basedesc = gracedisplay + "(Su): " + gracedesc + "\n" + sharedisplay + "(Sp): " + sharedesc;
            string vardesc = basedesc + "\n" + necrogreater.Name + "(Su): " + necrogreater.Description;
            var schoolicon = NecromancyProgression.Icon;
            var baseicon = NecromancyBaseFeature.Icon;
            var shareicon = Helpers.GetIcon("6cbb040023868574b992677885390f92");
            var FxGraceBuff = Helpers.GetFx("cbfe312cb8e63e240a859efaad8e467c");//Common necro buff fx
            var FxGraceCast = Helpers.GetFx("61602c5b0ac793d489c008e9cb58f631");//cure light wound
            var FxEssenceBuff = Helpers.GetFx("afc207ce60f237746a561a2877ec29e3");//Vampiric touch buff fx
            var FxEssenceCast = Helpers.GetFx("e93261ee4c3ea474e923f6a645a3384f");// False life fx on cast
            var FxEssenceCastOnCaster = Helpers.GetFx("7bcb8adb7d3292f458bd616af9e6b743");//Vampiric shield hit target


            //CREATING HEALING GRACE

            // Resource 
            var gracepool = Helpers.CreateAbilityResource(Gracename + "pool", "", "", Helpers.getGuid(Gracename + "pool"), baseicon);
            gracepool.SetFixedResource(0);

            var gracepooladd = gracepool.CreateAddAbilityResource();
            gracepooladd.RestoreAmount = false;
            gracepooladd.RestoreOnLevelUp = false;

            var gracepoollogic = Helpers.CreateResourceLogic(gracepool, true);


            //Status Buff to select who can recieve Healing grace
            var gracestatus = Helpers.CreateBuff(Gracename + "StatusBuff", "Healing Grace Target", "This creature is a valid target for the Healing Grace ability", Helpers.getGuid(Gracename + "StatusBuff"), schoolicon, FxGraceBuff, null);
            gracestatus.Stacking = StackingType.Stack;

            // Buff to signal that Healing Grace should be used automatically
            var autograce = Helpers.CreateBuff(Gracename + "AutoGraceBuff", "Healing Grace(Auto)", gracetoggledesc, Helpers.getGuid(Gracename + "AutoGraceBuff"), schoolicon, null, null);

            //The usable ability of healing grace
            var gracetarget = Helpers.CreateAbility(Gracename + "AbilityTargeted", gracedisplay, gracetargetdesc, Helpers.getGuid(Gracename + "AbilityTargeted"), baseicon, AbilityType.Supernatural, CommandType.Free, AbilityRange.Long, "Instantaneous", "",
                gracepoollogic,
                Helpers.Create<AbilityTargetHasBuffByCaster>(f => {
                    f.CheckedFacts = new BlueprintUnitFact[] { gracestatus };
                    f.Inverted = false;
                }),
                Helpers.CreateRunActions(Helpers.Create<ContextActionHealinGrace>(a => { a.pool = gracepool; a.status = gracestatus; a.PrefabLink = FxGraceCast; })),
                Helpers.Create<AbilitySpawnFx>(f => {
                    f.PrefabLink = FxGraceCast;
                    f.Anchor = AbilitySpawnFxAnchor.SelectedTarget;
                    f.PositionAnchor = AbilitySpawnFxAnchor.None;
                    f.OrientationAnchor = AbilitySpawnFxAnchor.None;
                    f.DestroyOnCast = true;
                })
                );
            gracetarget.CanTargetEnemies = true;
            gracetarget.CanTargetFriends = true;
            gracetarget.CanTargetSelf = true;
            gracetarget.CanTargetPoint = false;
            gracetarget.Animation = Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle.Directional;
            gracetarget.ResourceAssetIds = new string[] { FxGraceBuff.AssetId, FxGraceCast.AssetId };
            //Toggle for the autobuff
            var autogracetoggle = Helpers.CreateActivatableAbility(Gracename + "ToggleAuto", "Toggle Automatic Healing Grace", gracetoggledesc, Helpers.getGuid(Gracename + "ToggleAuto"), schoolicon, autograce, AbilityActivationType.Immediately, CommandType.Free, null);
            autogracetoggle.DeactivateImmediately = true;
            //Token Buff that we use to cast it on automatic mod and to expend all resource after a certain time
            var gracetoken = Helpers.CreateBuff(Gracename + "TokenBuff", "", "", Helpers.getGuid(Gracename + "TokenBuff"), baseicon, null, null, Helpers.Create<RemovePoolOnTimer>(r => { r.pool = gracepool; r.autobuff = autograce; r.Grace = gracetarget; r.status = gracestatus; }));
            //gracetoken.SetBuffFlags(BuffFlags.HiddenInUi);
            gracetoken.Stacking = StackingType.Replace;

            //The main Feature of Healing grace
            var Grace = Helpers.CreateFeature(Gracename + "Feature", gracedisplay, gracedesc, Helpers.getGuid(Gracename + "Feature"), baseicon, FeatureGroup.None,
                gracetarget.CreateAddFact(),
                autogracetoggle.CreateAddFact(),
                Helpers.Create<ManageHealingGrace>(p => { p.pool = gracepool; p.status = gracestatus; p.token = gracetoken; }),
                gracepooladd
                );

            //CREATING SHARE ESSENCE

            var EssenceCasterBuff = Helpers.CreateBuff(sharename + "CasterBuff", sharedisplay, sharedesc, Helpers.getGuid(sharename + "CasterBuff"), shareicon, null, null,
               Helpers.Create<AddContextStatBonus>(c => {
                   c.Stat = StatType.DamageNonLethal;
                   c.Multiplier = 1;
                   c.Descriptor = ModifierDescriptor.UntypedStackable;
                   c.Value = new ContextValue() { ValueType = ContextValueType.Shared, ValueShared = AbilitySharedValue.StatBonus };
               }));
            EssenceCasterBuff.SetBuffFlags(BuffFlags.RemoveOnRest | BuffFlags.HiddenInUi);
            EssenceCasterBuff.Stacking = StackingType.Stack;

            var EssenceTargetBuff = Helpers.CreateBuff(sharename + "TargetBuff", sharedisplay, sharedesc, Helpers.getGuid(sharename + "TargetBuff"), shareicon, FxEssenceBuff, null,
                Helpers.Create<AddContextStatBonus>(c => {
                    c.Stat = StatType.TemporaryHitPoints;
                    c.Multiplier = 1;
                    c.Descriptor = ModifierDescriptor.Profane;
                    c.Value = new ContextValue() { ValueType = ContextValueType.Shared, ValueShared = AbilitySharedValue.StatBonus };
                }));
            EssenceTargetBuff.SetBuffFlags(BuffFlags.RemoveOnRest | BuffFlags.HiddenInUi);
            EssenceTargetBuff.Stacking = StackingType.Stack;
            var resource = SchoolUtility.GetWizardBaseResource(SpellSchool.Necromancy);

            var ShareEssence = Helpers.CreateAbility(sharename + "Ability", sharedisplay, sharedesc, Helpers.getGuid(sharename + "Ability"), shareicon, AbilityType.SpellLike,
                CommandType.Standard, AbilityRange.Touch, "Instantaneous", "",
                resource.logic,
                Helpers.CreateRunActions(Helpers.Create<ContextActionDeal>(a => { a.ResultSharedValue = AbilitySharedValue.StatBonus; a.WriteResultToSharedValue = true; }),
                Helpers.CreateApplyBuff(EssenceCasterBuff, Helpers.CreateContextDuration(), false, false, true, false, true),
                Helpers.CreateApplyBuff(EssenceTargetBuff, Helpers.CreateContextDuration(1, DurationRate.Hours), false)),
                Helpers.Create<AbilitySpawnFx>(f => {
                    f.PrefabLink = FxEssenceCast;
                    f.Anchor = AbilitySpawnFxAnchor.SelectedTarget;
                    f.PositionAnchor = AbilitySpawnFxAnchor.None;
                    f.OrientationAnchor = AbilitySpawnFxAnchor.None;
                    f.DestroyOnCast = true;
                }),
                Helpers.Create<AbilitySpawnFx>(f => {
                    f.PrefabLink = FxEssenceCastOnCaster;
                    f.Anchor = AbilitySpawnFxAnchor.Caster;
                    f.PositionAnchor = AbilitySpawnFxAnchor.None;
                    f.OrientationAnchor = AbilitySpawnFxAnchor.None;
                    f.DestroyOnCast = true;
                })

                );
            ShareEssence.CanTargetFriends = true;
            ShareEssence.CanTargetSelf = false;
            ShareEssence.Animation = Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle.Touch;
            ShareEssence.EffectOnAlly = AbilityEffectOnUnit.Helpful;
            ShareEssence.ResourceAssetIds = new string[] { FxEssenceBuff.AssetId, FxEssenceCast.AssetId, FxEssenceCastOnCaster.AssetId };

            var LifeBaseFeature = SchoolUtility.CreateFeature(name + "BaseFeature", basedisplay, basedesc, baseicon, components: new BlueprintComponent[]{ShareEssence.CreateAddFact(),
                Grace.CreateAddFact(),
                resource.add,
                SchoolUtility.SpeciaListComponent(SpellSchool.Necromancy)}
                );
            var progression = SchoolUtility.CreateSchoolVariantProgression(NecromancyProgression, name, "Focused School - Life", vardesc, true,
                SchoolUtility.BuildLevelEntry(
                    (1, LifeBaseFeature),
                    (1, OppositionSchool),
                    (1, OppositionSchool),
                    (8, necrogreater)
                ));
            var oppositionnecro = library.Get<BlueprintFeature>("a9bb3dcb2e8d44a49ac36c393c114bd9");
            oppositionnecro.AddComponent(Helpers.PrerequisiteNoFeature(progression));


        }


        public class ContextActionDeal : ContextAction
        {

            public override string GetCaption()
            {
                return string.Format("doing some shit");
            }

            public override void RunAction()
            {
                UnitEntityData Caster = Context.MaybeCaster;
                UnitEntityData Target = Context.MainTarget.Unit;
                if (Caster == null)
                {
                    UberDebug.LogError(this, "Can't apply buff: Caster is null", Array.Empty<object>());
                    return;
                }
                if (Target == null)
                {
                    UberDebug.LogError(this, "Can't apply buff: Target is null", Array.Empty<object>());
                    return;
                }

                RuleRollDice ruleRollDice = new RuleRollDice(Caster, new DiceFormula(1, DiceType.D6));
                Rulebook.Trigger<RuleRollDice>(ruleRollDice);
                int bonus = Math.Max(Caster.Descriptor.Progression.GetClassLevel(wizardclass) / 2, 1);
                int prevalue = ruleRollDice.Result + bonus;
                int hpleft = Caster.MaxHP - (Caster.Damage + Caster.DamageNonLethal);
                int value = Math.Min(hpleft - 1, prevalue);


                if (this.WriteResultToSharedValue)
                {
                    base.Context[this.ResultSharedValue] = value;
                }

            }


            public bool WriteResultToSharedValue;

            public AbilitySharedValue ResultSharedValue;


        }
        public class RemovePoolOnTimer : OwnedGameLogicComponent<UnitDescriptor>
        {
            public override void OnFactActivate()
            {
                var main = AccessTools.TypeByName("TurnBased.Main, TurnBased");
                bool IsInTB = false;
                if (main != null) //Turnbased is installed
                {
                    var mod = AccessTools.Field(main, "Mod").GetValue(null);
                    var core = AccessTools.Property(mod.GetType(), "Core").GetValue(mod);
                    IsInTB = (bool)AccessTools.Property(core.GetType(), "Enabled").GetValue(core);//TB activated
                }
                bool autocast = Owner.Buffs.HasFact(autobuff);
                float Timemod = 0;

                if (autocast)

                {
                    if (IsInTB && Owner.Unit.IsInCombat)
                        Timemod += 0.01f;
                    else
                        Timemod = 0;
                }
                else
                {

                    if (IsInTB && Owner.Unit.IsInCombat)
                        Timemod += 0.01f;
                    else
                        Timemod += 10f;

                }

                Buff buff = (Buff)base.Fact;
                buff.EndTime = Timemod > 0 ? Game.Instance.TimeController.GameTime + Timemod.Seconds() : buff.EndTime;
            }

            public override void OnFactDeactivate()
            {
                if (Owner.Resources.GetResourceAmount(pool) > 0)
                {
                    int EffectPotential = 0;
                    UnitEntityData UnitToAffect = Owner.Unit;
                    foreach (UnitEntityData unit in Game.Instance.State.Units)
                    {
                        if (!unit.Descriptor.State.IsDead)
                        {
                            if ((unit.IsAlly(Owner.Unit) && !unit.Descriptor.HasFact(Undead)) || (unit.IsEnemy(Owner.Unit) && unit.Descriptor.HasFact(Undead)))
                            {
                                if (unit.IsUnitInRange(Owner.Unit.Position, 90.Feet().Meters, true))
                                {
                                    if (unit.Descriptor.HasFact(status))
                                    {
                                        if ((unit.IsAlly(Owner.Unit) && unit.Damage > EffectPotential) || (unit.IsEnemy(Owner.Unit) && unit.HPLeft > EffectPotential))
                                        {
                                            EffectPotential = unit.IsAlly(Owner.Unit) ? unit.Damage : unit.IsEnemy(Owner.Unit) ? unit.HPLeft : EffectPotential;
                                            UnitToAffect = unit;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    bool notarget = (UnitToAffect == Owner.Unit && !Owner.HasFact(status)) || (UnitToAffect == Owner.Unit && Owner.Unit.Damage < 1);
                    if (!notarget)
                    {
                        TargetWrapper Target = new TargetWrapper(UnitToAffect);
                        Ability GraceTocast = new Ability(Grace, Owner);
                        AbilityData GraceData = new AbilityData(Grace, Owner);
                        UnitUseAbility unitUseAbility = new UnitUseAbility(GraceData, Target);
                        unitUseAbility.IgnoreCooldown(null);
                        AbilityExecutionContext GraceContext = new AbilityExecutionContext(GraceData, new AbilityParams(), Target);
                        Owner.Resources.Spend(pool, 1);
                        GraceData.Cast(GraceContext).InstantDeliver();
                    }
                    else
                        Owner.Resources.Restore(pool, 200);

                }
            }
            public BlueprintAbility Grace;
            public BlueprintBuff status;
            public BlueprintAbilityResource pool;
            public BlueprintBuff autobuff;
        }
        public class AbilityTargetHasBuffByCaster : BlueprintComponent, IAbilityTargetChecker
        {
            public bool CanTarget(UnitEntityData caster, TargetWrapper target)
            {
                UnitEntityData unit = target.Unit;
                if (unit == null)
                {
                    return false;
                }
                bool flag = false;
                bool flag2 = false;
                foreach (BlueprintUnitFact blueprint in this.CheckedFacts)
                {
                    flag = unit.Descriptor.HasFact(blueprint);

                    if (flag)
                    {
                        int count = 0;
                        foreach (Buff buff in unit.Descriptor.Buffs)
                        {
                            if (buff.Context.MaybeCaster == caster && buff.Blueprint == blueprint)
                                count++;

                        }
                        if (count > 0) flag2 = true;

                        break;
                    }
                }
                return flag2 != this.Inverted;
            }

            public BlueprintUnitFact[] CheckedFacts;
            public bool Inverted;
        }
        public class ManageHealingGrace : RuleInitiatorLogicComponent<RuleCastSpell>, IApplyAbilityEffectHandler
        {

            public void OnAbilityEffectApplied(AbilityExecutionContext context) { }
            public void OnTryToApplyAbilityEffect(AbilityExecutionContext context, TargetWrapper target)
            {


                if (context.Caster == Owner.Unit && context.Ability.Blueprint.Type == AbilityType.Spell && !context.Ability.IsSpellCopy && target.IsUnit)
                {
                    if (count == 0)
                    {

                        int amount = context.Caster.Descriptor.Resources.GetResourceAmount(pool);
                        context.Caster.Descriptor.Resources.Spend(pool, amount);

                        int ownerlevel = Owner.Progression.GetClassLevel(wizardclass);
                        int multiplier = ownerlevel >= 20 ? 3 : ownerlevel >= 11 ? 2 : 1;
                        int poolgain = (context.SpellLevel) * multiplier;
                        context.Caster.Descriptor.Resources.Spend(pool, -poolgain);

                        var contexpartymember = context.Ability.Blueprint.GetComponent<AbilityEffectRunAction>()?.Actions?.Actions?.OfType<ContextActionPartyMembers>()?.FirstOrDefault();
                        if (contexpartymember != null)
                        {
                            UnitGroup group = Game.Instance.Player.MainCharacter.Value.Group;
                            for (int i = 0; i < group.Count; i++)
                            {
                                bool Present = false;
                                var member = group[i];
                                foreach (Buff buff in member.Buffs)
                                {
                                    if (buff.Blueprint == status && buff.Context.MaybeCaster == context.MaybeCaster)
                                    {
                                        Present = true;
                                        break;
                                    }
                                }
                                if (!Present)
                                    member.Descriptor.AddBuff(status, context, new TimeSpan?(2.Rounds().Seconds));
                            }

                        }
                    }
                    count++;
                    bool present = false;
                    foreach (Buff buff in target.Unit.Buffs)
                    {
                        if (buff.Blueprint == status && buff.Context.MaybeCaster == context.MaybeCaster)
                        {
                            present = true;
                            break;
                        }
                    }
                    if (!present)
                        target.Unit.Descriptor.AddBuff(status, context, new TimeSpan?(2.Rounds().Seconds));



                }


            }
            public void OnAbilityEffectAppliedToTarget(AbilityExecutionContext context, TargetWrapper target) { }

            public override void OnEventAboutToTrigger(RuleCastSpell evt)
            {
                if (evt.Spell.Blueprint.Type == AbilityType.Spell && evt.Spell.SpellLevel > 0)
                {
                    count = 0;
                    Owner.AddBuff(token, evt.Context, new TimeSpan?(2.Seconds()));
                }

            }
            public override void OnEventDidTrigger(RuleCastSpell evt)
            {
            }

            private int count = 0;
            public BlueprintBuff token;
            public BlueprintBuff status;
            public BlueprintAbilityResource pool;
        }
        public class ContextActionHealinGrace : ContextAction
        {
            public override string GetCaption()
            {
                return string.Format("Heal or Deal Damage");
            }

            public override void RunAction()
            {

                if (base.Target.Unit == null || !Target.Unit.Descriptor.HasFact(status))
                {
                    UberDebug.LogError(this, "Invalid target for effect '{0}'", new object[]
                    {
                    base.GetType().Name
                    });
                    return;
                }
                if (base.Context.MaybeCaster == null)
                {
                    UberDebug.LogError(this, "Caster is missing", Array.Empty<object>());
                    return;
                }

                int bonus = Context.MaybeCaster.Descriptor.Resources.GetResourceAmount(pool);

                if (!Target.Unit.Descriptor.HasFact(Undead))
                {
                    int HealingTotal = bonus + 1;
                    int TargetMissingHP = Target.Unit.MaxHP - Target.Unit.HPLeft;
                    int HealingDone = TargetMissingHP < HealingTotal ? TargetMissingHP : HealingTotal;
                    Context.TriggerRule<RuleHealDamage>(new RuleHealDamage(base.Context.MaybeCaster, base.Target.Unit, DiceFormula.Zero, HealingDone));
                    HealingTotal -= HealingDone;

                    if (HealingTotal > 0)
                    {
                        int countguard = 1;
                        while (HealingTotal > 0)
                        {
                            UnitEntityData newtarget = FindAnotherTarget(Context.MaybeCaster, false);
                            if (newtarget == Context.MaybeCaster && !newtarget.Descriptor.HasFact(status))
                                break;
                            int newTargetMissingHP = newtarget.MaxHP - newtarget.HPLeft;
                            int newHealingDone = newTargetMissingHP < HealingTotal ? newTargetMissingHP : HealingTotal;
                            Context.TriggerRule<RuleHealDamage>(new RuleHealDamage(base.Context.MaybeCaster, newtarget, DiceFormula.Zero, newHealingDone));
                            var prefab = PrefabLink.Load(false);
                            FxHelper.SpawnFxOnUnit(prefab, newtarget.View, null, default(Vector3));
                            HealingTotal -= newHealingDone;

                            if (countguard > Game.Instance.State.Units.Count)
                                break;
                            countguard++;

                        }
                    }

                    Context.MaybeCaster.Descriptor.Resources.Spend(pool, bonus);

                }
                else
                {
                    int DamageTotal = bonus + 1;
                    int DamageDone = Target.Unit.HPLeft < DamageTotal ? Target.Unit.HPLeft : DamageTotal;

                    DirectDamage directDamage = new DirectDamage(new DiceFormula(0, DiceType.D10), DamageDone);
                    DamageBundle damage = new DamageBundle(new BaseDamage[]
                    {
                        directDamage
                    });
                    RuleDealDamage evt = new RuleDealDamage(Context.MaybeCaster, Target.Unit, damage);
                    Rulebook.Trigger<RuleDealDamage>(evt);
                    DamageTotal -= DamageDone;

                    if (DamageTotal > 0)
                    {
                        int countguard = 1;
                        while (DamageTotal > 0)
                        {
                            UnitEntityData newtarget = FindAnotherTarget(Context.MaybeCaster, true);
                            if (newtarget == Context.MaybeCaster && !newtarget.Descriptor.HasFact(status))
                                break;

                            int newDamageDone = newtarget.HPLeft < DamageTotal ? newtarget.HPLeft : DamageTotal;

                            DirectDamage newdirectDamage = new DirectDamage(new DiceFormula(0, DiceType.D10), newDamageDone);
                            DamageBundle newdamage = new DamageBundle(new BaseDamage[]
                            {
                        newdirectDamage
                            });
                            RuleDealDamage newevt = new RuleDealDamage(Context.MaybeCaster, Target.Unit, newdamage);
                            Rulebook.Trigger<RuleDealDamage>(newevt);
                            var prefab = PrefabLink.Load(false);
                            FxHelper.SpawnFxOnUnit(prefab, newtarget.View, null, default(Vector3));
                            DamageTotal -= newDamageDone;

                            if (countguard > Game.Instance.State.Units.Count)
                                break;
                            countguard++;

                        }
                    }

                    Context.MaybeCaster.Descriptor.Resources.Spend(pool, bonus);
                }

                foreach (UnitEntityData unitEntityData in Game.Instance.State.Units)
                {
                    if (unitEntityData.Descriptor.Buffs.HasFact(status))
                    {

                        Buff CasterBuff = FindCasterBuff(unitEntityData);
                        if (CasterBuff != null)
                            unitEntityData.Buffs.RemoveFact(CasterBuff);
                    }
                }
            }
            public Buff FindCasterBuff(UnitEntityData unit)
            {
                foreach (Buff buff in unit.Descriptor.Buffs)
                {
                    if (buff.Context.MaybeCaster == Context.MaybeCaster && buff.Blueprint == status)
                        return buff;

                }
                return null;

            }

            public UnitEntityData FindAnotherTarget(UnitEntityData caster, bool searchenemy)
            {
                int EffectPotential = 0;
                UnitEntityData UnitToAffect = caster;
                foreach (UnitEntityData unit in Game.Instance.State.Units)
                {
                    if (!unit.Descriptor.State.IsDead)
                    {
                        if ((unit.IsAlly(caster) && !unit.Descriptor.HasFact(Undead) && !searchenemy) || (unit.IsEnemy(caster) && unit.Descriptor.HasFact(Undead) && searchenemy))
                        {
                            if (unit.IsUnitInRange(caster.Position, 60.Feet().Meters, true))
                            {
                                if (FindCasterBuff(unit) != null)
                                {
                                    if ((unit.IsAlly(caster) && unit.Damage > EffectPotential) || (unit.IsEnemy(caster) && unit.HPLeft > EffectPotential))
                                    {
                                        EffectPotential = unit.IsAlly(caster) ? unit.Damage : unit.IsEnemy(caster) ? unit.HPLeft : EffectPotential;
                                        UnitToAffect = unit;
                                    }
                                }
                            }
                        }
                    }
                }
                return UnitToAffect;
            }
            public PrefabLink PrefabLink;
            public BlueprintAbilityResource pool;
            public BlueprintBuff status;
        }

        static void CreatePhantasmSubSchool()
        {
            string name = "PhantasmSchool";
            string terrorname = "Terror";
            string terrordesc = "As a standard action, you can make a melee touch attack that causes a creature to be assailed by nightmares only it can see. " +
                "The creature provokes an attack of opportunity from the strongest ally in range. Creatures with more Hit Dice than your wizard level are unaffected. " +
                "This is a mind-affecting fear effect. You can use this ability a number of times per day equal to 3 + your Intelligence modifier.\n " +
                "Moder Note: The ally making the AoO will be the ally in range with the highest attack bonus.";
            string basename = "Terror and Deceptive Flourish";
            string flourishdesc = "You gain a +2 enhancement bonus on Bluff skill checks. This bonus increases by 1 for every 5 wizard levels you have, up to a maximum of +6 at 20th level. "/* +
                "You're also able to weave illusion magic into your spells, making them harder to decipher for the purpose of counterspelling, " +
                "the diffilculty to counter you spell is increased by 2, at lvl 10 the difficulty is increase by 4 and by 6 at level 20."*/;
            string BaseDesc = "Deceptive Flourish: " + flourishdesc + "\nTerror(Su): " + terrordesc;
            var illugreaterfeat = library.Get<BlueprintFeature>("f0585eb111ede2c4ebf00b057d069463");
            string VarDesc = "Illusionists use magic to weave confounding images, figments, and phantoms to baffle and vex their foes.\n" + BaseDesc + "\n" + illugreaterfeat.Name + " : " + illugreaterfeat.Description;
            Sprite icon = Helpers.GetIcon("6717dbaef00c0eb4897a1c908a75dfe5");
            Sprite schoolicon = IllusionProgression.Icon;
            var terrorfx = Helpers.GetFx("bd9712a252d288e4991ea29a8569e23b");

            string bedevilname = "BedevilingAura";
            string bedevildisplay = "Bedeviling Aura";
            string bedevildesc = "At 8th level, you can emit a 30-foot aura that bedevils your enemies with phantasmal assailants. Enemies within this aura move at half speed, are unable to take attacks of opportunity," +
                " and are considered to be flanked. This is a mind-affecting effect. You can use this ability for a number of rounds per day equal to your wizard level. " +
                "These rounds do not need to be consecutive";
            Sprite bedevilicon = Helpers.GetIcon("ecaa0def35b38f949bd1976a6c9539e0");
            var BedevilFxCast = Helpers.GetFx("790eb82d267bf0749943fba92b7953c2");
            var BedevilFxAura = Helpers.GetFx("dfadb7fa26de0384d9d9a6dabb0bea72");
            var BedevilFxBuff = Helpers.GetFx("396af91a93f6e2b468f5fa1a944fae8a");



            // TERROR ABILITY
            var baseresource = SchoolUtility.GetWizardBaseResource(SpellSchool.Illusion);
            var Terror = Helpers.CreateAbility(name + "AbilityCast", terrorname, terrordesc, Helpers.getGuid(name + "AbilityCast"), icon, AbilityType.Supernatural, CommandType.Standard, AbilityRange.Touch, "Instantaneous", "",
                Helpers.CreateDeliverTouch(),
                Helpers.CreateRunActions(Helpers.CreateConditional(Helpers.Create<ContextConditionHitDice>(c => { c.HitDice = 1; c.AddSharedValue = true; }),
                //if true:
                Helpers.Create<ContextActionProvokeAOO>()
                )),
                Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, min: 1, max: 20, classes: new BlueprintCharacterClass[] { wizardclass }),
                Helpers.CreateCalculateSharedValue(Helpers.CreateContextDiceValue(DiceType.Zero, new ContextValue(), new ContextValue() { ValueType = ContextValueType.Rank })),
                Helpers.CreateSpellDescriptor(SpellDescriptor.Fear | SpellDescriptor.MindAffecting),
                Helpers.Create<AbilitySpawnFx>(f => {
                    f.Anchor = AbilitySpawnFxAnchor.SelectedTarget;
                    f.PrefabLink = terrorfx;
                    f.PositionAnchor = AbilitySpawnFxAnchor.None;
                    f.OrientationAnchor = AbilitySpawnFxAnchor.None;
                }),
                Helpers.Create<AbilityTargetHasNoFactUnless>(f => {
                    f.CheckedFacts = new BlueprintUnitFact[] { Undead };
                    f.UnlessFact = library.Get<BlueprintFeature>("1a5e7191279e7cd479b17a6ca438498c");//bloodline undead arcana
                }),
                Helpers.Create<AbilityTargetHasFact>(f => {
                    f.CheckedFacts = new BlueprintUnitFact[] { Plant, Construct };
                    f.Inverted = true;
                }),
                 Helpers.Create<AbilityTargetHasNoFactUnless>(f => {
                     f.CheckedFacts = new BlueprintUnitFact[] { Vermine };
                     f.UnlessFact = library.Get<BlueprintFeature>("02707231be1d3a74ba7e38a426c8df37");//bloodline serpentine arcana
                 })
                );
            Terror.ResourceAssetIds = new string[] { terrorfx.AssetId };
            Terror.Animation = Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle.Touch;
            Terror.CanTargetEnemies = true;
            Terror.CanTargetFriends = true;
            Terror.CanTargetSelf = false;


            var TerrorCast = Helpers.CreateAbility(name + "Ability", terrorname, terrordesc, Helpers.getGuid(name + "Ability"), icon, AbilityType.Supernatural, CommandType.Standard, AbilityRange.Touch, "Instantaneous", "",
                baseresource.logic,
                Helpers.Create<AbilityEffectStickyTouch>(c => c.TouchDeliveryAbility = Terror)
                );
            TerrorCast.ResourceAssetIds = new string[] { terrorfx.AssetId };
            TerrorCast.Animation = Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle.Touch;
            TerrorCast.CanTargetEnemies = true;
            TerrorCast.CanTargetFriends = true;
            TerrorCast.CanTargetSelf = false;
            //TerrorCast.CustomRange = ;

            // BEDEVILING AURA

            var bedevileffectbuff = Helpers.CreateBuff(bedevilname + "EffectBuff", bedevildisplay, bedevildesc, Helpers.getGuid(bedevilname + "EffectBuff"), bedevilicon, BedevilFxBuff, null,
                Helpers.Create<AddCondition>(c => c.Condition = UnitCondition.Slowed),
                Helpers.Create<AddCondition>(c => c.Condition = UnitCondition.DisableAttacksOfOpportunity),
                Helpers.CreateSpellComponent(SpellSchool.Illusion),
                Helpers.Create<SpellDescriptorComponent>(d => d.Descriptor = SpellDescriptor.MindAffecting),
                Helpers.Create<AbilityTargetHasNoFactUnless>(f => {
                    f.CheckedFacts = new BlueprintUnitFact[] { Undead };
                    f.UnlessFact = library.Get<BlueprintFeature>("1a5e7191279e7cd479b17a6ca438498c");//bloodline undead arcana
                }),
                Helpers.Create<AbilityTargetHasFact>(f => {
                    f.CheckedFacts = new BlueprintUnitFact[] { Plant, Construct };
                    f.Inverted = true;
                }),
                 Helpers.Create<AbilityTargetHasNoFactUnless>(f => {
                     f.CheckedFacts = new BlueprintUnitFact[] { Vermine };
                     f.UnlessFact = library.Get<BlueprintFeature>("02707231be1d3a74ba7e38a426c8df37");//bloodline serpentine arcana
                 })
                 );
            bedevileffectbuff.SetBuffFlags(BuffFlags.Harmful);

            var bedevilarea = Helpers.Create<BlueprintAbilityAreaEffect>(a => {
                a.Fx = BedevilFxAura;
                a.AffectDead = false;
                a.AffectEnemies = true;
                a.AggroEnemies = true;
                a.Shape = AreaEffectShape.Cylinder;
                a.Size = 30.Feet();
                a.SpellResistance = false;
                a.name = bedevilname + "Area";

            });
            bedevilarea.SetComponents(
                Helpers.Create<AbilityAreaEffectBuff>(c => {
                    c.Condition = Helpers.CreateConditionsCheckerAnd(Helpers.Create<ContextConditionIsEnemy>()
                        );
                    c.Buff = bedevileffectbuff;
                }),
                Helpers.Create<AbilityTargetHasNoFactUnless>(f => {
                    f.CheckedFacts = new BlueprintUnitFact[] { Undead };
                    f.UnlessFact = library.Get<BlueprintFeature>("1a5e7191279e7cd479b17a6ca438498c");//bloodline undead arcana
                }),
                Helpers.Create<AbilityTargetHasFact>(f => {
                    f.CheckedFacts = new BlueprintUnitFact[] { Plant, Construct };
                    f.Inverted = true;
                }),
                 Helpers.Create<AbilityTargetHasNoFactUnless>(f => {
                     f.CheckedFacts = new BlueprintUnitFact[] { Vermine };
                     f.UnlessFact = library.Get<BlueprintFeature>("02707231be1d3a74ba7e38a426c8df37");//bloodline serpentine arcana
                 }),
                Helpers.CreateSpellComponent(SpellSchool.Illusion),
                Helpers.Create<SpellDescriptorComponent>(d => d.Descriptor = SpellDescriptor.MindAffecting)
                );
            library.AddAsset(bedevilarea, Helpers.getGuid(bedevilarea.name));

            var bedevilresource = SchoolUtility.CopySchoolresource("ccd9239740802bd4eab4cb751467205d", true);
            var bedevilbuff = Helpers.CreateBuff(bedevilname + "Buff", "", "", Helpers.getGuid(bedevilname + "Buff"), bedevilicon, BedevilFxCast, null,
                Helpers.Create<AddAreaEffect>(a => a.AreaEffect = bedevilarea)
                );
            bedevilbuff.SetBuffFlags(BuffFlags.HiddenInUi);

            var Illugreat = library.Get<BlueprintActivatableAbility>("8ba47f5bfecc69347b89d677fa0ccaf1");
            var bedevilability = Helpers.CreateActivatableAbility(bedevilname + "Ability", bedevildisplay, bedevildesc, Helpers.getGuid(bedevilname + "Ability"), bedevilicon, bedevilbuff, AbilityActivationType.WithUnitCommand, CommandType.Standard, Illugreat.ActivateWithUnitAnimation,
                bedevilresource.ToggleLogic);
            bedevilability.DeactivateImmediately = true;
            bedevilability.DeactivateIfCombatEnded = true;
            bedevilability.IsOnByDefault = false;
            bedevilability.OnlyInCombat = true;
            bedevilability.ResourceAssetIds = new string[] { BedevilFxCast.AssetId, BedevilFxBuff.AssetId, BedevilFxAura.AssetId };

            var bedevilfeature = Helpers.CreateFeature(bedevilname + "GreaterFeature", bedevildisplay, bedevildesc, Helpers.getGuid(bedevilname + "GreaterFeature"), bedevilicon, FeatureGroup.WizardFeat,
                bedevilresource.add,
                bedevilability.CreateAddFact());

            var CounterSpellResist = SchoolUtility.CreateFeature("IllusionistCounterSpellResist", "", "", icon, 1, true, true);
            var BaseFeature = SchoolUtility.CreateFeature(name + "BaseFeature", basename, BaseDesc, icon, 1, false, false,
                baseresource.add,
                TerrorCast.CreateAddFact(),
                CounterSpellResist.CreateAddFact(),
                Helpers.Create<ReplaceAbilitiesStat>(rs => { rs.Ability = new BlueprintAbility[] { TerrorCast }; rs.Stat = StatType.Intelligence; }),
                SchoolUtility.SpeciaListComponent(SpellSchool.Illusion),
                Helpers.CreateAddContextStatBonus(StatType.CheckBluff, ModifierDescriptor.Enhancement),
                Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, ContextRankProgression.StartPlusDivStep, min: 1, max: 5,
                startLevel: -5, stepLevel: 5, classes: new BlueprintCharacterClass[] { wizardclass })
                );
            var OppositionIllusion = library.Get<BlueprintFeature>("6750ead44c0c034428c6509c68110375");
            var Progression = SchoolUtility.CreateSchoolVariantProgression(IllusionProgression, name, "Focused School - Phantasm", VarDesc, true,
                SchoolUtility.BuildLevelEntry(
                    (1, BaseFeature),
                    (1, OppositionSchool),
                    (1, OppositionSchool),
                    (8, bedevilfeature)
                    ));

            OppositionIllusion.AddComponent(Helpers.PrerequisiteNoFeature(Progression));
            //Update to illusion base school (Adding Deceptive flourish)
            IllusionBaseFeature.AddComponents(Helpers.CreateAddContextStatBonus(StatType.CheckBluff, ModifierDescriptor.Enhancement), Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, ContextRankProgression.StartPlusDivStep, min: 1, max: 5,
                startLevel: -5, stepLevel: 5, classes: new BlueprintCharacterClass[] { wizardclass }));
            IllusionBaseFeature.SetDescription("Deceptive Flourish: " + flourishdesc + "\n" + IllusionBaseFeature.Description);
            IllusionProgression.SetDescription(IllusionProgression.Description + "\nDeceptive Flourish: " + flourishdesc);
        }

        public class ContextActionProvokeAOO : ContextAction
        {
            public override string GetCaption()
            {
                return "Target provokes AoO";
            }

            public override void RunAction()
            {
                UnitEntityData target = base.Target.Unit;
                UnitEntityData maybeCaster = base.Context.MaybeCaster;
                if (target == null || maybeCaster == null)
                {
                    return;
                }
                int count = 1;
                int strength = -10;
                UnitEntityData AllyToAttack = null;
                foreach (UnitEntityData Ally in target.CombatState.EngagedBy)
                {
                    count++;
                    /*if (target.CombatState.EngagedBy.Count() - count == 0)
                        break;*/
                    var mainweapon = Ally.GetThreatHand();
                    if (Ally.IsAlly(maybeCaster) && mainweapon != null)
                    {

                        var Allystrength = Rulebook.Trigger<RuleCalculateAttackBonus>(new RuleCalculateAttackBonus(Ally, target, mainweapon.MaybeWeapon, 0));
                        if (Allystrength.Result > strength)
                        {

                            strength = Allystrength.Result;
                            AllyToAttack = Ally;
                        }
                    }
                }
                if (AllyToAttack != null)
                    Game.Instance.CombatEngagementController.ForceAttackOfOpportunity(AllyToAttack, target);
            }
        }

        static void AddTurnbasedInterface()
        {
            var diviner = library.Get<BlueprintFeature>("54d21b3221ea82a4d90d5a91b7872f3d");//diviner base feature
            diviner.AddComponent(Helpers.Create<TurnbasedInterface>());
        }

        public class TurnbasedInterface : RuleInitiatorLogicComponent<RuleInitiativeRoll>
        {



            public override void OnEventAboutToTrigger(RuleInitiativeRoll evt)
            {
                var main = AccessTools.TypeByName("TurnBased.Main, TurnBased");
                if (main != null) //Turnbased is installed
                {
                    var mod = AccessTools.Field(main, "Mod").GetValue(null);
                    var core = AccessTools.Property(mod.GetType(), "Core").GetValue(mod);
                    var isEnabled = (bool)AccessTools.Property(core.GetType(), "Enabled").GetValue(core);

                    if (isEnabled)
                    {
                        var combat = AccessTools.Property(core.GetType(), "Combat").GetValue(core);
                        HashSet<UnitEntityData> unitsToSurprise = (HashSet<UnitEntityData>)AccessTools.Field(combat.GetType(), "_unitsToSurprise").GetValue(combat);
                        unitsToSurprise.Add(Owner.Unit);


                    }


                }

            }


            public override void OnEventDidTrigger(RuleInitiativeRoll evt)
            {
            }
        }

        static void CreatePoleiheiraAdherent()
        {
            //needed thing
            var icon = Helpers.GetIcon("de18c849c41dbfa44801d812376c707d");
            var iconbook = Helpers.GetIcon("7bdec6e495a95024685f82139447df87");//tome of understanding
            var iconmount = Helpers.GetIcon("816044bcf51aaa846a768f97aaad795e");// taldan horseshoe
            var icontravel = Helpers.GetIcon("4669e27501fdd6444810e69666f69ab5");//jublilost moon map
            string name = "PoleiheiraAdherent";
            string Archetype = "Poleiheira Adherent";
            string ArchetypeDesc = "Poleiheira adherents are wizards who partake in great odysseys. These adherents bond to a book known as the Poleiheira. This allows them to record their travels as well as any lost magic and lore they encounter.";
            string Bond = "Bonded Book";
            string BondDesc = "A Poleiheira adherent forms a bond with a spellbook. This bonded book becomes intrinsically tied to a Poleiheira adherent’s conscious and subconscious mind. The book always opens to the right page, and she can record any number of spells and other information in her bonded book—when she turns pages, more blank pages appear.\n" +
                "Each time a Poleiheira adherent attains a new wizard level, she gains four spells(rather than two) to add to the bonded book. And like all bonded item, the bonded book can be used once per day to restore any one spell that the wizard had prepared for this day.";
            string spellsupdesc = "Each time a Poleiheira adherent attains a new wizard level, she gains can add tow more spell to her spellbook";
            string Schoolname = "Great Odyssey";
            string SchoolDesc = "A Poleiheira adherent specializes in exploration and travel rather than a particular school of magic. She gains the abilities below.\n" +
                "    Mount : When she travel Poleiheira adherent can summon a magical steed to her side that lasts for 2 hours, this allow her to resist fatigue for as much time. The duration of this ability increase by one Hour on level 5, 10, 15 and 20\n" +
                "    Great Traveler : At 8th level, a Poleiheira adherent become even more adept to traveling long distance. When traveling on the regional map she augments the travel speed of her group by 30%. ";
            string mount = "Mounts";
            string mountdesc = "When she travel Poleiheira adherent can summon a magical steed to her side that lasts for 2 hours, this allow her to resist fatigue for as much time.";
            string mounupgradedesc = "The duration of the Poleiheira adherent mount ability increase by one Hour to a total of";
            string traveler = "Great Traveler";
            string travelerdesc = "At 8th level, a Poleiheira adherent become even more adept to traveling long distance. When traveling on the regional map she augments the travel speed of her group by 30%.";

            //Initialising progression 
            var AdherantProgression = Helpers.CreateProgression(name + "Progression", Schoolname, SchoolDesc, Helpers.getGuid(name + "Progression"), icontravel, FeatureGroup.SpecialistSchool);
            AdherantProgression.IsClassFeature = true;
            AdherantProgression.Classes = AdherantProgression.Classes.AddToArray(wizardclass);
            List<LevelEntry> addFeatures = new List<LevelEntry>();


            //School power for lvl 1+
            var Mount1 = Helpers.CreateFeature(name + mount + "Feature1", mount, mountdesc, Helpers.getGuid(name + mount + "Feature1"), iconmount, FeatureGroup.WizardFeat,
                Helpers.Create<AddWearinessHours>(p => p.Hours = -2)
                );
            var Mount2 = Helpers.CreateFeature(name + mount + "Feature2", mount + " Upgrade", mounupgradedesc + " 3 hours.", Helpers.getGuid(name + mount + "Feature2"), iconmount, FeatureGroup.WizardFeat,
                Helpers.Create<AddWearinessHours>(p => p.Hours = -3),
                Helpers.Create<RemoveFeatureOnApply>(f => f.Feature = Mount1)
                );
            var Mount3 = Helpers.CreateFeature(name + mount + "Feature3", mount + " Upgrade", mounupgradedesc + " 4 hours.", Helpers.getGuid(name + mount + "Feature3"), iconmount, FeatureGroup.WizardFeat,
                Helpers.Create<AddWearinessHours>(p => p.Hours = -4),
                Helpers.Create<RemoveFeatureOnApply>(f => f.Feature = Mount2)
                );

            var Mount4 = Helpers.CreateFeature(name + mount + "Feature4", mount + " Upgrade", mounupgradedesc + " 5 hours.", Helpers.getGuid(name + mount + "Feature4"), iconmount, FeatureGroup.WizardFeat,
                Helpers.Create<AddWearinessHours>(p => p.Hours = -5),
                Helpers.Create<RemoveFeatureOnApply>(f => f.Feature = Mount3)
                );

            var Mount5 = Helpers.CreateFeature(name + mount + "Feature5", mount + " Upgrade", mounupgradedesc + " 6 hours.", Helpers.getGuid(name + mount + "Feature5"), iconmount, FeatureGroup.WizardFeat,
                Helpers.Create<AddWearinessHours>(p => p.Hours = -6),
                Helpers.Create<RemoveFeatureOnApply>(f => f.Feature = Mount4)
                );

            //"School" of the poleiheira adherant 
            var archetypeselection = Helpers.CreateFeatureSelection(name + "Archetype", Archetype, ArchetypeDesc, Helpers.getGuid(name + "Archetype"), icontravel, FeatureGroup.WizardFeat,
                Helpers.Create<PrerequisiteNoArchetype>(n => { n.CharacterClass = wizardclass; n.Archetype = thassilonian; }),
                Helpers.PrerequisiteClassLevel(wizardclass, 1)
                );
            archetypeselection.Features = archetypeselection.Features.AddToArray(AdherantProgression);
            archetypeselection.AllFeatures = archetypeselection.AllFeatures.AddToArray(AdherantProgression);
            archetypeselection.HideNotAvailibleInUI = true;

            //lvl 8 feature
            var GreatTraveler = Helpers.CreateFeature(name + "GreatTravelerFeature", traveler, travelerdesc, Helpers.getGuid(name + "GreatTravelerFeature"), icontravel, FeatureGroup.None,
                Helpers.Create<GreatTravelerEffect>());

            //Create Bonded Book : empty, just for show
            var BondedBook = Helpers.CreateFeature(name + "bondedbook", Bond, BondDesc, Helpers.getGuid(name + "bondedbook"), iconbook, FeatureGroup.WizardFeat,
                Helpers.PrerequisiteFeature(archetypeselection),
                Helpers.CreateAddFact(BondedItem));

            // Creating the feature for new spell selection
            var MoreSpell = Helpers.CreateParametrizedFeature(name + "MoreSpellParamFeature", Bond + ": supplemental spell", spellsupdesc, Helpers.getGuid(name + "MoreSpellParamFeature"), iconbook, FeatureGroup.WizardFeat, FeatureParameterType.LearnSpell,
                Helpers.Create<LearnSpellParametrized>(l => {
                    l.SpellcasterClass = wizardclass;
                    l.SpellList = wizardlist;
                }));
            MoreSpell.BlueprintParameterVariants = library.Get<BlueprintParametrizedFeature>("f327a765a4353d04f872482ef3e48c35").BlueprintParameterVariants; //spell specialisation
            MoreSpell.HideInCharacterSheetAndLevelUp = true;
            MoreSpell.HideNotAvailibleInUI = true;
            MoreSpell.IsClassFeature = true;
            MoreSpell.SpellList = wizardlist;
            MoreSpell.SpellcasterClass = wizardclass;
            MoreSpell.SpellLevelPenalty = 0;
            MoreSpell.Ranks = 1;

            var MoreSpellSelection = Helpers.CreateFeatureSelection(MoreSpell.name + "Selection", "Learn new spell", BondDesc, Helpers.getGuid(MoreSpell.name + "Selection"), icon, FeatureGroup.WizardFeat);
            MoreSpellSelection.HideInCharacterSheetAndLevelUp = true;
            MoreSpellSelection.HideInUI = true;
            MoreSpellSelection.HideNotAvailibleInUI = true;
            MoreSpellSelection.IsClassFeature = true;
            MoreSpellSelection.Ranks = 1;
            MoreSpellSelection.Features = MoreSpellSelection.Features.AddToArray(MoreSpell);
            MoreSpellSelection.AllFeatures = MoreSpellSelection.AllFeatures.AddToArray(MoreSpell);

            //Filling level entry for progression 
            for (int i = 1; i < 21; i++)
            {
                if (i == 1)
                    addFeatures.Add(Helpers.LevelEntry(1, Mount1));
                if (i > 1)
                {
                    if (i == 5)
                        addFeatures.Add(Helpers.LevelEntry(i, MoreSpellSelection, MoreSpellSelection, Mount2));
                    else if (i == 8)
                        addFeatures.Add(Helpers.LevelEntry(i, MoreSpellSelection, MoreSpellSelection, GreatTraveler));
                    else if (i == 10)
                        addFeatures.Add(Helpers.LevelEntry(i, MoreSpellSelection, MoreSpellSelection, Mount3));
                    else if (i == 15)
                        addFeatures.Add(Helpers.LevelEntry(i, MoreSpellSelection, MoreSpellSelection, Mount4));
                    else if (i == 20)
                        addFeatures.Add(Helpers.LevelEntry(i, MoreSpellSelection, MoreSpellSelection, Mount5));
                    else
                        addFeatures.Add(Helpers.LevelEntry(i, MoreSpellSelection, MoreSpellSelection));

                }
                //Log.Write("Level " + i + " : entry = " + addFeatures[i].Features.ToString());
            }
            AdherantProgression.LevelEntries = AdherantProgression.LevelEntries.AddToArray(addFeatures);


            //Make other bond inabailable
            string bookbondid = Helpers.getGuid(name + "bondedbook");
            var noadherent = Helpers.PrerequisiteNoFeature(archetypeselection);

            foreach (BlueprintFeature bond in Arcanebondselection.AllFeatures)
            {
                if (bond.AssetGuid != bookbondid)
                    bond.AddComponent(noadherent);

            }

            //Finishing setting school and bon selection
            Arcanebondselection.AllFeatures = Arcanebondselection.AllFeatures.AddToArray(BondedBook);
            Arcanebondselection.Features = Arcanebondselection.Features.AddToArray(BondedBook);
            SpecialistSchoolSelection.Features = SpecialistSchoolSelection.Features.AddToArray(archetypeselection);
            SpecialistSchoolSelection.AllFeatures = SpecialistSchoolSelection.AllFeatures.AddToArray(archetypeselection);


        }



        public class GreatTravelerEffect : OwnedGameLogicComponent<UnitDescriptor>, IAreaActivationHandler, IGlobalSubscriber, IPartyHandler, IUnitLifeStateChanged
        {

            public bool OwnerIsActive()
            {

                return Owner.State.IsConscious && Game.Instance.Player.Party.HasItem(Owner.Unit);

            }
            public bool OtherAdherentInParty()
            {

                var flag = false;
                foreach (UnitEntityData Member in Game.Instance.Player.Party)
                {
                    if (Member == Owner.Unit)
                        continue;
                    if (Member.Descriptor.HasFact(Fact) && Member.Descriptor.State.IsConscious)
                        flag = true;
                }
                return flag;

            }

            public void Switch()
            {
                var OwnerActive = OwnerIsActive();
                var OtherAdherent = OtherAdherentInParty();
                if (OwnerActive || OtherAdherent)
                    Activated = true;
                else
                    Activated = false;
            }

            public void ManageSpeed()
            {

                if (Activated)
                {
                    if (!Changed)
                    {
                        Game.Instance.Player.GlobalMap.SpeedModifier += 0.3f;
                        Changed = true;
                    }
                }
                else
                {
                    if (Changed)
                    {
                        Game.Instance.Player.GlobalMap.SpeedModifier -= 0.3f;
                        Changed = false;
                    }
                }




            }



            public void OnAreaActivated()
            {
                if (Game.Instance.CurrentMode == GameModeType.GlobalMap)
                {
                    Switch();
                    ManageSpeed();
                }

            }
            public void HandleUnitLifeStateChanged(UnitEntityData unit, UnitLifeState prevLifeState)
            {
                if (unit.Descriptor.HasFact(Fact) && Game.Instance.CurrentMode == GameModeType.GlobalMap)
                {
                    Switch();
                    ManageSpeed();
                }


            }


            public void HandleAddCompanion(UnitEntityData unit)
            {
                if (unit.Descriptor.HasFact(Fact) && Game.Instance.CurrentMode == GameModeType.GlobalMap)
                {
                    Switch();
                    ManageSpeed();
                }

            }

            public void HandleCompanionActivated(UnitEntityData unit)
            {
                if (unit.Descriptor.HasFact(Fact) && Game.Instance.CurrentMode == GameModeType.GlobalMap)
                {
                    Switch();
                    ManageSpeed();
                }


            }


            public void HandleCompanionRemoved(UnitEntityData unit)
            {
                if (unit.Descriptor.HasFact(Fact) && Game.Instance.CurrentMode == GameModeType.GlobalMap)
                {
                    Switch();
                    ManageSpeed();
                }

            }
            static bool Changed;
            public float modifier = 2;
            static bool Activated;
            public bool InGlobaleMap = (Game.Instance.CurrentMode == GameModeType.GlobalMap);

        }


        static void CreateSpellBinder()
        {
            //needed thing
            var icon = Helpers.GetIcon("de18c849c41dbfa44801d812376c707d");

            string name = "SpellBinderFalseArchetype";
            string Name = "Spell Bond";
            string Name2 = "Archetype : Spellbinder";
            var spellspecialisation = library.Get<BlueprintParametrizedFeature>("f327a765a4353d04f872482ef3e48c35");
            var levlspellX = spellspecialisation.BlueprintParameterVariants;//Spell list from spellspecialisation

            //creating copy of the list
            foreach (BlueprintAbility spell in levlspellX)
            {
                bool Iswizardspell = spell.GetComponents<SpellListComponent>().Any((SpellListComponent p) => p.SpellList == wizardlist && p.SpellLevel > 0);
                if (Iswizardspell && !spell.IsFullRoundAction)
                {
                    if (spell.HasVariants)
                    {
                        foreach (BlueprintAbility Variant in spell.Variants)
                        {
                            var varcopy = library.CopyAndAdd<BlueprintAbility>(Variant, Variant.name + "BoundCopy", Helpers.getGuid(Variant.name + "BoundCopy"));
                            varcopy.ActionType = CommandType.Standard;
                            Helpers.SetField(varcopy, "m_IsFullRoundAction", true);

                        }
                    }
                    else
                    {
                        var copy = library.CopyAndAdd<BlueprintAbility>(spell, spell.name + "BoundCopy", Helpers.getGuid(spell.name + "BoundCopy"));
                        copy.ActionType = CommandType.Standard;
                        Helpers.SetField(copy, "m_IsFullRoundAction", true);

                    }

                }
            }

            //Starting progression
            var SpellBinderProgression = Helpers.CreateProgression(name + "Progression", "", "", Helpers.getGuid(name + "Progression"), icon, FeatureGroup.WizardFeat);
            SpellBinderProgression.IsClassFeature = true;
            SpellBinderProgression.Classes = SpellBinderProgression.Classes.AddToArray(wizardclass);
            List<LevelEntry> addFeatures = new List<LevelEntry>();

            //Starting Bond Selection
            var BondSelection = Helpers.CreateFeatureSelection(name + "Selection", Name, "At 1st level, a spellbinder selects any one spell that he knows as a bonded spell. As a full-round action, the spellbinder may replace a spell of the same  level as his bonded spell with his bonded spell. For example, a spellbinder who selects magic missile as his bonded spell could spend a full-round action to exchange any 1st-level spell that he has prepared with magic missile. At 3rd level, and every two levels thereafter, a spellbinder may select another spell he knows and add it to his list of bonded spells, to a maximum of nine bonded spells at 17th level.",
                Helpers.getGuid(name + "Selection"), icon, FeatureGroup.WizardFeat, Helpers.PrerequisiteClassLevel(wizardclass, 1));

            //Initialise Array of Parametrized feature
            BlueprintParametrizedFeature[] spellbondwiz = new BlueprintParametrizedFeature[9];//{null, null, null, null, null, null, null, null, null };
            BlueprintParametrizedFeature[] spellbondabju = new BlueprintParametrizedFeature[9];//] { null, null, null, null, null, null, null, null, null };
            BlueprintParametrizedFeature[] spellbondevoc = new BlueprintParametrizedFeature[9];//] { null, null, null, null, null, null, null, null, null };
            BlueprintParametrizedFeature[] spellbondconj = new BlueprintParametrizedFeature[9];//] { null, null, null, null, null, null, null, null, null };
            BlueprintParametrizedFeature[] spellbondnecro = new BlueprintParametrizedFeature[9];//] { null, null, null, null, null, null, null, null, null };
            BlueprintParametrizedFeature[] spellbondillu = new BlueprintParametrizedFeature[9];//] { null, null, null, null, null, null, null, null, null };
            BlueprintParametrizedFeature[] spellbondtrans = new BlueprintParametrizedFeature[9];//] { null, null, null, null, null, null, null, null, null };
            BlueprintParametrizedFeature[] spellbondench = new BlueprintParametrizedFeature[9];//] { null, null, null, null, null, null, null, null, null };
            BlueprintParametrizedFeature[][] allspellbond = new BlueprintParametrizedFeature[][] { spellbondwiz, spellbondabju, spellbondconj, spellbondench, spellbondevoc, spellbondillu, spellbondnecro, spellbondtrans };
            //Loop creating parametrized feature
            for (int i = 0; i < 8; i++)
            {
                for (int j = 1; j < 10; j++)
                {

                    //Creating parametrized feature
                    allspellbond[i][j - 1] = Helpers.CreateParametrizedFeature(name + i + "&" + j, Name + " Spell level " + j, BondSelection.Description, Helpers.getGuid(name + i + "&" + j), icon, FeatureGroup.WizardFeat, FeatureParameterType.LearnSpell,
                    //Helpers.PrerequisiteFeature(Helpers.elf),
                    Helpers.PrerequisiteClassLevel(wizardclass, j == 1 ? 1 : (j * 2 - 1)),
                    Helpers.Create<SpontaneousCastingParametrized>(l => l.SpellLevel = j));


                    // if wizard
                    if (i == 0)
                    {
                        var nothass = Helpers.Create<PrerequisiteNoArchetype>(n => { n.CharacterClass = wizardclass; n.Archetype = thassilonian; });

                        allspellbond[0][j - 1].AddComponents(nothass);
                    }
                    //if thassilonian
                    if (i > 0)
                    {
                        var schoolprereq = Helpers.PrerequisiteFeature(WizFeatureList[i]);
                        allspellbond[i][j - 1].AddComponent(schoolprereq);
                    }


                    // For spell level 1
                    if (j == 1)
                    {
                        var progcompo = Helpers.Create<AddFeatureOnApply>(f => f.Feature = SpellBinderProgression);
                        allspellbond[i][0].AddComponents(progcompo);
                    }


                    // Adding no self component
                    var noself = Helpers.PrerequisiteNoFeature(allspellbond[i][j - 1]);
                    allspellbond[i][j - 1].AddComponent(noself);
                    // Seting option right
                    allspellbond[i][j - 1].BlueprintParameterVariants = levlspellX;// Spell list = Spellspecialisation
                    allspellbond[i][j - 1].IsClassFeature = true;
                    allspellbond[i][j - 1].Ranks = 1;
                    allspellbond[i][j - 1].SpecificSpellLevel = true;
                    allspellbond[i][j - 1].SpellLevel = j;
                    allspellbond[i][j - 1].SpellcasterClass = wizardclass;
                    allspellbond[i][j - 1].SpellList = spellLists[i];
                    allspellbond[i][j - 1].HideNotAvailibleInUI = true;

                    //CReating level entry for progression
                    if (i == 0)
                        addFeatures.Add(Helpers.LevelEntry(j == 1 ? 1 : (j * 2 - 1), BondSelection));


                }

                //Adding the thing to Bond Selection
                BondSelection.Features = BondSelection.Features.AddRangeToArray(allspellbond[i]);//BondSelection.Features.AddRangeToArray(spellbondwiz);
                BondSelection.AllFeatures = BondSelection.AllFeatures.AddRangeToArray(allspellbond[i]);//BondSelection.AllFeatures.AddRangeToArray(spellbondwiz);
                BondSelection.HideNotAvailibleInUI = true;





            }

            //Adding the level entry
            SpellBinderProgression.LevelEntries = SpellBinderProgression.LevelEntries.AddToArray(addFeatures);

            //Creating a new step of bond selection (so that thassilonian have their school feature before making the choice of Spell for spellbinder)
            var BondSelectionStep = Helpers.CreateFeatureSelection(name + "SelectionStep", Name2, "A spellbinder is awizard who forges an arcane bond between himself and one or more wizard spells. These spells become so well understood by the spellbinder that he can prepare them in spell slots that already have other spells prepared in them.",
                Helpers.getGuid(name + "SelectionStep"), icon, FeatureGroup.WizardFeat, Helpers.PrerequisiteClassLevel(wizardclass, 1));
            BondSelectionStep.Features = BondSelectionStep.Features.AddToArray(BondSelection);
            BondSelectionStep.AllFeatures = BondSelectionStep.AllFeatures.AddToArray(BondSelection);



            //Adding stuff to arcane bond
            Arcanebondselection.Features = Arcanebondselection.Features.AddToArray(BondSelectionStep);
            Arcanebondselection.AllFeatures = Arcanebondselection.AllFeatures.AddToArray(BondSelectionStep);

        }



        public class SpontaneousCastingParametrized : ParametrizedFeatureComponent
        {




            public override void OnFactActivate()
            {

                BlueprintAbility[] SpellSpontaneous = { null, null, null, null, null, null, null, null, null, null };
                BlueprintAbility blueprintAbility = (!(base.Param != null)) ? null : (base.Param.Value.Blueprint as BlueprintAbility);
                if (blueprintAbility.HasVariants)
                {
                    var Hasvariant = blueprintAbility.Variants;
                    foreach (BlueprintAbility variant in Hasvariant)
                    {
                        var copy = library.TryGet<BlueprintAbility>(Helpers.getGuid(variant.name + "BoundCopy"));
                        BlueprintAbility[] SpellSpontaneousmany = { null, null, null, null, null, null, null, null, null, null };
                        SpellSpontaneousmany[SpellLevel] = copy != null ? copy : variant;
                        Owner.DemandSpellbook(wizardclass).AddSpellConversionList(SpellSpontaneousmany);
                    }

                }
                else
                {
                    var copy = library.TryGet<BlueprintAbility>(Helpers.getGuid(blueprintAbility.name + "BoundCopy"));
                    SpellSpontaneous[SpellLevel] = copy != null ? copy : blueprintAbility;
                    Owner.DemandSpellbook(wizardclass).AddSpellConversionList(SpellSpontaneous);
                }


                if (!Owner.DemandSpellbook(wizardclass).IsKnown(blueprintAbility))
                {

                    Owner.DemandSpellbook(wizardclass).AddKnown(SpellLevel, blueprintAbility);

                }
            }
            public override void OnFactDeactivate()
            {

            }




            public int SpellLevel;

        }
        public class AddArchetypeOnFeatureApply : OwnedGameLogicComponent<UnitDescriptor>
        {
            public override void OnFactActivate()
            {
                base.Owner.Progression.AddArchetype(this.CharacterClass, this.Archetype);


            }
            public BlueprintCharacterClass CharacterClass;
            public BlueprintArchetype Archetype;

        }
        public class SchoolUtility
        {
            public static void AddSchoolVariant(BlueprintProgression originalschool, BlueprintProgression newschool)
            {
                var Selection = library.TryGet<BlueprintFeatureSelection>(Helpers.getGuid(originalschool.name + "Selection"));
                if (Selection == null)
                {
                    string sourcedesc = originalschool.Description;
                    string deletestart = "\n";
                    int deleteindex = sourcedesc.IndexOf(deletestart);
                    int todelete = sourcedesc.Length - deleteindex;
                    string NewDesc = string.Empty;
                    if (deleteindex > 0)
                        NewDesc = sourcedesc.Remove(deleteindex, todelete);
                    var NewSlection = Helpers.CreateFeatureSelection(originalschool.name + "Selection", originalschool.Name, NewDesc.Length > 0 ? NewDesc : originalschool.Description, Helpers.getGuid(originalschool.name + "Selection"), originalschool.Icon, FeatureGroup.SpecialistSchool);
                    NewSlection.Features = NewSlection.Features.AddToArray(originalschool);
                    NewSlection.AllFeatures = NewSlection.AllFeatures.AddToArray(originalschool);
                    Selection = NewSlection;
                    Selection.HideInCharacterSheetAndLevelUp = false;

                }

                Selection.Features = Selection.Features.AddToArray(newschool);
                Selection.AllFeatures = Selection.AllFeatures.AddToArray(newschool);
                if (SpecialistSchoolSelection.Features.HasItem(originalschool))
                {
                    SpecialistSchoolSelection.Features = SpecialistSchoolSelection.Features.RemoveFromArray(originalschool);
                    SpecialistSchoolSelection.Features = SpecialistSchoolSelection.Features.AddToArray(Selection);
                }
                if (SpecialistSchoolSelection.AllFeatures.HasItem(originalschool))
                {
                    SpecialistSchoolSelection.AllFeatures = SpecialistSchoolSelection.AllFeatures.RemoveFromArray(originalschool);
                    SpecialistSchoolSelection.AllFeatures = SpecialistSchoolSelection.AllFeatures.AddToArray(Selection);
                }
            }
            public static BlueprintProgression CreateSchoolVariantProgression(BlueprintProgression originalschool, string name, string DisplayName, string Description, bool addvariant = false, params LevelEntry[] levelEntries)
            {
                BlueprintProgression newschool = library.CopyAndAdd(originalschool, originalschool.name + name, Helpers.getGuid(originalschool.name + name));
                newschool.LevelEntries = Array.Empty<LevelEntry>();
                newschool.LevelEntries = levelEntries;
                newschool.SetNameDescription(DisplayName, Description);
                if (addvariant)
                    AddSchoolVariant(originalschool, newschool);
                return newschool;
            }
            public static LevelEntry[] BuildLevelEntry(params (int level, BlueprintFeature feature)[] featurebylevels)
            {
                LevelEntry[] levelEntries = new LevelEntry[] { };
                BlueprintFeature[][] level = new BlueprintFeature[21][];
                foreach (var featurebylevel in featurebylevels)
                {
#if DEBUG
                    //Log.Write(featurebylevel.level + " " + featurebylevel.feature.ToString());
#endif
                    if (level[featurebylevel.level] == null)
                        level[featurebylevel.level] = new BlueprintFeature[] { };
                    level[featurebylevel.level] = level[featurebylevel.level].AddToArray(featurebylevel.feature);
                }

                for (int i = 1; i < 21; i++)
                {
                    if (level[i] != null)
                        levelEntries = levelEntries.AddToArray(Helpers.LevelEntry(i, level[i]));
                }
                return levelEntries;
            }
            public static BlueprintFeature CreateFeature(String name, String displayName, String description, Sprite icon, int rank = 1, bool hideinui = false, bool hideinsheet = false, params BlueprintComponent[] components)
            {
                var feat = Helpers.CreateFeature(name, displayName, description, Helpers.getGuid(name), icon, FeatureGroup.None, components);
                feat.Ranks = rank;
                feat.IsClassFeature = true;
                feat.HideInUI = hideinui;
                feat.HideInCharacterSheetAndLevelUp = hideinsheet;
                return feat;
            }

            public BlueprintComponent[] CreateBaseAbility()
            {
                BlueprintComponent[] Compo = new BlueprintComponent[1];
                return Compo;
            }

            public static wizardressource CreateWizardResource(string name, Sprite icon, bool SetAsBase= true)
            {
                var resource = Helpers.CreateAbilityResource(name, "", "", Helpers.getGuid(name), icon);
                if(SetAsBase)
                resource.SetWizardBaseResource();

                var addresource = Helpers.CreateAddAbilityResource(resource);
                var resourcelogic = Helpers.CreateResourceLogic(resource, true);
                var togglelogic = Helpers.CreateActivatableResourceLogic(resource, ActivatableAbilityResourceLogic.ResourceSpendType.NewRound);

                var result = new wizardressource()
                {
                    add = addresource,
                    logic = resourcelogic,
                    resource = resource,
                    ToggleLogic = togglelogic

                };

                return result;
            }
            public static wizardressource CopySchoolresource(string guid, bool activatable = false)
            {
                BlueprintAbilityResource resource = library.Get<BlueprintAbilityResource>(guid);
                var addresource = Helpers.CreateAddAbilityResource(resource);

                var resourcelogic = Helpers.CreateResourceLogic(resource, true);
                var togglelogic = Helpers.CreateActivatableResourceLogic(resource, ActivatableAbilityResourceLogic.ResourceSpendType.NewRound);
                var result = new wizardressource()
                {
                    add = addresource,
                    logic = resourcelogic,
                    resource = resource,
                    ToggleLogic = togglelogic

                };

                return result;

            }

            public static wizardressource GetWizardBaseResource(SpellSchool school)
            {
                var resource = schoolbaseresources.Value[(int)school];
                var addresource = Helpers.CreateAddAbilityResource(resource);
                var resourcelogic = Helpers.CreateResourceLogic(resource, true);
                var togglelogic = Helpers.CreateActivatableResourceLogic(resource, ActivatableAbilityResourceLogic.ResourceSpendType.NewRound);
                var result = new wizardressource()
                {
                    add = addresource,
                    logic = resourcelogic,
                    resource = resource,
                    ToggleLogic = togglelogic

                };

                return result;
            }

            public class wizardressource
            {
                public BlueprintAbilityResource resource;
                public AddAbilityResources add;
                public AbilityResourceLogic logic;
                public ActivatableAbilityResourceLogic ToggleLogic;

            };
            public static AddSpecialSpellList SpeciaListComponent(SpellSchool school, BlueprintCharacterClass @class = null) => SpeciaListComponent(specialistSchoolList.Value[(int)school], @class);
            public static AddSpecialSpellList SpeciaListComponent(BlueprintSpellList list, BlueprintCharacterClass @class = null)
            {
                return Helpers.Create<AddSpecialSpellList>(s => {
                    s.CharacterClass = @class == null ? wizardclass : @class;
                    s.SpellList = list;

                });

            }

            static readonly Lazy<BlueprintSpellList[]> specialistSchoolList = new Lazy<BlueprintSpellList[]>(() =>
            {
                var result = new BlueprintSpellList[(int)SpellSchool.Universalist + 1];
                var library = Main.library;
                result[(int)SpellSchool.Abjuration] = library.Get<BlueprintSpellList>("c7a55e475659a944f9229d89c4dc3a8e");
                result[(int)SpellSchool.Conjuration] = library.Get<BlueprintSpellList>("69a6eba12bc77ea4191f573d63c9df12");
                result[(int)SpellSchool.Divination] = library.Get<BlueprintSpellList>("d234e68b3d34d124a9a2550fdc3de9eb");
                result[(int)SpellSchool.Enchantment] = library.Get<BlueprintSpellList>("c72836bb669f0c04680c01d88d49bb0c");
                result[(int)SpellSchool.Evocation] = library.Get<BlueprintSpellList>("79e731172a2dc1f4d92ba229c6216502");
                result[(int)SpellSchool.Illusion] = library.Get<BlueprintSpellList>("d74e55204daa9b14993b2e51ae861501");
                result[(int)SpellSchool.Necromancy] = library.Get<BlueprintSpellList>("5fe3acb6f439db9438db7d396f02c75c");
                result[(int)SpellSchool.Transmutation] = library.Get<BlueprintSpellList>("becbcfeca9624b6469319209c2a6b7f1");
                return result;
            });
            static readonly Lazy<BlueprintAbilityResource[]> schoolbaseresources = new Lazy<BlueprintAbilityResource[]>(() =>
            {
                var result = new BlueprintAbilityResource[(int)SpellSchool.Universalist + 1];
                var library = Main.library;
                result[(int)SpellSchool.Abjuration] = library.Get<BlueprintAbilityResource>("870a9cc29d8d0e945b7fbd7926378197");
                result[(int)SpellSchool.Conjuration] = library.Get<BlueprintAbilityResource>("69903f87566b4ff47805cd03e117c14c");
                result[(int)SpellSchool.Divination] = library.Get<BlueprintAbilityResource>("7fbeac3a41c4e41489be06e0b4d79603");
                result[(int)SpellSchool.Enchantment] = library.Get<BlueprintAbilityResource>("cfcbfc3a05bd41f43af7b5121e918ecd");
                result[(int)SpellSchool.Evocation] = library.Get<BlueprintAbilityResource>("f58f284c1d357a4449ce408cdfe6776a");
                result[(int)SpellSchool.Illusion] = library.Get<BlueprintAbilityResource>("be7f82229e15ac94db39dbac5ec3f07d");
                result[(int)SpellSchool.Necromancy] = library.Get<BlueprintAbilityResource>("d3c8231b4ab43d248944b6da83776522");
                result[(int)SpellSchool.Transmutation] = library.Get<BlueprintAbilityResource>("438920d3a99d02146ac1b5e1fb3c6055");
                return result;
            });


        }
        [HarmonyPatch(typeof(UnitUseAbility), "MakeConcentrationCheckIfCastingIsDifficult")]
        private static class HandleDisruptionCheck
        {
            static BlueprintBuff disruption = library.Get<BlueprintBuff>(Guid("DisruptionBuff"));
            static void Postfix(UnitUseAbility __instance)
            {
                if (__instance.Cutscene || __instance.ConcentrationCheckFailed || !__instance.Spell.Blueprint.IsSpell || __instance.Spell.StickyTouch != null)
                {
                    return;
                }
                if (__instance.Executor.Descriptor.HasFact(disruption))
                {
                    int DC = 2 * (__instance.Spell.SpellLevel + 5);
                    var falseDamage = new RuleDealDamage(__instance.Executor, __instance.Executor, new DamageBundle());
                    AccessTools.Property(typeof(RuleDealDamage), "Damage").SetValue(falseDamage, DC);
                    RuleCheckConcentration concentration = new RuleCheckConcentration(__instance.Executor, __instance.Spell, falseDamage);
                    AccessTools.Property(typeof(UnitUseAbility), "ConcentrationCheckFailed").SetValue(__instance, !Rulebook.Trigger(concentration).Success);
                }
            }

        }
        [HarmonyPatch(typeof(ActionBarGroupSlot), "SetToggleAdditionalSpells")]
        private static class AddCounterSpellConversion
        {
            static void Postfix(ActionBarGroupSlot __instance, AbilityData spell, ref List<AbilityData> ___Conversion, ref ButtonPF ___ToggleAdditionalSpells)
            {
                if (spell.Spellbook != null)
                {
                    MechanicActionBarSlotMemorizedSpell mechanicActionBarSlotMemorizedSpell = __instance.MechanicSlot as MechanicActionBarSlotMemorizedSpell;
                    SpellSlot spellSlot = (mechanicActionBarSlotMemorizedSpell != null) ? mechanicActionBarSlotMemorizedSpell.SpellSlot : null;

                    if (spellSlot != null)
                    {
                        foreach (Ability ability in spell.Caster.Abilities)
                        {
                            if (ability.Blueprint.GetComponent<CounterSpellMastery>())
                            {
                                AbilityData item = new AbilityData(ability)
                                {
                                    ParamSpellSlot = spellSlot
                                };
                                
                                ___Conversion.Add(item);

                            }
                        }
                        BlueprintAbility spellBlueprint = spell.Blueprint;
                        if (___Conversion.Any((AbilityData s) => s.Blueprint != spellBlueprint && s.Blueprint.GetComponent<CounterSpellMastery>() && s.Blueprint.GetComponent<CounterSpellMastery>().IsAbilityVisible(s)) || (spellBlueprint.Variants != null && spellBlueprint.Variants.Any<BlueprintAbility>()))
                        {
                            if (___ToggleAdditionalSpells != null)
                            {
                                ___ToggleAdditionalSpells.gameObject.SetActive(true);
                            }
                        }
                    }

                }
        }

        }

        [HarmonyPatch(typeof(DescriptionBuilder))]
        [Harmony12.HarmonyPatch("Buff")]
        private static class ShowCasterNameOnBuffToolTip
        {
            static void Postfix(TooltipData data, DescriptionBody body, bool isTooltip)
            {

                if (data.Buff != null)
                {

                    string charname = data.Buff.Context.MaybeCaster.CharacterName != null ? data.Buff.Context.MaybeCaster.CharacterName : "unknow";
                    DescriptionBuilder.Templates.Separator1(body.ContentBox);
                    body.ContentBox.Add(DescriptionTemplatesBase.Bricks.TitleH4).SetText("This effect was created by " + charname + ".");



                }
                else
                {
                    UberDebug.LogErrorChannel("UI", "No buff data inside tooltipdata", Array.Empty<object>());
                }

            }

        }

        [HarmonyPatch(typeof(UnitCombatState))]
        [Harmony12.HarmonyPatch("IsFlanked", MethodType.Getter)]
        private static class MakeCreatureFlankedByBuff
        {
            static void Postfix(UnitCombatState __instance, ref bool __result)
            {
                try
                {
                    if (AllwaysFlanked == null)
                        AllwaysFlanked = library.Get<BlueprintBuff>(Helpers.getGuid("BedevilingAuraEffectBuff"));
                    if (__instance.Unit.Descriptor.HasFact(AllwaysFlanked))
                        __result = true;

                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
            }

            static BlueprintBuff AllwaysFlanked;
        }



    }

}
