using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Harmony12;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Localization;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.RuleSystem;
using Kingmaker.ResourceLinks;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Items.Slots;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;
using Kingmaker.Visual.HitSystem;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;
using Kingmaker.UI.ServiceWindow;
using Kingmaker.UI.GenericSlot;
using Kingmaker.UI.Group;
using Kingmaker.Utility;
using Kingmaker.Visual.Sound;
using Kingmaker.Visual.Animation.Kingmaker;
using UnityEngine;

namespace thelostgrimoire
{
    class GolemDiscovery
    {

        public static BlueprintArchetype thearchetypeforthisnicegolem = createconstructarchetype();
        public static BlueprintUnit animalCompanionUnitCentipede = Main.library.CopyAndAdd<BlueprintUnit>("f9df16ffd0c8cec4d99a0ae6f025a3f8", "dummycopyunitforcompanion", Helpers.getGuid("dummycopyunitforcompanion"));

        public static void Load()
        {
            Main.SafeLoad(CreateAnimalAllyFeatLine, "Golem Companion");
            Main.SafeLoad(CreateConstructSpell, "Construct Spell");
        }
        public static void CreateAnimalAllyFeatLine()
        {
            
            Main.ApplyPatch(typeof(GetPortraitFolderPathPatch), "Test");
            Main.ApplyPatch(typeof(DisallowAddingFeatureToConstructPatch), "Do not add Animal companion feature to golem ");
            Main.ApplyPatch(typeof(DisallowSharingFeaturewithConstructPatch), "Do not share animal companion feature with golem");
            Main.ApplyPatch(typeof(DisableSteamDragonAnimationPatch), "Make the steam dragon not move while iddle"); 
            var cannyobserver = Helpers.GetIcon("4093d5a0eb5cae94e909eb1e0e1a6b36");

           
            BlueprintCharacterClass wizardclass = Main.library.Get<BlueprintCharacterClass>("ba34257984f4c41408ce1dc2004e342e");

            BlueprintFeature AnimalCompanionEmptyCompanion = Main.library.Get<BlueprintFeature>("472091361cf118049a2b4339c4ea836a");
            

            



            //creating Construct Companion Rank
            var constructrank = Helpers.CreateFeature("ContstructCompanionRank", "Construct Companion", "Construct Companion", Helpers.getGuid("ConstructCompanionRank"), cannyobserver, FeatureGroup.None);
            constructrank.HideInUI = true;
            constructrank.Ranks = 20;
            constructrank.IsClassFeature = true;

            //Getting the pet progression from domain to wizard
            BlueprintProgression domainAnimalCompanionProgression = Main.library.Get<BlueprintProgression>("125af359f8bc9a145968b5d8fd8159b8");
            BlueprintProgression golemCompanionProgression = Main.library.CopyAndAdd(domainAnimalCompanionProgression, "golemCompanionProgression", Helpers.getGuid("golemcCompanionProgression"));
            golemCompanionProgression.Classes = (BlueprintCharacterClass[])Array.Empty<BlueprintCharacterClass>();
            golemCompanionProgression.Classes = golemCompanionProgression.Classes.AddToArray(wizardclass);

            golemCompanionProgression.LevelEntries = Array.Empty<LevelEntry>();
            List<LevelEntry> addFeatures = new List<LevelEntry>();
            addFeatures.Add(Helpers.LevelEntry(2, constructrank));
            addFeatures.Add(Helpers.LevelEntry(3, constructrank));
            addFeatures.Add(Helpers.LevelEntry(4, constructrank));
            addFeatures.Add(Helpers.LevelEntry(5, constructrank));
            addFeatures.Add(Helpers.LevelEntry(6, constructrank));
            addFeatures.Add(Helpers.LevelEntry(7, constructrank));
            addFeatures.Add(Helpers.LevelEntry(8, constructrank));
            addFeatures.Add(Helpers.LevelEntry(9, constructrank));
            addFeatures.Add(Helpers.LevelEntry(10, constructrank));
            addFeatures.Add(Helpers.LevelEntry(11, constructrank));
            addFeatures.Add(Helpers.LevelEntry(12, constructrank));
            addFeatures.Add(Helpers.LevelEntry(13, constructrank));
            addFeatures.Add(Helpers.LevelEntry(14, constructrank));
            addFeatures.Add(Helpers.LevelEntry(15, constructrank));
            addFeatures.Add(Helpers.LevelEntry(16, constructrank));
            addFeatures.Add(Helpers.LevelEntry(17, constructrank));
            addFeatures.Add(Helpers.LevelEntry(18, constructrank));
            addFeatures.Add(Helpers.LevelEntry(19, constructrank));
            addFeatures.Add(Helpers.LevelEntry(20, constructrank));
            
            golemCompanionProgression.LevelEntries = addFeatures.ToArray();
            //Creating feature list 
            BlueprintFeature MudGolemCompanion = AddMudGolemCompanion();
            BlueprintFeature GolemCompanion = AddGolemCompanion();
            BlueprintFeature ScarecrowCompanion = AddScarecrowCompanion();
            
            BlueprintFeature IceGolem = AddIceGolemCompanion();
            BlueprintFeature SteamDragon = AddSteamDragonCompanion();
            BlueprintFeature[] CompanionList = new BlueprintFeature[]
            {
                GolemCompanion,
                ScarecrowCompanion,
                MudGolemCompanion,
                IceGolem,
                SteamDragon
            };



            //Creating an Ability to resurect Golem ? TODO later only if there is balance problem = pet construct wont resurect on rest and this ability must be used, and can be only 1 time per rest and cost diamond dust


         


            //Creating an Abilities to heal the golem 
            var persuasion = Main.library.Get<BlueprintFeature>("1621be43793c5bb43be55493e9c45924");
            var PatchGolemResource = Helpers.CreateAbilityResource("PatchGolemResource", "", "",
               Helpers.getGuid("PatchGolemResource"), cannyobserver);
            PatchGolemResource.SetFixedResource(3);

            BlueprintAbility PatchGolem = Helpers.CreateAbility("PatchConstruct", "Patch Construct", "Three time per day per day as a full round action you can use expensive material to make a quick patch to a damaged " +
                "construct under your control and restore a some of its health. The healing amount is based on you intelligence modifier.",
                Helpers.getGuid("PatchConstruct"),
                persuasion.Icon,
                AbilityType.SpellLike,
                CommandType.Standard,
                AbilityRange.Personal,
                "",
                "",
                
                Helpers.CreateContextRankConfig(ContextRankBaseValueType.BaseStat, ContextRankProgression.AsIs, AbilityRankType.Default, null, null, startLevel: 3, stepLevel: 1, false, StatType.Intelligence),
                Helpers.CreateRunActions(Helpers.Create<ContextActionsOnPet>(p =>
                p.Actions = Helpers.CreateActionList(Helpers.Create<ContextActionHealTarget>(h => h.Value = DiceType.D8.CreateContextDiceValue(1, Helpers.CreateContextValueRank()))))),
                PatchGolemResource.CreateAddAbilityResource()
                );
            PatchGolem.MaterialComponent.Item = Main.library.Get<BlueprintItem>("92752bbbf04dfa1439af186f48aee0e9"); //Diamond Dust 
            PatchGolem.MaterialComponent.Count = 2;
            PatchGolem.AddComponent(Helpers.CreateResourceLogic(PatchGolemResource));
            Helpers.SetField(PatchGolem, "m_IsFullRoundAction", true);
            
            //Golem constructor main ability
            BlueprintFeatureSelection GolemConstructor = Helpers.CreateFeatureSelection(
                "Golem Constructor",
                "Arcane Discovery : Golem Constructor",
                "You have learned the art and craft of creating a single type of golem (such as stone golems) and after hours of work, one of the construct is now ready to travel with you." +
                "\nGolem are construct and are not living creature: they do not need to drink, sleep or eat and are immune to wide variety of effect.",
                Helpers.getGuid("ArcaneDiscoveriesgolemconstructor"),
                cannyobserver,
                FeatureGroup.WizardFeat,
                Helpers.PrerequisiteClassLevel(wizardclass, 9),
                Helpers.Create<AddFeatureOnApply>(x => x.Feature = golemCompanionProgression),
                Helpers.Create<AnimalAllyAdjustToLevelLogic>(),
                PatchGolem.CreateAddFact(),
                Helpers.Create<PrerequisitePet>(p => p.NoCompanion = true)

            ) ; 
            
            GolemConstructor.SetFeatures(CompanionList);
            GolemConstructor.Groups = GolemConstructor.Groups.AddToArray(FeatureGroup.Feat);

            Main.library.AddFeats(GolemConstructor);
            Main.library.AddFeats("8c3102c2ff3b69444b139a98521a4899", GolemConstructor);

            //Disable Taking animal companion if you have the golem constructor feat
            var noconstructprereq = Helpers.PrerequisiteNoFeature(GolemConstructor);
            AnimalCompanionEmptyCompanion.AddComponent(noconstructprereq);
          



        }
      
        private static BlueprintFeature AddSteamDragonCompanion()
        {

            BlueprintFeature cannyobserver = Main.library.Get<BlueprintFeature>("68a23a419b330de45b4c3789649b5b41");

            //Getting Protrait
            PortraitData portraitData = new PortraitData("thelostgrimoireSteamDragon");
            BlueprintPortrait portrait = Helpers.Create<BlueprintPortrait>();
            portrait.Data = portraitData;
            Main.library.AddAsset(portrait, Helpers.getGuid("SteamDragonPortrait"));

           

            //getting the golem
            BlueprintUnit SteamDragon = Main.library.CopyAndAdd<BlueprintUnit>("64589573eba29d541b5240c7485e1aec", "SteamDragonCompanion", Helpers.getGuid("SteamDragonCompanion"));
            SteamDragon.Prefab.AssetId = "1d5a2ef7c2529be49879b1399b55a34a";
            SteamDragon.Visual.FootstepSoundSizeType = FootstepSoundSizeType.BootMetalMedium;
            

            var Name = SteamDragon.LocalizedName.CreateCopy();
            SteamDragon.LocalizedName = Name;
            AccessTools.Field(SteamDragon.LocalizedName.GetType(), "String").SetValue(SteamDragon.LocalizedName, Helpers.CreateString($"{SteamDragon.name}.{"String"}", "Steam Dragon"));

            //Editing Golem for balance reason and shit
            //remove Magic Immunity, its too good 
            SteamDragon.AddFacts = SteamDragon.AddFacts.RemoveFromArray(Main.library.Get<BlueprintFeature>("4ccca90d0556b554eb6e7dbd665c4d41"));
            // No DR either you get it from archetype
            SteamDragon.AddFacts = SteamDragon.AddFacts.RemoveFromArray(Main.library.Get<BlueprintFeature>("5a5a56af90a05024e8e1f2f50187c2d1"));
            //And a tidy bit less of base Nat AC
            SteamDragon.AddFacts = SteamDragon.AddFacts.RemoveFromArray(Main.library.Get<BlueprintUnitFact>("65c289f08343f5349b6dafbc0240d6ef"));
            SteamDragon.AddFacts = SteamDragon.AddFacts.AddToArray(Main.library.Get<BlueprintUnitFact>("e73864391ccf0894997928443a29d755"));
            //Remove Breath weapon to make a new one
            SteamDragon.RemoveComponents<AddAbilityToCharacterComponent>();
            //Add Vulnerability to Electricity
            SteamDragon.AddFacts = SteamDragon.AddFacts.AddToArray(Main.library.Get<BlueprintFeature>("da61d098617134741b58a4887ca94537"));
            //Add fire resistance
            SteamDragon.AddFacts = SteamDragon.AddFacts.AddToArray(Main.library.Get<BlueprintFeature>("137697b2929df514c9e4a3de66f60bc2"));
            //Add clockwork feat
            SteamDragon.AddFacts = SteamDragon.AddFacts.AddToArray(Main.library.Get<BlueprintFeature>("797f25d709f559546b29e7bcb181cc74"));
            SteamDragon.AddFacts = SteamDragon.AddFacts.AddToArray(Main.library.Get<BlueprintFeature>("15e7da6645a7f3d41bdad7c8c4b9de1e"));
            //Add visual change
            SteamDragon.AddFacts = SteamDragon.AddFacts.AddRangeToArray(AddSteamDragonApperance());

            //Changing the Abilities a bit
            SteamDragon.Strength = 14;
            SteamDragon.Dexterity = 15;
            SteamDragon.Wisdom = 11;
            SteamDragon.Charisma = 1;
            SteamDragon.Size = Size.Huge;
            SteamDragon.Speed = 30.Feet();

            //Changing those weapons
            BlueprintItemWeapon EmptyHandWeapon = Main.library.Get<BlueprintItemWeapon>("20375b5a0c9243d45966bd72c690ab74");
            SteamDragon.Body.EmptyHandWeapon = EmptyHandWeapon;
            SteamDragon.Body.DisableHands = false;
            BlueprintItemWeapon SteamDragonBite = Main.library.CopyAndAdd<BlueprintItemWeapon>("9c20ebc9f7b701743944b1c76d4bf598", "SteamDragonCompanionBite", Helpers.getGuid("SteamDragonCompanionBite"));
            SteamDragonBite.Enchantments.Add(Main.library.Get<BlueprintWeaponEnchantment>("ab39e7d59dd12f4429ffef5dca88dc7b"));
            BlueprintItemWeapon SteamDragonClaw = Main.library.CopyAndAdd<BlueprintItemWeapon>("96cb163919afd3445a4b863c677f95a1", "SteamDragonCompanionClaw", Helpers.getGuid("SteamDragonCompanionClaw"));
            SteamDragonClaw.Enchantments.Add(Main.library.Get<BlueprintWeaponEnchantment>("ab39e7d59dd12f4429ffef5dca88dc7b"));
            BlueprintItemWeapon SteamDragonWing = Main.library.CopyAndAdd<BlueprintItemWeapon>("06c210c77c30a36478cb8dbf225fb364", "SteamDragonCompanionWing", Helpers.getGuid("SteamDragonCompanionWing"));
            SteamDragonWing.Enchantments.Add(Main.library.Get<BlueprintWeaponEnchantment>("ab39e7d59dd12f4429ffef5dca88dc7b"));
            BlueprintItemWeapon SteamDragonTail = Main.library.CopyAndAdd<BlueprintItemWeapon>("ae822725634c6f0418b8c48bd29df255", "SteamDragonCompanionTail", Helpers.getGuid("SteamDragonCompanionTail"));
            SteamDragonTail.Enchantments.Add(Main.library.Get<BlueprintWeaponEnchantment>("ab39e7d59dd12f4429ffef5dca88dc7b"));


            SteamDragon.Body.PrimaryHand = SteamDragonBite;
            //SteamDragon.Body.SecondaryHand = SteamDragonClaw;
            SteamDragon.Body.AdditionalLimbs = Array.Empty<BlueprintItemWeapon>();
            SteamDragon.Body.AdditionalLimbs = new BlueprintItemWeapon[] { SteamDragonClaw, SteamDragonClaw } ;
            SteamDragon.Body.AdditionalSecondaryLimbs = new BlueprintItemWeapon[] { SteamDragonWing, SteamDragonWing, SteamDragonTail };

            //Size: Not Large, baby, not yet
            ChangeUnitSize unitSizeless = Helpers.Create<ChangeUnitSize>(x => x.SizeDelta = -3);
            FieldInfo typeFieldless = unitSizeless.GetType().GetField("m_Type", BindingFlags.NonPublic | BindingFlags.Instance);
            object deltaless = unitSizeless.GetType().GetNestedType("ChangeType", BindingFlags.NonPublic).GetField("Delta").GetValue(unitSizeless);
            typeFieldless.SetValue(unitSizeless, deltaless);

            BlueprintFeature changesizedamnit = Helpers.CreateFeature("SteamDragonsizechange",
                "",
                "",
                Helpers.getGuid("SteamDragonsizechange"),
                cannyobserver.Icon,
                FeatureGroup.None,
                unitSizeless);
            SteamDragon.AddFacts = SteamDragon.AddFacts.AddToArray(changesizedamnit);

            //Adjusting HP to size per construct rule
            BlueprintFeature AdjustHP = Helpers.CreateFeature("SteamDragonAdjustHp",
               "",
               "",
               Helpers.getGuid("SteamDragonAdjustHp"),
               cannyobserver.Icon,
               FeatureGroup.None,
               Helpers.CreateAddStatBonus(StatType.HitPoints, -30, ModifierDescriptor.UntypedStackable)
               );
            SteamDragon.AddFacts = SteamDragon.AddFacts.AddToArray(AdjustHP);
            // Setting the golem right with class, archetype and all
            var ConstructType = Main.library.Get<BlueprintFeature>("fd389783027d63343b4a5634bd81645f");
            List<LevelEntry> addtypeFeatures = new List<LevelEntry>();
            addtypeFeatures.Add(Helpers.LevelEntry(1, ConstructType));

            //Fixing bleed immunity in the ui
            var bleedimmune = Main.library.Get<BlueprintBuff>("3f6038d75ccffaa40b338f4b13f9e4b6");
            Helpers.SetLocalizedStringField(bleedimmune, "m_DisplayName", "Bleed Immunity");
            Helpers.SetLocalizedStringField(bleedimmune, "m_Description", "This creature is immune to bleed damage and the bleeding condition");
            var stoneskinspell = Main.library.Get<BlueprintAbility>("c66e86905f7606c4eaa5c774f0357b2b");
            Helpers.SetField(bleedimmune, "m_Icon", stoneskinspell.Icon);
            
            SteamDragon.ComponentsArray = animalCompanionUnitCentipede.ComponentsArray;
            SteamDragon.ReplaceComponent<AddClassLevels>(l =>
            {
                l.Archetypes = Array.Empty<BlueprintArchetype>();

                l.CharacterClass = Main.library.Get<BlueprintCharacterClass>("fd66bdea5c33e5f458e929022322e6bf");
                l.Archetypes = l.Archetypes.AddToArray(thearchetypeforthisnicegolem);
                l.CharacterClass.Archetypes = Array.Empty<BlueprintArchetype>();
                l.CharacterClass.Archetypes = l.CharacterClass.Archetypes.AddToArray(thearchetypeforthisnicegolem);

                l.Selections = Array.Empty<SelectionEntry>();
               


            });


            // Brain and faction
            SteamDragon.Brain = animalCompanionUnitCentipede.Brain;
            SteamDragon.Faction = Main.library.Get<BlueprintFaction>("d8de50cc80eb4dc409a983991e0b77ad"); // Neutral faction

            //Portrait 
            Helpers.SetField(SteamDragon, "m_Portrait", portrait);

         

            //for the size change at level 11
            ChangeUnitSize unitSizeplus = Helpers.Create<ChangeUnitSize>(x => x.SizeDelta = -2);
            FieldInfo typeField = unitSizeplus.GetType().GetField("m_Type", BindingFlags.NonPublic | BindingFlags.Instance);
            object delta = unitSizeplus.GetType().GetNestedType("ChangeType", BindingFlags.NonPublic).GetField("Delta").GetValue(unitSizeplus);
            typeField.SetValue(unitSizeplus, delta);

            

            //Creating the Feature
            BlueprintAbility dragonbreathfeature = Main.library.Get<BlueprintAbility>("5e826bcdfde7f82468776b55315b2403");
            BlueprintFeature ConstructCompanionSteamDragonFeature = Main.library.CopyAndAdd<BlueprintFeature>("f9ef7717531f5914a9b6ecacfad63f46", "SteamDragonCompanionFeature", Helpers.getGuid("SteamDragonCompanionFeature"));
            ConstructCompanionSteamDragonFeature.SetNameDescription("Steam Dragon", "Size Small\nSpeed 30 ft.\nAC +7 natural armor\nResistance to Fire(20) and weakness to Elecritcity.\nAttacks: 1 Bite 1d4, 2 Claw 1d3, 2 Wing 1d3, 1 Tail 1d4 (All natural weapons are considered Adamantine weapons) \nAbility Scores: Str 14, Dex 15, Con --, Int --, Wis 11, Cha 1 \nAt 11th level size becomes Large, Str +4, Dex +4,  +4 natural armor and gain the Breath Weapon Ability");
            Helpers.SetField(ConstructCompanionSteamDragonFeature, "m_Icon", dragonbreathfeature.Icon);

            //Adding the pet
            AddPet addPetFact = ConstructCompanionSteamDragonFeature.ComponentsArray.OfType<AddPet>().First();
            ConstructCompanionSteamDragonFeature.RemoveComponent(addPetFact);
            addPetFact = UnityEngine.Object.Instantiate(addPetFact);
            ConstructCompanionSteamDragonFeature.AddComponent(addPetFact);
            addPetFact.Pet = SteamDragon;
            addPetFact.LevelRank = Main.library.Get<BlueprintFeature>(Helpers.getGuid("ConstructCompanionRank"));

            //Upgrade Feature
            addPetFact.UpgradeFeature = Helpers.CreateFeature(
                "SteamDragonCompanionUpgradeFeature",
                "",
                "",
                Helpers.getGuid("SteamDragonCompanionUpgradeFeature"),
                cannyobserver.Icon,
                FeatureGroup.None,
                
                unitSizeplus,
                
                Helpers.Create<AddStatBonus>(x =>
                {
                    x.Stat = StatType.AC;
                    x.Value = 4;
                    x.Descriptor = ModifierDescriptor.NaturalArmor;
                }),
                Helpers.Create<AddStatBonus>(x =>
                {
                    x.Stat = StatType.Strength;
                    x.Value = 4;
                }),
                Helpers.Create<AddStatBonus>(x =>
                {
                    x.Stat = StatType.Dexterity;
                    x.Value = 4;
                }),
                 Helpers.Create<AddStatBonus>(x =>
                 {
                     x.Stat = StatType.HitPoints;
                     x.Descriptor = ModifierDescriptor.UntypedStackable;
                     x.Value = 10;
                 }),
                 CreateSteamDragonBreathWeapon().CreateAddFact()

            );
            addPetFact.UpgradeLevel = 11;

            return ConstructCompanionSteamDragonFeature;
        }

