using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisplayMagicianShared.NVIDIA
{
    internal static class NvConstants
    {
        public const int BinaryDataMax = 4096;
        public const int UnicodeStringLength = 2048;

        public const int AdvancedDisplayHeads = 4;
        public const int MaxDisplayHeads = 2;

        public const UInt32 NV_MAX_HEADS = 4;
        public const UInt32 NV_MAX_VID_PROFILES = 4;
        public const UInt32 NV_MAX_VID_STREAMS = 4;
        public const UInt32 NV_ADVANCED_DISPLAY_HEADS = 4;
        public const UInt32 NV_GENERIC_STRING_MAX = 4096;
        public const UInt32 NV_LONG_STRING_MAX = 256;
        public const UInt32 NV_MAX_ACPI_IDS = 16;
        public const UInt32 NV_MAX_AUDIO_DEVICES = 16;
        public const UInt32 NV_MAX_AVAILABLE_CPU_TOPOLOGIES = 256;
        public const UInt32 NV_MAX_AVAILABLE_SLI_GROUPS = 256;
        public const UInt32 NV_MAX_AVAILABLE_DISPLAY_HEADS = 2;
        public const UInt32 NV_MAX_DISPLAYS = NV_MAX_PHYSICAL_GPUS * NV_ADVANCED_DISPLAY_HEADS;
        public const UInt32 NV_MAX_GPU_PER_TOPOLOGY = 8;
        public const UInt32 NV_MAX_GPU_TOPOLOGIES = NV_MAX_PHYSICAL_GPUS;
        public const UInt32 NV_MAX_HEADS_PER_GPU = 32;
        public const UInt32 NV_MAX_LOGICAL_GPUS = 64;
        public const UInt32 NV_MAX_PHYSICAL_BRIDGES = 100;
        public const UInt32 NV_MAX_PHYSICAL_GPUS = 64;
        public const UInt32 NV_MAX_VIEW_MODES = 8;
        public const UInt32 NV_PHYSICAL_GPUS = 32;
        public const UInt32 NV_SHORT_STRING_MAX = 64;
        public const UInt32 NV_SYSTEM_HWBC_INVALID_ID = 0xffffffff;
        public const UInt32 NV_SYSTEM_MAX_DISPLAYS = NV_MAX_PHYSICAL_GPUS * NV_MAX_HEADS;
        public const UInt32 NV_SYSTEM_MAX_HWBCS = 128;
        public const UInt32 NV_MOSAIC_MAX_DISPLAYS = 64;
        public const UInt32 NV_MOSAIC_DISPLAY_SETTINGS_MAX = 40;
        public const UInt32 NV_MOSAIC_TOPO_IDX_DEFAULT = 0;
        public const UInt32 NV_MOSAIC_TOPO_IDX_LEFT_EYE = 0;
        public const UInt32 NV_MOSAIC_TOPO_IDX_RIGHT_EYE = 1;
        public const UInt32 NV_MOSAIC_TOPO_NUM_EYES = 2;
        public const UInt32 NV_MOSAIC_TOPO_MAX = (UInt32)Topology.Max;
        public const UInt32 NVAPI_MAX_MOSAIC_DISPLAY_ROWS = 8;
        public const UInt32 NVAPI_MAX_MOSAIC_DISPLAY_COLUMNS = 8;
        public const UInt32 NVAPI_MAX_MOSAIC_TOPOS = 16;
        public const UInt32 NVAPI_GENERIC_STRING_MAX = 4096;
        public const UInt32 NVAPI_LONG_STRING_MAX = 256;
        public const UInt32 NVAPI_SHORT_STRING_MAX = 64;
        public const UInt32 NVAPI_MAX_PHYSICAL_GPUS = 64;
        public const UInt32 NVAPI_MAX_PHYSICAL_GPUS_QUERIED = 32;
        public const UInt32 NVAPI_UNICODE_STRING_MAX = 2048;
        public const UInt32 NVAPI_BINARY_DATA_MAX = 4096;
        public const UInt32 NVAPI_SETTING_MAX_VALUES = 100;
        public const UInt32 NV_EDID_DATA_SIZE = 256;
    }
}
