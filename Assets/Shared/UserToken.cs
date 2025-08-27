using System;
using System.Collections.Generic;
using System.Net;

#nullable enable 
namespace iBMSApp.Shared
{
    public class UserToken
    {
        public HttpStatusCode StatusCode { get; set; }
        public string Token { get; set; } = null!;

        //public string? RefreshToken { get; set; }
        public DateTime ExpireTime { get; set; }
        public string Name { get; set; } = null!;
        public List<string> Roles = new List<string>();
    }
}