        private static BlueprintFeature AddIceGolemCompanion()
        {

            BlueprintFeature cannyobserver = Main.library.Get<BlueprintFeature>("68a23a419b330de45b4c3789649b5b41");

            //Getting Protrait
            PortraitData portraitData = new PortraitData("thelostgrimoireIceGolem");
            BlueprintPortrait portrait = Helpers.Create<BlueprintPortrait>();
            portrait.Data = portraitData;
            Main.library.AddAsset(portrait, Helpers.getGuid("IceGolemPortrait"));

           

            //getting the golem
            BlueprintUnit IceGolem = Main.library.CopyAndAdd<BlueprintUnit>("dfd21dba15fe7dd4f95961ff27d91836", "IceGolemCompanion", Helpers.getGuid("IceGolemCompanion"));

            //Editing Golem for balance reason and shit
            //remove Magic Immunity, its too good 
            IceGolem.AddFacts = IceGolem.AddFacts.RemoveFromArray(Main.library.Get<BlueprintFeature>("2617c0ea094687643a14fd99c4529523"));
            // No DR either you get it from archetype
            IceGolem.AddFacts = IceGolem.AddFacts.RemoveFromArray(Main.library.Get<BlueprintFeature>("769396cce5ee41f4cb4b6579ff271269"));
            //And a tidy bit less of base Nat AC
            IceGolem.AddFacts = IceGolem.AddFacts.RemoveFromArray(Main.library.Get<BlueprintUnitFact>("66b08b1f48983c54eb3f175d24ac7039"));
            IceGolem.AddFacts = IceGolem.AddFacts.AddToArray(Main.library.Get<BlueprintUnitFact>("16fc201a83edcde4cbd64c291ebe0d07"));
            //Add the effect to look icy
            IceGolem.AddFacts = IceGolem.AddFacts.AddRangeToArray(AddIceGolemApperance());
            // Add the cold abiltity from medium water elemental
            IceGolem.AddFacts = IceGolem.AddFacts.AddToArray(Main.library.Get<BlueprintFeature>("fef69bdc821cdbd4e85fe820bfd77c9f"));
            //Add coldimmunity and fire weakness
            IceGolem.AddFacts = IceGolem.AddFacts.AddToArray(Main.library.Get<BlueprintFeature>("9ae23798a9284e044ad2716a772a410e"));
            IceGolem.AddFacts = IceGolem.AddFacts.AddToArray(Main.library.Get<BlueprintFeature>("8e934134fec60ab4c8972c85a7b62f89"));
            //Add death throes
            IceGolem.AddFacts = IceGolem.AddFacts.AddToArray(CreateIceGolemIcyBurst());
            //Remove slow because it's not for you
            IceGolem.RemoveComponents<AddAbilityToCharacterComponent>();
            //Give it the right name ^^

            // Helpers.SetLocalizedStringField(, "LocalizedName", "Ice Golem");
            var Name = IceGolem.LocalizedName.CreateCopy();
            IceGolem.LocalizedName = Name;
            AccessTools.Field(IceGolem.LocalizedName.GetType(), "String").SetValue(IceGolem.LocalizedName, Helpers.CreateString($"{IceGolem.name}.{"String"}", "Ice Golem"));
            //IceGolem.LocalizedName.String = Helpers.CreateString($"{IceGolem.name}.{"String"}", "Ice Golem");

            //Edit visual 
            IceGolem.Visual.BloodType = Kingmaker.Visual.HitSystem.BloodType.WaterElemental;
            IceGolem.Visual.BloodPuddleFx.AssetId = "b6a8750499b0ec647ba68430e83bfc2f";
            IceGolem.Visual.DefaultArmorSoundType = ArmorSoundType.Stone;
            

            //Changing the Abilities a bit
            IceGolem.Strength = 10;
            IceGolem.Dexterity = 11;
            IceGolem.Wisdom = 11;
            IceGolem.Charisma = 1;
            IceGolem.Speed = 30.Feet();
            //Changing those weapons
            BlueprintItemWeapon EmptyHandWeapon = Main.library.Get<BlueprintItemWeapon>("20375b5a0c9243d45966bd72c690ab74");
            IceGolem.Body.EmptyHandWeapon = EmptyHandWeapon;
            IceGolem.Body.DisableHands = false;
            BlueprintItemWeapon GolemCompanionSlam = Main.library.Get<BlueprintItemWeapon>("bbdf5d550dc406640a77c5d2a05244ca");
            IceGolem.Body.PrimaryHand = GolemCompanionSlam;
            IceGolem.Body.AdditionalLimbs = Array.Empty<BlueprintItemWeapon>();

            //Size: Not Large, baby, not yet
            ChangeUnitSize unitSizeless = Helpers.Create<ChangeUnitSize>(x => x.SizeDelta = -2);
            FieldInfo typeFieldless = unitSizeless.GetType().GetField("m_Type", BindingFlags.NonPublic | BindingFlags.Instance);
            object deltaless = unitSizeless.GetType().GetNestedType("ChangeType", BindingFlags.NonPublic).GetField("Delta").GetValue(unitSizeless);
            typeFieldless.SetValue(unitSizeless, deltaless);

            BlueprintFeature changesizedamnit = Helpers.CreateFeature("IceGolemsizechange",
                "",
                "",
                Helpers.getGuid("IceGolemsizechange"),
                cannyobserver.Icon,
                FeatureGroup.None,
                unitSizeless);
            IceGolem.AddFacts = IceGolem.AddFacts.AddToArray(changesizedamnit);
           
            //Adjusting HP to size per construct rule
            BlueprintFeature AdjustHP = Helpers.CreateFeature("IceGolemAdjustHp",
               "",
               "",
               Helpers.getGuid("IceGolemAdjustHp"),
               cannyobserver.Icon,
               FeatureGroup.None,
               Helpers.CreateAddStatBonus(StatType.HitPoints, -20, ModifierDescriptor.UntypedStackable)
               );
            IceGolem.AddFacts = IceGolem.AddFacts.AddToArray(AdjustHP);

           

            //Fixing bleed immunity in the ui
            var bleedimmune = Main.library.Get<BlueprintBuff>("3f6038d75ccffaa40b338f4b13f9e4b6");
            Helpers.SetLocalizedStringField(bleedimmune, "m_DisplayName", "Bleed Immunity");
            Helpers.SetLocalizedStringField(bleedimmune, "m_Description", "This creature is immune to bleed damage and the bleeding condition");
            var stoneskinspell = Main.library.Get<BlueprintAbility>("c66e86905f7606c4eaa5c774f0357b2b");
            Helpers.SetField(bleedimmune, "m_Icon", stoneskinspell.Icon);

            //Level and shit
            IceGolem.ComponentsArray = animalCompanionUnitCentipede.ComponentsArray;
            IceGolem.ReplaceComponent<AddClassLevels>(l =>
            {
                l.Archetypes = Array.Empty<BlueprintArchetype>();
                
                l.CharacterClass = Main.library.Get<BlueprintCharacterClass>("fd66bdea5c33e5f458e929022322e6bf");
                l.Archetypes = l.Archetypes.AddToArray(thearchetypeforthisnicegolem);
                l.CharacterClass.Archetypes = Array.Empty<BlueprintArchetype>();
                l.CharacterClass.Archetypes = l.CharacterClass.Archetypes.AddToArray(thearchetypeforthisnicegolem);

                l.Selections = Array.Empty<SelectionEntry>();
              


            });
           

             // Brain and faction
            IceGolem.Brain = animalCompanionUnitCentipede.Brain;
            IceGolem.Faction = Main.library.Get<BlueprintFaction>("d8de50cc80eb4dc409a983991e0b77ad"); // Neutral faction

            //Portrait (maybe SHENNANINGANS because the m_Portait field doesn't exist
            Helpers.SetField(IceGolem, "m_Portrait", portrait);

           

            //for the size change at level 11
            ChangeUnitSize unitSizeplus = Helpers.Create<ChangeUnitSize>(x => x.SizeDelta = -1);
            FieldInfo typeField = unitSizeplus.GetType().GetField("m_Type", BindingFlags.NonPublic | BindingFlags.Instance);
            object delta = unitSizeplus.GetType().GetNestedType("ChangeType", BindingFlags.NonPublic).GetField("Delta").GetValue(unitSizeplus);
            typeField.SetValue(unitSizeplus, delta);

            //Dealing with iterative attack
            AddMechanicsFeature addMechanicsFeature = Helpers.Create<AddMechanicsFeature>();
            Traverse traverse = Traverse.Create(addMechanicsFeature);
            traverse.Field("m_Feature").SetValue(AddMechanicsFeature.MechanicsFeatureType.IterativeNaturalAttacks);

            //Creating the Feature
            BlueprintAbility icyprison = Main.library.Get<BlueprintAbility>("65e8d23aef5e7784dbeb27b1fca40931");
            BlueprintFeature ConstructCompanionIceGolemFeature = Main.library.CopyAndAdd<BlueprintFeature>("f9ef7717531f5914a9b6ecacfad63f46", "IceGolemCompanionFeature", Helpers.getGuid("IceGolemCompanionFeature"));
            ConstructCompanionIceGolemFeature.SetNameDescription("Ice Golem", "Size Small\nSpeed 30 ft.\nAC +2 natural armor, Cold Immunity and Fire weakness\nAttacks 1 Slam 1d6 +1d8 Cold \nHas the death throes ability and inflict cold damage to attacking creature\nAbility Scores Str 10, Dex 11, Con --, Int --, Wis 11, Cha 1 \nAt 11th level size becomes Large, Str +4,  +4 natural armor and gain the Icy Breath ability.");
            Helpers.SetField(ConstructCompanionIceGolemFeature, "m_Icon", icyprison.Icon);

            //Adding the pet
            AddPet addPetFact = ConstructCompanionIceGolemFeature.ComponentsArray.OfType<AddPet>().First();
            ConstructCompanionIceGolemFeature.RemoveComponent(addPetFact);
            addPetFact = UnityEngine.Object.Instantiate(addPetFact);
            ConstructCompanionIceGolemFeature.AddComponent(addPetFact);
            addPetFact.Pet = IceGolem;
            addPetFact.LevelRank = Main.library.Get<BlueprintFeature>(Helpers.getGuid("ConstructCompanionRank"));


            //Upgrade Feature
            addPetFact.UpgradeFeature = Helpers.CreateFeature(
                "IceGolemCompanionUpgradeFeature",
                "",
                "",
                Helpers.getGuid("IceGolemCompanionUpgradeFeature"),
                cannyobserver.Icon,
                FeatureGroup.None,
                addMechanicsFeature,
                unitSizeplus,
                Helpers.Create<AddStatBonus>(x =>
                {
                    x.Stat = StatType.AC;
                    x.Value = 4;
                    x.Descriptor = ModifierDescriptor.NaturalArmor;
                }),
                Helpers.Create<AddStatBonus>(x =>
                {
                    x.Stat = StatType.Strength;
                    x.Value = 4;
                }),
                Helpers.Create<AddStatBonus>(x =>
                {
                    x.Stat = StatType.Dexterity;
                    x.Value = 0;
                }),
                Helpers.Create<AddStatBonus>(x =>
                {
                    x.Stat = StatType.HitPoints;
                    x.Value = 10;
                }),
                CreateIceGolemBreathWeapon().CreateAddFact()

            ) ;
            addPetFact.UpgradeLevel = 11;

            return ConstructCompanionIceGolemFeature;
        }

