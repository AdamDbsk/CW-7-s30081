using Biuro_Podróży.AppHost.Models.DTOs;
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
public class ClientsController(IDataBaseService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetClients() {
        return Ok(await service.GetAllClients());
    }
    [HttpGet("{id}/trips")]
    public async Task<IActionResult> GetClientTrips([FromRoute] int id) {
        try {
            return Ok(await service.GetClientTripsAsync(id));
        } catch(NotFoundException e) {
            return NotFound(e.Message);
        }
    }
    [HttpPost]
    public async Task<IActionResult> AddClient([FromBody] ClientCreateDTO clientDTO) {
        try {
            var client = await service.CreateClientAsync(clientDTO);
            return Created($"clients/{client.ClientID}", client);
        } catch (NotFoundException e) {
            return NotFound(e.Message);
        }
    }
    [HttpPut("{clientID}/trips/{tripID}")]
    public async Task<IActionResult> RegisterClientToTrip([FromRoute] int clientID, [FromRoute]int tripID) {
        try {
            await service.RegisterClientTrip(clientID, tripID);
            return NoContent();
        } catch (NotFoundException e) {
            return NotFound(e.Message);
        }
    }
    [HttpDelete("{clientID}/trips/{tripID}")]
    public async Task<IActionResult> RemoveRegistration([FromRoute] int clientID, [FromRoute] int tripID) {
        try {
            await service.RemoveRegistresionForTrip(clientID, tripID);
            return NoContent();
        } catch (NotFoundException e) {
            return NotFound(e.Message);
        }
    }
}
