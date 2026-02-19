using FluentAssertions;
using Resumetry.Application.Services;
using Resumetry.Domain.Enums;
using Xunit;

namespace Resumetry.Application.Tests.Services;

public class StatusStateEngineTests
{
    #region Null / Empty

    [Fact]
    public void AvailableStatuses_WithNullCollection_ReturnsApplied()
    {
        var result = StatusStateEngine.AvailableStatuses(null!);

        result.Should().BeEquivalentTo([StatusEnum.Applied]);
    }

    [Fact]
    public void AvailableStatuses_WithEmptyCollection_ReturnsApplied()
    {
        var result = StatusStateEngine.AvailableStatuses([]);

        result.Should().BeEquivalentTo([StatusEnum.Applied]);
    }

    #endregion

    #region Applied branch

    [Fact]
    public void AvailableStatuses_WithAppliedOnly_ReturnsRejectedAndScreen()
    {
        var result = StatusStateEngine.AvailableStatuses([StatusEnum.Applied]);

        result.Should().BeEquivalentTo([StatusEnum.Rejected, StatusEnum.Screen]);
    }

    [Fact]
    public void AvailableStatuses_WithAppliedAndRejected_ReturnsEmpty()
    {
        var result = StatusStateEngine.AvailableStatuses([StatusEnum.Applied, StatusEnum.Rejected]);

        result.Should().BeEmpty();
    }

    [Fact]
    public void AvailableStatuses_WithAppliedAndScreen_ReturnsInterview()
    {
        // Screen branch fires before Applied branch; Screen has no Interview yet
        var result = StatusStateEngine.AvailableStatuses([StatusEnum.Applied, StatusEnum.Screen]);

        result.Should().BeEquivalentTo([StatusEnum.Interview]);
    }

    #endregion

    #region Screen branch

    [Fact]
    public void AvailableStatuses_WithScreenOnly_ReturnsInterview()
    {
        var result = StatusStateEngine.AvailableStatuses([StatusEnum.Screen]);

        result.Should().BeEquivalentTo([StatusEnum.Interview]);
    }

    [Fact]
    public void AvailableStatuses_WithScreenAndInterview_ReturnsOfferNoOfferWithdrawn()
    {
        // Interview branch fires first; no terminal statuses present yet
        var result = StatusStateEngine.AvailableStatuses([StatusEnum.Screen, StatusEnum.Interview]);

        result.Should().BeEquivalentTo([StatusEnum.Offer, StatusEnum.NoOffer, StatusEnum.Withdrawn]);
    }

    #endregion

    #region Interview branch

    [Fact]
    public void AvailableStatuses_WithInterviewOnly_ReturnsOfferNoOfferWithdrawn()
    {
        var result = StatusStateEngine.AvailableStatuses([StatusEnum.Interview]);

        result.Should().BeEquivalentTo([StatusEnum.Offer, StatusEnum.NoOffer, StatusEnum.Withdrawn]);
    }

    [Fact]
    public void AvailableStatuses_WithInterviewAndOffer_ReturnsEmpty()
    {
        var result = StatusStateEngine.AvailableStatuses([StatusEnum.Interview, StatusEnum.Offer]);

        result.Should().BeEmpty();
    }

    [Fact]
    public void AvailableStatuses_WithInterviewAndNoOffer_ReturnsEmpty()
    {
        var result = StatusStateEngine.AvailableStatuses([StatusEnum.Interview, StatusEnum.NoOffer]);

        result.Should().BeEmpty();
    }

    [Fact]
    public void AvailableStatuses_WithInterviewAndWithdrawn_ReturnsEmpty()
    {
        var result = StatusStateEngine.AvailableStatuses([StatusEnum.Interview, StatusEnum.Withdrawn]);

        result.Should().BeEmpty();
    }

    #endregion

    #region No matching branch

    [Fact]
    public void AvailableStatuses_WithOnlyTerminalStatus_ReturnsEmpty()
    {
        var result = StatusStateEngine.AvailableStatuses([StatusEnum.Offer]);

        result.Should().BeEmpty();
    }

    [Fact]
    public void AvailableStatuses_WithRejectedOnly_ReturnsEmpty()
    {
        var result = StatusStateEngine.AvailableStatuses([StatusEnum.Rejected]);

        result.Should().BeEmpty();
    }

    #endregion
}
