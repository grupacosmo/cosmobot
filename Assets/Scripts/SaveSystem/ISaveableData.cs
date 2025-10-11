namespace Cosmobot
{
    // all objects that need to be saved should implement this interface
    public interface ISaveableData
    {
        bool LoadData(GameData data);
        bool SaveData(in GameData data);
    }
}
