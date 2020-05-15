using System.ServiceModel;

namespace HeliosPlus.InterProcess
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