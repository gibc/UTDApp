using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using UDTApp.Views;

namespace UDTApp.ViewModels
{
    public static class cons
    {
        public const string SetUpRegion = "SetUpRegion";
        public const string PageZero = "PageZero";
        public const string PageOne = "PageOne";
        public const string PageTwo = "PageTwo";
        public const string PageThree = "PageThree";
    }

    public class SetUpViewModel : BindableBase
    {
        private readonly IRegionManager _regionManager;
        private readonly IUnityContainer _container;
        private List<string> PageNames;
        private int pageIndex = 0;
        
        public DelegateCommand WindowLoadedCommand { get; set; }
        public DelegateCommand NextCommand { get; set; }
        public DelegateCommand PreviousCommand { get; set; }

        public SetUpViewModel(IUnityContainer container, IRegionManager regionManager)
        {
            _regionManager = regionManager;
            _container = container;
            WindowLoadedCommand = new DelegateCommand(windowLoaded);
            PageNames = new List<string>() { cons.PageZero, cons.PageOne, cons.PageTwo, cons.PageThree };
            NextCommand = new DelegateCommand(moveNext, canMoveNext);
            PreviousCommand = new DelegateCommand(movePrevious, canMovePevious);
        }

        private void windowLoaded()
        {
            dynamic view = new PageZero();
            _regionManager.AddToRegion(cons.SetUpRegion, view);
             view = new PageOne();
            _regionManager.AddToRegion(cons.SetUpRegion, view);
            view = new PageTwo();
            _regionManager.AddToRegion(cons.SetUpRegion, view);
            view = new PageThree();
            _regionManager.AddToRegion(cons.SetUpRegion, view);
            Navigate();

        }

        private void Navigate()
        {
            _regionManager.RequestNavigate(cons.SetUpRegion, PageNames[pageIndex]);
        }

        private void moveNext()
        {
            pageIndex++;
            Navigate();
            NextCommand.RaiseCanExecuteChanged();
            PreviousCommand.RaiseCanExecuteChanged();
        }

        private bool canMoveNext()
        {
            return pageIndex < PageNames.Count-1;
        }

        private void movePrevious()
        {
            pageIndex--;
            Navigate();
            NextCommand.RaiseCanExecuteChanged();
            PreviousCommand.RaiseCanExecuteChanged();
        }

        private bool canMovePevious()
        {
            return pageIndex > 0;
        }
    }
}
