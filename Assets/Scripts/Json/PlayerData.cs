using System;

[Serializable]
public class PlayerData
{
    public string playerName;
    public int winsParkour;
    public int winsLaser;
    public int winsMinigame3;
}

// Renombrá los valores según tus minijuegos
public enum MiniGame
{
    Parkour,
    Laser,
    Minigame3
}