        private static BlueprintFeature AddMudGolemCompanion()
        {

            BlueprintFeature cannyobserver = Main.library.Get<BlueprintFeature>("68a23a419b330de45b4c3789649b5b41");

            //Getting Protrait
            PortraitData portraitData = new PortraitData("thelostgrimoireMudGolem");
            BlueprintPortrait portrait = Helpers.Create<BlueprintPortrait>();
            portrait.Data = portraitData;
            Main.library.AddAsset(portrait, Helpers.getGuid("MudGolemPortrait"));


            //getting the golem
            BlueprintUnit MudGolem = Main.library.CopyAndAdd<BlueprintUnit>("1f9760775b988b749aa15355efca74d1", "MudGolemCompanion", Helpers.getGuid("MudGolemCompanion"));
            var strangecomponent = MudGolem.ComponentsArray.OfType<Kingmaker.UnitLogic.Mechanics.Components.Fixers.FixUnitOnPostLoad_AddNewFact>().First();
            //Editing Golem for balance reason and shit
            //remove Magic Immunity, its too good for now
            MudGolem.AddFacts = MudGolem.AddFacts.RemoveFromArray(Main.library.Get<BlueprintFeature>("407c8ca4e2edb18478a156077f1095b6"));
            // No DR either you get it from archetype
            MudGolem.AddFacts = MudGolem.AddFacts.RemoveFromArray(Main.library.Get<BlueprintFeature>("a519018333016cd4696b1b81f8c22a29"));
            //Remove cursed wound because either useless or enemy wont know how to deal with it
            MudGolem.AddFacts = MudGolem.AddFacts.RemoveFromArray(Main.library.Get<BlueprintFeature>("4e937d8f914329d4c9834ca42596dba6"));
            //Remove slippery mud, you'll get back on upgrade but we've got to recreate it, so..
            MudGolem.AddFacts = MudGolem.AddFacts.RemoveFromArray(Main.library.Get<BlueprintActivatableAbility>("80bf9a7396834b94693c841839b9de42"));
            var SlipperyMud = CreateMudGolemSlipperyMud();
            
           //Change Haste, it's wrong with no ressource, we'll redo it
           MudGolem.AddFacts = MudGolem.AddFacts.RemoveFromArray(Main.library.Get<BlueprintAbility>("cf5dbfcb40c6f7245aa19e7234489530"));
            MudGolem.AddFacts = MudGolem.AddFacts.AddToArray(CreateMudGolemHaste());
            //And a tidy bit less of base Nat AC
            MudGolem.AddFacts = MudGolem.AddFacts.RemoveFromArray(Main.library.Get<BlueprintUnitFact>("73a90b2a70d576f429ad401e7a5a8a4f"));
            MudGolem.AddFacts = MudGolem.AddFacts.AddToArray(Main.library.Get<BlueprintUnitFact>("e73864391ccf0894997928443a29d755"));
            

            //Changing the Abilities a bit
            MudGolem.Strength = 14;
            MudGolem.Dexterity = 11;
            MudGolem.Wisdom = 11;
            MudGolem.Charisma = 1;

            //Changing those weapons
            BlueprintItemWeapon EmptyHandWeapon = Main.library.Get<BlueprintItemWeapon>("20375b5a0c9243d45966bd72c690ab74");
            MudGolem.Body.EmptyHandWeapon = EmptyHandWeapon;
            MudGolem.Body.DisableHands = false;
            BlueprintItemWeapon GolemCompanionSlam = Main.library.Get<BlueprintItemWeapon>("bbdf5d550dc406640a77c5d2a05244ca");
            MudGolem.Body.PrimaryHand = GolemCompanionSlam;
            MudGolem.Body.AdditionalLimbs = Array.Empty<BlueprintItemWeapon>();

            //Size: Not Large, baby, not yet
            ChangeUnitSize unitSizeless = Helpers.Create<ChangeUnitSize>(x => x.SizeDelta = -2);
            FieldInfo typeFieldless = unitSizeless.GetType().GetField("m_Type", BindingFlags.NonPublic | BindingFlags.Instance);
            object deltaless = unitSizeless.GetType().GetNestedType("ChangeType", BindingFlags.NonPublic).GetField("Delta").GetValue(unitSizeless);
            typeFieldless.SetValue(unitSizeless, deltaless);

            BlueprintFeature changesizedamnit = Helpers.CreateFeature("Golemsizechange",
                "",
                "",
                Helpers.getGuid("MudGolemsizechange"),
                cannyobserver.Icon,
                FeatureGroup.None,
                unitSizeless);
            MudGolem.AddFacts = MudGolem.AddFacts.AddToArray(changesizedamnit);

            //Adjusting HP to size per construct rule
            BlueprintFeature AdjustHP = Helpers.CreateFeature("MudGolemAdjustHp",
               "",
               "",
               Helpers.getGuid("MudGolemAdjustHp"),
               cannyobserver.Icon,
               FeatureGroup.None,
               Helpers.CreateAddStatBonus(StatType.HitPoints, -20, ModifierDescriptor.UntypedStackable)
               );
            MudGolem.AddFacts = MudGolem.AddFacts.AddToArray(AdjustHP);

            // Setting the golem right with class, archetype and all
            var ConstructType = Main.library.Get<BlueprintFeature>("fd389783027d63343b4a5634bd81645f");
            List<LevelEntry> addtypeFeatures = new List<LevelEntry>();
            addtypeFeatures.Add(Helpers.LevelEntry(1, ConstructType));

            //Fixing bleed immunity in the ui
            var bleedimmune = Main.library.Get<BlueprintBuff>("3f6038d75ccffaa40b338f4b13f9e4b6");
            Helpers.SetLocalizedStringField(bleedimmune, "m_DisplayName", "Bleed Immunity");
            Helpers.SetLocalizedStringField(bleedimmune, "m_Description", "This creature is immune to bleed damage and the bleeding condition");
            var stoneskinspell = Main.library.Get<BlueprintAbility>("c66e86905f7606c4eaa5c774f0357b2b");
            Helpers.SetField(bleedimmune, "m_Icon", stoneskinspell.Icon);

            MudGolem.ComponentsArray = animalCompanionUnitCentipede.ComponentsArray;
            MudGolem.ReplaceComponent<AddClassLevels>(l =>
            {
                l.Archetypes = Array.Empty<BlueprintArchetype>();

                l.CharacterClass = Main.library.Get<BlueprintCharacterClass>("fd66bdea5c33e5f458e929022322e6bf");
                l.Archetypes = l.Archetypes.AddToArray(thearchetypeforthisnicegolem);
                l.CharacterClass.Archetypes = Array.Empty<BlueprintArchetype>();
                l.CharacterClass.Archetypes = l.CharacterClass.Archetypes.AddToArray(thearchetypeforthisnicegolem);

                l.Selections = Array.Empty<SelectionEntry>();
                


            });
            // Brain and faction
            MudGolem.Brain = animalCompanionUnitCentipede.Brain;
            MudGolem.Faction = Main.library.Get<BlueprintFaction>("d8de50cc80eb4dc409a983991e0b77ad"); // Neutral faction

            //Portrait
            Helpers.SetField(MudGolem, "m_Portrait", portrait);


            //for the size change at level 11
            ChangeUnitSize unitSizeplus = Helpers.Create<ChangeUnitSize>(x => x.SizeDelta = -1);
            FieldInfo typeField = unitSizeplus.GetType().GetField("m_Type", BindingFlags.NonPublic | BindingFlags.Instance);
            object delta = unitSizeplus.GetType().GetNestedType("ChangeType", BindingFlags.NonPublic).GetField("Delta").GetValue(unitSizeplus);
            typeField.SetValue(unitSizeplus, delta);
            
            
            //Creating the Feature
            BlueprintAbility slowspell = Main.library.Get<BlueprintAbility>("f492622e473d34747806bdb39356eb89");
            BlueprintFeature ConstructCompanionMudGolemFeature = Main.library.CopyAndAdd<BlueprintFeature>("f9ef7717531f5914a9b6ecacfad63f46", "MudGolemCompanionFeature", Helpers.getGuid("MudGolemCompanionFeature"));
            ConstructCompanionMudGolemFeature.SetNameDescription("Mud Golem", "Size Small\nSpeed 20 ft.\nAC +6 natural armor\nAttacks 1 Slam 1d6 \nAbility Scores Str 14, Dex 11, Con --, Int --, Wis 11, Cha 1 and can Hast itself once a day.\n Gains DR/Adamantine with level  \nAt 11th level size becomes Large, Str +4, Dex -2,  +4 natural armor and gain the Slippery Mud ability.");
            Helpers.SetField(ConstructCompanionMudGolemFeature, "m_Icon", slowspell.Icon);

            //Adding the pet
            AddPet addPetFact = ConstructCompanionMudGolemFeature.ComponentsArray.OfType<AddPet>().First();
            ConstructCompanionMudGolemFeature.RemoveComponent(addPetFact);
            addPetFact = UnityEngine.Object.Instantiate(addPetFact);
            ConstructCompanionMudGolemFeature.AddComponent(addPetFact);
            addPetFact.Pet = MudGolem;
            addPetFact.LevelRank = Main.library.Get<BlueprintFeature>(Helpers.getGuid("ConstructCompanionRank"));

            //Upgrade Feature
            addPetFact.UpgradeFeature = Helpers.CreateFeature(
                "MudGolemCompanionUpgradeFeature",
                "",
                "",
                Helpers.getGuid("MudGolemCompanionUpgradeFeature"),
                cannyobserver.Icon,
                FeatureGroup.None,
                
                unitSizeplus,
                Helpers.Create<AddStatBonus>(x =>
                {
                    x.Stat = StatType.AC;
                    x.Value = 4;
                    x.Descriptor = ModifierDescriptor.NaturalArmor;
                }),
                Helpers.Create<AddStatBonus>(x =>
                {
                    x.Stat = StatType.Strength;
                    x.Value = 4;
                }),
                Helpers.Create<AddStatBonus>(x =>
                {
                    x.Stat = StatType.Dexterity;
                    x.Value = -2;
                }),
                Helpers.Create<AddStatBonus>(x =>
                {
                    x.Stat = StatType.HitPoints;
                    x.Value = 10;
                }),
                Helpers.CreateAddFact(SlipperyMud)

            ); 
            addPetFact.UpgradeLevel = 11;
           
            return ConstructCompanionMudGolemFeature;
        }
        private static BlueprintFeature AddGolemCompanion()
        {

            BlueprintFeature cannyobserver = Main.library.Get<BlueprintFeature>("68a23a419b330de45b4c3789649b5b41");

            //Getting Protrait
            PortraitData portraitData = new PortraitData("thelostgrimoireGolem");
            BlueprintPortrait portrait = Helpers.Create<BlueprintPortrait>();
            portrait.Data = portraitData;
            Main.library.AddAsset(portrait, Helpers.getGuid("StoneGolemPortrait"));


            //getting the golem
            BlueprintUnit StoneGolem = Main.library.CopyAndAdd<BlueprintUnit>("dfd21dba15fe7dd4f95961ff27d91836", "StoneGolemCompanion", Helpers.getGuid("StoneGolemCompanion"));

            //Editing Golem for balance reason and shit
            //remove Magic Immunity, its too good 
            StoneGolem.AddFacts = StoneGolem.AddFacts.RemoveFromArray(Main.library.Get<BlueprintFeature>("2617c0ea094687643a14fd99c4529523"));
            // No DR either you get it from archetype
            StoneGolem.AddFacts = StoneGolem.AddFacts.RemoveFromArray(Main.library.Get<BlueprintFeature>("769396cce5ee41f4cb4b6579ff271269"));
            //And a tidy bit less of base Nat AC
            StoneGolem.AddFacts = StoneGolem.AddFacts.RemoveFromArray(Main.library.Get<BlueprintUnitFact>("66b08b1f48983c54eb3f175d24ac7039"));
            StoneGolem.AddFacts = StoneGolem.AddFacts.AddToArray(Main.library.Get<BlueprintUnitFact>("da6417809bdedfa468dd2fd0cc74be92"));
            //Remove slow because it's either op or useless
            //BlueprintAbility SlowAbility = Main.library.Get<BlueprintAbility>("341cd9a1610563d44938145e4b8d5432");
            StoneGolem.RemoveComponents<AddAbilityToCharacterComponent>();

            //Changing the Abilities a bit
            StoneGolem.Strength = 20;
            StoneGolem.Dexterity = 11;
            StoneGolem.Wisdom = 11;
            StoneGolem.Charisma = 1;

            //Changing those weapons
            BlueprintItemWeapon EmptyHandWeapon = Main.library.Get<BlueprintItemWeapon>("20375b5a0c9243d45966bd72c690ab74");
            StoneGolem.Body.EmptyHandWeapon = EmptyHandWeapon;
            StoneGolem.Body.DisableHands = false;
            BlueprintItemWeapon GolemCompanionSlam = Main.library.Get<BlueprintItemWeapon>("bbdf5d550dc406640a77c5d2a05244ca");
            StoneGolem.Body.PrimaryHand = GolemCompanionSlam;
            StoneGolem.Body.AdditionalLimbs = Array.Empty<BlueprintItemWeapon>();

            //Size: Not Large, baby, not yet
            ChangeUnitSize unitSizeless = Helpers.Create<ChangeUnitSize>(x => x.SizeDelta = -1);
            FieldInfo typeFieldless = unitSizeless.GetType().GetField("m_Type", BindingFlags.NonPublic | BindingFlags.Instance);
            object deltaless = unitSizeless.GetType().GetNestedType("ChangeType", BindingFlags.NonPublic).GetField("Delta").GetValue(unitSizeless);
            typeFieldless.SetValue(unitSizeless, deltaless);

            BlueprintFeature changesizedamnit = Helpers.CreateFeature("Golemsizechange",
                "",
                "",
                Helpers.getGuid("Golemsizechange"),
                cannyobserver.Icon,
                FeatureGroup.None,
                unitSizeless);
            StoneGolem.AddFacts = StoneGolem.AddFacts.AddToArray(changesizedamnit);

            //Adjusting HP to size per construct rule
            BlueprintFeature AdjustHP = Helpers.CreateFeature("StoneGolemAdjustHp",
               "",
               "",
               Helpers.getGuid("StoneGolemAdjustHp"),
               cannyobserver.Icon,
               FeatureGroup.None,
               Helpers.CreateAddStatBonus(StatType.HitPoints, -10, ModifierDescriptor.UntypedStackable)
               );
            StoneGolem.AddFacts = StoneGolem.AddFacts.AddToArray(AdjustHP);

            // Setting the golem right with class, archetype and all
            var ConstructType = Main.library.Get<BlueprintFeature>("fd389783027d63343b4a5634bd81645f");
            List<LevelEntry> addtypeFeatures = new List<LevelEntry>();
            addtypeFeatures.Add(Helpers.LevelEntry(1, ConstructType));

            //Fixing bleed immunity in the ui
            var bleedimmune = Main.library.Get<BlueprintBuff>("3f6038d75ccffaa40b338f4b13f9e4b6");
            Helpers.SetLocalizedStringField(bleedimmune, "m_DisplayName", "Bleed Immunity");
            Helpers.SetLocalizedStringField(bleedimmune, "m_Description", "This creature is immune to bleed damage and the bleeding condition");
            var stoneskinspell = Main.library.Get<BlueprintAbility>("c66e86905f7606c4eaa5c774f0357b2b");
            Helpers.SetField(bleedimmune, "m_Icon", stoneskinspell.Icon);

            StoneGolem.ComponentsArray = animalCompanionUnitCentipede.ComponentsArray;
            StoneGolem.ReplaceComponent<AddClassLevels>(l =>
            {
                l.Archetypes = Array.Empty<BlueprintArchetype>();

                l.CharacterClass = Main.library.Get<BlueprintCharacterClass>("fd66bdea5c33e5f458e929022322e6bf");
                l.Archetypes = l.Archetypes.AddToArray(thearchetypeforthisnicegolem);
                l.CharacterClass.Archetypes = Array.Empty<BlueprintArchetype>();
                l.CharacterClass.Archetypes = l.CharacterClass.Archetypes.AddToArray(thearchetypeforthisnicegolem);

                l.Selections = Array.Empty<SelectionEntry>();


            });

            // Brain and faction
            StoneGolem.Brain = animalCompanionUnitCentipede.Brain;
            StoneGolem.Faction = Main.library.Get<BlueprintFaction>("d8de50cc80eb4dc409a983991e0b77ad"); // Neutral faction

            //Portrait (maybe SHENNANINGANS because the m_Portait field doesn't exist
            Helpers.SetField(StoneGolem, "m_Portrait", portrait);


            //for the size change at level 11
            ChangeUnitSize unitSizeplus = Helpers.Create<ChangeUnitSize>(x => x.SizeDelta = 0);
            FieldInfo typeField = unitSizeplus.GetType().GetField("m_Type", BindingFlags.NonPublic | BindingFlags.Instance);
            object delta = unitSizeplus.GetType().GetNestedType("ChangeType", BindingFlags.NonPublic).GetField("Delta").GetValue(unitSizeplus);
            typeField.SetValue(unitSizeplus, delta);

            //Dealing with iterative attack
            AddMechanicsFeature addMechanicsFeature = Helpers.Create<AddMechanicsFeature>();
            Traverse traverse = Traverse.Create(addMechanicsFeature);
            traverse.Field("m_Feature").SetValue(AddMechanicsFeature.MechanicsFeatureType.IterativeNaturalAttacks);

            //Creating the Feature
            BlueprintFeature camouflagefeature = Main.library.Get<BlueprintFeature>("ff1b5aa8dcc7d7d4d9aa85e1cb3f9e88");
            BlueprintFeature ConstructCompanionStoneGolemFeature = Main.library.CopyAndAdd<BlueprintFeature>("f9ef7717531f5914a9b6ecacfad63f46", "StoneGolemCompanionFeature", Helpers.getGuid("StoneGolemCompanionFeature"));
            ConstructCompanionStoneGolemFeature.SetNameDescription("Stone Golem", "Size Medium\nSpeed 20 ft.\nAC +9 natural armor\nAttacks 1 Slam 1d8 \nAbility Scores Str 20, Dex 11, Con --, Int --, Wis 11, Cha 1\nAt 11th level size becomes Large, Str +4, Dex -2,  +6 natural armor and gain the Slow Ability.");
            Helpers.SetField(ConstructCompanionStoneGolemFeature, "m_Icon", camouflagefeature.Icon);

            //Adding the pet
            AddPet addPetFact = ConstructCompanionStoneGolemFeature.ComponentsArray.OfType<AddPet>().First();
            ConstructCompanionStoneGolemFeature.RemoveComponent(addPetFact);
            addPetFact = UnityEngine.Object.Instantiate(addPetFact);
            ConstructCompanionStoneGolemFeature.AddComponent(addPetFact);
            addPetFact.Pet = StoneGolem;
            addPetFact.LevelRank = Main.library.Get<BlueprintFeature>(Helpers.getGuid("ConstructCompanionRank"));

            //Upgrade Feature
            addPetFact.UpgradeFeature = Helpers.CreateFeature(
                "StoneGolemCompanionUpgradeFeature",
                "",
                "",
                Helpers.getGuid("StoneGolemCompanionUpgradeFeature"),
                cannyobserver.Icon,
                FeatureGroup.None,
                addMechanicsFeature,
                unitSizeplus,
                createstonegolemslow().CreateAddFact(),
                Helpers.Create<AddStatBonus>(x =>
                {
                    x.Stat = StatType.AC;
                    x.Value = 6;
                    x.Descriptor = ModifierDescriptor.NaturalArmor;
                }),
                Helpers.Create<AddStatBonus>(x =>
                {
                    x.Stat = StatType.Strength;
                    x.Value = 4;
                }),
                Helpers.Create<AddStatBonus>(x =>
                {
                    x.Stat = StatType.Dexterity;
                    x.Value = -2;
                }),
                Helpers.Create<AddStatBonus>(x =>
                {
                    x.Stat = StatType.HitPoints;
                    x.Value = 10;
                })


            );
            addPetFact.UpgradeLevel = 11;

            return ConstructCompanionStoneGolemFeature;
        }
        private static BlueprintFeature AddScarecrowCompanion()
        {

            BlueprintFeature cannyobserver = Main.library.Get<BlueprintFeature>("68a23a419b330de45b4c3789649b5b41");

            //Getting Protrait
            PortraitData portraitData = new PortraitData("thelostgrimoireScarecrow");
            BlueprintPortrait portrait = Helpers.Create<BlueprintPortrait>();
            portrait.Data = portraitData;
            Main.library.AddAsset(portrait, Helpers.getGuid("ScarecrowPortrait"));

            BlueprintUnitFact reducedReachFact = Main.library.Get<BlueprintUnitFact>("c33f2d68d93ceee488aa4004347dffca"); 

            //getting the Scarecrow
            BlueprintUnit ScarecrowBase = Main.library.CopyAndAdd<BlueprintUnit>("19a8d635157d1814fb55fe270e1b5ccc", "ScarecroweCompanion", Helpers.getGuid("ScarecroweCompanion"));

          
            // Remove the gaze attack for now and redo it as this version is op
            var Scarecrowgazeattack = Main.library.Get<BlueprintFeature>("97677858a439d4d47af6ecc6a5d678f2");
           
            ScarecrowBase.AddFacts = ScarecrowBase.AddFacts.RemoveFromArray(Scarecrowgazeattack);
           
            

            //We change abit of the scarcrow stat as it will succeed too much on attack 
            ScarecrowBase.Strength = 10;
            ScarecrowBase.Charisma = 14;

            //Only one attack for the scarecrow whatever its level and getting rid of that enchant that doesn't work (we'll add it back later)
            BlueprintItemWeapon ScarecrowCompanionSlam = Main.library.CopyAndAdd<BlueprintItemWeapon>("10687d9b0cc29d54cb86a06c07f1400e", "ScarecrowCompanionSlam", Helpers.getGuid("ScarecrowCompanionSlam"));
            ScarecrowCompanionSlam.Enchantments.Clear();

            ScarecrowBase.Body.PrimaryHand = ScarecrowCompanionSlam;
            ScarecrowBase.Body.AdditionalLimbs = Array.Empty<BlueprintItemWeapon>();

            ScarecrowBase.AddFacts = ScarecrowBase.AddFacts.AddToArray(CreateScarecrowFearTouch());
            // Setting the golem right with class, archetype and all
            var ConstructType = Main.library.Get<BlueprintFeature>("fd389783027d63343b4a5634bd81645f");
            List<LevelEntry> addtypeFeatures = new List<LevelEntry>();
            addtypeFeatures.Add(Helpers.LevelEntry(1, ConstructType));

            //Fixing bleed immunity in the ui
            var bleedimmune = Main.library.Get<BlueprintBuff>("3f6038d75ccffaa40b338f4b13f9e4b6");
            Helpers.SetLocalizedStringField(bleedimmune, "m_DisplayName", "Bleed Immunity");
            Helpers.SetLocalizedStringField(bleedimmune, "m_Description", "This creature is immune to bleed damage and the bleeding condition");
            var stoneskinspell = Main.library.Get<BlueprintAbility>("c66e86905f7606c4eaa5c774f0357b2b");
            Helpers.SetField(bleedimmune, "m_Icon", stoneskinspell.Icon);

            //Setting Archetype and Class
            ScarecrowBase.ComponentsArray = animalCompanionUnitCentipede.ComponentsArray;
            ScarecrowBase.ReplaceComponent<AddClassLevels>(l =>
            {
                l.Archetypes = Array.Empty<BlueprintArchetype>();

                l.CharacterClass = Main.library.Get<BlueprintCharacterClass>("fd66bdea5c33e5f458e929022322e6bf");
                l.Archetypes = l.Archetypes.AddToArray(thearchetypeforthisnicegolem);
                l.CharacterClass.Archetypes = Array.Empty<BlueprintArchetype>();
                l.CharacterClass.Archetypes = l.CharacterClass.Archetypes.AddToArray(thearchetypeforthisnicegolem);

                l.Selections = Array.Empty<SelectionEntry>();


            });
            // Brain and faction
            ScarecrowBase.Brain = animalCompanionUnitCentipede.Brain;
            ScarecrowBase.Faction = Main.library.Get<BlueprintFaction>("d8de50cc80eb4dc409a983991e0b77ad"); // Neutral faction

            //Portrait (maybe SHENNANINGANS because the m_Portait field doesn't exist
            Helpers.SetField(ScarecrowBase, "m_Portrait", portrait);


            //for the size change at level 11
            ChangeUnitSize unitSizeplus = Helpers.Create<ChangeUnitSize>(x => x.SizeDelta = 1);
            FieldInfo typeField = unitSizeplus.GetType().GetField("m_Type", BindingFlags.NonPublic | BindingFlags.Instance);
            object delta = unitSizeplus.GetType().GetNestedType("ChangeType", BindingFlags.NonPublic).GetField("Delta").GetValue(unitSizeplus);
            typeField.SetValue(unitSizeplus, delta);

            

            //Creating the Feature
            BlueprintAbility fear = Main.library.Get<BlueprintAbility>("d2aeac47450c76347aebbc02e4f463e0");
            BlueprintFeature ConstructCompanionScarecrowFeature = Main.library.CopyAndAdd<BlueprintFeature>("f9ef7717531f5914a9b6ecacfad63f46", "ScarecrowCompanionFeature", Helpers.getGuid("ScarecrowCompanionFeature"));
            ConstructCompanionScarecrowFeature.SetNameDescription("Scarecrow", "Size Medium\nSpeed 20 ft.\nAC +6 natural armor\nAttacks 1 Slam 1d8 + Fear Touch \nAbility Scores Str 10, Dex 14, Con --, Int --, Wis 11, Cha 14\nAt 11th level size becomes Large, Str +4, Dex +4, Wis +4, Cha +6, +2 natural armor and gain the Fascinating Gaze ability.");
            Helpers.SetField(ConstructCompanionScarecrowFeature, "m_Icon", fear.Icon);

            //Adding the pet
            AddPet addPetFact = ConstructCompanionScarecrowFeature.ComponentsArray.OfType<AddPet>().First();
            ConstructCompanionScarecrowFeature.RemoveComponent(addPetFact);
            addPetFact = UnityEngine.Object.Instantiate(addPetFact);
            ConstructCompanionScarecrowFeature.AddComponent(addPetFact);
            addPetFact.Pet = ScarecrowBase;
            addPetFact.LevelRank = Main.library.Get<BlueprintFeature>(Helpers.getGuid("ConstructCompanionRank"));

            //Upgrade Feature
            addPetFact.UpgradeFeature = Helpers.CreateFeature(
                "ScarecrowCompanionUpgradeFeature",
                "",
                "",
                Helpers.getGuid("ScarecrowCompanionUpgradeFeature"),
                cannyobserver.Icon,
                FeatureGroup.None,

                unitSizeplus,
                Helpers.Create<AddStatBonus>(x =>
                {
                    x.Stat = StatType.AC;
                    x.Value = 2;
                    x.Descriptor = ModifierDescriptor.NaturalArmor;
                }),
                Helpers.Create<AddStatBonus>(x =>
                {
                    x.Stat = StatType.Strength;
                    x.Value = 4;
                }),
                Helpers.Create<AddStatBonus>(x =>
                {
                    x.Stat = StatType.Dexterity;
                    x.Value = 4;
                }),
                Helpers.Create<AddStatBonus>(x =>
                {
                    x.Stat = StatType.Wisdom;
                    x.Value = 4;
                }),
                Helpers.Create<AddStatBonus>(x =>
                {
                    x.Stat = StatType.Charisma;
                    x.Value = 6;
                }),
                Helpers.Create<AddStatBonus>(x =>
                {
                    x.Stat = StatType.HitPoints;
                    x.Value = 10;
                }),
                Helpers.CreateAddFact(reducedReachFact),
                Helpers.CreateAddFact(CreateScarecrowFascinatingGaze())
            ) ;
            addPetFact.UpgradeLevel = 11;
            return ConstructCompanionScarecrowFeature;
        }
        private static BlueprintAbility CreateSteamDragonBreathWeapon()
        {
            var Icon = Helpers.GetIcon("f0bd350c96848364d8c8f7d3167499e9");//breath weapon icon
            var resource = Helpers.CreateAbilityResource("SteamDragonCompanionBreathWeaponResource", "", "", Helpers.getGuid("SteamDragonCompanionBreathWeaponResource"), Icon);
            resource.SetIncreasedByLevelStartPlusDivStep(1,9, 1, 2, 1, 1, 0, new BlueprintCharacterClass[] { Main.library.Get<BlueprintCharacterClass>("fd66bdea5c33e5f458e929022322e6bf") }, new BlueprintArchetype[] { thearchetypeforthisnicegolem });
            
            var resourcelogic = Helpers.CreateResourceLogic(resource);
            resourcelogic.Amount = 1;
            var ability = Main.library.CopyAndAdd<BlueprintAbility>("f0bd350c96848364d8c8f7d3167499e9", "SteamDragonCompanionBreathWeaponAbility", Helpers.getGuid("SteamDragonCompanionBreathWeaponAbility"));
            var rankconfig = Helpers.CreateContextRankConfig(ContextRankBaseValueType.CasterLevel, ContextRankProgression.AsIs, AbilityRankType.DamageDice, null, null, 0,0,false, StatType.Unknown) ;
            var paramDc = Helpers.Create<ContextCalculateAbilityParamsBasedOnClass>(p =>
            {
                p.CharacterClass = Main.library.Get<BlueprintCharacterClass>("fd66bdea5c33e5f458e929022322e6bf");
                p.StatType = StatType.Constitution;

            });
            ability.SetNameDescription("Breath Weapon", "Two time a day, a steam dragon can breath a line of fire, the number of use per day of this breath weapon increase by 1 every other level after the 9th. This breath weapon deals 1d6 points of fire damage per level in a 60-foot line. Those caught in the area of the breath can attempt a Reflex save to halve the normal damage.");
            ability.RemoveComponents<ContextRankConfig>();
            ability.RemoveComponents<AbilityResourceLogic>();
            ability.AddComponents(paramDc, rankconfig, resource.CreateAddAbilityResource(), resourcelogic);
            return ability;
        }

