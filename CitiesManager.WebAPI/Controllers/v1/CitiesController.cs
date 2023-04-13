using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CitiesManager.Infrastructure.DatabaseContext;
using CitiesManager.Core.Models;
using Microsoft.AspNetCore.Authorization;

namespace CitiesManager.WebAPI.Controllers.v1
{
    /// <summary>
    /// Version 1 of Cities Controller
    /// </summary>
    //[Authorize] //Localized Requirement of Authorization Policy
    [ApiVersion("1.0")]
    //[EnableCors("4100Client")]
    public class CitiesController : CustomControllerBase
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initialize the ApplicationDbContext
        /// </summary>
        /// <param name="context"></param>
        public CitiesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Cities
        /// <summary>
        /// To Get list of cities (including city ID and city Name) frrom 'cities' table
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        //[Produces("application/xml")]
        public async Task<ActionResult<IEnumerable<City>>> GetCities()
        {
            if (_context.Cities == null)
            {
                return NotFound();
            }
            return await _context.Cities.OrderBy(x => x.CityName).ToListAsync();
        }

        /// <summary>
        /// Fetches the city using the city ID
        /// </summary>
        /// <param name="cityID"></param>
        /// <returns></returns>
        // GET: api/Cities/5
        [HttpGet("{cityID}")]
        public async Task<ActionResult<City>> GetCity(Guid cityID)
        {
            if (_context.Cities == null)
            {
                return NotFound();
            }
            var city = await _context.Cities.FirstOrDefaultAsync(x => x.CityID == cityID);

            if (city == null)
            {
                return Problem(detail: "Invalid City ID",
                               statusCode: 400,
                               title: "City Search");
            }

            return city;
        }

        /// <summary>
        /// Updates the city name
        /// </summary>
        /// <param name="cityID"></param>
        /// <param name="city"></param>
        /// <returns></returns>
        // PUT: api/Cities/5
        [HttpPut("{cityID}")]
        public async Task<IActionResult> PutCity(Guid cityID, [Bind(nameof(City.CityID), nameof(City.CityName))] City city)
        {
            if (cityID != city.CityID)
            {
                return BadRequest();
            }

            var existitingCity = await _context.Cities.FindAsync(cityID);
            if (existitingCity == null)
            {
                return NotFound();
            }

            existitingCity.CityName = city.CityName;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CityExists(cityID))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        /// <summary>
        /// Adds city to database
        /// </summary>
        /// <param name="city"></param>
        /// <returns></returns>
        // POST: api/Cities
        [HttpPost]
        public async Task<ActionResult<City>> PostCity([Bind(nameof(City.CityID), nameof(City.CityName))] City city)
        {
            if (_context.Cities == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Citys'  is null.");
            }
            _context.Cities.Add(city);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCity", new { cityID = city.CityID }, city);
        }

        /// <summary>
        /// Deletes City from the database using the city ID
        /// </summary>
        /// <param name="cityID"></param>
        /// <returns></returns>
        // DELETE: api/Cities/5
        [HttpDelete("{cityID}")]
        public async Task<IActionResult> DeleteCity(Guid cityID)
        {
            if (_context.Cities == null)
            {
                return NotFound();
            }
            var city = await _context.Cities.FindAsync(cityID);
            if (city == null)
            {
                return NotFound();
            }

            _context.Cities.Remove(city);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CityExists(Guid cityID)
        {
            return (_context.Cities?.Any(e => e.CityID == cityID)).GetValueOrDefault();
        }
    }
}
