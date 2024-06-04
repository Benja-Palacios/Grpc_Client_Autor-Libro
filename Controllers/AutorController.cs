using Api.Microservice.Autor.Aplicacion;
using Grpc_AutorImagen;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Microservice.Autor.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AutorController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly AutorImagenService.AutorImagenServiceClient _grpcClient;

        public AutorController(IMediator mediator, AutorImagenService.AutorImagenServiceClient grpcClient)
        {
            this._mediator = mediator;
            this._grpcClient = grpcClient;
        }

        [HttpPost]
        public async Task<ActionResult<Unit>> Crear([FromForm] AutorDto data)
        {
            var idAutor = await _mediator.Send(new Nuevo.Ejecuta
            {
                Nombre = data.Nombre,
                Apellido = data.Apellido,
                FechaNacimiento = data.FechaNacimiento
            });

            if (!data.Imagen.Equals(null)) { 
                using var ms = new MemoryStream();
                await data.Imagen.CopyToAsync(ms);
                var imagenBytes = ms.ToArray();

                var grpcRequest = new ImagenRequest
                {
                    Contenido = Google.Protobuf.ByteString.CopyFrom(imagenBytes),
                    IdAutorLibro = idAutor
                };

                var grpcResponse = await _grpcClient.GuardarImagenAsync(grpcRequest);
                if (!grpcResponse.Mensaje.Equals("Imagen guardada correctamente.")) {
                    return BadRequest("Error al guardar la imagen.");
                }
            }
            return Ok();
        }

        [HttpGet]
        public async Task<ActionResult<List<AutorDto>>> GetAutores()
        {
            return await _mediator.Send(new Consulta.ListaAutor());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AutorDto>> GetAutorLibro(string id)
        {
            return await _mediator.Send(new ConsultarFiltro.AutorUnico { AutoGuid = id });
        }
    }
}