       private static BlueprintAbility CreateIceGolemBreathWeapon()
        {
            var Icon = Helpers.GetIcon("1e5458fc51153d44f8e1226b53feaf55");//breath weapon icon
            //resource
            var resource = Helpers.CreateAbilityResource("IceGolemBreathWeaponResource", "", "",
               Helpers.getGuid("IceGolemBreathWeaponResource"), Icon);
            resource.SetIncreasedByLevelStartPlusDivStep(1, 1, 1, 3, 1, 1, 0, new BlueprintCharacterClass[] { Main.library.Get<BlueprintCharacterClass>("fd66bdea5c33e5f458e929022322e6bf") }, new BlueprintArchetype[] { thearchetypeforthisnicegolem });
            var resourcelogic = Helpers.CreateResourceLogic(resource);

            //Ability
            var ability = Helpers.CreateAbility("IceGolemBreathWeaponAbility", "Icy Breath", "An ice golem can breath a 15feet cone of ice as a swift action, the cone deals 5d6 cold damage to every creature inside it or half as much if they succeed at a reflex saving throw.", Helpers.getGuid("IceGolemBreathWeaponAbility"),
                Icon, AbilityType.Supernatural, CommandType.Swift, AbilityRange.Projectile, "", "",
                Helpers.Create<AbilityDeliverProjectile>(p =>
                {
                    p.Projectiles = new BlueprintProjectile[] { Main.library.Get<BlueprintProjectile>("5af8b717a209fd444a1e4d077ed776f0") };
                    p.Type = AbilityProjectileType.Cone;
                    p.Length = 15.Feet();
                    p.LineWidth = 5.Feet();


                }),
                Helpers.Create<AbilityEffectRunAction>(a =>
                {
                    a.SavingThrowType = SavingThrowType.Reflex;
                    a.Actions = Helpers.CreateActionList(Helpers.CreateActionDealDamage(Kingmaker.Enums.Damage.DamageEnergyType.Cold, Helpers.CreateContextDiceValue(DiceType.D6, 5), true, true));

                }),
                Helpers.Create<ContextCalculateAbilityParamsBasedOnClass>(p =>
                {
                    p.CharacterClass = Main.library.Get<BlueprintCharacterClass>("fd66bdea5c33e5f458e929022322e6bf");
                    p.StatType = StatType.Constitution;

                }),
                Helpers.CreateAddAbilityResource(resource),
                resourcelogic
                ) ;
            ability.ResourceAssetIds = new string[] { "d7263bcbe9cddf648b6df1816d50ba8f", "921700b1280d41f44a12fada77bbfe87" };
            ability.Animation = Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle.BreathWeapon;
            ability.CanTargetEnemies = true;
            ability.CanTargetFriends = true;
            ability.CanTargetPoint = true;
            ability.CanTargetSelf = true;
            ability.EffectOnEnemy = AbilityEffectOnUnit.Harmful;
            //feature

            return ability;
        }
        
