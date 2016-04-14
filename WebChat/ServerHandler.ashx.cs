using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebChat
{
    /// <summary>
    /// Summary description for ServerHandler
    /// </summary>
    public class ServerHandler : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            context.Response.Write("Hello World");
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}