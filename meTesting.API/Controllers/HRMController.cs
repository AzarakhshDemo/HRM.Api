using MediatR;
using MediatR.Wrappers;
using meTesting.Bus.SDK;
using meTesting.HRM.Services;
using meTesting.Sauron;
using meTesting.TransactionIdGenerator;
using Microsoft.AspNetCore.Mvc;
using Serilog.Context;

namespace meTesting.API.Controllers;

[ApiController]
[Route("[controller]")]
public class HRMController : ControllerBase
{


    private readonly ILogger<HRMController> _logger;
    private readonly IChartService chartService;
    private readonly IMediator m;

    public HRMController(ILogger<HRMController> logger, IChartService chartService,
        IMediator m)
    {
        _logger = logger;
        this.chartService = chartService;
        this.m = m;
    }
    [HttpGet("ReassignmentReqs")]
    public async Task<IActionResult> Get()
    {
        var t = chartService.GetAllReassignmentRequest();
        return Ok(t);
    }
    [HttpPost("Reassign")]
    public async Task<IActionResult> Get([FromBody] ReassignRequest req)
    {
        _logger.LogInformation("receive request");
        var r = await m.Send(req);
        _logger.LogInformation("request processed");

        return Ok(r);
    }
}


public class ReassignRequest : IRequest<ReassignResult>
{
    public int UserId { get; set; }
    public int PositionId { get; set; }
}
public class ReassignResult
{
}

public class ReassignHandler(IChartService chartService,
    IPersonelService personelService,
    IPersonelAttrributeService personelAttrributeService,
    TrGen trGen,
    Publisher publisher
    ) : IRequestHandler<ReassignRequest, ReassignResult>
{
    public async Task<ReassignResult> Handle(ReassignRequest request, CancellationToken cancellationToken)
    {

        var score = 0;
        var hireDate = personelAttrributeService.GetAttributeValue(request.UserId, PersonelAttributeEnum.HIRE_DATE);
        var hireDur = DateTime.Now - hireDate;

        if (hireDur > TimeSpan.FromDays(365))
            score++;

        if (score <= 0)
            return default;

        var t = new ReassignResult
        { };
        bool someOtherConditions = false;
        if (someOtherConditions)
        {
            chartService.AssignToPosition(request.UserId, request.PositionId);

            return t;
        }

        var trid = trGen.GetNewId();
        using var ctx = LogContext.PushProperty(SauronConstants.TRANSACTION_KEY, trid);

        ReassignmentRequest? tr = chartService.CreateReassignmentRequest(request.UserId, request.UserId, request.PositionId, trid);

        publisher.Publish(tr);

        return t;
    }
}
