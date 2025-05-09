﻿using Biuro_Podróży.AppHost.Models.DTOs;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using System.Diagnostics;
using Biuro_Podróży.AppHost.Models;
using System.Reflection.PortableExecutable;

namespace Biuro_Podróży.AppHost.Sevices;

public interface IDataBaseService 
{
    public Task<IEnumerable<ClientGetDTO>> GetAllClients();
    public Task<IEnumerable<TripWithCountryGetDTO>> GetAllTripsWithCountriesAsync();
    public Task<IEnumerable<TripWithRegisterationGetDTO>> GetClientTripsAsync(int id);
    public Task<ClientModel> CreateClientAsync(ClientCreateDTO client);
    public Task RegisterClientTrip(int clientID,int tripID);
    public Task RemoveRegistresionForTrip(int clientID, int tripID);
}
public class DataBaseService(IConfiguration config) : IDataBaseService {
    
    private readonly string? connectionString = config.GetConnectionString("Default");

    public async Task<IEnumerable<TripWithCountryGetDTO>> GetAllTripsWithCountriesAsync() {
        List<TripWithCountryGetDTO> result = new List<TripWithCountryGetDTO>();

        await using SqlConnection connection = new SqlConnection(connectionString);
        string sqlQuery = "SELECT Trip.IdTrip,Trip.Name,Description,DateFrom,DateTo,MaxPeople,Country.IdCountry,Country.Name FROM Trip " +
            "INNER JOIN Country_Trip ON Trip.IdTrip = Country_Trip.IdTrip " +
            "INNER JOIN Country ON Country_Trip.IdCountry = Country.IdCountry;";
        await using SqlCommand command = new SqlCommand(sqlQuery, connection);
        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();
        while(await reader.ReadAsync()) {
            result.Add(new TripWithCountryGetDTO {
                TripID = reader.GetInt32(0),
                Name = reader.GetString(1),
                Description = reader.GetString(2),
                DateFrom = reader.GetDateTime(3),
                DateTo = reader.GetDateTime(4),
                MaxPeople = reader.GetInt32(5),
                Country = new CountryGetDTO {
                    CountryID = reader.GetInt32(6),
                    CountryName = reader.GetString(7)
                }
            });
        }

        return result;
    }

    public async Task<IEnumerable<TripWithRegisterationGetDTO>> GetClientTripsAsync(int id) {
        List<TripWithRegisterationGetDTO> result = new List<TripWithRegisterationGetDTO>();

        await using SqlConnection connection = new SqlConnection(connectionString);
        string sqlQuery = "SELECT 1 FROM Client WHERE IdClient = @id";

        await using SqlCommand commandA = new SqlCommand(sqlQuery, connection);
        commandA.Parameters.AddWithValue("@id", id);
        await connection.OpenAsync();
        await using (var reader = await commandA.ExecuteReaderAsync()) {
            if (!reader.HasRows) 
                throw new NotFoundException($"Client with id: {id} does not exist");
        }


        sqlQuery = "SELECT Trip.IdTrip,Trip.Name,Trip.Description,Trip.DateFrom,Trip.DateTo,Trip.MaxPeople,Client_Trip.RegisteredAt,Client_Trip.PaymentDate FROM client " +
           "INNER JOIN Client_Trip ON Client.IdClient = Client_Trip.IdClient " +
           "INNER JOIN Trip ON Client_Trip.IdTrip = Trip.IdTrip " +
           "WHERE Client.IdClient = @id;";


        await using SqlCommand commandB = new SqlCommand(sqlQuery, connection);
        commandB.Parameters.AddWithValue("@id", id);

        await using (var reader = await commandB.ExecuteReaderAsync()) {
            if (!reader.HasRows)
                throw new NotFoundException($"Client with id: {id} has no trips");

            while (await reader.ReadAsync()) {

                result.Add(new TripWithRegisterationGetDTO {
                    TripID = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Description = reader.GetString(2),
                    DateFrom = reader.GetDateTime(3),
                    DateTo = reader.GetDateTime(4),
                    MaxPeople = reader.GetInt32(5),
                    RegisteredAt = reader.GetInt32(6),
                    PaymentDate = reader.IsDBNull(7) ? null : reader.GetInt32(7)

                });
            }
        }

        return result;
    }

