using System;
using System.Collections.Generic;
using Code.Entities.StatSystem;
using Code.Modules;
using Code.Souls.Core;
using UnityEngine;
using UnityEngine.Events;

namespace Code.ManaSystem
{
    public class ManaModule : MonoBehaviour, IModule, IAfterInitModule
    {
        public UnityEvent OnCollectMana;
        
        [SerializeField] private StatSO maxManaStatData;
        [SerializeField] private StatSO additionalManaGainPercentStatData;
        [SerializeField] private int startManaValue;
        
        public event Action<int, int> OnGodManaValueChanged;
        public event Action<int, int> OnDevilManaValueChanged;

        private Dictionary<SoulType, int> _soulTypeManaValueDict;

        private int _maxManaValue;
        private float _additionalManaPercent;
        private EntityStatCompo _statCompo;

        public void Initialize(ModuleOwner owner)
        {
            _soulTypeManaValueDict = new Dictionary<SoulType, int>();
            _statCompo = owner.GetModule<EntityStatCompo>();

            Debug.Assert(_statCompo != null, $"stat compo not fount: {owner.name}");
        }

        public void AfterInitialize()
        {
            _maxManaValue = (int)_statCompo.SubscribeStat(maxManaStatData, HandleMaxManaValueChanged, 50);
            _additionalManaPercent = _statCompo.SubscribeStat(additionalManaGainPercentStatData,
                HandleAdditionalManaPercentChanged, 50);

            _soulTypeManaValueDict.Add(SoulType.God, startManaValue);
            _soulTypeManaValueDict.Add(SoulType.Devil, startManaValue);
            
            OnGodManaValueChanged?.Invoke(startManaValue, _maxManaValue);
            OnDevilManaValueChanged?.Invoke(startManaValue, _maxManaValue);
        }

        private void OnDestroy()
        {
            _statCompo.UnSubscribeStat(maxManaStatData, HandleMaxManaValueChanged);
            _statCompo.UnSubscribeStat(additionalManaGainPercentStatData, HandleAdditionalManaPercentChanged);
        }

        private void HandleMaxManaValueChanged(StatSO stat, float currentValue, float previousValue)
        {
            _maxManaValue = (int)currentValue;

            SetSoulManaValue(SoulType.God, GetSoulManaValue(SoulType.God));
            SetSoulManaValue(SoulType.Devil, GetSoulManaValue(SoulType.Devil));

            OnGodManaValueChanged?.Invoke(GetSoulManaValue(SoulType.God), _maxManaValue);
            OnDevilManaValueChanged?.Invoke(GetSoulManaValue(SoulType.Devil), _maxManaValue);
        }

        private void HandleAdditionalManaPercentChanged(StatSO stat, float currentValue, float previousValue)
        {
            _additionalManaPercent = currentValue;
        }

        public void AddManaValue(SoulType type, int value)
        {
            int addValue = GetFinalAddedValue(value);
            int newValue = GetSoulManaValue(type) + addValue;

            SetSoulManaValue(type, newValue);

            var action = type == SoulType.God ? OnGodManaValueChanged : OnDevilManaValueChanged;
            action?.Invoke(GetSoulManaValue(type), _maxManaValue);
            
            OnCollectMana?.Invoke();
        }

        public void UsedManaValue(SoulType type, int value) => AddManaValue(type, -value);

        public int GetSoulManaValue(SoulType type) => _soulTypeManaValueDict.GetValueOrDefault(type, 0);

        public void SetSoulManaValue(SoulType type, int value) =>
            _soulTypeManaValueDict[type] = Mathf.Clamp(value, 0, _maxManaValue);

        //추가될 마나의 추가적으로 부여되는 마나를 계산한 값 반환
        private int GetFinalAddedValue(int value) => Mathf.RoundToInt(value * _additionalManaPercent);
    }
}