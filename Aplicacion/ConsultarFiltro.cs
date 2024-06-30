using Api.Microservice.Autor.Modelo;
using Api.Microservice.Autor.Persistencia;
using AutoMapper;
using Grpc_AutorImagen;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Polly;

namespace Api.Microservice.Autor.Aplicacion
{
    public class ConsultarFiltro
    {
        public class AutorUnico : IRequest<AutorDto>
        {
            public string AutoGuid { get; set; }
        }

        public class Manejador : IRequestHandler<AutorUnico, AutorDto>
        {
            private readonly ContextoAutor _context;
            private readonly IMapper _mapper;
            private readonly AutorImagenService.AutorImagenServiceClient _grpcClient;
            private readonly IAsyncPolicy<ImagenResponse> _fallbackPolicy;

            public Manejador(ContextoAutor context, IMapper mapper, AutorImagenService.AutorImagenServiceClient grpcClient, IAsyncPolicy<ImagenResponse> fallbackPolicy)
            {
                _context = context;
                _mapper = mapper;
                _grpcClient = grpcClient;
                _fallbackPolicy = fallbackPolicy;
            }

            public async Task<AutorDto> Handle(AutorUnico request, CancellationToken cancellationToken)
            {
                var autor = await _context.AutorLibros
                                             .FirstOrDefaultAsync(a => a.AutorLibroGuid == request.AutoGuid, cancellationToken);
                if (autor == null)
                    throw new Exception("No se encontró el autor.");

                var autorDto = _mapper.Map<AutorLibro, AutorDto>(autor);

                var grpcRequest = new ImagenConsultaRequest
                {
                    IdAutorLibro = autor.AutorLibroGuid
                };

                try
                {
                    var grpcResponse = await _fallbackPolicy.ExecuteAsync(async () =>
                    {
                        return await _grpcClient.ObtenerImagenAsync(grpcRequest);
                    });

                    if (grpcResponse != null && grpcResponse.Contenido != null)
                    {
                        autorDto.Imagenes = grpcResponse.Contenido.ToByteArray();
                    }
                }
                catch (Exception ex)
                {
                    // Log the error or handle it accordingly
                    // Console.WriteLine($"Error fetching image for {autor.AutorLibroGuid}: {ex.Message}");
                }

                return autorDto;
            }
        }
    }
}
