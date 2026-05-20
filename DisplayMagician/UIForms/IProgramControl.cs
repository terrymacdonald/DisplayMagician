using System;

namespace DisplayMagician.UIForms
{
    public interface IProgramControl
    {
        int ProgramNumber { get; set; }
        void ChangePriority(int priority);
    }
}
