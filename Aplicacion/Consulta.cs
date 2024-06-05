using Api.Microservice.Autor.Modelo;
using Api.Microservice.Autor.Persistencia;
using AutoMapper;
using Grpc_AutorImagen;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Api.Microservice.Autor.Aplicacion
{
    public class Consulta
    {
        public class ListaAutor : IRequest<List<AutorDto>>
        {

        }
        //recibe ListaAutor y regresa una lista de AutorLibro
        public class Manejador : IRequestHandler<ListaAutor, List<AutorDto>>
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
            public async Task<List<AutorDto>> Handle(ListaAutor request, CancellationToken cancellationToken)
            {
                var autores = await _context.AutorLibros.ToListAsync(cancellationToken);
                var autoresDto = _mapper.Map<List<AutorLibro>, List<AutorDto>>(autores);

                foreach (var autor in autoresDto)
                {
                    var grpcRequest = new ImagenConsultaRequest
                    {
                        IdAutorLibro = autor.AutorLibroId
                    };

                    var grpcResponse = await _grpcClient.ObtenerImagenAsync(grpcRequest);

                    if (grpcResponse != null && grpcResponse.Contenido != null)
                    {
                        autor.Imagenes = grpcResponse.Contenido.ToByteArray();
                    }
                }
                return autoresDto;
            }
        }
    }
}
