using Api.Microservice.Autor.Modelo;
using Api.Microservice.Autor.Persistencia;
using FluentValidation;
using Grpc_AutorImagen;
using MediatR;

namespace Api.Microservice.Autor.Aplicacion
{
    //esta clase se encarga del transporte de los datos del controlador hacia la logica de mapeo
    public class Nuevo
    {
        public class Ejecuta : IRequest 
        {
            public string Nombre { get; set; }
            public string Apellido { get; set; }
            public DateTime? FechaNacimiento { get; set; }
            public IFormFile Imagen { get; set; }
        }
        //clase para validar la clase ejecuta a traves del apifluent validator
        public class EjecutaValidacion : AbstractValidator<Ejecuta>
        {
            public EjecutaValidacion()
            {
                RuleFor(p => p.Nombre).NotEmpty();
                RuleFor(p => p.Apellido).NotEmpty();
            }
        }

        public class Manejador : IRequestHandler<Ejecuta>
        {
            public readonly ContextoAutor _context;
            private readonly AutorImagenService.AutorImagenServiceClient _grpcClient;

            public Manejador(ContextoAutor context, AutorImagenService.AutorImagenServiceClient grpcClient)
            {
                _context = context;
                _grpcClient = grpcClient;
            }

            public async Task<Unit> Handle(Ejecuta request, CancellationToken cancellationToken)
            {

                using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

                try
                {
                    //Guardar el Autor
                    var autorLibro = new AutorLibro
                    {
                        Nombre = request.Nombre,
                        Apellido = request.Apellido,
                        FechaNacimiento = request.FechaNacimiento,
                        AutorLibroGuid = Convert.ToString(Guid.NewGuid())
                    };
                    _context.AutorLibros.Add(autorLibro);
                    var respuesta = await _context.SaveChangesAsync();

                    if (respuesta > 0)
                    {
                        if (!request.Imagen.Equals(null))
                        {
                            using var ms = new MemoryStream();
                            await request.Imagen.CopyToAsync(ms, cancellationToken);
                            var imagenBytes = ms.ToArray();

                            var grpcRequest = new ImagenRequest
                            {
                                Contenido = Google.Protobuf.ByteString.CopyFrom(imagenBytes),
                                IdAutorLibro = autorLibro.AutorLibroGuid
                            };
                            var grpcResponse = await _grpcClient.GuardarImagenAsync(grpcRequest);

                            if (!grpcResponse.Mensaje.Equals("Imagen guardada correctamente."))
                            {
                                throw new Exception("Error al guardar la imagen.");
                            }
                        }
                        await transaction.CommitAsync(cancellationToken);
                        return Unit.Value;
                    }
                    throw new Exception("No se puede insertar el Autor del Libro");

                }
                catch (Exception ex) {
                    await transaction.RollbackAsync(cancellationToken);
                    throw;
                }
            }
        }

    }
}
