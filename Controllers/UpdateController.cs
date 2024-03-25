using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SnowBackend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.AspNetCore.Http;

namespace SnowBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DataController : ControllerBase
    {
        private static readonly List<UserConnection> users = new();
        private static System.Timers.Timer timer;

        private readonly int _updateInterval;
        private readonly ILogger<DataController> _logger;

        public DataController(ILogger<DataController> logger)
        {
            _logger = logger;
            _updateInterval = 5;
        }

        [HttpPost]
        [Route("startDataUpdate")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> StartDataUpdate(IFormFile file, Guid userId, string hash)
        {
            _logger.LogInformation($"Received update from {userId} with hash {hash}");

            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            if (!HashTool.HealthyCheck(file, hash))
                return BadRequest("File damaged during transaction");

            var newDataRow = await FileTool.ReadFile(file);

            if (newDataRow == null)
                return BadRequest("Invalid file format.");

            var user = users.FirstOrDefault(u => u.UserId == userId);
            if (user != null)
            {
                user.FileData = newDataRow;
            }
            else
            {
                user = new UserConnection
                {
                    UserId = userId,
                    IsActive = true,
                    FileData = newDataRow,
                    Hash = hash
                };
                users.Add(user);
            }

            if (timer == null)
            {
                timer = new System.Timers.Timer(TimeSpan.FromSeconds(_updateInterval).TotalMilliseconds);
                timer.Elapsed += TimerElapsed;
                timer.Start();

                _logger.LogInformation("Data update starting");
            }

            return Ok(user.FileData);
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            // Generate new random data every x seconds
            foreach (var user in users)
            {
                user.GenerateNewCurrentData();
            }
            _logger.LogInformation("Data updated.");
        }

        [HttpGet]
        [Route("getData")]
        public IActionResult GetData(Guid userId)
        {
            var user = users.FirstOrDefault(u => u.UserId == userId);
            if (user == null || user.CurrentData == null)
                return Unauthorized();

            return Ok(user.CurrentData);
        }

        [HttpDelete]
        [Route("deleteUser")]
        public IActionResult DeleteUser(Guid userId)
        {
            _logger.LogInformation($"Deleting user: {userId}");

            var user = users.FirstOrDefault(u => u.UserId == userId);
            if (user == null)
                return NotFound("User not found");

            if (users.Remove(user))
            {
                _logger.LogInformation("User removed successfully.");
                return Ok();
            }
            else
            {
                _logger.LogError("Failed to remove user.");
                return BadRequest("Failed to remove user.");
            }
        }
    }
}
