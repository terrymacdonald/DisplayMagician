using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using EDIDParser;
using EDIDParser.Descriptors;
using EDIDParser.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NvAPIWrapper.Mosaic;
using NvAPIWrapper.Native.Exceptions;

namespace DisplayMagicianShared.NVIDIA
{
    public class SurroundTopologyDisplay
    {
        public SurroundTopologyDisplay(GridTopologyDisplay display)
        {
            DisplayId = display.DisplayDevice.DisplayId;
            Rotation = display.Rotation.ToRotation();
            Overlap = new Point(display.Overlap.HorizontalOverlap, display.Overlap.VerticalOverlap);
            PixelShift = display.PixelShiftType.ToPixelShift();

            try
            {
                NvAPIWrapper.Native.GeneralApi.Initialize();
                //var phyGPU = display.DisplayDevice.PhysicalGPU;
                var bytes = display.DisplayDevice.PhysicalGPU.ReadEDIDData(display.DisplayDevice.Output);
                DisplayName = new EDID(bytes).Descriptors
                    .Where(descriptor => descriptor is StringDescriptor)
                    .Cast<StringDescriptor>()
                    .FirstOrDefault(descriptor => descriptor.Type == StringDescriptorType.MonitorName)?.Value;
            }
            catch (NVIDIAApiException ex)
            {
                //Debug.WriteLine($"SurroundTopologyDisplay/NVIDIAApiException exception: {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
                // If we hit here then we cannot find the DisplayName from the EDID Data from the GPU
                // So we just make one up using the DisplayID
                DisplayName = $"Display #{DisplayId}";
            }
        }

        public SurroundTopologyDisplay()
        {
        }

        public uint DisplayId { get; set; }

        public string DisplayName { get; set; }

        public Point Overlap { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public PixelShift PixelShift { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public Rotation Rotation { get; set; }

        
        /// <inheritdoc />
        public override string ToString()
        {
            return DisplayName ?? $"SurroundTopologyDisplay #{DisplayId}";
        }

        public GridTopologyDisplay ToGridTopologyDisplay()
        {
            return new GridTopologyDisplay(DisplayId, new Overlap(Overlap.X, Overlap.Y), Rotation.ToRotate(), 0,
                PixelShift.ToPixelShiftType());
        }

        // The public override for the Object.Equals
        public override bool Equals(object obj)
        {
            return this.Equals(obj as SurroundTopologyDisplay);
        }

        // SurroundTopologyDisplay are equal if their contents are equal
        public bool Equals(SurroundTopologyDisplay other)
        {

            // If parameter is null, return false.
            if (Object.ReferenceEquals(other, null))
                return false;

            // Optimization for a common success case.
            if (Object.ReferenceEquals(this, other))
                return true;

            // If run-time types are not exactly the same, return false.
            if (this.GetType() != other.GetType())
                return false;

            // Check whether the SurroundTopologyDisplay properties are equal
            // Two SurroundTopologyDisplay are equal only when they have the same data exactly
            if (DisplayId == other.DisplayId &&
                Overlap.Equals(other.Overlap) &&
                PixelShift == other.PixelShift &&
                Rotation == other.Rotation)
                return true;
            else
                return false;
        }

        // If Equals() returns true for this object compared to  another
        // then GetHashCode() must return the same value for these objects.
        public override int GetHashCode()
        {
            // Get hash code for the DisplayId field if it is not null.
            int hashDisplayId = DisplayId.GetHashCode();

            // Get hash code for the Overlap field if it is not null.
            int hashOverlap = Overlap.GetHashCode();

            // Get hash code for the PixelShift field if it is not null.
            int hashPixelShift = PixelShift.GetHashCode();

            // Get hash code for the Rotation field if it is not null.
            int hashRotation = Rotation.GetHashCode();

            //Calculate the hash code for the product.
            return hashDisplayId ^ hashOverlap ^ hashPixelShift ^ hashRotation;
        }

    }

    // Custom comparer for the ProfileViewportTargetDisplay class
    class SurroundTopologyDisplayComparer : IEqualityComparer<SurroundTopologyDisplay>
    {
        // Products are equal if their names and product numbers are equal.
        public bool Equals(SurroundTopologyDisplay x, SurroundTopologyDisplay y)
        {

            //Check whether the compared objects reference the same data.
            if (Object.ReferenceEquals(x, y)) return true;

            //Check whether any of the compared objects is null.
            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            // Check whether the SurroundTopologyDisplay properties are equal
            // Two SurroundTopologyDisplay are equal only when they have the same data exactly
            if (x.DisplayId == y.DisplayId &&
                x.Overlap.Equals(y.Overlap) &&
                x.PixelShift == y.PixelShift &&
                x.Rotation == y.Rotation)
                return true;
            else
                return false;
        }

        // If Equals() returns true for a pair of objects
        // then GetHashCode() must return the same value for these objects.
        public int GetHashCode(SurroundTopologyDisplay surroundTopologyDisplay)
        {
            // Check whether the object is null
            if (Object.ReferenceEquals(surroundTopologyDisplay, null)) return 0;

            // Get hash code for the DisplayId field if it is not null.
            int hashDisplayId = surroundTopologyDisplay.DisplayId.GetHashCode();

            // Get hash code for the Overlap field if it is not null.
            int hashOverlap = surroundTopologyDisplay.Overlap.GetHashCode();

            // Get hash code for the PixelShift field if it is not null.
            int hashPixelShift = surroundTopologyDisplay.PixelShift.GetHashCode();

            // Get hash code for the Rotation field if it is not null.
            int hashRotation = surroundTopologyDisplay.Rotation.GetHashCode();

            //Calculate the hash code for the product.
            return hashDisplayId ^ hashOverlap ^ hashPixelShift ^ hashRotation;
        }

    }
}