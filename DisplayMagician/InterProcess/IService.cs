using System.ServiceModel;

namespace DisplayMagician.InterProcess
{
    [ServiceContract]
    internal interface IService
    {
        int HoldProcessId { [OperationContract] get; }
        InstanceStatus Status { [OperationContract] get; }

        [OperationContract(IsOneWay = true)]
        void StopHold();
    }
}