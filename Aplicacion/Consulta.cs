using Api.Microservice.Autor.Modelo;
using Api.Microservice.Autor.Persistencia;
using AutoMapper;
using Grpc_AutorImagen;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Polly;

namespace Api.Microservice.Autor.Aplicacion
{
    public class Consulta
    {
        public class ListaAutor : IRequest<List<AutorDto>>
        {
        }

        public class Manejador : IRequestHandler<ListaAutor, List<AutorDto>>
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

            public async Task<List<AutorDto>> Handle(ListaAutor request, CancellationToken cancellationToken)
            {
                var autores = await _context.AutorLibros.ToListAsync(cancellationToken);
                var autoresDto = _mapper.Map<List<AutorLibro>, List<AutorDto>>(autores);

                foreach (var autor in autoresDto)
                {
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
                            autor.Imagenes = grpcResponse.Contenido.ToByteArray();
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log the error or handle it accordingly
                        // Console.WriteLine($"Error fetching image for {autor.AutorLibroGuid}: {ex.Message}");
                    }
                }
                return autoresDto;
            }
        }
    }


}
