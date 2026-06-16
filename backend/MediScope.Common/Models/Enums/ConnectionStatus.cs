// File: MediScope.Common/Models/Enums/ConnectionStatus.cs
namespace MediScope.Common.Models.Enums
{
    public enum ConnectionStatus
    {
        PendingAdmin,
        PendingDoctor,
        Active,
        DeclinedDoctor,
        RejectedAdmin,
        Revoked
    }
}