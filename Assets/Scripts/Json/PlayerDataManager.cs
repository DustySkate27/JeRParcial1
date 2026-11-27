using System;
using System.IO;
using Photon.Pun;
using UnityEngine;

public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager Instance { get; private set; }

    private const string FileName = "playerdata.json";
    private const int MinNameLength = 3;
    private const int MaxNameLength = 16;

    private PlayerData data;
    private string FilePath => Path.Combine(Application.persistentDataPath, FileName);

    public event Action<string> OnNameChanged;
    public event Action<MiniGame, int> OnWinsChanged;

    public string PlayerName => data.playerName;

    // Se crea solo antes de cargar la primera escena
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoCreate()
    {
        if (Instance != null) return;
        new GameObject("PlayerDataManager").AddComponent<PlayerDataManager>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Load();
        PhotonNetwork.NickName = data.playerName; // Photon lo sincroniza con el resto
    }

    #region Load / Save

    private void Load()
    {
        try
        {
            if (File.Exists(FilePath))
            {
                data = JsonUtility.FromJson<PlayerData>(File.ReadAllText(FilePath));
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"No se pudo leer el JSON, se crea uno nuevo: {e.Message}");
        }

        if (data == null)
        {
            data = new PlayerData();
        }

        if (string.IsNullOrWhiteSpace(data.playerName))
        {
            data.playerName = GenerateRandomName();
            Save();
        }
    }

    private void Save()
    {
        try
        {
            File.WriteAllText(FilePath, JsonUtility.ToJson(data, true));
        }
        catch (Exception e)
        {
            Debug.LogError($"No se pudo guardar el JSON: {e.Message}");
        }
    }

    #endregion

    #region Name

    private string GenerateRandomName()
    {
        return $"Jugador_{UnityEngine.Random.Range(1000, 10000)}";
    }

    /// <summary>
    /// Cambia el nombre, lo guarda y lo sincroniza con Photon (también si ya estás en una sala).
    /// Devuelve false si el nombre no es válido.
    /// </summary>
    public bool TrySetName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName)) return false;

        newName = newName.Trim();
        if (newName.Length < MinNameLength || newName.Length > MaxNameLength) return false;
        if (newName == data.playerName) return true;

        data.playerName = newName;
        Save();

        PhotonNetwork.NickName = newName;
        OnNameChanged?.Invoke(newName);
        return true;
    }

    #endregion

    #region Wins

    public int GetWins(MiniGame game)
    {
        switch (game)
        {
            case MiniGame.Parkour: return data.winsParkour;
            case MiniGame.Laser: return data.winsLaser;
            case MiniGame.Minigame3: return data.winsMinigame3;
            default: return 0;
        }
    }

    public void AddWin(MiniGame game)
    {
        switch (game)
        {
            case MiniGame.Parkour: data.winsParkour++; break;
            case MiniGame.Laser: data.winsLaser++; break;
            case MiniGame.Minigame3: data.winsMinigame3++; break;
        }

        Save();
        OnWinsChanged?.Invoke(game, GetWins(game));
    }

    /// <summary>Devuelve una copia de todos los datos (para pantallas de estadísticas).</summary>
    public PlayerData GetDataCopy()
    {
        return JsonUtility.FromJson<PlayerData>(JsonUtility.ToJson(data));
    }

    #endregion
}