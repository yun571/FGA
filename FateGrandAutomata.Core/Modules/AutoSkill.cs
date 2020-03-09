﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CoreAutomata;

namespace FateGrandAutomata
{
    public class AutoSkill
    {
        Dictionary<char, Action> _currentArray;
        readonly Dictionary<char, Action> _defaultFunctionArray,
            _startingMemberFunctionArray,
            _subMemberFunctionArray,
            _enemyTargetArray;

        public bool IsFinished { get; private set; }

        public Battle Battle { get; private set; }
        public Card Card { get; private set; }

        void WaitForAnimationToFinish(int? Timeout = null)
        {
            var img = ImageLocator.Battle;

            // slow devices need this. do not remove.
            Game.BattleScreenRegion.WaitVanish(img, 2);

            Game.BattleScreenRegion.Exists(img, Timeout ?? 5);
        }

        void CastSkill(Location Location)
        {
            Location.Click();

            if (Preferences.SkillConfirmation)
            {
                Game.BattleSkillOkClick.Click();
            }

            WaitForAnimationToFinish();
        }

        void SelectSkillTarget(Location Location)
        {
            Location.Click();

            WaitForAnimationToFinish();
        }

        void CastNoblePhantasm(Location Location)
        {
            if (!Battle.HasClickedAttack)
            {
                Battle.ClickAttack();

                // There is a delay after clicking attack before NP Cards come up. DON'T DELETE!
                AutomataApi.Wait(2);
            }

            Location.Click();
        }

        void OpenMasterSkillMenu()
        {
            Game.BattleMasterSkillOpenClick.Click();
            
            AutomataApi.Wait(0.3);
        }

        void CastMasterSkill(Location Location)
        {
            OpenMasterSkillMenu();

            CastSkill(Location);
        }

        void ChangeArray(Dictionary<char, Action> NewArray)
        {
            _currentArray = NewArray;
        }

        void BeginOrderChange()
        {
            OpenMasterSkillMenu();

            Game.BattleMasterSkill3Click.Click();

            if (Preferences.SkillConfirmation)
            {
                Game.BattleSkillOkClick.Click();
            }

            AutomataApi.Wait(0.3);

            ChangeArray(_startingMemberFunctionArray);
        }

        void SelectStartingMember(Location Location)
        {
            Location.Click();

            ChangeArray(_subMemberFunctionArray);
        }

        void SelectSubMemeber(Location Location)
        {
            Location.Click();

            AutomataApi.Wait(0.3);

            Game.BattleOrderChangeOkClick.Click();

            WaitForAnimationToFinish(15);

            ChangeArray(_defaultFunctionArray);
        }

        void SelectTarget()
        {
            ChangeArray(_enemyTargetArray);
        }

        void SelectEnemyTarget(Location Location)
        {
            Location.Click();

            AutomataApi.Wait(0.5);

            // Exit any extra menu
            Game.BattleExtrainfoWindowCloseClick.Click();

            ChangeArray(_defaultFunctionArray);
        }

