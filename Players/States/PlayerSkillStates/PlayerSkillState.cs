using Code.Entities;
using Code.Players.Modules;
using Code.Skills.Core;
using UnityEngine;

namespace Code.Players.States.PlayerSkillStates
{
    public class PlayerSkillState : PlayerCanMoveState
    {
        protected readonly PlayerSkillManagement _skillManagement;
        protected Skill _currentSkill;
        
        public PlayerSkillState(Entity entity, int animationHash) : base(entity, animationHash)
        {
            _skillManagement = entity.GetModule<PlayerSkillManagement>();
            Debug.Assert(_skillManagement != null, "state variable: skill management is null");
        }

        public override void Enter()
        {
            //여기서 스킬 사용 조건을 확인한다.
            if(!CanUseSkill())
            {
                _skillManagement.CurrentSkill.CancelSkill();
                _player.ChangeState("IDLE");
                return;
            }
            
            base.Enter();
            OnUseSkill();
        }

        protected virtual void OnUseSkill()
        {
            _movement.StopMovement();
            _currentSkill = _skillManagement.CurrentSkill;
            Debug.Assert(_currentSkill != null, "current skill is null");
            if (_currentSkill == null) return;

            _skillManagement.OnUseSkillEvent?.Invoke();
            if (_currentSkill.UseSkill())
            {
                var skillData = _currentSkill.SkillData;
                _player.PlayerData.UsedMana(skillData.soulType, skillData.needManaValue);
            }
        }

        protected virtual bool CanUseSkill()
        {
            return true;
        }

        public override void Update()
        {
            base.Update();
            
            if(_isTriggerCall)
                _player.ChangeState("IDLE");
        }

        public override void Exit()
        {
            _skillManagement.OnEndSkillEvent?.Invoke();
            
            base.Exit();
        }
    }
}