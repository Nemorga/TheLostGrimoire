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
using Kingmaker.Designers.EventConditionActionSystem.Conditions;
using Kingmaker.Designers.EventConditionActionSystem.Evaluators;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Designers;
using Kingmaker.Designers.Mechanics.Facts;
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
        static BlueprintArchetype thassilonian = Main.library.Get<BlueprintArchetype>("55a8ce15e30d71547a44c69bb2e8a84f");
        static BlueprintFeatureSelection Arcanebondselection = library.Get<BlueprintFeatureSelection>("03a1781486ba98043afddaabf6b7d8ff");
        static BlueprintFeatureSelection SpecialistSchoolSelection = library.Get<BlueprintFeatureSelection>("5f838049069f1ac4d804ce0862ab5110");
        static BlueprintSpellbook Wizardbook = library.Get<BlueprintSpellbook>("5a38c9ac8607890409fcb8f6342da6f4");
        static BlueprintFeature BondedItem = library.Get<BlueprintFeature>("2fb5e65bd57caa943b45ee32d825e9b9");
        //SpecialistSchoolSelection
        static BlueprintSpellList wizardlist = Main.library.Get<BlueprintSpellList>("ba0401fdeb4062f40a7aa95b6f07fe89");
        static BlueprintSpellList ThassAbjurationList = library.Get<BlueprintSpellList>("280dd5167ccafe449a33fbe93c7a875e");
        static BlueprintSpellList ThassConjurationList = library.Get<BlueprintSpellList>("5b154578f228c174bac546b6c29886ce");
        static BlueprintSpellList ThassEnchantmentList = library.Get<BlueprintSpellList>("ac551db78c1baa34eb8edca088be13cb");
        static BlueprintSpellList ThassEvocationList = library.Get<BlueprintSpellList>("17c0bfe5b7c8ac3449da655cdcaed4e7");
        static BlueprintSpellList ThassIllusionList = library.Get<BlueprintSpellList>("c311aed33deb7a346ab715baef4a0572");
        static BlueprintSpellList ThassNecromancyList = library.Get<BlueprintSpellList>("5c08349132cb6b04181797f58ccf38ae");
        static BlueprintSpellList ThassTransmutationList = library.Get<BlueprintSpellList>("f3a8f76b1d030a64084355ba3eea369a");

        

        static BlueprintFeature ThassAbju = library.Get<BlueprintFeature>("15c681d5a76c1a742abe2760376ddf6d");//ThassilonianAbjurationFeature
        static BlueprintFeature ThassConj = library.Get<BlueprintFeature>("1a258cd8e93461a4ab011c73a2c43dac");//ThassilonianConjurationFeature
        static BlueprintFeature ThassEnch = library.Get<BlueprintFeature>("e1ebc61a71c55054991863a5f6f6d2c2");//ThassilonianEnchantmentFeature
        static BlueprintFeature ThassEvoc = library.Get<BlueprintFeature>("5e33543285d1c3d49b55282cf466bef3");//ThassilonianEvocationFeature
        static BlueprintFeature ThassIllu = library.Get<BlueprintFeature>("aa271e69902044b47a8e62c4e58a9dcb");//ThassilonianIllusionFeature
        static BlueprintFeature ThassNecr = library.Get<BlueprintFeature>("fb343ede45ca1a84496c91c190a847ff");//ThassilonianNecromancyFeature
        static BlueprintFeature ThassTran = library.Get<BlueprintFeature>("dd163630abbdace4e85284c55d269867");//ThassilonianTransmutationFeature

        static BlueprintFeature[] WizFeatureList = new BlueprintFeature[]{null,ThassAbju, ThassConj, ThassEnch, ThassEvoc, ThassIllu, ThassNecr, ThassTran };
        static BlueprintSpellList[] spellLists = new BlueprintSpellList[] { wizardlist, ThassAbjurationList, ThassConjurationList, ThassEnchantmentList, ThassEvocationList, ThassIllusionList, ThassNecromancyList, ThassTransmutationList };


        internal static void Load()
        {
            // Load  feats
            Main.SafeLoad(CreateSpellBinder, "Mage Domain");
            Main.SafeLoad(CreatePoleiheiraAdherent, "Travel Far");

            //needed patch

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
            var Mount2 = Helpers.CreateFeature(name + mount + "Feature2", mount+" Upgrade",mounupgradedesc+" 3 hours.", Helpers.getGuid(name + mount + "Feature2"), iconmount, FeatureGroup.WizardFeat,
                Helpers.Create<AddWearinessHours>(p => p.Hours = -3),
                Helpers.Create<RemoveFeatureOnApply>(f => f.Feature = Mount1 )
                );
            var Mount3 = Helpers.CreateFeature(name + mount + "Feature3", mount+" Upgrade", mounupgradedesc + " 4 hours.", Helpers.getGuid(name + mount + "Feature3"), iconmount, FeatureGroup.WizardFeat,
                Helpers.Create<AddWearinessHours>(p => p.Hours = -4),
                Helpers.Create<RemoveFeatureOnApply>(f => f.Feature = Mount2)
                );

            var Mount4 = Helpers.CreateFeature(name + mount + "Feature4", mount+" Upgrade", mounupgradedesc + " 5 hours.", Helpers.getGuid(name + mount + "Feature4"), iconmount, FeatureGroup.WizardFeat,
                Helpers.Create<AddWearinessHours>(p => p.Hours = -5),
                Helpers.Create<RemoveFeatureOnApply>(f => f.Feature = Mount3)
                );

            var Mount5 = Helpers.CreateFeature(name + mount + "Feature5", mount+ " Upgrade", mounupgradedesc + " 6 hours.", Helpers.getGuid(name + mount + "Feature5"), iconmount, FeatureGroup.WizardFeat,
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
            var MoreSpell = Helpers.CreateParametrizedFeature(name+"MoreSpellParamFeature", Bond + ": supplemental spell", spellsupdesc, Helpers.getGuid(name + "MoreSpellParamFeature"), iconbook, FeatureGroup.WizardFeat, FeatureParameterType.LearnSpell, 
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
            for (int i = 1; i<21; i++)
            {
                if (i == 1)
                    addFeatures.Add(Helpers.LevelEntry(1, Mount1));
                if (i > 1)
                {
                    if (i == 5)
                        addFeatures.Add(Helpers.LevelEntry(i, MoreSpellSelection, MoreSpellSelection, Mount2));
                    else if (i == 8)
                        addFeatures.Add(Helpers.LevelEntry(i, MoreSpellSelection, MoreSpellSelection, GreatTraveler));
                    else if(i == 10)
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
            for (int i = 0; i <8; i++)
            {
                for (int j = 1; j < 10; j++)
                {
                    
                    //Creating parametrized feature
                    allspellbond[i][j - 1] = Helpers.CreateParametrizedFeature(name + i+"&"+j, Name+" Spell level "+j, BondSelection.Description, Helpers.getGuid(name + i + "&" + j), icon, FeatureGroup.WizardFeat, FeatureParameterType.LearnSpell,
                    //Helpers.PrerequisiteFeature(Helpers.elf),
                    Helpers.PrerequisiteClassLevel(wizardclass, j == 1 ? 1 : (j * 2 - 1)),
                    Helpers.Create<SpontaneousCastingParametrized>(l => l.SpellLevel = j));

                    
                    // if wizard
                    if (i == 0)
                    {
                        var nothass = Helpers.Create<PrerequisiteNoArchetype>(n => { n.CharacterClass = wizardclass; n.Archetype = thassilonian; });
                        
                        allspellbond[0][j-1].AddComponents(nothass);
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
                    if(i == 0)
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
                    var copy = library.TryGet<BlueprintAbility>(Helpers.getGuid("SpellBinder" + blueprintAbility.name + base.Owner.Unit.UniqueId));//copy exist already ?

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
                
            }




            public int SpellLevel;

        }

    

    }
}
