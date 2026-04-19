using System;
using Code.Combat;
using Code.Entities;
using Code.ManaSystem;
using Code.Modules;
using Code.Souls.Core;
using EventSystem;
using UnityEngine;

namespace Code.Players
{
    public class PlayerData : MonoBehaviour, IModule
    {
        public delegate void PlayerDataValueChanged<in T>(T current, T max); //current, prev or current, max


        [field: SerializeField] public GameEventChannelSO PlayerChannel { get; private set; }
        public event PlayerDataValueChanged<int> OnPlayerHealthChanged;
        public event PlayerDataValueChanged<int> OnPlayerGodManaChanged;
        public event PlayerDataValueChanged<int> OnPlayerDevilManaChanged;
        public event Action OnPlayerManaChanged;
        
        [field: SerializeField] public int GodManaValue { get; private set; }
        [field: SerializeField] public int DevilManaValue { get; private set; }
        [field: SerializeField] public int MaxManaValue { get; private set; }
        [field: SerializeField] public int HealthValue { get; private set; }
        [field: SerializeField] public int MaxHealthValue { get; private set; }
        [field: SerializeField] public int ComboCount { get; private set; }
        
        private Entity _player;
        private HealthModule _healthModule;
        private ManaModule _manaModule;
        
        public void Initialize(ModuleOwner owner)
        {
            _player = owner as Entity;
            _healthModule = owner.GetModule<HealthModule>();
            _manaModule = owner.GetModule<ManaModule>();
            
            Debug.Assert(_player != null, $"owner is not player: {owner.name}");
            Debug.Assert(_healthModule != null, $"health module is not fount: {owner.name}");
            Debug.Assert(_manaModule != null, $"mana module is not fount: {owner.name}");
            
            _healthModule.OnHealthChangeEvent.AddListener(HandleHealthChanged);
            PlayerChannel.AddListener<PlayerAddManaEvent>(HandleAddMana);
            _manaModule.OnGodManaValueChanged += HandleGodManaValueChanged;
            _manaModule.OnDevilManaValueChanged += HandleDevilManaValueChanged;
        }

        private void OnDestroy()
        {
            PlayerChannel.RemoveListener<PlayerAddManaEvent>(HandleAddMana);
            
            _healthModule.OnHealthChangeEvent.RemoveListener(HandleHealthChanged);
            _manaModule.OnGodManaValueChanged -= HandleGodManaValueChanged;
            _manaModule.OnDevilManaValueChanged -= HandleDevilManaValueChanged;
        }

        private void HandleHealthChanged(int currentValue, int maxValue)
        {
            HealthValue = currentValue;
            MaxHealthValue = maxValue;
            
            OnPlayerHealthChanged?.Invoke(HealthValue, MaxHealthValue);
        }
        
        private void HandleGodManaValueChanged(int currentValue, int maxValue)
        {
            GodManaValue = currentValue;
            MaxManaValue = maxValue;
            
            OnPlayerGodManaChanged?.Invoke(GodManaValue, MaxManaValue);
            OnPlayerManaChanged?.Invoke();
        }
        
        private void HandleDevilManaValueChanged(int currentValue, int maxValue)
        {
            DevilManaValue = currentValue;
            MaxManaValue = maxValue;
            
            OnPlayerDevilManaChanged?.Invoke(DevilManaValue, MaxManaValue);
            OnPlayerManaChanged?.Invoke();
        }

        private void HandleAddMana(PlayerAddManaEvent evt)
        {
            AddMana(SoulType.God, evt.manaAmount);
            AddMana(SoulType.Devil, evt.manaAmount);
        }

        public void AddMana(SoulType type, int amount) => _manaModule.AddManaValue(type, amount);
        public void UsedMana(SoulType type, int amount) => _manaModule.UsedManaValue(type, amount);
    }
}