        private static BlueprintBuff CreateIceGolemIcyBurst()
        {
            var Icon = Helpers.GetIcon("ba48abb52b142164eba309fd09898856");
            
            //spell
            var spell = Main.library.CopyAndAdd<BlueprintAbility>("65f2dddafbddbd948aa7595d0566b8a8", "IceGolemCompanionDeathThroes", Helpers.getGuid("IceGolemCompanionDeathThroes"));
            spell.ComponentsArray = Array.Empty<BlueprintComponent>();

            //deal damage component
            var damagecompo = Helpers.CreateActionDealDamage(Kingmaker.Enums.Damage.DamageEnergyType.Cold, Helpers.CreateContextDiceValue(DiceType.D6, 5), true, true);

            //Component runaction
            var runaction = Helpers.CreateRunActions(damagecompo);
            runaction.SavingThrowType = SavingThrowType.Reflex;
            

            //descriptor component
            var descriptor = Helpers.Create<SpellDescriptorComponent>();
            descriptor.Descriptor = SpellDescriptor.Cold;
            //aoe comonent
            var aoe = Helpers.CreateAbilityTargetsAround(10.Feet(), TargetType.Any);
            //fx component
            PrefabLink fx = new PrefabLink();
            fx.AssetId = "2f714487dbde9554f85ba4b9627575dc"; 
            var spawnfx = Helpers.Create<AbilitySpawnFx>();
            spawnfx.PrefabLink = fx;
            spawnfx.Time = AbilitySpawnFxTime.OnStart;
            //ability param component
            var param = Helpers.Create<ContextCalculateAbilityParamsBasedOnClass>(p =>
            {
                p.CharacterClass = Main.library.Get<BlueprintCharacterClass>("fd66bdea5c33e5f458e929022322e6bf");
                p.StatType = StatType.Constitution;

            });
            spell.AddComponents(runaction, descriptor, aoe, spawnfx, param);
            spell.ResourceAssetIds = new string[] { "2f714487dbde9554f85ba4b9627575dc" };
            Helpers.SetField(spell, "m_Icon", Icon);

            

            //buff 
            var buff = Main.library.CopyAndAdd<BlueprintBuff>("19050da71d3afb043b57159d44fc2449", "IceGolemCompanionDeathThroesBuff", Helpers.getGuid("IceGolemCompanionDeathThroesBuff"));
            buff.ComponentsArray = Array.Empty<BlueprintComponent>();
            var Ondeath = Helpers.Create<DeathActions>(a =>
            {
                a.Actions = Helpers.CreateActionList(Helpers.Create<ContextActionCastSpell>(p => p.Spell = spell));
            });
            buff.AddComponent(Ondeath);
            Helpers.SetField(buff, "m_Icon", Icon);
            buff.SetBuffFlags(BuffFlags.StayOnDeath);

           

            return buff;
        }

        private static BlueprintBuff[] AddSteamDragonApperance()
        {
            PrefabLink buff1fx = new PrefabLink();
            buff1fx.AssetId = "f684f2a037e944f4a894037c86e4194b"; //petrification
            PrefabLink buff2fx = new PrefabLink();
            buff2fx.AssetId = "53c86872d2be80b48afc218af1b204d7"; //rage
            PrefabLink buff3fx = new PrefabLink();
            buff3fx.AssetId = "85c69a4547e5f4e4c8ad90e79aff373a"; //lichking
            PrefabLink buff4fx = new PrefabLink();
            buff4fx.AssetId = "4ae01d6a8b5cdaa45a115fcf6aca4a48"; //wintermouth steam
            PrefabLink buff5fx = new PrefabLink();
            buff5fx.AssetId = Helpers.getGuid("NewFXforSpectrecycle"); //scpectre FX
            PrefabLink fire = new PrefabLink();
            fire.AssetId = "b936c023ae73e59498c67759ecbdb177";//Fire00_StandartHit
            PrefabLink wind = new PrefabLink();
            wind.AssetId = "d0d11b277d0e19c479e42ce891deec02";//WindProjectile00_Hit
            PrefabLink decal = new PrefabLink();
            decal.AssetId = "85a59070f10741745af33c96a5d967f4";//Fire00_MajorHit_Decal
            var icon = Helpers.GetIcon("68a23a419b330de45b4c3789649b5b41");
            //empty buff1 
            var buff1 = Helpers.CreateBuff("Steamlookpart1", "", "", Helpers.getGuid("Steamlookpart1"), icon, buff1fx);
            buff1.SetBuffFlags(BuffFlags.HiddenInUi | BuffFlags.StayOnDeath);
            buff1.ResourceAssetIds = new string[] { buff1fx.AssetId };
            //empty buff2 
            var buff2 = Helpers.CreateBuff("Steamlookpart2", "", "", Helpers.getGuid("Steamlookpart2"), icon, buff2fx);
            buff2.SetBuffFlags(BuffFlags.HiddenInUi | BuffFlags.StayOnDeath);
            buff2.ResourceAssetIds = new string[] { buff2fx.AssetId };
            //empty buff3 
            var buff3 = Helpers.CreateBuff("Steamlookpart3", "", "", Helpers.getGuid("Steamlookpart3"), icon, buff3fx);
            buff3.ResourceAssetIds = new string[] { buff3fx.AssetId};
            buff3.SetBuffFlags(BuffFlags.HiddenInUi | BuffFlags.StayOnDeath);
            // empty buff 4
            var buff4 = Helpers.CreateBuff("Steamlookpart4", "", "", Helpers.getGuid("Steamlookpart4"), icon, buff4fx);
            buff4.SetBuffFlags(BuffFlags.HiddenInUi | BuffFlags.StayOnDeath);
            buff4.ResourceAssetIds = new string[] { buff4fx.AssetId };



            var buff5 = Helpers.CreateBuff("Steamlookpart5", "", "", Helpers.getGuid("Steamlookpart5"), icon, buff4fx);
            buff5.SetBuffFlags(BuffFlags.HiddenInUi | BuffFlags.StayOnDeath);
            buff5.ResourceAssetIds = new string[] { buff4fx.AssetId };
            //Buff5
            var buff6 = Helpers.CreateBuff("Steamlookpart6", "", "", Helpers.getGuid("Steamlookpart6"), icon, buff5fx, null,
                Helpers.Create<AddIncomingDamageTrigger>(p => {
                    p.TriggerOnStatDamageOrEnergyDrain = false;
                    p.Actions = Helpers.CreateActionList(
                        Helpers.Create<ContextActionSpawnFx>(f => f.PrefabLink = fire),
                        Helpers.Create<ContextActionSpawnFx>(f => f.PrefabLink = wind),
                        Helpers.Create<ContextActionSpawnFx>(f => f.PrefabLink = decal));
                })

                );
            buff6.SetBuffFlags(BuffFlags.HiddenInUi | BuffFlags.StayOnDeath);
            buff6.ResourceAssetIds = new string[] { buff5fx.AssetId, fire.AssetId, wind.AssetId, decal.AssetId };
            ResourcesLibrary.LibraryObject.ResourceNamesByAssetId[buff5fx.AssetId] = buff5fx.AssetId;
            //Final Buff
            var buffcollection = new BlueprintBuff[]
            {
                buff1,buff2,buff3, buff4,buff5, buff6

            };

            return buffcollection;
        }
        private static BlueprintBuff[] AddIceGolemApperance()
        {
            var buff1fx = Main.library.Get<BlueprintBuff>("7aeaf147211349b40bb55c57fec8e28d");//stoneskin
            var buff2fx = Main.library.Get<BlueprintBuff>("a713733858adb1e4b9696e8e58f5f1bb");//watermephit visual
            var buff3fx = Main.library.Get<BlueprintBuff>("0f278683e67dd09418c78a2c3ad3dfa5");// common cold buff
            var buff4fx = Main.library.Get<BlueprintBuff>("935971f811c2ada4ba1fab1714a8a506");// cold theme buff
            var icon = buff1fx.Icon;//We need an icon due to how the helpers work, I think
            //empty buff1 
            var buff1 = Helpers.CreateBuff("Icylookpart1", "", "", Helpers.getGuid("Icylookpart1"), icon, buff1fx.FxOnStart);
            buff1.SetBuffFlags(BuffFlags.HiddenInUi | BuffFlags.StayOnDeath);
            buff1.ResourceAssetIds = new string[] { buff1fx.FxOnStart.AssetId};
            //empty buff2 
            var buff2 = Helpers.CreateBuff("Icylookpart2", "", "", Helpers.getGuid("Icylookpart2"), icon, buff2fx.FxOnStart);
            buff2.SetBuffFlags(BuffFlags.HiddenInUi | BuffFlags.StayOnDeath);
            buff2.ResourceAssetIds = new string[] { buff2fx.FxOnStart.AssetId};
            //empty buff3 
            var buff3 = Helpers.CreateBuff("Icylookpart3", "", "", Helpers.getGuid("Icylookpart3"), icon, buff3fx.FxOnStart);
            buff3.SetBuffFlags(BuffFlags.HiddenInUi | BuffFlags.StayOnDeath);
            buff3.ResourceAssetIds = new string[] { buff3fx.FxOnStart.AssetId };
            // empty buff 4
            PrefabLink cold = new PrefabLink();
            cold.AssetId = "a7ba9833751047848a7eefbb3c9953b5";//Cold00_StandartHit
            PrefabLink decal = new PrefabLink();
            decal.AssetId = "934310124d9f2c84abb77cba20d6ef0e";//Cold00_StandartHit_Decal
            var buff4 = Helpers.CreateBuff("Icylookpart4", "", "", Helpers.getGuid("Icylookpart4"), icon, buff4fx.FxOnStart, null,
                Helpers.Create<AddIncomingDamageTrigger>(p => {
                    p.TriggerOnStatDamageOrEnergyDrain = false;
                    p.Actions = Helpers.CreateActionList(
                        Helpers.Create<ContextActionSpawnFx>(f => f.PrefabLink = cold),
                        Helpers.Create<ContextActionSpawnFx>(f => f.PrefabLink = decal));
                    }) );
            buff4.SetBuffFlags(BuffFlags.HiddenInUi | BuffFlags.StayOnDeath);
            buff4.ResourceAssetIds = new string[] { buff4fx.FxOnStart.AssetId, cold.AssetId, decal.AssetId };
            //Final Buff
            var buffcollection = new BlueprintBuff[]
            {
                buff1,buff2,buff3, buff4

            };

            return buffcollection;
        }
       
        private static BlueprintActivatableAbility CreateMudGolemSlipperyMud()
        {

            //base thing we need
            var Icon = Main.library.Get<BlueprintAbility>("e418c20c8ce362943a8025d82c865c1c");
            //resource
            var resource = Helpers.CreateAbilityResource("MudGolemCompanionSlipperyMudResource", "", "",
               Helpers.getGuid("MudGolemCompanionSlipperyMudResource"), Icon.Icon);
            resource.SetIncreasedByLevelStartPlusDivStep(0, 1, 1, 2, 1, 1, 0, new BlueprintCharacterClass[] { Main.library.Get<BlueprintCharacterClass>("fd66bdea5c33e5f458e929022322e6bf") }, new BlueprintArchetype[] { thearchetypeforthisnicegolem });
         
           

            

            var ability = Main.library.CopyAndAdd<BlueprintActivatableAbility>("80bf9a7396834b94693c841839b9de42", "MudGolemCompanionSlipperyMudAbility", Helpers.getGuid("MudGolemCompanionSlipperyMudAbility"));
            ability.IsOnByDefault = false;
           
            ability.AddComponent(resource.CreateAddAbilityResource());
            ability.AddComponent(Helpers.CreateActivatableResourceLogic(resource, ActivatableAbilityResourceLogic.ResourceSpendType.NewRound));

            return ability;
        }


