using Microsoft.AspNetCore.Mvc.Filters;
using System.Diagnostics;

public class ExecutionTimeActionFilter : IActionFilter
{
    private Stopwatch _stopwatch;

    public void OnActionExecuting(ActionExecutingContext context)
    {
        // Inicia o cronômetro antes da execução da ação
        _stopwatch = Stopwatch.StartNew();
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // Para o cronômetro após a execução da ação
        _stopwatch.Stop();

        // Loga a duração da execução da ação
        var actionName = context.ActionDescriptor.DisplayName;
        var elapsedTime = _stopwatch.Elapsed;
        Console.WriteLine($"Ação '{actionName}' executada em {elapsedTime.TotalMilliseconds} ms.");
    }
}
