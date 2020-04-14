namespace HeliosDisplayManagement.Uplay
{
    public class UplayAppIdNamePair
    {
        public UplayAppIdNamePair(uint appId, string name)
        {
            AppId = appId;
            Name = name;
        }

        public UplayAppIdNamePair()
        {
        }

        public uint AppId { get; set; }
        public string Name { get; set; }
    }
}