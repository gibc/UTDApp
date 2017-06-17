using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using UDTApp.Models;

namespace UDTApp.ViewModels
{
    public class PageOneViewModel : SetupPageBase
    {
        public PageOneViewModel()
        {
            IsMasterVisible = true;       
        }
    }
}