        private static BlueprintAbility CreateMudGolemHaste()
        {
            var Icon = Main.library.Get<BlueprintAbility>("e418c20c8ce362943a8025d82c865c1c");
            var resource = Helpers.CreateAbilityResource("MudGolemCompanionHasteResource", "", "",
                Helpers.getGuid("MudGolemCompanionHasteResource"), Icon.Icon);
            resource.SetFixedResource(1);

            var ability = Main.library.CopyAndAdd<BlueprintAbility>("cf5dbfcb40c6f7245aa19e7234489530", "MudGolemCompanionHasteAbility", Helpers.getGuid("MudGolemCompanionHasteAbility"));
            ability.ActionType = CommandType.Free;
            ability.AddComponent(resource.CreateAddAbilityResource());
            ability.AddComponent(Helpers.CreateResourceLogic(resource));

            return ability;


        }
  
       
        private static BlueprintFeature CreateScarecrowFascinatingGaze()
        {


            //Imunebuff
            var immunebuff = Main.library.Get<BlueprintBuff>("a50373fa77d30d34c8c6efb198b36921");//fascinate immunity buff

            var hypnotisme = Main.library.Get<BlueprintAbility>("88367310478c10b47903463c5d0152b0");//for Icon

            //Buff for effect, bard's one 
            var bardeffectbuff = Main.library.CopyAndAdd<BlueprintBuff>("2d4bd347dec7d8648afd502ee40ae661", "ScarecroWCompanionGazeEffectBuff", Helpers.getGuid("ScarecroWCompanionGazeEffectBuff"));// for Icon
            Helpers.SetLocalizedStringField(bardeffectbuff, "m_Description", "Three time a day, a scarecrow can make its eyes fascinating for two round. " +
                "All enemy around it in a 20 feets radius must make a will saving throw (DC= 10+ 1/2 the scarecrow level + the scarecrow Charisma Modifier) or be fascinated for as long as long as this ability last. " +
                "If an enemy succeed at the saving throw, it cannot be affected until 24hours have passed.");



            
            //Area (base is bard's one because it has the right condition
            var area = Main.library.CopyAndAdd<BlueprintAbilityAreaEffect>("a4fc1c0798359974e99e1d790935501d", "ScarecrowCompanionGazeArea", Helpers.getGuid("ScarecrowCompanionGazeArea"));
            area.Size = 20.Feet();
            area.Fx.AssetId = "8a80d991f3d68e84293e098a6faa7620";
            //rebuild component array (maybe should have kept
            area.ComponentsArray = Array.Empty<BlueprintComponent>();
            var runaction = Helpers.CreateAreaEffectRunAction(
                //enter:
                Helpers.CreateConditional(Helpers.CreateConditionsCheckerAnd(Helpers.CreateConditionHasFact(immunebuff, true), Helpers.Create<ContextConditionIsEnemy>()),
                    //if true:
                    Helpers.CreateActionSavingThrow(SavingThrowType.Will,Helpers.CreateConditionalSaved(
                        //Sucess:
                        Helpers.CreateApplyBuff(immunebuff, Helpers.CreateContextDuration(1, DurationRate.Days),false,false,false,false, false),
                        //failed:
                        Helpers.CreateApplyBuff(bardeffectbuff, Helpers.CreateContextDuration (), false, false, false, true, true)
                    ))),
                 //exit :
                 Helpers.CreateConditional(Helpers.CreateConditionsCheckerAnd(Helpers.CreateConditionHasFact(bardeffectbuff), Helpers.Create<ContextConditionIsEnemy>()),
                    //if true:
                    Helpers.Create<ContextActionRemoveBuff>(r => r.Buff = bardeffectbuff)));

            
            //put the construct class for save
            var param = Helpers.Create<ContextCalculateAbilityParamsBasedOnClass>(p => {
                p.CharacterClass = Main.library.Get<BlueprintCharacterClass>("fd66bdea5c33e5f458e929022322e6bf");
                p.StatType = StatType.Charisma;

            });
            area.AddComponents(runaction, param);

            //Buff
            var buff = Main.library.CopyAndAdd<BlueprintBuff>("6b8768da33a182442b22566fbe642fc2", "ScarcrowCompanionGazeBuff", Helpers.getGuid("ScarcrowCompanionGazeBuff"));
            //buff.ComponentsArray.OfType<Kingmaker.UnitLogic.Buffs.Components.AddAreaEffect>().First().AreaEffect = area;
            buff.ReplaceComponent<AddAreaEffect>(a => a.AreaEffect = area);
            buff.RemoveComponent(buff.ComponentsArray.OfType<ContextCalculateAbilityParamsBasedOnClass>().First());// it's on the area rather than here
            buff.SetBuffFlags(BuffFlags.HiddenInUi);
            buff.ResourceAssetIds = new string[] { "8a80d991f3d68e84293e098a6faa7620" };

            //Ability ressource
            var Resource = Helpers.CreateAbilityResource("ScarecrowCompanionGazeResource", "", "",
              Helpers.getGuid("ScarecrowCompanionGazeResource"), bardeffectbuff.Icon);
            Resource.SetFixedResource(3);

            //Ability 
            var ability = Helpers.CreateAbility("ScarecrowCompanionGazeAbility", "Fascinating Gaze", "Three time a day, a scarecrow can make its eyes fascinating for two round. " +
                "All enemy around it in a 20 feets radius must make a will saving throw (DC= 10+ 1/2 the scarecrow level + the scarecrow Charisma Modifier) or be fascinated for as long as long as this ability last. " +
                "If an enemy succeed at the saving throw, it cannot be affected until 24hours have passed.", Helpers.getGuid("ScarecrowCompanionGazeAbility"),
                hypnotisme.Icon,
                AbilityType.SpellLike,
                CommandType.Free,
                AbilityRange.Personal,
                "2 rounds",
                "Will",
                Helpers.CreateRunActions(Helpers.CreateApplyBuff(buff, Helpers.CreateContextDuration(2) , false, false, false, false)),
                Resource.CreateAddAbilityResource(),
                Helpers.CreateResourceLogic(Resource)
                );
            ability.ResourceAssetIds = new string[] { "8a80d991f3d68e84293e098a6faa7620", "725b02acb7286094688c0d5da974dcdc", "396af91a93f6e2b468f5fa1a944fae8a" };
           

            //Feature
            var feature = Helpers.CreateFeature("ScarecrowCompanionFascinateGazeFeature", "Fascinating Gaze", "Three time a day, a scarecrow can make its eyes fascinating for two round. " +
                "All enemy around it in a 20 feets radius must make a will saving throw (DC= 10+ 1/2 the scarecrow level + the scarecrow Charisma Modifier) or be fascinated for as long as long as this ability last. " +
                "If an enemy succeed at the saving throw, it cannot be affected until 24hours have passed.", Helpers.getGuid("ScarecrowCompanionFascinateGazeFeature"), hypnotisme.Icon, FeatureGroup.None, Helpers.CreateAddFact(ability));
            


            return feature;
        }
        private static BlueprintFeature CreateScarecrowFearTouch()
        {
            
            var enchant = Main.library.Get<BlueprintWeaponEnchantment>("436316d7c48c396418a8b2a91ec8f7ec");//base game enchant, maybe a bit op; testing will tell
            var weapon = Main.library.Get<BlueprintItemWeapon>("bbdf5d550dc406640a77c5d2a05244ca");//scarecrow slam
            
            var shaken = Main.library.Get<BlueprintBuff>("25ec6cb6ab1845c48a95f9c20b034220");//shaken condition
            var fear = Main.library.Get<BlueprintAbility>("d2aeac47450c76347aebbc02e4f463e0");// fear spel for icon

            var FearTouch = Helpers.Create<BuffEnchantWornItem>();
            FearTouch.Enchantment = enchant;
            FearTouch.Slot = EquipSlotBase.SlotType.PrimaryHand;

            var buff = Helpers.CreateBuff("scarecrowfeartouchbuff", "Touch of Fear", "When a scarecrow blablabla", Helpers.getGuid("scarecrowfeartouchbuff"),
                fear.Icon,
                null, null,FearTouch);
            buff.SetBuffFlags(BuffFlags.HiddenInUi);




            var feature = Helpers.CreateFeature("scarecrowfeartouchfeature", "Touch of Fear", "When a scarecrow hit a foe in with its slam attack, this target must make a will saving throw" +
                "(DC= 10+ 1/2 the scarecrow level + the scarecrow Charisma Modifier) or be frightened and cower. Even if the saving throw is a success, the target is Shaken.", Helpers.getGuid("scarecrowfeartouchfeature"), fear.Icon, FeatureGroup.None,
                Helpers.Create<AddBuffOnCombatStart>(b =>
                {
                    b.Feature = buff;
                    b.CheckParty = true;
                    
                    
                })
                );
            
            return feature;
        }
        

        private static BlueprintFeature createstonegolemslow()
        {

            BlueprintAbility GolembaseSlowAbility = Main.library.Get<BlueprintAbility>("341cd9a1610563d44938145e4b8d5432");
            var resource = Helpers.CreateAbilityResource("CompanionStoneGolemSlowRessource", "", "",
                Helpers.getGuid("CompanionStoneGolemSlowRessource"), GolembaseSlowAbility.Icon);
            resource.SetFixedResource(3);

            var ability = Main.library.CopyAndAdd<BlueprintAbility>(GolembaseSlowAbility, "StoneGolemCompanionSlowAbility", Helpers.getGuid("StoneGolemCompanionSlowAbility"));
            ability.AddComponent(Helpers.CreateResourceLogic(resource));
            ability.ActionType = CommandType.Swift;
            Helpers.SetLocalizedStringField(ability, "m_Description", "A stone golem companion can use a Slow effect as a swift action 3 time per day. The effect has a range of 10 feet in a burst centered around the golem, and lasts for 7 rounds. A DC 17 Will save is required to negate the effect. The save DC is Constitution-based.");

            var feature = Helpers.CreateFeature("StoneGolemCompanionSlowFeature", "Stone Golem Slow",
                "A stone golem companion can use a Slow effect as a free action 3 time per day.The effect has a range of 10 feet in a burst centered around the golem, and lasts for 7 rounds.A DC 17 Will save is required to negate the effect.The save DC is Constitution - based.",
                Helpers.getGuid("StoneGolemCompanionSlowFeature"),
                GolembaseSlowAbility.Icon,
                FeatureGroup.None,
                ability.CreateAddFact(),
                resource.CreateAddAbilityResource());

                



            return feature;
        }
        private static BlueprintArchetype createconstructarchetype() {

            //Setting the Construct companion Arcehtype
            // Adding the construct type feature
            var ConstructType = Main.library.Get<BlueprintFeature>("fd389783027d63343b4a5634bd81645f");
            var CharacterClass = Main.library.Get<BlueprintCharacterClass>("fd66bdea5c33e5f458e929022322e6bf");
            //Natural Ac Bonus
            BlueprintFeature NatAC = Main.library.CopyAndAdd<BlueprintFeature>("0d20d88abb7c33a47902bd99019f2ed1", "ConstructSkin", Helpers.getGuid("ConstructSkin"));
            Helpers.SetLocalizedStringField(NatAC, "m_DisplayName", "Construct Skin");
            Helpers.SetLocalizedStringField(NatAC, "m_Description", "A construct companion receive bonuses to its natural armor.");

            //Attack and Save Bonus
            BlueprintFeature Attack = Main.library.CopyAndAdd<BlueprintFeature>("71d6955fe81a9a34b97390fef1104362", "MagicEnhancedAttacks", Helpers.getGuid("MagicEnhancedAttack"));
            Helpers.SetLocalizedStringField(Attack, "m_DisplayName", "Magic-Enhanced Attack");
            Helpers.SetLocalizedStringField(Attack, "m_Description", "All attacks made by a construct companion are treated as magic weapons for the purpose of overcoming damage reduction. " +
                "A construct companion gains a +1 enhancement bonus on attack rolls and damage and +1 resistance bonus to saving throws. " +
                "These bonuses increase by 1 for every 3 HD the animal companion has.");

            //DR/adamantine 
            //We're copying the barbarian scaling DR
            BlueprintFeature DR = Main.library.CopyAndAdd<BlueprintFeature>("e71bd204a2579b1438ebdfbf75aeefae", "ResistantMaterial", Helpers.getGuid("ConstructResistantMaterial"));
            Helpers.SetLocalizedStringField(DR, "m_DisplayName", "Resistant Material");
            Helpers.SetLocalizedStringField(DR, "m_Description", "Through your magic and alchemical discovery, you are able to improve your construct's solidity.");
            DR.Ranks = 20;
            var modifydrcompo = Helpers.CreateContextRankConfig(ContextRankBaseValueType.FeatureRank, ContextRankProgression.AsIs, AbilityRankType.Default, null, 20, 0, 1, false, StatType.Unknown, null, null, null, DR);
            var Drreplace = new BlueprintFeature[] { DR };
            DR.ReplaceContextRankConfig(modifydrcompo);
            

            //Stat Bonus, No dex No con
            BlueprintFeature Stats = Main.library.CopyAndAdd<BlueprintFeature>("0c80276018694f24fbaf59ec7b841f2b", "ConstructArcaneProwess", Helpers.getGuid("ConstructArcaneProwessFeature"));
            Helpers.SetLocalizedStringField(Stats, "m_DisplayName", "Arcane Prowess");
            Helpers.SetLocalizedStringField(Stats, "m_Description", "Through your magic, you give your construct a greater strength.");
            Stats.RemoveComponents<AddStatBonus>();
            Stats.AddComponent(Helpers.CreateAddStatBonus(StatType.Strength, 2, ModifierDescriptor.None));

            //The SpellImunity that the golem will gain later
            BlueprintFeature SpellImune = Main.library.Get<BlueprintFeature>("2617c0ea094687643a14fd99c4529523");



            //Creating the Archetype
            //The golem will gain no feat, due to construct being sensless idiot 

            BlueprintArchetype ConstructCompanionArchetype = Helpers.Create<BlueprintArchetype>(l =>
            {
                l.name = "Constructcompanionarchetype";
                l.LocalizedName = Helpers.CreateString("ConstructCompanionArchetype.Name", "Construct Companion");
                l.LocalizedDescription = Helpers.CreateString("ConstructCompanionArchetype.Description", "Construct are magically created automatons of great power.");
            });
            Main.library.AddAsset(ConstructCompanionArchetype, Helpers.getGuid("ConstructCompanionArchetype"));

            List<LevelEntry> addFeatures = new List<LevelEntry>();
            addFeatures.Add(Helpers.LevelEntry(1, ConstructType));
            addFeatures.Add(Helpers.LevelEntry(2,DR));
            addFeatures.Add(Helpers.LevelEntry(3, Attack, DR, NatAC));
            addFeatures.Add(Helpers.LevelEntry(4, DR, Stats));
            addFeatures.Add(Helpers.LevelEntry(5, DR, NatAC));
            addFeatures.Add(Helpers.LevelEntry(6, Attack, DR, Stats));
            addFeatures.Add(Helpers.LevelEntry(7, DR, NatAC));
            addFeatures.Add(Helpers.LevelEntry(8, DR, Stats));
            addFeatures.Add(Helpers.LevelEntry(9, Attack, DR, NatAC));
            addFeatures.Add(Helpers.LevelEntry(10, DR, Stats));
            addFeatures.Add(Helpers.LevelEntry(11, DR, SpellImune, NatAC));
            addFeatures.Add(Helpers.LevelEntry(12, Attack, DR, Stats));
            addFeatures.Add(Helpers.LevelEntry(13, DR, NatAC));
            addFeatures.Add(Helpers.LevelEntry(14, DR, Stats));
            addFeatures.Add(Helpers.LevelEntry(15, DR, Attack, NatAC));
            addFeatures.Add(Helpers.LevelEntry(16, DR, Stats));
            ConstructCompanionArchetype.AddFeatures = addFeatures.ToArray();
            ConstructCompanionArchetype.RemoveFeatures = CharacterClass.Progression.LevelEntries;
            ConstructCompanionArchetype.ReplaceClassSkills = true;
            ConstructCompanionArchetype.ClassSkills = Array.Empty<StatType>();

            return ConstructCompanionArchetype;



        }

