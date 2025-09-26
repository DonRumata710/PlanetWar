using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Interfaces.Models;
using Microsoft.AspNetCore.Authorization;

namespace ChatServer
{
    [Authorize]
    [EnableCors]
    public class ChatHub : Hub
    {
        public async Task Send(int chat, string message)
        {
            var userId = Context.User.Identity.Name == ClaimTypes.NameIdentifier;
            await Clients.All.SendAsync("Send", chat, userId, message);
        }
    }
}
