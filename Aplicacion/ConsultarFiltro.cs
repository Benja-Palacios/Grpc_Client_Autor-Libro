using Api.Microservice.Autor.Modelo;
using Api.Microservice.Autor.Persistencia;
using AutoMapper;
using Grpc_AutorImagen;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Api.Microservice.Autor.Aplicacion
{
    public class ConsultarFiltro
    {
        public class AutorUnico : IRequest<AutorDto>
        {
            public int AutoGuid { get; set; }
        }

        public class Manejador : IRequestHandler<AutorUnico, AutorDto>
        {
            private readonly ContextoAutor _context;
            private readonly IMapper _mapper;
            private readonly AutorImagenService.AutorImagenServiceClient _grpcClient;

            public Manejador(ContextoAutor context, IMapper mapper, AutorImagenService.AutorImagenServiceClient grpcClient)
            {
                this._context = context;
                this._mapper = mapper;
                this._grpcClient = grpcClient;
            }
            public async Task<AutorDto> Handle(AutorUnico request, CancellationToken cancellationToken)
            {
                var autor = await _context.AutorLibros.FindAsync(request.AutoGuid);
                if (autor == null)
                    throw new Exception("No se encontró el autor.");

                var autorDto = _mapper.Map<AutorLibro, AutorDto>(autor);

                // Obtener imagen asociada al autor
                var grpcRequest = new ImagenConsultaRequest
                {
                    IdAutorLibro = autor.AutorLibroId
                };
                var grpcResponse = await _grpcClient.ObtenerImagenAsync(grpcRequest);

                if (grpcResponse != null && grpcResponse.Contenido != null)
                {
                    autorDto.Imagenes = grpcResponse.Contenido.ToByteArray();
                }

                return autorDto;
            }
        }


    }
}
