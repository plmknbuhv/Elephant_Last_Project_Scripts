using Code.Entities;
using Code.Players.Modules;
using Code.Skills.Core;
using UnityEngine;

namespace Code.Players.States
{
    public abstract class PlayerCanAttackState : PlayerCanMoveState
    {
        protected PlayerAttackComponent _attackCompo;
        private PlayerSkillManagement _skillManagement;
        private PlayerSoulManagement _soulManagement;

        private bool _canSlotChanged;
        
        public PlayerCanAttackState(Entity entity, int animationHash) : base(entity, animationHash)
        {
            _skillManagement = entity.GetModule<PlayerSkillManagement>();
            _soulManagement = entity.GetModule<PlayerSoulManagement>();
            _attackCompo = entity.GetModule<PlayerAttackComponent>();
            
            Debug.Assert(_attackCompo != null, "state variable: attack component is null");
            Debug.Assert(_skillManagement != null, "state variable: skill management is null");
            Debug.Assert(_soulManagement != null, "state variable: ability management is null");
        }

        public override void Enter()
        {
            base.Enter();
            
            _canSlotChanged = true;
            if (_movement.IsGrounded) _attackCompo.SetCanAttack(true);
            
            _player.PlayerInput.OnAttackPressed += HandleAttackPressed;
            _player.PlayerInput.OnSkillPressed += HandleSkillPressed;
            _player.PlayerInput.OnSlotChangePressed += HandleSlotChangePressed;
        }

        private async void HandleSlotChangePressed()
        {
            if (!_canSlotChanged) return;
            
            _canSlotChanged = false;
            
            _soulManagement.ChangeSlot();
            
            await Awaitable.WaitForSecondsAsync(0.5f, _entity.destroyCancellationToken);
            _canSlotChanged = true;
        }

        protected virtual void HandleAttackPressed()
        {
            _preInputModule.AddToBuffer("ATTACK", 1);
        }
        
        private void HandleSkillPressed(int idx, bool isPressed)
        {
            SkillKeyType keyType = (SkillKeyType)idx;
            
            var skill = _skillManagement.GetSkill(keyType);
            if (skill == null || !_skillManagement.CanUseSkill(skill))
            {
                //스킬 없음 Feedback보내기
                if(skill != null)
                    Debug.Log($"스킬이 쿨타임중이거나 사용중이거나 마나가 부족합니다. : {skill.name}");
                else
                {
                    Debug.Log($"장착하고 있는 스킬 없음 {keyType}");
                }
                return;
            }
            
            if (isPressed && skill is IChargeable { IsCharging: false } chargeable)
            {
                chargeable.StartCharging();
                _player.ChangeState("CHARGING");
            }
            else if(isPressed)
            {
                _skillManagement.SetCurrentSkill(keyType);
                _player.ChangeState(skill.SkillData.skillStateName);
            }
        }
        
        public override void Exit()
        {
            _player.PlayerInput.OnAttackPressed -= HandleAttackPressed;
            _player.PlayerInput.OnSkillPressed -= HandleSkillPressed;
            _player.PlayerInput.OnSlotChangePressed -= HandleSlotChangePressed;
            base.Exit();
        }
    }
}