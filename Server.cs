using System;
using System.Collections.Generic;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Stripe;
using Stripe.Terminal;

namespace StripeExample
{
  public class Program
  {
    public static void Main(string[] args)
    {
      WebHost.CreateDefaultBuilder(args)
        .UseUrls("http://0.0.0.0:4242")
        .UseWebRoot("public")
        .UseStartup<Startup>()
        .Build()
        .Run();
    }

  private static Location createLocation(){
    var options = new LocationCreateOptions
    {
      DisplayName = "HQ",
      Address = new AddressOptions
      {
        Line1 = "1272 Valencia Street",
        City = "San Francisco",
        State = "CA",
        Country = "US",
        PostalCode = "94110",
      },
    };

    var service = new LocationService();
    var location = service.Create(options);

    return location;
  }
  }

  public class Startup
  {
    public void ConfigureServices(IServiceCollection services)
    {
      services.AddMvc().AddNewtonsoftJson();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
      // This is a public sample test API key.
      // Don’t submit any personally identifiable information in requests made with this key.
      // Sign in to see your own test API key embedded in code samples.
      StripeConfiguration.ApiKey = "sk_test_51LI99cJRV6jzzuCAv8XiE1I13m16OabPZX5wAXX0mDIiAc4TjPgDGzP6QXqLWWBH4LPHM7s0xXXOqZVfFyWaA0pP00kcDAmfAb";

      if (env.IsDevelopment()) app.UseDeveloperExceptionPage();
      app.UseRouting();
      app.UseDefaultFiles();
      app.UseStaticFiles();
      app.UseEndpoints(endpoints => endpoints.MapControllers());
    }
  }

  // The ConnectionToken's secret lets you connect to any Stripe Terminal reader
  // and take payments with your Stripe account.
  // Be sure to authenticate the endpoint for creating connection tokens.
  [Route("connection_token")]
  [ApiController]
  public class ConnectionTokenApiController : Controller
  {
    [HttpPost]
    public ActionResult Post()
    {
      var options = new ConnectionTokenCreateOptions{};
      var service = new ConnectionTokenService();
      var connectionToken = service.Create(options);

      return Json(new {secret = connectionToken.Secret});
    }
  }



  [Route("create_payment_intent")]
  [ApiController]
  public class PaymentIntentApiController : Controller
  {
    [HttpPost]
    public ActionResult Post(PaymentIntentCreateRequest request)
    {
      var service = new PaymentIntentService();

      // For Terminal payments, the 'payment_method_types' parameter must include
      // 'card_present' and the 'capture_method' must be set to 'manual'
      var options = new PaymentIntentCreateOptions
      {
          Amount = long.Parse(request.Amount),
          Currency = "cad",
          PaymentMethodTypes = new List<string> { "card_present" },
          CaptureMethod = "manual",
      };
      var intent = service.Create(options);

      return Json(intent);
    }

    public class PaymentIntentCreateRequest
    {
      [JsonProperty("amount")]
      public string Amount { get; set; }
    }
  }



  [Route("capture_payment_intent")]
  [ApiController]
  public class CapturePaymentIntentApiController : Controller
  {
    [HttpPost]
    public ActionResult Post(PaymentIntentCaptureRequest request)
    {
      var service = new PaymentIntentService();
      var intent = service.Capture(request.PaymentIntentId, null);
      return Json(intent);
    }

    public class PaymentIntentCaptureRequest
    {
      [JsonProperty("payment_intent_id")]
      public string PaymentIntentId { get; set; }
    }
  }
}