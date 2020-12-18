using System;
using System.Configuration;
using Microsoft.Owin.Hosting;
using Topshelf;

namespace WebApi
{
	public class Service
	{
		protected IDisposable WebAppHolder { get; set; }

		public bool Start(HostControl hostControl)
		{
            int puerto;
            string protocol = ConfigurationManager.AppSettings.Get("Protocol") ?? "http";

            if (!int.TryParse(ConfigurationManager.AppSettings.Get("Puerto"), out puerto))
                throw new ArgumentException("No se encuentra configurado el numero de puerto");

            if (WebAppHolder == null)
            {
                WebAppHolder = WebApp.Start<Startup>(String.Format("{1}://*:{0}", puerto, protocol));
                
                System.Console.BackgroundColor = ConsoleColor.Green;
                System.Console.ForegroundColor = ConsoleColor.White;
                System.Console.WriteLine($"Started listening on port {protocol} {puerto}.");
                System.Console.BackgroundColor = ConsoleColor.Black;
                System.Console.ForegroundColor = ConsoleColor.White;
            }

            return true;
        }

		public bool Stop(HostControl hostControl)
		{
			if (WebAppHolder != null)
			{
				WebAppHolder.Dispose();
				WebAppHolder = null;
			}
			return true;
		}
	}
}