        public static void CreateConstructSpell()
        {
            //Make whole 
            var makewhole = CreateMakeWholeSpell();
            makewhole.AddToSpellList(Helpers.wizardSpellList, 2);

            //Make whole, greater
            var greatermakewhole = CreateGreaterMakeWholeSpell();
            greatermakewhole.AddToSpellList(Helpers.wizardSpellList, 4);

            //Fast repair 
            var rapidrepair = CreateRapidRepairSpell();
            rapidrepair.AddToSpellList(Helpers.wizardSpellList, 5);

            var unbreakableconstruct = CreateUnbreakableConstructSpell();
            unbreakableconstruct.AddToSpellList(Helpers.wizardSpellList, 5);

            //Code Skill
            var codeskill = CreateCodeSkillSpell();
            codeskill.AddToSpellList(Helpers.wizardSpellList, 3);

            //Program Feat 

            var programfeat = CreateProgramFeatSpell();
            programfeat.AddToSpellList(Helpers.wizardSpellList, 5);
      

        }
        private static BlueprintAbility CreateMakeWholeSpell()
        {
            var Icon = Helpers.GetIcon("47808d23c67033d4bbab86a1070fd62f");
            var fx = new PrefabLink();
            fx.AssetId = "61602c5b0ac793d489c008e9cb58f631";
           
            var spell = Helpers.CreateAbility("makewhole", "Make Whole", "This spell repairs 1d6 points of damage per level when cast on a construct creature(maximum 5d6).", Helpers.getGuid("MakeWholeSpell"), Icon,
                AbilityType.Spell,
                CommandType.Standard,
                AbilityRange.Close,
                "Instantaneous", "None, Harmless",
                Helpers.Create<AbilitySpawnFx>(f => 
                {
                    f.PrefabLink = fx;
                    f.Anchor = AbilitySpawnFxAnchor.SelectedTarget;
                    f.DestroyOnCast = true;
                }),
                Helpers.CreateRunActions(Helpers.Create<ContextActionHealTarget>(h => h.Value = Helpers.CreateContextDiceValue(DiceType.D6, Helpers.CreateContextValue(AbilityRankType.DamageDice)))),
                Helpers.CreateContextRankConfig(ContextRankBaseValueType.CasterLevel, ContextRankProgression.BonusValue, AbilityRankType.DamageDice, null, 5, 1 ,1, false , StatType.Unknown , null),
                Helpers.CreateSpellComponent(SpellSchool.Transmutation),
                Helpers.CreateSpellDescriptor(SpellDescriptor.RestoreHP),
                Helpers.Create<AbilityTargetHasFact>(f => f.CheckedFacts = new BlueprintUnitFact[] { Main.library.Get<BlueprintFeature>("fd389783027d63343b4a5634bd81645f") })
             

                );
            spell.CanTargetEnemies = true;
            spell.CanTargetFriends = true;
            spell.EffectOnAlly = AbilityEffectOnUnit.Helpful;
            spell.EffectOnEnemy = AbilityEffectOnUnit.Helpful;
            spell.Animation = Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle.Directional;
            spell.AvailableMetamagic = Kingmaker.UnitLogic.Abilities.Metamagic.Empower | Kingmaker.UnitLogic.Abilities.Metamagic.Maximize | Kingmaker.UnitLogic.Abilities.Metamagic.Quicken | Kingmaker.UnitLogic.Abilities.Metamagic.Reach;
            spell.SpellResistance = false;
            spell.ResourceAssetIds = new string[] { fx.AssetId };
            return spell;
        }

        private static BlueprintAbility CreateGreaterMakeWholeSpell()
        {
            var Icon = Helpers.GetIcon("0d657aa811b310e4bbd8586e60156a2d");
            var fx = new PrefabLink();
            fx.AssetId = "61602c5b0ac793d489c008e9cb58f631";

            var spell = Helpers.CreateAbility("greatermakewhole", "Make Whole, Greater", "This spell repairs 1d6 points of damage plus 1 point per level when cast on a construct creature (maximum 10d6+10).", Helpers.getGuid("GreaterMakeWholeSpell"), Icon,
                AbilityType.Spell,
                CommandType.Standard,
                AbilityRange.Custom,
                "Instantaneous", "None, Harmless",
                Helpers.Create<AbilitySpawnFx>(f =>
                {
                    f.PrefabLink = fx;
                    f.Anchor = AbilitySpawnFxAnchor.SelectedTarget;
                    f.DestroyOnCast = true;
                }),
                Helpers.CreateRunActions(Helpers.Create<ContextActionHealTarget>(h => h.Value = Helpers.CreateContextDiceValue(DiceType.D6, Helpers.CreateContextValue(AbilityRankType.DamageDice), Helpers.CreateContextValue(AbilityRankType.DamageBonus)))),
                Helpers.CreateContextRankConfig(ContextRankBaseValueType.CasterLevel, ContextRankProgression.BonusValue, AbilityRankType.DamageDice, null, 10 , 1, 1, false, StatType.Unknown, null),
                Helpers.CreateContextRankConfig(ContextRankBaseValueType.CasterLevel, ContextRankProgression.BonusValue, AbilityRankType.DamageBonus, null, 10, 1, 1, false, StatType.Unknown, null),
                Helpers.CreateSpellComponent(SpellSchool.Transmutation),
                Helpers.CreateSpellDescriptor(SpellDescriptor.RestoreHP),
                Helpers.Create<AbilityTargetHasFact>(f => f.CheckedFacts = new BlueprintUnitFact[] { Main.library.Get<BlueprintFeature>("fd389783027d63343b4a5634bd81645f") })


                );
            spell.CustomRange = 10.Feet();
            spell.CanTargetEnemies = true;
            spell.CanTargetFriends = true;
            spell.EffectOnAlly = AbilityEffectOnUnit.Helpful;
            spell.EffectOnEnemy = AbilityEffectOnUnit.Helpful;
            spell.Animation = Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle.Directional;
            spell.AvailableMetamagic = Kingmaker.UnitLogic.Abilities.Metamagic.Empower | Kingmaker.UnitLogic.Abilities.Metamagic.Maximize | Kingmaker.UnitLogic.Abilities.Metamagic.Quicken | Kingmaker.UnitLogic.Abilities.Metamagic.Reach;
            spell.SpellResistance = false;
            spell.ResourceAssetIds = new string[] { fx.AssetId };
            return spell;
        }
        private static BlueprintAbility CreateRapidRepairSpell()
        {
            var fasthealing5 = Main.library.Get<BlueprintBuff>("37a5e51e9e3a23049a77ba70b4e7b2d2");
            var Icon = Helpers.GetIcon("e418c20c8ce362943a8025d82c865c1c");
            var fx = new PrefabLink();
            fx.AssetId = "61602c5b0ac793d489c008e9cb58f631";

            var spell = Helpers.CreateAbility("Rapidrepairspell", "Rapid Repair", "The targeted construct gains fast healing 5. This does not stack with any fast healing the construct already has. Fast healing has no effect on a construct that has been brought to 0 hit points or destroyed", Helpers.getGuid("RapidRepairSpell"), Icon,
                AbilityType.Spell,
                CommandType.Standard,
                AbilityRange.Touch,
                "1 round/Level", "None, Harmless",
                Helpers.Create<AbilitySpawnFx>(f =>
                {
                    f.PrefabLink = fx;
                    f.Anchor = AbilitySpawnFxAnchor.SelectedTarget;
                    f.DestroyOnCast = true;
                }),
                Helpers.CreateRunActions(Helpers.CreateApplyBuff(fasthealing5, Helpers.CreateContextDuration(Helpers.CreateContextValueRank(AbilityRankType.Default), DurationRate.Rounds),true,false)),
                Helpers.CreateContextRankConfig(ContextRankBaseValueType.CasterLevel, ContextRankProgression.BonusValue, AbilityRankType.Default, null, 20, 1, 1, false, StatType.Unknown, null),
                
                Helpers.CreateSpellComponent(SpellSchool.Transmutation),
                Helpers.CreateSpellDescriptor(SpellDescriptor.RestoreHP),
                Helpers.Create<AbilityTargetHasFact>(f => f.CheckedFacts = new BlueprintUnitFact[] { Main.library.Get<BlueprintFeature>("fd389783027d63343b4a5634bd81645f") })


                );

            spell.CanTargetEnemies = true;
            spell.CanTargetFriends = true;
            spell.EffectOnAlly = AbilityEffectOnUnit.Helpful;
            spell.EffectOnEnemy = AbilityEffectOnUnit.Helpful;
            spell.Animation = Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle.Touch;
            spell.AvailableMetamagic =  Kingmaker.UnitLogic.Abilities.Metamagic.Quicken | Kingmaker.UnitLogic.Abilities.Metamagic.Reach | Kingmaker.UnitLogic.Abilities.Metamagic.Extend ;
            spell.SpellResistance = false;
            spell.ResourceAssetIds = new string[] { fx.AssetId };
            return spell;
        }


        private static BlueprintAbility CreateUnbreakableConstructSpell()
        {
            var magearmor = Main.library.Get<BlueprintAbility>("9e1ad5d6f87d19e4d8883d63a6e35568");
            var fx = new PrefabLink();
            fx.AssetId = "f447e243ab2c1da4c851c019c3196526";

            var feat = Helpers.CreateFeature("BonusDRFeat", "Bonus DR", "", Helpers.getGuid("UnbreakableBonusDrFeat"), magearmor.Icon, FeatureGroup.None,
                
                Helpers.CreateAddFact(Main.library.Get<BlueprintFeature>(Helpers.getGuid("ConstructResistantMaterial"))),
                Helpers.CreateAddFact(Main.library.Get<BlueprintFeature>(Helpers.getGuid("ConstructResistantMaterial"))),
                Helpers.CreateAddFact(Main.library.Get<BlueprintFeature>(Helpers.getGuid("ConstructResistantMaterial"))),
                Helpers.CreateAddFact(Main.library.Get<BlueprintFeature>(Helpers.getGuid("ConstructResistantMaterial"))),
                Helpers.CreateAddFact(Main.library.Get<BlueprintFeature>(Helpers.getGuid("ConstructResistantMaterial")))

                );
            feat.HideInUI = true;
            feat.HideInCharacterSheetAndLevelUp = true;
            feat.Ranks = 1;

            var buff = Helpers.CreateBuff("UnbreakableConstructBuff", "Unbreakable", "This spell increases the target’s DR/adamantine by 5", Helpers.getGuid("UnbreakableConstructBuff"), magearmor.Icon, fx, null,
                Helpers.Create<AddTemporaryFeat>(t => 
                {
                    t.Feat = feat;
                })
                ) ;
            //buff.Stacking = StackingType.Replace;
            buff.SetBuffFlags(BuffFlags.IsFromSpell);
          

            var spell = Helpers.CreateAbility("UnbreakableConstructSpell", "Unbreakable Construct", "This spell increases the target’s DR/adamantine by 5 or its hardness by 5. ", Helpers.getGuid("UnbreakableConstructSpell"), magearmor.Icon,
                AbilityType.Spell,
                CommandType.Standard,
                AbilityRange.Close,
                "1 round/Level", "None, Harmless",
                Helpers.Create<AbilitySpawnFx>(f =>
                {
                    f.PrefabLink = fx;
                    f.Anchor = AbilitySpawnFxAnchor.SelectedTarget;
                    f.DestroyOnCast = true;
                }),
                Helpers.CreateRunActions(Helpers.CreateApplyBuff(buff, Helpers.CreateContextDuration(Helpers.CreateContextValueRank(AbilityRankType.Default), DurationRate.Rounds), true, false)),
                Helpers.CreateContextRankConfig(ContextRankBaseValueType.CasterLevel, ContextRankProgression.BonusValue, AbilityRankType.Default, null, 20, 1, 1, false, StatType.Unknown, null),
                Helpers.CreateSpellComponent(SpellSchool.Transmutation),
                Helpers.CreateSpellDescriptor(SpellDescriptor.None),
                Helpers.Create<AbilityTargetHasFact>(f => f.CheckedFacts = new BlueprintUnitFact[] { Main.library.Get<BlueprintFeature>("fd389783027d63343b4a5634bd81645f") })


                );

            spell.CanTargetEnemies = false;
            spell.CanTargetFriends = true;
            spell.EffectOnAlly = AbilityEffectOnUnit.Helpful;
            spell.Animation = Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle.Directional;
            spell.AvailableMetamagic = Kingmaker.UnitLogic.Abilities.Metamagic.Quicken | Kingmaker.UnitLogic.Abilities.Metamagic.Reach | Kingmaker.UnitLogic.Abilities.Metamagic.Extend;
            spell.SpellResistance = false;
            spell.ResourceAssetIds = new string[] { fx.AssetId };
            return spell;
        }

        private static BlueprintAbility CreateCodeSkillSpell()
        {
            //Thing we need
            var icon = Helpers.GetIcon("4093d5a0eb5cae94e909eb1e0e1a6b36");
            var bufffx = new PrefabLink();
            bufffx.AssetId = "352469f228a3b1f4cb269c7ab0409b8e";
            var fx = new PrefabLink();
            fx.AssetId = "09f795c3900b21b47a1254bcb3f263c8";
            var perceptionI = Helpers.GetIcon("f74c6bdf5c5f5374fb9302ecdc1f7d64");
            var AthleticsI = Helpers.GetIcon("9db907332bdaec1468cff3a99efef5b4");
            var MobilityI = Helpers.GetIcon("52dd89af385466c499338b7297896ded");

            //The buffs
            var Athleticsbuff = Helpers.CreateBuff("AthleticsCodeSkillBuff", "Athletics Coded", "", Helpers.getGuid("AthleticsCodeSkillBuff"), AthleticsI, fx, null,
                
                Helpers.CreateAddContextStatBonus(StatType.SkillAthletics, ModifierDescriptor.Inherent, ContextValueType.Rank, AbilityRankType.Default),
                Helpers.CreateContextRankConfig(ContextRankBaseValueType.OwnerSummClassLevelWithArchetype, ContextRankProgression.AsIs, AbilityRankType.Default, 1, 16, 1, 1, false, StatType.SkillAthletics, null, 
                new BlueprintCharacterClass[] { Main.library.Get<BlueprintCharacterClass>("fd66bdea5c33e5f458e929022322e6bf") },
                Main.library.Get<BlueprintArchetype>(Helpers.getGuid("ConstructCompanionArchetype")))

    );
            Athleticsbuff.Stacking = StackingType.Replace;
            Athleticsbuff.SetBuffFlags(BuffFlags.IsFromSpell);

            var Mobilitybuff = Helpers.CreateBuff("MobilityCodeSkillBuff", "Mobility Coded", "", Helpers.getGuid("MobilityCodeSkillBuff"), MobilityI, fx, null,
                Helpers.CreateAddContextStatBonus(StatType.SkillMobility, ModifierDescriptor.Inherent, ContextValueType.Rank, AbilityRankType.Default),
                Helpers.CreateContextRankConfig(ContextRankBaseValueType.OwnerSummClassLevelWithArchetype, ContextRankProgression.AsIs, AbilityRankType.Default, 1, 16, 1, 1, false, StatType.SkillMobility, null,
                new BlueprintCharacterClass[] { Main.library.Get<BlueprintCharacterClass>("fd66bdea5c33e5f458e929022322e6bf") },
                Main.library.Get<BlueprintArchetype>(Helpers.getGuid("ConstructCompanionArchetype")))

    );
            Mobilitybuff.Stacking = StackingType.Replace;
            Mobilitybuff.SetBuffFlags(BuffFlags.IsFromSpell);

            var Perceptionbuff = Helpers.CreateBuff("PerceptionCodeSkillBuff", "Perception Coded", "", Helpers.getGuid("PerceptionCodeSkillBuff"), perceptionI, fx, null,
                Helpers.Create<AddClassSkill>(s => s.Skill = StatType.SkillPerception)

    );
            Perceptionbuff.Stacking = StackingType.Replace;
            Perceptionbuff.SetBuffFlags(BuffFlags.IsFromSpell);

            //Preapring the spell component
            var applyAthlecticsbuff = Helpers.CreateRunActions(Helpers.CreateApplyBuff(Athleticsbuff, Helpers.CreateContextDuration(Helpers.CreateContextValueRank(AbilityRankType.StatBonus), DurationRate.TenMinutes), true, false));
            var applyMobilitybuff = Helpers.CreateRunActions(Helpers.CreateApplyBuff(Mobilitybuff, Helpers.CreateContextDuration(Helpers.CreateContextValueRank(AbilityRankType.StatBonus), DurationRate.TenMinutes), true, false));
            var applyPerceptionbuff = Helpers.CreateRunActions(Helpers.CreateApplyBuff(Perceptionbuff, Helpers.CreateContextDuration(Helpers.CreateContextValueRank(AbilityRankType.StatBonus), DurationRate.TenMinutes), true, false));

            var rankconsfig = Helpers.CreateContextRankConfig(ContextRankBaseValueType.CasterLevel, ContextRankProgression.BonusValue, AbilityRankType.StatBonus, null, 20, 1, 1, false, StatType.Unknown, null);
            var constructcheck = Helpers.Create<AbilityTargetHasFact>(f => f.CheckedFacts = new BlueprintUnitFact[] { Main.library.Get<BlueprintFeature>("fd389783027d63343b4a5634bd81645f") });

            //Base spell
            var spell = Helpers.CreateAbility("CodeSkillBaseSpell", "Code Skill", "Code Skill Gives a cronstruct a bonus in certain skills, this bonus is equal to the construct level or if the construct allready has rank in this skill, it make it a class skill.", Helpers.getGuid("CodeSkillBaseSpell"), icon,
                AbilityType.Spell,
                CommandType.Standard,
                AbilityRange.Close,
                "10 min/Level", "None, Harmless",
                Helpers.Create<AbilitySpawnFx>(f =>
                {
                    f.PrefabLink = fx;
                    f.Anchor = AbilitySpawnFxAnchor.SelectedTarget;
                    f.DestroyOnCast = true;
                }),
                
                Helpers.CreateSpellComponent(SpellSchool.Transmutation),
                Helpers.CreateSpellDescriptor(SpellDescriptor.None)
                


                );

            spell.CanTargetEnemies = false;
            spell.CanTargetFriends = true;
            spell.EffectOnAlly = AbilityEffectOnUnit.Helpful;
            spell.Animation = Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle.Directional;
            spell.AvailableMetamagic = Kingmaker.UnitLogic.Abilities.Metamagic.Quicken | Kingmaker.UnitLogic.Abilities.Metamagic.Reach | Kingmaker.UnitLogic.Abilities.Metamagic.Extend;
            spell.SpellResistance = false;
            spell.ResourceAssetIds = new string[] { fx.AssetId };

            //Creating the variant
            var athleticsspell = Main.library.CopyAndAdd<BlueprintAbility>(spell, "CodeSkillAthleticsSpell", Helpers.getGuid("CodeSkillAthleticsSpell"));
            athleticsspell.AddComponents(applyAthlecticsbuff, rankconsfig, constructcheck);
            
            Helpers.SetField(athleticsspell, "m_Icon", AthleticsI);
            Helpers.SetLocalizedStringField(athleticsspell, "m_DisplayName", spell.Name + ": Athletics");

            var Mobilityspell = Main.library.CopyAndAdd<BlueprintAbility>(spell, "CodeSkillMobilitySpell", Helpers.getGuid("CodeSkillMobilitySpell"));
            Mobilityspell.AddComponents(applyMobilitybuff, rankconsfig, constructcheck);
            
            Helpers.SetField(Mobilityspell, "m_Icon", MobilityI);
            Helpers.SetLocalizedStringField(Mobilityspell, "m_DisplayName", spell.Name + ": Mobility");

            var Perceptionspell = Main.library.CopyAndAdd<BlueprintAbility>(spell, "CodeSkillPerceptionSpell", Helpers.getGuid("CodeSkillPerceptionSpell"));
            Perceptionspell.AddComponents(applyPerceptionbuff, rankconsfig, constructcheck);
            Helpers.SetField(Perceptionspell, "m_Icon", perceptionI);
            Helpers.SetLocalizedStringField(Perceptionspell, "m_DisplayName", spell.Name + ": Perception");

            //creating and adding the variant component
            var variantcompo = Helpers.CreateAbilityVariants(spell, Mobilityspell, Perceptionspell, athleticsspell);
            spell.AddComponent(variantcompo);

            return spell;
        }

