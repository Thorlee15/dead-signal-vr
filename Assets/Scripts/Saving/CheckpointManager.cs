using UnityEngine;
using System.IO;
using DeadSignal.Puzzles;

namespace DeadSignal.Saving
{
    /// <summary>
    /// Manages checkpoint saves and loads for puzzle progression.
    /// Uses JSON serialization for cross-platform compatibility.
    /// </summary>
    public class CheckpointManager : MonoBehaviour
    {
        private static CheckpointManager instance;

        [SerializeField] private string saveFolderName = "DeadSignalSaves";
        [SerializeField] private string checkpointFileName = "checkpoint.json";

        private string savePath;

        public static CheckpointManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<CheckpointManager>();
                    if (instance == null)
                    {
                        var obj = new GameObject("CheckpointManager");
                        instance = obj.AddComponent<CheckpointManager>();
                    }
                }
                return instance;
            }
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSavePath();
        }

        private void InitializeSavePath()
        {
            savePath = Path.Combine(Application.persistentDataPath, saveFolderName);

            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }
        }

        /// <summary>
        /// Saves the current puzzle state to a checkpoint file
        /// </summary>
        public void SaveCheckpoint(PuzzleState state)
        {
            if (state == null)
            {
                Debug.LogError("Cannot save null PuzzleState");
                return;
            }

            try
            {
                string json = JsonUtility.ToJson(state, true);
                string filePath = Path.Combine(savePath, checkpointFileName);

                File.WriteAllText(filePath, json);
                Debug.Log($"Checkpoint saved to: {filePath}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to save checkpoint: {ex.Message}");
            }
        }

        /// <summary>
        /// Loads puzzle state from checkpoint file
        /// </summary>
        public PuzzleState LoadCheckpoint()
        {
            string filePath = Path.Combine(savePath, checkpointFileName);

            if (!File.Exists(filePath))
            {
                Debug.LogWarning("No checkpoint file found. Starting fresh.");
                return new PuzzleState();
            }

            try
            {
                string json = File.ReadAllText(filePath);
                PuzzleState state = JsonUtility.FromJson<PuzzleState>(json);
                Debug.Log($"Checkpoint loaded from: {filePath}");
                return state;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to load checkpoint: {ex.Message}. Starting fresh.");
                return new PuzzleState();
            }
        }

        /// <summary>
        /// Checks if a checkpoint file exists
        /// </summary>
        public bool HasCheckpoint()
        {
            string filePath = Path.Combine(savePath, checkpointFileName);
            return File.Exists(filePath);
        }

        /// <summary>
        /// Deletes the checkpoint file
        /// </summary>
        public void DeleteCheckpoint()
        {
            string filePath = Path.Combine(savePath, checkpointFileName);

            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                    Debug.Log($"Checkpoint deleted: {filePath}");
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Failed to delete checkpoint: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Gets the timestamp of the last checkpoint save
        /// </summary>
        public System.DateTime GetCheckpointTimestamp()
        {
            string filePath = Path.Combine(savePath, checkpointFileName);

            if (!File.Exists(filePath))
                return System.DateTime.MinValue;

            try
            {
                var fileInfo = new FileInfo(filePath);
                return fileInfo.LastWriteTime;
            }
            catch
            {
                return System.DateTime.MinValue;
            }
        }

        /// <summary>
        /// Auto-saves checkpoint when puzzle state changes
        /// </summary>
        public void SetupAutoSave(PuzzleState state)
        {
            if (state != null)
            {
                state.OnStateChanged += () => SaveCheckpoint(state);
            }
        }
    }
}
