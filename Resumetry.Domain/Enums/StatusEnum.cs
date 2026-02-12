namespace Resumetry.Domain.Enums
{
    /*
     * Expected flow of a job application:
     * 
     * Applied -> Rejected
     * Applied -> Screen -> Interview 
     *      Interview -> Offer
     *      Interview -> NoOffer
     *      Interview -> Withdrawn
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