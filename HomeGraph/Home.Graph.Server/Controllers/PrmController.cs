using Home.Common.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MQTTnet.Extensions;
using System.Collections.Generic;

namespace Home.Graph.Server.Controllers
{
    [Route("api/prm")]
    [ApiController]
    public class PrmController : ControllerBase
    {

        public class UserPrmDataWithDetails : UserPrmData
        {

        }

        [Route("search"), HttpGet]
        public List<UserPrmDataWithDetails> GetList(string nameSearch = null, string tagSearch = null)
        {
            return new List<UserPrmDataWithDetails>();
        }

        [Route("users/{id}"), HttpGet]
        public UserPrmDataWithDetails Get(string id)
        {
            return null;
        }

        [Route("users"), HttpPost]
        public UserPrmDataWithDetails UpsertUserData([FromBody] UserPrmDataWithDetails account)
        {
            return null;
        }

        [Route("users/{id}"), HttpDelete]
        public bool Delete(string id, bool deleteUser = false)
        {
            return false;
        }
    }
}