    public async Task<ClientModel> CreateClientAsync(ClientCreateDTO client) {

        await using var connection = new SqlConnection(connectionString);

        const string sqlQuery = "insert into Client (FirstName, LastName, Email, Telephone, Pesel) " +
            "values (@FirstName, @LastName, @Email, @Telephone,@Pesel); Select scope_identity()";


        await using var command = new SqlCommand(sqlQuery, connection);
        command.Parameters.AddWithValue("@FirstName", client.FirstName);
        command.Parameters.AddWithValue("@LastName", client.LastName);
        command.Parameters.AddWithValue("@Email", client.Email);
        command.Parameters.AddWithValue("@Telephone", client.PhoneNumber);
        command.Parameters.AddWithValue("@Pesel", client.Pesel);

        await connection.OpenAsync();
        var id = Convert.ToInt32(await command.ExecuteScalarAsync());

        return new ClientModel {
            ClientID = id,
            FirstName = client.FirstName,
            LastName = client.LastName,
            Email = client.Email,
            PhoneNumber = client.PhoneNumber,
            Pesel = client.Pesel
        };
    }

    public async Task RegisterClientTrip(int clientID, int tripID) {
        await using var connection = new SqlConnection(connectionString);

        string sqlQuery = "SELECT 1 FROM Client WHERE IdClient = @idClient";
        await using SqlCommand commandA = new SqlCommand(sqlQuery, connection);
        commandA.Parameters.AddWithValue("@idClient", clientID);
        
        await connection.OpenAsync();

        await using (var reader = await commandA.ExecuteReaderAsync()) {
            if (!reader.HasRows)
                throw new NotFoundException($"Client with id: {clientID} does not exist");
        }

        

        sqlQuery = "SELECT MaxPeople FROM Trip WHERE IdTrip = @idTrip";
        await using SqlCommand commandB = new SqlCommand(sqlQuery, connection);
        commandB.Parameters.AddWithValue("@idTrip", tripID);

        int maxPeople = 0;

        await using (var reader = await commandB.ExecuteReaderAsync()) {
            if (!reader.HasRows)
                throw new NotFoundException($"Trip with id: {tripID} does not exist");

            await reader.ReadAsync();
            maxPeople = reader.GetInt32(0);
        }

        sqlQuery = "SELECT COUNT(Trip.IdTrip) FROM Trip INNER JOIN Client_Trip ON Trip.IdTrip = Client_Trip.IdTrip WHERE Trip.IdTrip = @idTrip";
        await using SqlCommand commandC = new SqlCommand(sqlQuery, connection);
        commandC.Parameters.AddWithValue("@idTrip", tripID);

        int peopleCount = Convert.ToInt32(await commandC.ExecuteScalarAsync());
        
        if(peopleCount>=maxPeople)
            throw new NotFoundException($"Trip with id: {tripID} has already reached the limit of participants: {peopleCount}");

        sqlQuery = "insert into Client_Trip (IdClient, IdTrip, RegisteredAt, PaymentDate) " +
            "values (@IdClient, @IdTrip, @RegisteredAt,null )";

        await using var commandD = new SqlCommand(sqlQuery, connection);
        commandD.Parameters.AddWithValue("@IdClient", clientID);
        commandD.Parameters.AddWithValue("@IdTrip", tripID);
        commandD.Parameters.AddWithValue("@RegisteredAt", int.Parse(DateTime.Today.ToString("yyyyMMdd")));

        await commandD.ExecuteNonQueryAsync();
    }

    public async Task RemoveRegistresionForTrip(int clientID, int tripID) {

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        string sql = "SELECT COUNT(IdClient) from client_trip WHERE IdClient = @IdClient and IdTrip = @IdTrip";
        await using var commandA = new SqlCommand(sql, connection);
        commandA.Parameters.AddWithValue("@IdClient", clientID);
        commandA.Parameters.AddWithValue("@IdTrip", tripID);

        await using (var reader = await commandA.ExecuteReaderAsync()) {
            if (!reader.HasRows)
                throw new NotFoundException($"Registration of client with id: {clientID} to trip with id: {tripID} does not exist");
        }

        sql = "DELETE FROM Client_Trip WHERE IdClient = @IdClient and IdTrip = @IdTrip";
        await using var commandB = new SqlCommand(sql, connection);
        commandB.Parameters.AddWithValue("@IdClient", clientID);
        commandB.Parameters.AddWithValue("@IdTrip", tripID);
        await commandB.ExecuteNonQueryAsync();
    }

    public async Task<IEnumerable<ClientGetDTO>> GetAllClients() {
        List<ClientGetDTO> result = new List<ClientGetDTO>();

        await using SqlConnection connection = new SqlConnection(connectionString);
        string sqlQuery = "select IdClient,FirstName,LastName,Email,Telephone,Pesel from client";
        await using SqlCommand command = new SqlCommand(sqlQuery, connection);
        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync()) {
            result.Add(new ClientGetDTO {
                ClientID = reader.GetInt32(0),
                FirstName = reader.GetString(1),
                LastName = reader.GetString(2),
                Email = reader.GetString(3),
                PhoneNumber = reader.GetString(4),
                Pesel = reader.GetString(5),
            });
        }

        return result;
    }
}
