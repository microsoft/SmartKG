// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace SmartKG.KGBot.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WelcomeController : Controller
    {
        // GET api/welcome
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "Welcome to SmarKG" };
        }          
    }
}
