using Code.Entities;
using Code.Players.Modules;
using Code.Skills.Core;
using UnityEngine;

namespace Code.Players.States
{
    public class PlayerChargingState : PlayerState
    {
        private readonly int _chargingEndTrigger = Animator.StringToHash("CHARGING_END");
        private PlayerSkillManagement _skillManagement;
        
        public PlayerChargingState(Entity entity, int animationHash) : base(entity, animationHash)
        {
            _skillManagement = entity.GetModule<PlayerSkillManagement>();
            Debug.Assert(_skillManagement != null, "state variable: skill management is null");
        }

        public override void Enter()
        {
            base.Enter();
            
            _player.PlayerInput.OnSkillPressed += HandleSkillReleased;
        }

        public override void Update()
        {
            base.Update();
            
            if(_isTriggerCall) 
                _player.ChangeState("IDLE");
        }

        private void HandleSkillReleased(int idx, bool isPressed)
        {
            if (isPressed) return;
            
            var skill = _skillManagement.GetSkill((SkillKeyType)idx);
            if (skill is IChargeable { IsCharging: true } chargeable)
            {
                chargeable.ReleaseCharging();
                _entityAnimator.SetParam(_chargingEndTrigger);
            }
        }

        public override void Exit()
        {
            _player.PlayerInput.OnSkillPressed -= HandleSkillReleased;
            base.Exit();
        }
    }
}