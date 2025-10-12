using FYP2025.Application.DTOs;
using FYP2025.Domain.Entities;
using FYP2025.Domain.Repositories;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using FYP2025.Domain.Enums;

namespace FYP2025.Api.Features.Categories
{
    [ApiController]
    [Route("api/categories")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public CategoriesController(ICategoryRepository categoryRepository, IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        // GET api/categories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories()
        {
            var categories = await _categoryRepository.GetAllAsync();
            var categoryDtos = _mapper.Map<IEnumerable<CategoryDto>>(categories);
            return Ok(categoryDtos);
        }

        // GET api/categories/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryDto>> GetCategory(string id) 
        {
            var category = await _categoryRepository.GetByIdAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            var categoryDto = _mapper.Map<CategoryDto>(category);
            return Ok(categoryDto);
        }

        // POST api/categories
        [HttpPost]
        public async Task<ActionResult<CategoryDto>> CreateCategory([FromBody] CreateCategoryDto createCategoryDto)
        {
            var category = _mapper.Map<Category>(createCategoryDto);
            category.Id = Guid.NewGuid().ToString(); 
            await _categoryRepository.AddAsync(category);

            var categoryDto = _mapper.Map<CategoryDto>(category);
            return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, categoryDto);
        }

        // PUT api/categories/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(string id, [FromBody] UpdateCategoryDto updateCategoryDto) 
        {
            if (!await _categoryRepository.ExistsAsync(id))
            {
                return NotFound();
            }

            var category = _mapper.Map<Category>(updateCategoryDto);
            category.Id = id; 

            await _categoryRepository.UpdateAsync(category);

            return NoContent();
        }

        // DELETE api/categories/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(string id) 
        {
            if (!await _categoryRepository.ExistsAsync(id))
            {
                return NotFound();
            }

            await _categoryRepository.DeleteAsync(id);
            return NoContent();
        }

        [HttpGet("main/{mainCategory}")]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetByMainCategory(MainCategoryType mainCategory)
        {
            var categories = await _categoryRepository.GetByMainCategoryAsync(mainCategory);
            var dtos = _mapper.Map<IEnumerable<CategoryDto>>(categories);
            return Ok(dtos);
        }

    }
}
