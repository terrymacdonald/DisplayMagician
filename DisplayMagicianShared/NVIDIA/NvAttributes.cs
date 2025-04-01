using System;

namespace DisplayMagicianShared.NVIDIA
{
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Delegate)]
    internal class AcceptsAttribute : Attribute
    {
        public AcceptsAttribute(params Type[] types)
        {
            Types = types;
        }

        public Type[] Types { get; set; }
    }

    [AttributeUsage(AttributeTargets.Delegate)]
    internal class FunctionIdAttribute : Attribute
    {
        public FunctionIdAttribute(FunctionId functionId)
        {
            FunctionId = functionId;
        }

        public FunctionId FunctionId { get; set; }
    }

    [AttributeUsage(AttributeTargets.Struct)]
    internal class StructureVersionAttribute : Attribute
    {
        public StructureVersionAttribute()
        {
        }

        public StructureVersionAttribute(int versionNumber)
        {
            VersionNumber = versionNumber;
        }

        public int VersionNumber { get; set; }
    }
}
