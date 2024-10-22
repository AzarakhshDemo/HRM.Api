using MediatR;
using meTesting.Aether.SDK;
using meTesting.Automation;
using meTesting.Chart;
using meTesting.HRM.Services;

namespace meTesting.HRM.API.Event;

public class LetterStateChangeHandler(IChartService chartService,
    ILogger<LetterStateChangeHandler> logger,
    NotifSender notifSender) : IRequestHandler<LetterStateChangeArgs>
{
    public async Task Handle(LetterStateChangeArgs request, CancellationToken cancellationToken)
    {
        logger.LogInformation($"{GetType().Name} Fired");

        var req = chartService.GetReassignmentRequest(request.TransactionId);

        if (request.NewState == LetterState.Signed)
        {
            req.State = ReassignmentRequest.ReassignmentRequestStateEnum.Approved;
            chartService.AssignToPosition(req.UserId, req.PositionId);

            await notifSender.Send(req.CreatorId.ToString(), $"Your reassignment request has been Approved");
        }
        if (request.NewState == LetterState.Rejected)
        {
            req.State = ReassignmentRequest.ReassignmentRequestStateEnum.Rejected;

            await notifSender.Send(req.CreatorId.ToString(), $"Your reassignment request has been rejected");
        }
    }
}

public class LetterStateChangeArgs : IRequest
{
    public string TransactionId { get; set; }
    public LetterState NewState { get; set; }
}
