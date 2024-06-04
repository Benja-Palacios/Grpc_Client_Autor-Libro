using Api.Microservice.Autor.Modelo;
using AutoMapper;

namespace Api.Microservice.Autor.Aplicacion
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<AutorLibro, AutorDto>();
        }
    }
}
