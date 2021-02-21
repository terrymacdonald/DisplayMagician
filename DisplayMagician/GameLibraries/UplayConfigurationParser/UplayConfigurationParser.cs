using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// This configuration parses logic is kept here for possible future use
// It was really difficult to find this logic in some obscure webpage
// so I'm keeping it in case I need it later.
namespace DisplayMagician.GameLibraries.UplayConfigurationParser
{
    class UplayConfigurationParser
    {

        /*def _convert_data(self, data):
        # calculate object size (konrad's formula)
        if data > 256 * 256:
            data = data - (128 * 256 * math.ceil(data / (256 * 256)))
            data = data - (128 * math.ceil(data / 256))
        else:
            if data > 256:
                data = data - (128 * math.ceil(data / 256))
        return data*/

        internal static decimal ConvertData (decimal data)
        {
            if (data > 65536)
            {
                data = data - (128 * 256 * Math.Ceiling(data / 65536));
            }
            else if (data > 256)
            {
                data = data - (128 * Math.Ceiling(data / 256));
            }

            return data;

        }



        /*def _parse_configuration_header(self, header, second_eight= False):
        try:
            offset = 1
            multiplier = 1
            record_size = 0
            tmp_size = 0

            if second_eight:
                while header[offset] != 0x08 or(header[offset] == 0x08 and header[offset + 1] == 0x08) :
                    record_size += header[offset] * multiplier
                    multiplier *= 256
                    offset += 1
                    tmp_size += 1
            else:
                while header[offset] != 0x08 or record_size == 0:
                    record_size += header[offset] * multiplier
                    multiplier *= 256
                    offset += 1
                    tmp_size += 1

            record_size = self._convert_data(record_size)

            offset += 1  # skip 0x08

            # look for launch_id
            multiplier = 1
            launch_id = 0

            while header[offset] != 0x10 or header[offset + 1] == 0x10:
                launch_id += header[offset] * multiplier
                multiplier *= 256
                offset += 1

            launch_id = self._convert_data(launch_id)

            offset += 1  # skip 0x10

            multiplier = 1
            launch_id_2 = 0
            while header[offset] != 0x1A or(header[offset] == 0x1A and header[offset + 1] == 0x1A) :
                launch_id_2 += header[offset] * multiplier
                multiplier *= 256
                offset += 1

            launch_id_2 = self._convert_data(launch_id_2)

            #if object size is smaller than 128b, there might be a chance that secondary size will not occupy 2b
            if record_size - offset < 128 <= record_size:
                tmp_size -= 1
                record_size += 1

# we end up in the middle of header, return values normalized
# to end of record as well real yaml size and game launch_id
            return record_size - offset, launch_id, launch_id_2, offset + tmp_size + 1
        except:
# something went horribly wrong, do not crash it,
# just return 0s, this way it will be handled later in the code
# 10 is to step a little in configuration file in order to find next game
            return 0, 0, 0, 10*/

        //internal static decimal ParseConfigurationHeader(decimal data);
    }
}
