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
        APPLIED,
        REJECTED,
        SCREEN,
        INTERVIEW,
        OFFER,
        WITHDRAWN,
        NOOFFER
    }
}