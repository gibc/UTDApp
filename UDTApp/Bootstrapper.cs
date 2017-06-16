using System.Windows;
using Microsoft.Practices.Unity;
using Prism.Unity;
using UDTApp.Views;

namespace UDTApp
{
    public class Bootstrapper : UnityBootstrapper
    {
        protected override DependencyObject CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void InitializeShell()
        {
            Application.Current.MainWindow.Show();
        }

        protected override void ConfigureContainer()
        {
            base.ConfigureContainer();

            //Container.RegisterType(typeof(object), typeof(ViewA), "ViewA");
            //Container.RegisterType(typeof(object), typeof(ViewB), "ViewB");

            Container.RegisterTypeForNavigation<Data>("Data");
            Container.RegisterTypeForNavigation<SetUp>("SetUp");
        }
    }

    public static class UnityExtensions
    {
        public static void RegisterTypeForNavigation<T>(this IUnityContainer container, string name)
        {
            container.RegisterType(typeof(object), typeof(T), name);
        }
    }


}
