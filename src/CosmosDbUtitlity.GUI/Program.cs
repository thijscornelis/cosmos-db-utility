using Autofac;
using CosmosDbUtility.Application;
using CosmosDbUtility.GUI.Abstractions;
using CosmosDbUtility.Infrastructure;

namespace CosmosDbUtility.GUI;

internal static class Program
{
	 /// <summary>The main entry point for the application.</summary>
	 [STAThread]
	 private static void Main()
	 {
		  ApplicationConfiguration.Initialize();
		  var container = SetupDependencyInjection();
		  using var scope = container.BeginLifetimeScope();
		  var startupForm = scope.Resolve<Form1>();
		  System.Windows.Forms.Application.Run(startupForm);
	 }

	 private static void RegisterForms(ContainerBuilder builder)
	 {
		  var forms = typeof(Program).Assembly.GetTypes().Where(x => x.IsSubclassOf(typeof(Form))).ToArray();
		  if (forms.Length == 0) throw new Exception("Could not register any WinForms, thus nothing to launch here...");
		  builder.RegisterTypes(forms.ToArray()).InstancePerDependency();
	 }

	 private static IContainer SetupDependencyInjection()
	 {
		  var builder = new ContainerBuilder();
		  builder.RegisterType<CosmosDbDocumentRepository>().As<IDocumentRepository>();
		  builder.RegisterType<FileRepository>().As<IFileRepository>();
		  builder.RegisterType<Facade>().As<IFacade>();
		  RegisterForms(builder);
		  return builder.Build();
	 }
}
