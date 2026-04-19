using System.Collections.Generic;
using System.Linq;
using Code.Modules;
using UnityEngine;

namespace Code.Players.Modules
{
    public struct BufferInputData
    {
        public string StateName { get; private set; }
        public float InputTime { get; private set; }
        public int Priority { get; private set; }

        public BufferInputData(string stateName, float inputTime, int priority)
        {
            StateName = stateName;
            InputTime = inputTime;
            Priority = priority;
        }
    }

    public class PlayerPreInputModule : MonoBehaviour, IModule
    {
        [SerializeField] private float bufferWindow = 0.3f; // 유효 시간
        [SerializeField] private int maxSameStateCount = 3;
        public string LastChangeStateName { get; private set; }
        public string NextChangeStateName => _inputBuffer.FirstOrDefault().StateName ?? string.Empty; 

        private Player _player;
        private List<BufferInputData> _inputBuffer;

        public void Initialize(ModuleOwner owner)
        {
            _inputBuffer = new List<BufferInputData>();
            _player = owner as Player;

            Debug.Assert(_player != null, $"{owner.name} is not a player");
        }

        public bool CheckNextAction(out string stateName)
        {
            while (_inputBuffer.Count > 0)
            {
                var nextData = _inputBuffer[0];
                _inputBuffer.RemoveAt(0);

                if (Time.time - nextData.InputTime <= bufferWindow)
                {
                    stateName = nextData.StateName;
                    LastChangeStateName = stateName;
                    return true;
                }
            }

            stateName = string.Empty;
            return false;
        }

        public void AddToBuffer(string stateName, int priority, bool canOverlapState = true)
        {
            if (!canOverlapState && _inputBuffer.Any(x => x.StateName.Equals(stateName))) 
                return;

            if (_inputBuffer.Count(x => x.StateName.Equals(stateName)) >= maxSameStateCount) 
                return;
            
            float currentTime = Time.time;
            if(_inputBuffer.Any(x => Mathf.Abs(currentTime - x.InputTime) < 0.02f)) return;
            
            _inputBuffer.Add(new BufferInputData(stateName, currentTime, priority));
            _inputBuffer.Sort((a, b) => a.Priority.CompareTo(b.Priority));
        }

        public void ClearActionBuffer()
        {
            _inputBuffer.Clear();
        }
    }
}