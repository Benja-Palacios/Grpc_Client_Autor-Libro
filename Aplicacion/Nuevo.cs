using Api.Microservice.Autor.Modelo;
using Api.Microservice.Autor.Persistencia;
using FluentValidation;
using MediatR;

namespace Api.Microservice.Autor.Aplicacion
{
    //esta clase se encarga del transporte de los datos del controlador hacia la logica de mapeo
    public class Nuevo
    {
        public class Ejecuta : IRequest <int>
        {
            public string Nombre { get; set; }
            public string Apellido { get; set; }
            public DateTime? FechaNacimiento { get; set; }
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

        public class Manejador : IRequestHandler<Ejecuta, int>
        {
            public readonly ContextoAutor _context;
            public Manejador(ContextoAutor context)
            {
                _context = context;
            }

            public async Task<int> Handle(Ejecuta request, CancellationToken cancellationToken) // Y aquí
            {
                var autorLibro = new AutorLibro
                {
                    Nombre = request.Nombre,
                    Apellido = request.Apellido,
                    FechaNacimiento = request.FechaNacimiento,
                    AutorLibroGuid = Convert.ToString(Guid.NewGuid())
                };

                _context.AutorLibros.Add(autorLibro);
                await _context.SaveChangesAsync();

                return autorLibro.AutorLibroId; // Devuelve el ID en lugar de Unit.Value
            }
        }

    }
}
