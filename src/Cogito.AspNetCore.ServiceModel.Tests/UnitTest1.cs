using System;
using System.Diagnostics;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cogito.AspNetCore.ServiceModel.Tests
{

    [TestClass]
    public class UnitTest1
    {

        [ServiceContract(Namespace = "http://tempuri.org/", Name = "Math")]
        interface IMathService
        {
            [OperationContract(Action = "Add")]
            int Add(int x, int y);
        }

        class MathService : IMathService
        {
            public int Add(int x, int y)
            {
                return x + y;
            }

        }

        [TestMethod]
        public async Task TestMethod1()
        {
            var ns1 = (XNamespace)"http://schemas.xmlsoap.org/soap/envelope/";
            var ns2 = (XNamespace)"http://tempuri.org/";

            var x = new XDocument(
                new XElement(ns1 + "Envelope",
                    new XElement(ns1 + "Body",
                        new XElement(ns2 + "Add",
                            new XElement(ns2 + "x", 1),
                            new XElement(ns2 + "y", 2)))));

            var h = new ServiceHost(typeof(MathService));
            var q = new AspNetCoreRequestQueue();
            var b = new AspNetCoreBasicBinding();
            h.Description.Behaviors.Add(new AspNetCoreServiceBehavior(q));
            h.AddServiceEndpoint(typeof(IMathService), b, "http://localhost/math");
            h.Faulted += (s, a) => Trace.WriteLine("faulted");
            h.Open();

            await Task.Delay(TimeSpan.FromSeconds(15));

            // build fake SOAP request
            var r = new DefaultHttpContext();
            r.Request.Scheme = "http";
            r.Request.Host = new HostString("localhost", 80);
            r.Request.Path = "/math";
            r.Request.ContentType = "text/xml";
            r.Request.Headers["SOAPAction"] = "Add";
            r.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(x.ToString()));
            r.Response.Body = new MemoryStream();

            // execute request
            await q.SendAsync(r);

            r.Response.Body.Position = 0;
            var o = XDocument.Load(r.Response.Body);

            await Task.Delay(TimeSpan.FromSeconds(15));
        }
    }

}
