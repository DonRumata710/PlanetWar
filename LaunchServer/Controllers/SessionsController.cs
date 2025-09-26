using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using Interfaces.Models;
using System.Security.Claims;
using Serilog;
using MySqlX.XDevAPI;

namespace LaunchServer.Controllers
{
    [Route("sessions")]
    [Authorize]
    [ApiController]
    public class SessionsController : ControllerBase
    {
        public SessionsController(DatabaseService db)
        {
            database = db;
        }

        [HttpGet]
        public IActionResult Get(int? id)
        {
            if (id.HasValue)
            {
                return Ok(database.GetSession(id.Value));
            }
            else
            {
                database.ClearOutSessionList();
                return Ok(database.GetNotStartedSessions());
            }
        }

        [HttpGet]
        [Route("join")]
        public IActionResult Join(int sessionId)
        {
            try
            {
                Int32 userId = Int32.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                Log.Information("User " + userId.ToString() + " joined to session " + sessionId.ToString());

                database.AddPlayer(userId, sessionId);
                return Ok();
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                return StatusCode(400, ex.Message);
            }
        }

        [HttpGet]
        [Route("leave")]
        public IActionResult Leave()
        {
            Int32 userId = Int32.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            Log.Information("User " + userId.ToString() + " leaves session");

            database.LeaveSessions(userId);
            return Ok();
        }

        [HttpGet]
        [Route("default")]
        public IActionResult GetDefaultSettings()
        {
            SessionStartParameters res = new SessionStartParameters();
            res.PlanetCount = 4;
            res.PlayerLimit = 2;
            res.Size = 10;
            return Ok(res);
        }

        const int PlanetMinCount = 2;
        const int PlayerMinCount = 2;
        const int MinSize = 3;

        [HttpGet]
        [Route("min")]
        public IActionResult GetMinSettings()
        {
            SessionStartParameters res = new SessionStartParameters();
            res.PlanetCount = PlanetMinCount;
            res.PlayerLimit = PlayerMinCount;
            res.Size = MinSize;
            return Ok(res);
        }

        [HttpPost]
        public IActionResult RegisterSession([FromBody]SessionStartParameters param)
        {
            if (param.Name == null ||
                param.PlanetCount < PlanetMinCount ||
                param.PlayerLimit < PlayerMinCount ||
                param.Size < MinSize
            )
                return StatusCode(422);

            var userId = Int32.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            Log.Information("User " + userId.ToString() + " creates session");

            int id = database.CreateNewSession(param);

            return Ok(id);
        }

        [HttpPut]
        public IActionResult RefreshSession(int id, SessionStartParameters param)
        {
            var players = database.GetSessionPlayers(id);
            var userId = Int32.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (players.Count == 0 || players[0] != userId)
                return Forbid();

            database.RefreshSession(id, param);
            return Ok();
        }

        [HttpGet]
        [Route("start")]
        public IActionResult StartSession(int id)
        {
            int serverId = -1;
            string serverAddress = null;
            float minRatio = float.MaxValue;
            foreach(var server in database.ServerList())
            {
                float ratio = server.Value.ActiveSessions / (float)server.Value.SessionLimit;
                if (ratio < minRatio)
                {
                    serverId = server.Key;
                    serverAddress = server.Value.Address;
                    minRatio = ratio;
                }
            }

            if (serverId >= 0)
            {
                database.StartSession(id, serverId);
                return Ok(serverAddress);
            }
            else
            {
                return NotFound("No apropriate servers");
            }
        }

        private DatabaseService database;
    }
}