        public AutoSkill()
        {
            _defaultFunctionArray = new Dictionary<char, Action>
            {
                ['a'] = () => CastSkill(Game.BattleSkill1Click),
                ['b'] = () => CastSkill(Game.BattleSkill2Click),
                ['c'] = () => CastSkill(Game.BattleSkill3Click),
                ['d'] = () => CastSkill(Game.BattleSkill4Click),
                ['e'] = () => CastSkill(Game.BattleSkill5Click),
                ['f'] = () => CastSkill(Game.BattleSkill6Click),
                ['g'] = () => CastSkill(Game.BattleSkill7Click),
                ['h'] = () => CastSkill(Game.BattleSkill8Click),
                ['i'] = () => CastSkill(Game.BattleSkill9Click),

                ['j'] = () => CastMasterSkill(Game.BattleMasterSkill1Click),
                ['k'] = () => CastMasterSkill(Game.BattleMasterSkill2Click),
                ['l'] = () => CastMasterSkill(Game.BattleMasterSkill3Click),

                ['x'] = BeginOrderChange,
                ['t'] = SelectTarget,
                ['0'] = () => { },

                ['1'] = () => SelectSkillTarget(Game.BattleServant1Click),
                ['2'] = () => SelectSkillTarget(Game.BattleServant2Click),
                ['3'] = () => SelectSkillTarget(Game.BattleServant3Click),

                ['4'] = () => CastNoblePhantasm(Game.BattleNpCardClickArray[0]),
                ['5'] = () => CastNoblePhantasm(Game.BattleNpCardClickArray[1]),
                ['6'] = () => CastNoblePhantasm(Game.BattleNpCardClickArray[2])
            };

            _startingMemberFunctionArray = new Dictionary<char, Action>
            {
                ['1'] = () => SelectStartingMember(Game.BattleStartingMember1Click),
                ['2'] = () => SelectStartingMember(Game.BattleStartingMember2Click),
                ['3'] = () => SelectStartingMember(Game.BattleStartingMember3Click)
            };

            _subMemberFunctionArray = new Dictionary<char, Action>
            {
                ['1'] = () => SelectSubMemeber(Game.BattleSubMember1Click),
                ['2'] = () => SelectSubMemeber(Game.BattleSubMember2Click),
                ['3'] = () => SelectSubMemeber(Game.BattleSubMember3Click)
            };

            _enemyTargetArray = new Dictionary<char, Action>
            {
                ['1'] = () => SelectEnemyTarget(Game.BattleTargetClickArray[0]),
                ['2'] = () => SelectEnemyTarget(Game.BattleTargetClickArray[1]),
                ['3'] = () => SelectEnemyTarget(Game.BattleTargetClickArray[2]),
            };
        }

        readonly List<List<string>> _commandTable = new List<List<string>>();

        void InitCommands()
        {
            var stageCount = 0;

            foreach (var commandList in Preferences.SkillCommand.Split(','))
            {
                if (Regex.IsMatch(commandList, @"[^0]"))
                {
                    if (Regex.IsMatch(commandList, @"^[1-3]"))
                    {
                        throw new FormatException($"Error at '{commandList}': Skill Command cannot start with number '1', '2' and '3'!");
                    }

                    if (Regex.IsMatch(commandList, @"[^,]#") || Regex.IsMatch(commandList, @"#[^,]"))
                    {
                        throw new FormatException($"Error at '{commandList}': '#' must be preceded and followed by ','! Correct: ',#,'");
                    }

                    if (Regex.IsMatch(commandList, @"[^a-l1-6#tx]"))
                    {
                        throw new FormatException($"Error at '{commandList}': Skill Command exceeded alphanumeric range! Expected 'x' or range 'a' to 'l' for alphabets and '0' to '6' for numbers.");
                    }
                }

                if (stageCount >= _commandTable.Count)
                {
                    _commandTable.Add(new List<string>());
                }

                if (commandList == "#")
                {
                    ++stageCount;
                }
                else _commandTable[stageCount].Add(commandList);
            }
        }

        public void ResetState()
        {
            IsFinished = !Preferences.EnableAutoSkill;

            ChangeArray(_defaultFunctionArray);
        }

        public void Init(Battle BattleModule, Card CardModule)
        {
            Battle = BattleModule;
            Card = CardModule;

            if (Preferences.EnableAutoSkill)
            {
                InitCommands();
            }

            ResetState();
        }

        string GetCommandListFor(int Stage, int Turn)
        {
            if (Stage < _commandTable.Count)
            {
                var commandList = _commandTable[Stage];

                if (Turn < commandList.Count)
                {
                    return commandList[Turn];
                }
            }
            
            return null;
        }

        void ExecuteCommandList(string CommandList)
        {
            foreach (var command in CommandList)
            {
                _currentArray[command]();
            }
        }

        public void Execute()
        {
            var commandList = GetCommandListFor(Battle.CurrentStage, Battle.CurrentTurn);

            if (commandList != null)
            {
                ExecuteCommandList(commandList);
            }
            else if (Battle.CurrentStage >= _commandTable.Count)
            {
                // this will allow NP spam after all commands have been executed
                IsFinished = true;
            }
        }
    }
}