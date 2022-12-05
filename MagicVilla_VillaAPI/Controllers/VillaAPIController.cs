using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Logging;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.Dto;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace MagicVilla_VillaAPI.Controllers
{
    [ApiController]
    [Route("api/villaAPI")]
    public class VillaAPIController : ControllerBase
    {
        private readonly ILogging _logger;
        public VillaAPIController(ILogging logger)
        {
          _logger= logger;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<VillaDTO>> GetVillas()
        {
            _logger.Log("Getting all villas","");
            return Ok(VillaStore.villaList);
        }

        [HttpGet("{id:int}", Name = "GetVilla")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<VillaDTO> GetVilla(int id)
        {
            if(id == 0)
            {
                _logger.Log("Get villa error with Id " + id, "error");
                return BadRequest();
            }
            var villa = VillaStore.villaList.FirstOrDefault(u => u.Id == id);
            if(villa == null)
            {
                return NotFound();
            }

            return Ok(villa);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<VillaDTO> CreateVilla([FromBody]VillaDTO villaDTO)
        {
            //if (!ModelState.IsValid)return BadRequest(ModelState);

            if(VillaStore.villaList.FirstOrDefault(v => v.Name.ToLower() == villaDTO.Name.ToLower()) != null)
            {
                ModelState.AddModelError("CustomError", "Villa already exists");
                return BadRequest(ModelState);
            }

            if (villaDTO == null) return BadRequest();
            if (villaDTO.Id > 0) return StatusCode(StatusCodes.Status500InternalServerError);

            villaDTO.Id= VillaStore.villaList.OrderByDescending(v => v.Id).FirstOrDefault().Id+1;
            VillaStore.villaList.Add(villaDTO);
            return CreatedAtRoute("GetVilla",new {id = villaDTO.Id} ,villaDTO);

        }


        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult DeletVilla(int id)
        {
            if(id == 0) return BadRequest();
            
            var villa = VillaStore.villaList.FirstOrDefault(v => v.Id == id);
            if(villa == null) return NotFound();

            VillaStore.villaList.Remove(villa);
            return NoContent();
        }

        [HttpPut("{id:int}", Name = "UpdateVilla")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult UpdateVilla(int id,[FromBody] VillaDTO villaDTO)
        {
            if(villaDTO == null || id != villaDTO.Id) return BadRequest();
            
            var villa = VillaStore.villaList.FirstOrDefault(v => v.Id == villaDTO.Id);
            if(villa == null) return NotFound();
            villa.Name = villaDTO.Name;
            villa.Sqft= villaDTO.Sqft;
            villa.Occupancy = villaDTO.Occupancy;

            return Ok(villa);
        }

        [HttpPatch("{id:int}", Name = "UpdatePartialVilla")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult UpdatePartialVilla(int id, JsonPatchDocument<VillaDTO> patchDTO)
        {
            if(patchDTO == null || id == 0)return BadRequest();

            var villa = VillaStore.villaList.FirstOrDefault(v => v.Id ==id);
            if(villa == null) return NotFound();

            patchDTO.ApplyTo(villa, ModelState);

            if(!ModelState.IsValid) return BadRequest(ModelState);

            return NoContent();
        }

    }
}