        private static BlueprintAbility CreateProgramFeatSpell()
        {
            //Thing we need
            var icon = Helpers.GetIcon("4d9bf81b7939b304185d58a09960f589");
            var bufffx = new PrefabLink();
            bufffx.AssetId = "352469f228a3b1f4cb269c7ab0409b8e";
            var fx = new PrefabLink();
            fx.AssetId = "09f795c3900b21b47a1254bcb3f263c8";
            var weaponfocus = Main.library.Get<BlueprintParametrizedFeature>("f4201c85a991369408740c6888362e20");
            var improvedcritcal = Main.library.Get<BlueprintParametrizedFeature>("1e1f627d26ad36f43bbd26cc2bf8ac7e");


            var precisestrike = Main.library.Get<BlueprintFeature>("5662d1b793db90c4b9ba68037fd2a768") ;
            var outflank = Main.library.Get<BlueprintFeature>("422dab7309e1ad343935f33a4d6e9f11");

            var lightreflex = Main.library.Get<BlueprintFeature>("15e7da6645a7f3d41bdad7c8c4b9de1e");
            var greatfortitude = Main.library.Get<BlueprintFeature>("79042cb55f030614ea29956177977c52");
            var ironwill = Main.library.Get<BlueprintFeature>("175d1577bb6c9a04baf88eec99c66334");
            var toughness = Main.library.Get<BlueprintFeature>("d09b20029e9abfe4480b356c92095623");

            var dragonbitetype = Main.library.Get<BlueprintWeaponType>("12a8a3a89e62d6b4fbc09ecdc187a828");
            var dragonclawtype = Main.library.Get<BlueprintWeaponType>("d4f7aee36efe0b54e810c9d3407b6ab3");
            var slamtype = Main.library.Get<BlueprintWeaponType>("b6a50705eb5dfab4fafd577020f49c5e");
            var slamtypescarecrow = Main.library.Get<BlueprintWeaponType>("f18cbcb39a1b35643a8d129b1ec4e716");




            //The buffs
            var offensebuff = Helpers.CreateBuff("ProgramFeatOffenseBuff", "Offense Feats Programmed", "", Helpers.getGuid("ProgramFeatOffenseBuff"), weaponfocus.Icon, bufffx, null,

                Helpers.Create<AddTemporaryFeat>(f => f.Feat = weaponfocus),
                Helpers.Create<AddTemporaryFeat>(f => f.Feat = improvedcritcal),
                Helpers.Create<WeaponFocus>(w => {w.AttackBonus = 1; w.WeaponType = dragonbitetype;}),
                Helpers.Create<WeaponFocus>(w => {w.AttackBonus = 1; w.WeaponType = slamtype;}),
                Helpers.Create<WeaponFocus>(w => {w.AttackBonus = 1; w.WeaponType = dragonclawtype;}),
                Helpers.Create<WeaponFocus>(w => { w.AttackBonus = 1; w.WeaponType = slamtypescarecrow; }),
                Helpers.Create<WeaponTypeCriticalEdgeIncrease>(w =>  w.WeaponType = dragonbitetype),
                Helpers.Create<WeaponTypeCriticalEdgeIncrease>(w =>  w.WeaponType = slamtype),
                Helpers.Create<WeaponTypeCriticalEdgeIncrease>(w => w.WeaponType = slamtypescarecrow),
                Helpers.Create<WeaponTypeCriticalEdgeIncrease>(w =>  w.WeaponType = dragonclawtype)
                );
            offensebuff.Stacking = StackingType.Replace;
            offensebuff.SetBuffFlags(BuffFlags.IsFromSpell);

          

            var resistancebuff = Helpers.CreateBuff("ProgramFeatResistanceBuff", "Resistance Feats Programmed", "", Helpers.getGuid("ProgramFeatResistanceBuff"), toughness.Icon, bufffx, null,
                Helpers.Create<AddTemporaryFeat>(f => f.Feat = ironwill),
                Helpers.Create<AddTemporaryFeat>(f => f.Feat = greatfortitude),
                Helpers.Create<AddTemporaryFeat>(f => f.Feat = lightreflex),
                Helpers.Create<AddTemporaryFeat>(f => f.Feat = toughness)
                );
            resistancebuff.Stacking = StackingType.Replace;
            resistancebuff.SetBuffFlags(BuffFlags.IsFromSpell);

            var cooperationbuff = Helpers.CreateBuff("ProgmraFeatCoopBuff", "Cooperation Feat Programmed", "", Helpers.getGuid("ProgmraFeatCoopBuff"), outflank.Icon, bufffx, null,
               Helpers.Create<AddTemporaryFeat>(f => f.Feat = precisestrike),
               Helpers.Create<AddTemporaryFeat>(f => f.Feat = outflank)
               );
            cooperationbuff.Stacking = StackingType.Replace;
            resistancebuff.SetBuffFlags(BuffFlags.IsFromSpell);



            //Preapring the spell component
            var applyOffensebuff = Helpers.CreateRunActions(Helpers.CreateApplyBuff(offensebuff, Helpers.CreateContextDuration(Helpers.CreateContextValueRank(AbilityRankType.StatBonus), DurationRate.TenMinutes), true, false));
            var applyResistancebuff = Helpers.CreateRunActions(Helpers.CreateApplyBuff(resistancebuff, Helpers.CreateContextDuration(Helpers.CreateContextValueRank(AbilityRankType.StatBonus), DurationRate.TenMinutes), true, false));
            var applyCooperationbuff = Helpers.CreateRunActions(Helpers.CreateApplyBuff(cooperationbuff, Helpers.CreateContextDuration(Helpers.CreateContextValueRank(AbilityRankType.StatBonus), DurationRate.TenMinutes), true, false));

            var rankconsfig = Helpers.CreateContextRankConfig(ContextRankBaseValueType.CasterLevel, ContextRankProgression.BonusValue, AbilityRankType.StatBonus, null, 20, 1, 1, false, StatType.Unknown, null);
            var constructcheck = Helpers.Create<AbilityTargetHasFact>(f => f.CheckedFacts = new BlueprintUnitFact[] { Main.library.Get<BlueprintFeature>("fd389783027d63343b4a5634bd81645f") });

            //Base spell
            var spell = Helpers.CreateAbility("ProgramFeatBaseSpell", "Program Feat", "Program feat gives a construct the benefits of a collection of feat chosen by the caster when the spell is cast. " +
                "The collections are as follow : \n"+
                "- Offense Feats: Weapon focus and Improved Critical(for all the construct's primary weapon).\n" +
                "- Resistance Feats: Toughness, Inron Will, Lightning Refexes and Great Fortitude.\n" +
                "- Team - work Feats: Precise Strike and Outflank.\n" +
                "The construct must meet all the prerequisites of the feat."+
                "The feat must be passive and require no thought to activate for a non-intelligent"+
                "construct to gain the benefit from it.", 
                Helpers.getGuid("ProgramFeatBaseSpell"), icon,
                AbilityType.Spell,
                CommandType.Standard,
                AbilityRange.Touch,
                "10 min/Level", "None, Harmless",
                Helpers.Create<AbilitySpawnFx>(f =>
                {
                    f.PrefabLink = fx;
                    f.Anchor = AbilitySpawnFxAnchor.SelectedTarget;
                    f.DestroyOnCast = true;
                }),

                Helpers.CreateSpellComponent(SpellSchool.Transmutation),
                Helpers.CreateSpellDescriptor(SpellDescriptor.None)



                );

            spell.CanTargetEnemies = false;
            spell.CanTargetFriends = true;
            spell.EffectOnAlly = AbilityEffectOnUnit.Helpful;
            spell.Animation = Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle.Touch;
            spell.AvailableMetamagic = Kingmaker.UnitLogic.Abilities.Metamagic.Quicken | Kingmaker.UnitLogic.Abilities.Metamagic.Reach | Kingmaker.UnitLogic.Abilities.Metamagic.Extend;
            spell.SpellResistance = false;
            spell.ResourceAssetIds = new string[] { fx.AssetId, bufffx.AssetId };

            //Creating the variant
            var offensespell = Main.library.CopyAndAdd<BlueprintAbility>(spell, "ProgramFeatOffenseSpell", Helpers.getGuid("ProgramFeatOffenseSpell"));
            offensespell.AddComponents(applyOffensebuff, rankconsfig, constructcheck);
            Helpers.SetField(offensespell, "m_Icon", weaponfocus.Icon);
            Helpers.SetLocalizedStringField(offensespell, "m_DisplayName", spell.Name + ": Attack");

            var resistancespell = Main.library.CopyAndAdd<BlueprintAbility>(spell, "ProgramFeatOffenseSpelll", Helpers.getGuid("ProgramFeatOffenseSpelll"));
            resistancespell.AddComponents(applyResistancebuff, rankconsfig, constructcheck);
            Helpers.SetField(resistancespell, "m_Icon", toughness.Icon);
            Helpers.SetLocalizedStringField(resistancespell, "m_DisplayName", spell.Name + ": Resistance");

            var cooperationspell = Main.library.CopyAndAdd<BlueprintAbility>(spell, "ProgramFeatCoopSpell", Helpers.getGuid("ProgramFeatCoopSpell"));
            cooperationspell.AddComponents(applyCooperationbuff, rankconsfig, constructcheck);
            Helpers.SetField(cooperationspell, "m_Icon", outflank.Icon);
            Helpers.SetLocalizedStringField(cooperationspell, "m_DisplayName", spell.Name + ": Team-work");

            //creating and adding the variant component
            var variantcompo = Helpers.CreateAbilityVariants(spell, offensespell, resistancespell, cooperationspell);
            spell.AddComponent(variantcompo);

            return spell;
        }




        [HarmonyPatch(typeof(CustomPortraitsManager), "GetPortraitFolderPath", typeof(string))]
        private static class GetPortraitFolderPathPatch
        {
            private static bool Prefix(CustomPortraitsManager __instance, string id, ref string __result)
            {
                if (id.Contains("thelostgrimoire"))
                {
                    __result = Path.Combine(Directory.GetCurrentDirectory(), "Mods/thelostgrimoire/Resources", id);
                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(AddFeatureToCompanion))]
        [Harmony12.HarmonyPatch("TryAdd")]
        private static class DisallowAddingFeatureToConstructPatch
        {
            private static bool Prefix(AddFeatureToCompanion __instance)
            {
                if (__instance.Owner.Pet != null && __instance.Owner.Pet.Descriptor.Progression.Features.HasFact(Main.library.Get<BlueprintFeature>("fd389783027d63343b4a5634bd81645f"))) { return false; }
                return true;
            }
        }
        [HarmonyPatch(typeof(ShareFeaturesWithCompanion))]
        [Harmony12.HarmonyPatch("OnTurnOn")]
        private static class DisallowSharingFeaturewithConstructPatch
        {
            private static bool Prefix(ShareFeaturesWithCompanion __instance)
            {
                if (__instance.Owner.Pet != null && __instance.Owner.Pet.Descriptor.Progression.Features.HasFact(Main.library.Get<BlueprintFeature>("fd389783027d63343b4a5634bd81645f"))) { return false; }
                return true;
            }
        }

        [HarmonyPatch(typeof(UnitAnimationManager), nameof(UnitAnimationManager.ExecuteIfIdle), typeof(UnitAnimationType))]
        private static class DisableSteamDragonAnimationPatch
        {
            private static bool Prefix(UnitAnimationManager __instance, UnitAnimationType type)
            {
                 if (type == UnitAnimationType.VariantIdle && __instance.View?.EntityData?.Blueprint.AssetGuid == Helpers.getGuid("SteamDragonCompanion")) { return false; }
                return true;
            }
        }



    }







    class AnimalAllyAdjustToLevelLogic : OwnedGameLogicComponent<UnitDescriptor>
    {
        private readonly BlueprintFeature ConstructRank = Main.library.Get<BlueprintFeature>(Helpers.getGuid("ConstructCompanionRank"));
        private readonly BlueprintCharacterClass wizard = Main.library.Get<BlueprintCharacterClass>("ba34257984f4c41408ce1dc2004e342e");
        private void ManageBonus()
        {
            int wizLevel = base.Owner.Progression.GetClassLevel(wizard);
            int rank = base.Owner.Progression.Features.GetRank(ConstructRank);
            if (rank < wizLevel)
            {
                for (int i = 0; i < wizLevel ; i++)
                {
                    base.Owner.Progression.Features.AddFeature(ConstructRank, Helpers.GetMechanicsContext());
                }
            }
        }


        public override void OnTurnOn()
        {
            ManageBonus();
        }

        public override void OnTurnOff()
        {
            ManageBonus();
        }
    }
}