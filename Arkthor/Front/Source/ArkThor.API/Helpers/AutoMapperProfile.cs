namespace ArkThor.API.Helpers;

using AutoMapper;
using ArkThor.API.Entities;
using ArkThor.API.Models.Records;


public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        // CreateRequest -> User
      
        // CreateFileRecord -> FileRecord
        CreateMap<CreateFileRecord, FileRecord>();

     
    }
}