using AutoMapper;
using MagicVilla_VillaAPI.Logging;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.Dto;
using MagicVilla_VillaAPI.Repository.IRepository;
using MagicVillaVillaAPI.Migrations;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace MagicVilla_VillaAPI.Controllers
{
    [ApiController]
    [Route("api/villaNumber")]
    public class VillaNumberController : ControllerBase
    {
        private APIResponse _response;
        private readonly ILogging _logging;
        private readonly IVillaNumberRepository _villaNumberRepo;
        private readonly IVillaRepository _villaRepository;
        private readonly IMapper _mapper;

        public VillaNumberController(ILogging logging, IVillaNumberRepository villaNumberRepository, IVillaRepository villaRepository, IMapper mapper)
        {
             _logging = logging;
            _villaRepository = villaRepository;
            _villaNumberRepo = villaNumberRepository;
            _mapper = mapper;
            _response = new();
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<APIResponse>> GetAll()
        {
            try
            {
                _logging.Log("Getting all villa number", "");
                IEnumerable<VillaNumber> villanumberList = await _villaNumberRepo.GetAllAsync() ;
                _response.Result = _mapper.Map<List<VillaNumberDTO>>(villanumberList);
                _response.StatusCode = System.Net.HttpStatusCode.OK;
                return Ok(_response);
            }
            catch(Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.Message };
            }
            return _response;

        }

        [HttpGet("{id:int}", Name = "GetSingleVillaNo")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<APIResponse>> GetSingleOne(int id)
        {
            try
            {
                if(id == 0)
                {
                    _logging.Log("Get single villa number error with id " + id,"error");
                    _response.IsSuccess = false;
                    _response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    _response.ErrorMessages = new List<string> { "Id can't be 0" };
                    return BadRequest(_response);
                }

                var villaNumber = await _villaNumberRepo.GetAsync(v => v.VillaNo == id);

                if(villaNumber == null)
                {
                    _logging.Log("Get single villa number error with id " + id, "error");
                    _response.IsSuccess = false;
                    _response.StatusCode = System.Net.HttpStatusCode.NotFound;
                    _response.ErrorMessages = new List<string> { "Villa number with id " + id + "doesn't exist" };
                    return NotFound(_response);
                }

                _logging.Log("Get single villa number with id: " + id, "");
                _response.Result = _mapper.Map<VillaNumberDTO>(villaNumber);
                _response.StatusCode = System.Net.HttpStatusCode.OK;
                return Ok(_response);
            }
            catch(Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.Message };
            }

            return _response;
        }


        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<APIResponse>> CreateVillaNumber([FromBody] VillaNumberCreateDTO createDTO)
        {

            try
            {

                if (await _villaNumberRepo.GetAsync(v => v.VillaNo == createDTO.VillaNo) != null)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    _response.ErrorMessages =
                        new List<string>()
                        {
                            "Villanumber already exists"
                        };
                    return BadRequest(_response);
                }

        
                if(await _villaRepository.GetAsync(v => v.Id == createDTO.VillaNo) == null)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = System.Net.HttpStatusCode.NotFound;
                    _response.ErrorMessages =
                        new List<string>()
                        {
                            "Invalid Villa"
                        };
                    return NotFound(_response);
                }

                if (createDTO == null) return BadRequest();
                VillaNumber villaNo = _mapper.Map<VillaNumber>(createDTO);


                await _villaNumberRepo.CreateAsync(villaNo);

                _response.Result = _mapper.Map<VillaNumberDTO>(villaNo);
                _response.StatusCode = System.Net.HttpStatusCode.Created;
                return CreatedAtRoute("GetVilla", new { id = villaNo.VillaNo }, _response);
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
        public async Task<ActionResult<APIResponse>> DeleteVillaNumber(int id)
        {
            try
            {
                if(id == 0)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }

                VillaNumber villaNo = await _villaNumberRepo.GetAsync(v => v.VillaNo == id);

                if ( villaNo == null)
                {
                    _response.IsSuccess = false;
                    _response.ErrorMessages = new List<string>() { "VillaNumber doesn't exist" };
                    return NotFound(_response);
                }

                await _villaNumberRepo.RemoveAsync(villaNo);
                _response.StatusCode = System.Net.HttpStatusCode.NoContent;
                return Ok(_response);


            }catch(Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages
                    = new List<string>() { ex.Message };
            }

            return _response;
        }

        [HttpPut("{id:int}",Name = "UpdateVillaNumber")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<APIResponse>> UpdateVillaNumber(int id,[FromBody] VillaNumberUpdateDTO updateDTO)
        {
            try
            {
                if (updateDTO == null | id != updateDTO.VillaNo)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    _response.ErrorMessages = new List<string>() { "Bad request" };
                    return BadRequest(_response);
                }
                if (await _villaRepository.GetAsync(v => v.Id == updateDTO.VillaNo) == null)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = System.Net.HttpStatusCode.NotFound;
                    _response.ErrorMessages =
                        new List<string>()
                        {
                            "Invalid Villa"
                        };
                    return NotFound(_response);
                }
                var villaNumber = await _villaNumberRepo.GetAsync(v => v.VillaNo == id, tracked: false);
                if (villaNumber == null)
                {
                    _response.IsSuccess = false;
                    _response.ErrorMessages = new List<string>() { "Villa doesn't exist" };
                    _response.StatusCode = System.Net.HttpStatusCode.NotFound;
                    return NotFound(_response);
                }
                VillaNumber model = _mapper.Map<VillaNumber>(updateDTO);

                await _villaNumberRepo.UpdateAsync(model);
                _response.StatusCode = System.Net.HttpStatusCode.NoContent;
                _response.IsSuccess = true;
                return Ok(_response);
            }
            catch(Exception ex) 
            {
                _response.IsSuccess = false;
                _response.ErrorMessages
                    = new List<string>() { ex.Message };
            }
            return _response;

        }



    }
}
