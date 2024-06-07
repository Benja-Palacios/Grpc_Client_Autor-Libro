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
        public async Task<ActionResult<Unit>> Crear(Nuevo.Ejecuta data)
        {
            var result = await _mediator.Send(data);

            if (result.Equals(null))
            {
                return BadRequest("Error al guardar el autor o la imagen.");
            }

            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult<List<AutorDto>>> GetAutores()
        {
            var result = await _mediator.Send(new Consulta.ListaAutor());
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AutorDto>> GetAutorLibro(string id)
        {
            return await _mediator.Send(new ConsultarFiltro.AutorUnico { AutoGuid = id });
        }
    }
}
