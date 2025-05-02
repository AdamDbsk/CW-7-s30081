
using Biuro_Podróży.AppHost.Sevices;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biuro_Podróży.AppHost.Controllers;
[ApiController]
[Route("[controller]")]
public class TripsController(IDataBaseService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllTrips() {
        return Ok(await service.GetAllTripsWithCountriesAsync());
    }
}
