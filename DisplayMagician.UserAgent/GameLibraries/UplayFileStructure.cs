using ProtoBuf;
using System.Collections.Generic;

namespace DisplayMagician.GameLibraries
{
    // #####################################################################################################
    // # This set of classes are used for deserialising Uplay protobuf files
    // #####################################################################################################

    [ProtoContract]
    public class UplayCachedGame
    {
        [ProtoMember(1)]
        public uint UplayId { get; set; }
        [ProtoMember(2)]
        public uint InstallId { get; set; }
        [ProtoMember(3)]
        public string GameInfo { get; set; } = string.Empty;
    }

    [ProtoContract]
    public class UplayCachedGameCollection
    {
        [ProtoMember(1)]
        public List<UplayCachedGame> Games { get; set; } = new List<UplayCachedGame>();
    }

    // #####################################################################################################
    // # This set of classes are used for deserialising Uplay YAML enbedded within the protobuf file format
    // #####################################################################################################
    public class ProductInformation
    {
        public class Executable
        {
            public class Path
            {
                public string relative = string.Empty;
            }

            public class WorkingDirectory
            {
                public string register = string.Empty;
                public string append = string.Empty;
            }

            public Path? path;
            public WorkingDirectory? working_directory;
            public string internal_name = string.Empty;
            public string description = string.Empty;
            public string shortcut_name = string.Empty;
            public string icon_image = string.Empty;
        }

        public class StartGameItem
        {
            public bool after_game_report_enabled;
            public bool overlay_supported;
            public bool overlay_product_activation_enabled;
            public bool overlay_required;
            public bool overlay_shop_enabled;
            public bool legacy_ticket_enabled;
            public List<Executable> executables = new List<Executable>();

        }

        public class StartGame
        {
            public StartGameItem? online;
            public StartGameItem? offline;
        }

        public class DigitalDistribution
        {
            public int version;
        }

        public class Localization
        {
            public string l1 = string.Empty;
        }

        public class Club
        {
            public bool enabled;
        }

        public class Addon
        {
            public uint id;
            public bool is_visible;
            public string name = string.Empty;
            public string description = string.Empty;
            public string thumb_image = string.Empty;
        }

        public class Uplay
        {
            public string game_code = string.Empty;
            public string achievements = string.Empty;
            public string achievements_sync_id = string.Empty;
        }

        public class ThirdPartyPlatform
        {
            public string name = string.Empty;
        }

        public class Product
        {
            public string name = string.Empty;
            public string background_image = string.Empty;
            public string thumb_image = string.Empty;
            public string logo_image = string.Empty;
            public string dialog_image = string.Empty;
            public string icon_image = string.Empty;
            public ThirdPartyPlatform? third_party_platform;
            public string sort_string = string.Empty;
            public bool cloud_saves;
            public string forum_url = string.Empty;
            public string homepage_url = string.Empty;
            public string facebook_url = string.Empty;
            public string help_url = string.Empty;
            public bool after_game_report_ad;
            public bool force_safe_mode;
            public bool uplay_pipe_required;
            public bool show_properties;
            public bool game_streaming_enabled;
            public Uplay? uplay;
            public List<Addon> addons = new List<Addon>();
            public Club? club;
            public DigitalDistribution? digital_distribution;
            public bool is_ulc;
            public bool is_visible;
            public StartGame? start_game;
        }

        public string version = string.Empty;
        public Product? root;
        public Dictionary<string, Localization> localizations = new Dictionary<string, Localization>();
        public uint uplay_id;
        public uint install_id;
    }

}
