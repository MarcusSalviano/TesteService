using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ReceiverService.Filters;

public class GlobalExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        // Loga a exceção (opcional)
        Console.WriteLine(context.Exception);

        // Cria a resposta JSON genérica
        var jsonResponse = new
        {
            Message = "Ocorreu um erro inesperado. Entre em contato com o administrador do serviço.",
            //Details = context.Exception.Message
        };

        // Configura a resposta
        context.Result = new JsonResult(jsonResponse)
        {
            StatusCode = 500 // Código de status HTTP 500 (Internal Server Error)
        };

        context.ExceptionHandled = true; // Marca a exceção como tratada
    }
}
