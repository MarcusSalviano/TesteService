using Newtonsoft.Json;

namespace ReceiverService.Models.Dtos;

internal class ReceiverMessageDto
{
    public string Id { get; set; }
    public string Field1 { get; set; }
    public string Field2 { get; set; }
    public string Field3 { get; set; }
    public string Field4 { get; set; }
}