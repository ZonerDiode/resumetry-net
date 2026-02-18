namespace Resumetry.Domain.Enums
{
    /*
     * Expected flow of a job application:
     * 
     * Applied -> Rejected, Screen
     * 
     * Screen -> Interview 
     * 
     * Interview -> Offer, NoOffer, Withdrawn
     */
    public enum StatusEnum
    {
        Applied,
        Rejected,
        Screen,
        Interview,
        Offer,
        Withdrawn,
        NoOffer
    }
}