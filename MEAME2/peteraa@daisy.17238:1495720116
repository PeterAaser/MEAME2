using Nancy;
using System.Linq;
using System.Text;
using System.Collections.Generic;

public class HelloModule : NancyModule
{
  public HelloModule()
  {
    Get["/getJSON"] = _ => {
    string jsonString = "{ username: \"admin\", password: \"just kidding\" }";
    byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonString);

    return new Response()
      {
        StatusCode = HttpStatusCode.OK,
        ContentType = "application/json",
        ReasonPhrase = "Because why not!",
        Headers = new Dictionary<string, string>()
        {
          { "Content-Type", "application/json" },
          { "X-Custom-Header", "Sup?" }
        },
        Contents = c => c.Write(jsonBytes, 0, jsonBytes.Length)
      };
    };

    Get["/"] = parameters => "Nice meme you fucking idiot";

    Get["/fug/{meme}"] = parameters => {
      return $"Nice fucking meme :---DDD  {parameters.meme}";
    };

    Post["/"] = parameters =>
      {
        var id = this.Request.Body;
        long length = this.Request.Body.Length;
        byte[] data = new byte[length];
        id.Read(data, 0, (int)length);
        string body = System.Text.Encoding.Default.GetString(data);
        var p = body.Split('&')
        .Select(s => s.Split('='))
        .ToDictionary(k => k.ElementAt(0), v => v.ElementAt(1));

        if (p["username"] == "volkan")
          return "awesome!";
        else
          return "meh!";
      };
  }
}
