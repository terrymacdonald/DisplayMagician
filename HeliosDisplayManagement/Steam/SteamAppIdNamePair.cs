namespace HeliosPlus.Steam
{
    public class SteamAppIdNamePair
    {
        public SteamAppIdNamePair(uint appId, string name)
        {
            AppId = appId;
            Name = name;
        }

        public SteamAppIdNamePair()
        {
        }

        public uint AppId { get; set; }
        public string Name { get; set; }
    }
}