using Api.Microservice.Autor.Modelo;
using Microsoft.EntityFrameworkCore;

namespace Api.Microservice.Autor.Persistencia
{
    public class ContextoAutor : DbContext
    {
        public ContextoAutor(DbContextOptions<ContextoAutor> options) : base(options) 
        {

        }

        public DbSet<AutorLibro> AutorLibros { get; set; }
        public DbSet<GradoAcademico> GradoAcademicos { get; set; }
    }
}
