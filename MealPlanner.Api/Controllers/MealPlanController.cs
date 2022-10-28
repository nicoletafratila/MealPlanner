using AutoMapper;
using MealPlanner.Api.Repositories;
using MealPlanner.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace MealPlanner.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MealPlanController : ControllerBase
    {
        private readonly IMealPlanRepository _repository;
        private readonly IMapper _mapper;
        private readonly LinkGenerator _linkGenerator;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MealPlanController(IMealPlanRepository repository, IMapper mapper, LinkGenerator linkGenerator,
            IWebHostEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _mapper = mapper;
            _linkGenerator = linkGenerator;
            _webHostEnvironment = webHostEnvironment;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MealPlanModel>>> Get()
        {
            try
            {
                var results = await _repository.GetAllAsync();

                return StatusCode(StatusCodes.Status200OK, _mapper.Map<IEnumerable<MealPlanModel>>(results));
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EditMealPlanModel>> Get(int id)
        {
            try
            {
                var result = await _repository.GetByIdAsync(id);

                if (result == null) return NotFound();

                return StatusCode(StatusCodes.Status200OK, _mapper.Map<EditMealPlanModel>(result));
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }
    }
}