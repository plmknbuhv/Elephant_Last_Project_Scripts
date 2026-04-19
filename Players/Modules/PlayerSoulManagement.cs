using System.Collections.Generic;
using Code.Modules;
using Code.Skills.Core;
using Code.Souls.Core;
using EventSystem;
using UnityEngine;
using UnityEngine.Events;

namespace Code.Players.Modules
{
    public class PlayerSoulManagement : MonoBehaviour, IModule, IAfterInitModule
    {
        public UnityEvent OnSoulChangedEvent;
        [SerializeField] private GameEventChannelSO skillChannel;

        public SoulType CurrentSlotType { get; private set; }
        public SoulDataSO CurrentActiveSoul => _slotDict.GetValueOrDefault(CurrentSlotType);
        
        private Dictionary<SoulType, SoulDataSO> _slotDict;
        
        public void Initialize(ModuleOwner owner)
        {
            _slotDict = new Dictionary<SoulType, SoulDataSO>();
        }
        
        public void AfterInitialize()
        {
            for (int i = 0; i < (int)SoulType.END; i++)
            {
                _slotDict.Add((SoulType)i, null);
            }
            
            skillChannel.AddListener<PlayerSoulEquipEvent>(HandleSoulEquip);
        }

        private void OnDestroy()
        {
            skillChannel.RemoveListener<PlayerSoulEquipEvent>(HandleSoulEquip);
        }

        private void HandleSoulEquip(PlayerSoulEquipEvent evt)
        {
            EquipSoul(evt.targetType, evt.targetSoul);
        }

        private void EquipSoul(SoulType type, SoulDataSO soul)
        {
            _slotDict[type] = soul;

            if(CurrentSlotType == SoulType.None)
                CurrentSlotType = type;
                
            if (CurrentSlotType == type)
            {
                ActiveSkill(soul);
            }
        }

        public void ChangeSlot()
        {
            var tempSlot = CurrentSlotType == SoulType.God ? SoulType.Devil : SoulType.God;

            if (!_slotDict.TryGetValue(tempSlot, out SoulDataSO soul) 
                || soul == null 
                || CurrentSlotType == tempSlot)
                return;

            CurrentSlotType = tempSlot;
            ActiveSkill(CurrentActiveSoul);
        }

        private void ActiveSkill(SoulDataSO soul)
        {
            if (soul == null)
            {
                for (int i = 0; i < 3; i++)
                {
                    EquipSkillToKey((SkillKeyType)i, null);
                }
            }
            else
            {
                foreach (var matchData in soul.skills)
                {
                    EquipSkillToKey(matchData.keyType, matchData.targetSkill);
                }
            }
            
            var changeEvt = 
                SkillEvents.PlayerSoulChangeEvent.Initializer(soul);
            skillChannel.RaiseEvent(changeEvt);
            
            OnSoulChangedEvent?.Invoke();
        }

        private void EquipSkillToKey(SkillKeyType keyType, SkillDataSO skillData)
        {
            var evt = 
                SkillEvents.PlayerSkillEquipEvent.Initializer(keyType, skillData);
            skillChannel.RaiseEvent(evt);
        }
    }
}