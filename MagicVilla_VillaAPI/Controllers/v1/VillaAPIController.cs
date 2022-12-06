using AutoMapper;
using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Logging;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.Dto;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace MagicVilla_VillaAPI.Controllers.v1
{
    [ApiController]
    [Route("api/v{version:apiVersion}/villaAPI")]
    [ApiVersion("1.0")]
    public class VillaAPIController : ControllerBase
    {
        protected APIResponse _response;
        private readonly ILogging _logger;
        private readonly IVillaRepository _dbVilla;
        private readonly IMapper _mapper;
        public VillaAPIController(ILogging logger, IVillaRepository dbVilla, IMapper mapper)
        {
            _logger = logger;
            _dbVilla = dbVilla;
            _mapper = mapper;
            _response = new APIResponse();
        }

        [HttpGet]
        [ResponseCache(CacheProfileName = "Default30")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<APIResponse>> GetVillas([FromQuery(Name = "filterOccupancy")] int? occupancy,
            [FromQuery(Name = "search")] string? search, int pageSize = 0, int pageNumber = 1)
        {

            try
            {
                _logger.Log("Getting all villas", "");
                IEnumerable<Villa> villaList;
                    
                if (occupancy > 0)
                {
                    villaList = await _dbVilla.GetAllAsync(u => u.Occupancy == occupancy, pageSize:pageSize,
                        pageNumber:pageNumber);
                }
                else
                {
                    villaList = await _dbVilla.GetAllAsync(pageSize: pageSize,
                        pageNumber: pageNumber);
                }

                if (!string.IsNullOrEmpty(search))
                {
                    villaList = villaList.Where(u => u.Amenity.ToLower().Contains(search)
                    || u.Name.ToLower().Contains(search));
                }
                Pagination pagination = new() { PageNumber= pageNumber, PageSize = pageSize };

                Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(pageSize));

                _response.Result = _mapper.Map<List<VillaDTO>>(villaList);
                _response.StatusCode = System.Net.HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages
                    = new List<string>() { ex.Message };
            }
            return _response;

        }


        [HttpGet("{id:int}", Name = "GetVilla")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ResponseCache(Duration = 30)]
        public async Task<ActionResult<APIResponse>> GetVilla(int id)
        {
            try
            {
                if (id == 0)
                {
                    _logger.Log("Get villa error with Id " + id, "error");
                    _response.StatusCode = System.Net.HttpStatusCode.BadRequest;

                    return BadRequest(_response);
                }

                var villa = await _dbVilla.GetAsync(u => u.Id == id);
                if (villa == null)
                {
                    _response.StatusCode = System.Net.HttpStatusCode.NotFound;
                    _response.IsSuccess = false;
                    return NotFound(_response);
                }
                _response.Result = _mapper.Map<VillaDTO>(villa);
                _response.StatusCode = System.Net.HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages
                    = new List<string>() { ex.Message };
            }
            return _response;
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<APIResponse>> CreateVilla([FromBody] VillaCreateDTO createDTO)
        {
            //if (!ModelState.IsValid)return BadRequest(ModelState);

            try
            {

                if (await _dbVilla.GetAsync(v => v.Name.ToLower() == createDTO.Name.ToLower()) != null)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    _response.ErrorMessages =
                        new List<string>()
                        {
                            "Villa already exists"
                        };
                    return BadRequest(_response);
                }

                if (createDTO == null) return BadRequest();
                Villa villa = _mapper.Map<Villa>(createDTO);


                await _dbVilla.CreateAsync(villa);

                _response.Result = _mapper.Map<VillaDTO>(villa);
                _response.StatusCode = System.Net.HttpStatusCode.Created;
                return CreatedAtRoute("GetVilla", new { id = villa.Id }, _response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages
                    = new List<string>() { ex.Message };
            }
            return _response;
        }


        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<APIResponse>> DeleteVilla(int id)
        {
            try
            {


                if (id == 0)
                {
                    _response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    return BadRequest();
                }

                var villa = await _dbVilla.GetAsync(v => v.Id == id);
                if (villa == null)
                {
                    _response.StatusCode = System.Net.HttpStatusCode.NotFound;
                    return NotFound(_response);
                }

                await _dbVilla.RemoveAsync(villa);
                _response.StatusCode = System.Net.HttpStatusCode.NoContent;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages
                    = new List<string>() { ex.Message };
            }
            return _response;
        }

        [HttpPut("{id:int}", Name = "UpdateVilla")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<APIResponse>> UpdateVilla(int id, [FromBody] VillaUpdateDTO updateDTO)
        {
            try
            {
                if (updateDTO == null | id != updateDTO.Id)
                {
                    _response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }
                var villa = await _dbVilla.GetAsync(v => v.Id == id, tracked: false);
                if (villa == null)
                {
                    _response.StatusCode = System.Net.HttpStatusCode.NotFound;
                    _response.ErrorMessages = new List<string>()
                    {
                        "Villa doesn't exist"
                    };
                    return NotFound(_response);
                }

                Villa model = _mapper.Map<Villa>(updateDTO);



                await _dbVilla.UpdateAsync(model);
                _response.StatusCode = System.Net.HttpStatusCode.NoContent;
                _response.IsSuccess = true;


                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages
                    = new List<string>() { ex.Message };
            }
            return _response;
        }

        // EXAMPLE PATCH
        [HttpPatch("{id:int}", Name = "UpdatePartialVilla")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdatePartialVilla(int id, JsonPatchDocument<VillaUpdateDTO> updateDTO)
        {
            if (updateDTO == null || id == 0) return BadRequest();


            var villa = await _dbVilla.GetAsync(v => v.Id == id, tracked: false);
            if (villa == null) return NotFound();

            VillaUpdateDTO villaDTO = _mapper.Map<VillaUpdateDTO>(villa);



            if (villa == null) return NotFound();


            updateDTO.ApplyTo(villaDTO, ModelState);

            Villa model = _mapper.Map<Villa>(villaDTO);

            await _dbVilla.UpdateAsync(model);
            await _dbVilla.SaveAsync();

            if (!ModelState.IsValid) return BadRequest(ModelState);

            return NoContent();
        }

    }
}
