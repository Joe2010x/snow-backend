using Microsoft.AspNetCore.Mvc;
using SnowBackend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

using System.Security.Cryptography;
using System.Text;
using System.Reflection.Metadata.Ecma335;

namespace SnowBackend.Controllers
{
[ApiController]
    [Route("api/[controller]")]
    public class DataController : ControllerBase
    {
        // private static List<object> data = new List<object>();
        private static List<UserConnection> users = new ();
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
            _logger.LogInformation("received update from "+userId+" received hash "+hash);

            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            if (!HashTool.HealthyCheck(file,hash))
            {
                return BadRequest("File damaged during transaction");
            }

            var newDataRow =  FileTool.ReadFile(file);

            if (newDataRow == null )
            {
                return BadRequest("Invalid file format.");
            }

            var isExistUser = users.Exists(u => u.UserId == userId);
            UserConnection user = new UserConnection();
            if (isExistUser) {
                // existing user update data info;
                user = users.FirstOrDefault(u=> u.UserId == userId);
                user!.FileData = await newDataRow;
            } else {
                // new user add to user list;
                user.CurrentData = null;
                user.UserId = userId;
                user.IsActive = true;
                user.FileData = await newDataRow;
                user.Hash = hash;
                users.Add(user);
            }


            if (timer == null)
            {
                timer = new System.Timers.Timer();
                timer.Interval = TimeSpan.FromSeconds(_updateInterval).TotalMilliseconds; 
                timer.Elapsed += TimerElapsed;
                timer.Start();

                _logger.LogInformation("Data update starting");
            }

            return Ok(user.FileData);
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            // Generate new random data every x seconds
            users.ForEach(u=> u.GenerateNewCurrentData());
            _logger.LogInformation("Data updated.");
        }


        [HttpGet]
        [Route("getData")]
        public IActionResult GetData(Guid userId)
        {
            var currentData = users.FirstOrDefault(u=> u.UserId == userId)?.CurrentData ?? null;
            return currentData == null ? Unauthorized() : Ok(currentData);
        }

        [HttpDelete]
        [Route("deleteUser")]
        public IActionResult DeleteUser(Guid userId)
        {
            _logger.LogInformation(userId.ToString());
            try
            {
                var user = users.Where(u => u.UserId == userId).FirstOrDefault();
                if (user == null) {
                    return NotFound("User not found");
                }

                _logger.LogInformation("user removed successful.");
                return users.Remove(user) ? Ok() : BadRequest("Failed to remove user.");

            }
            catch (System.Exception)
            {
                _logger.LogInformation("user removed unsuccessful.");
                return BadRequest("Remove user unsuccessful!");
            }
        }
    }
}
