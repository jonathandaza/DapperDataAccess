using Topshelf;

namespace WebApi
{
	class Program
	{
		static void Main(string[] args)
		{
			HostFactory.Run(c =>
			{
				c.RunAsLocalSystem();

				c.Service<Service>(s =>
				{
					s.ConstructUsing(name => new Service());
					s.WhenStarted((service, control) => service.Start(control));
					s.WhenStopped((service, control) => service.Stop(control));
				});
				c.SetServiceName("TestAPI");
				c.SetDisplayName("TestAPI");
				c.SetDescription("TestAPI");
			});
		}
	}
}
