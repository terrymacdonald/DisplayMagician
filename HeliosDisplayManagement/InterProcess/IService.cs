using System.ServiceModel;

namespace HeliosDisplayManagement.InterProcess
{
    [ServiceContract]
    internal interface IService
    {
        InstanceStatus Status { [OperationContract] get; }

        int HoldProcessId { [OperationContract] get; }

        [OperationContract(IsOneWay = true)]
        void StopHold();
    }
}