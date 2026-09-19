using UnityEngine;
using System;

namespace DeadSignal.Puzzles
{
    /// <summary>
    /// Serializable container for puzzle state in the radio room.
    /// Persists player progress and allows save/load functionality.
    /// </summary>
    [System.Serializable]
    public class PuzzleState
    {
        [Header("Fuse Puzzle")]
        public bool fuseInstalled = false;

        [Header("Cable Connections")]
        public bool cable1Connected = false;
        public bool cable2Connected = false;

        [Header("Radio Tuning")]
        public int radioFrequencyIndex = 0; // 0, 1, or 2
        public bool radioTuned = false;

        [Header("Story Progress")]
        public bool clueDiscovered = false;
        public bool doorUnlocked = false;

        [Header("Timing")]
        public float sessionDuration = 0f;
        [SerializeField]
        private long timestampTicks;

        public DateTime Timestamp
        {
            get => new DateTime(timestampTicks);
            set => timestampTicks = value.Ticks;
        }

        public event Action OnStateChanged;

        /// <summary>
        /// Checks if puzzle is complete (player discovered clue and unlocked door)
        /// </summary>
        public bool IsPuzzleComplete => clueDiscovered && doorUnlocked;

        /// <summary>
        /// Checks if power system is active (fuse installed)
        /// </summary>
        public bool IsPowered => fuseInstalled;

        /// <summary>
        /// Checks if signal path is complete (cables connected)
        /// </summary>
        public bool IsSignalPathComplete => cable1Connected && cable2Connected;

        public PuzzleState()
        {
            Timestamp = DateTime.Now;
        }

        /// <summary>
        /// Installs fuse and enables power system
        /// </summary>
        public void InstallFuse()
        {
            if (!fuseInstalled)
            {
                fuseInstalled = true;
                OnStateChanged?.Invoke();
            }
        }

        /// <summary>
        /// Removes fuse and disables power system
        /// </summary>
        public void RemoveFuse()
        {
            if (fuseInstalled)
            {
                fuseInstalled = false;
                OnStateChanged?.Invoke();
            }
        }

        /// <summary>
        /// Connects a cable to the signal path
        /// </summary>
        public void ConnectCable(int cableIndex)
        {
            if (cableIndex == 0 && !cable1Connected)
            {
                cable1Connected = true;
                OnStateChanged?.Invoke();
            }
            else if (cableIndex == 1 && !cable2Connected)
            {
                cable2Connected = true;
                OnStateChanged?.Invoke();
            }
        }

        /// <summary>
        /// Disconnects a cable from the signal path
        /// </summary>
        public void DisconnectCable(int cableIndex)
        {
            if (cableIndex == 0 && cable1Connected)
            {
                cable1Connected = false;
                OnStateChanged?.Invoke();
            }
            else if (cableIndex == 1 && cable2Connected)
            {
                cable2Connected = false;
                OnStateChanged?.Invoke();
            }
        }

        /// <summary>
        /// Tunes radio to a specific frequency (0-2)
        /// </summary>
        public void TuneRadio(int frequencyIndex)
        {
            if (frequencyIndex >= 0 && frequencyIndex < 3 && frequencyIndex != radioFrequencyIndex)
            {
                radioFrequencyIndex = frequencyIndex;
                OnStateChanged?.Invoke();

                if (frequencyIndex == 2) // Final frequency triggers clue
                {
                    DiscoverClue();
                }
            }
        }

        /// <summary>
        /// Discovers the story clue at final radio frequency
        /// </summary>
        public void DiscoverClue()
        {
            if (!clueDiscovered)
            {
                clueDiscovered = true;
                OnStateChanged?.Invoke();
            }
        }

        /// <summary>
        /// Unlocks the next door/area when clue is discovered
        /// </summary>
        public void UnlockDoor()
        {
            if (!doorUnlocked && clueDiscovered)
            {
                doorUnlocked = true;
                OnStateChanged?.Invoke();
            }
        }

        /// <summary>
        /// Resets puzzle to initial state
        /// </summary>
        public void Reset()
        {
            fuseInstalled = false;
            cable1Connected = false;
            cable2Connected = false;
            radioFrequencyIndex = 0;
            radioTuned = false;
            clueDiscovered = false;
            doorUnlocked = false;
            Timestamp = DateTime.Now;
            sessionDuration = 0f;
            OnStateChanged?.Invoke();
        }

        /// <summary>
        /// Creates a deep copy of this state
        /// </summary>
        public PuzzleState Clone()
        {
            return new PuzzleState
            {
                fuseInstalled = this.fuseInstalled,
                cable1Connected = this.cable1Connected,
                cable2Connected = this.cable2Connected,
                radioFrequencyIndex = this.radioFrequencyIndex,
                radioTuned = this.radioTuned,
                clueDiscovered = this.clueDiscovered,
                doorUnlocked = this.doorUnlocked,
                sessionDuration = this.sessionDuration,
                timestampTicks = this.timestampTicks
            };
        }
    }
}
