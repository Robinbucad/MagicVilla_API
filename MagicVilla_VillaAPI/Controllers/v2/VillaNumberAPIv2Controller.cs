using AutoMapper;
using MagicVilla_VillaAPI.Logging;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.Dto;
using MagicVilla_VillaAPI.Repository.IRepository;
using MagicVillaVillaAPI.Migrations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace MagicVilla_VillaAPI.Controllers.v2
{
    [ApiController]
    [Route("api/v{version:apiVersion}/villaNumber")]
    [ApiVersion("2.0")]
    // [ApiVersionNeutral] This will be on v1 and v2
    public class VillaNumberAPIv2Controller : ControllerBase
    {
        private APIResponse _response;
        private readonly ILogging _logging;
        private readonly IVillaNumberRepository _villaNumberRepo;
        private readonly IVillaRepository _villaRepository;
        private readonly IMapper _mapper;

        public VillaNumberAPIv2Controller(ILogging logging, IVillaNumberRepository villaNumberRepository, IVillaRepository villaRepository, IMapper mapper)
        {
            _logging = logging;
            _villaRepository = villaRepository;
            _villaNumberRepo = villaNumberRepository;
            _mapper = mapper;
            _response = new();
        }



        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "Value 1", "value 2" };
        }
    }
}
