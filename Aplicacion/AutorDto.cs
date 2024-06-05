namespace Api.Microservice.Autor.Aplicacion
{
    public class AutorDto
    {
        public int AutorLibroId { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public IFormFile Imagen { get; set; }
        public byte[] Imagenes { get; set; }

    }
}
