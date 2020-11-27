namespace Moca.Domain
{
    public enum RequestStatus
    {
        PENDING =0,
        ACCEPTED,
        REJECTED
    }

    public enum ReparationStatus
    {
        DEFECTIVE =0,
        IN_SERVICE,
        REPAIRED,
        BEYOND_REPAIR
    }
}
