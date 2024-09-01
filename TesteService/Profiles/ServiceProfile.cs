using AutoMapper;
using ReceiverService.Models;
using ReceiverService.Models.Dtos;

namespace ReceiverService.Profiles;

public class ServiceProfile : Profile
{
    public ServiceProfile()
    {
        CreateMap<Receiver, ReceiverMessageDto>();
    }
}
