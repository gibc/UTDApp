using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections.ObjectModel;
using System.Windows.Input;
using UDTApp.Models;
using System.Windows;
using Prism.Events;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.ComponentModel;

namespace UDTApp.ViewModels
{
    public class DataViewModel : ValidatableBindableBase //Prism.Mvvm.BindableBase
    {
        private readonly IEventAggregator _eventAggregator;
        private string _fieldName = null;

        [Required]
        [StringLength(10, ErrorMessage = "Name cannot be longer than 10 characters.")]
        public string Name
        {
            get { return _fieldName; }
            set { SetProperty(ref _fieldName, value); }
        }

        private string _fieldValue = null;

        [Range(0, int.MaxValue, ErrorMessage = "Please enter valid integer Number")]
        public string Value
        {
            get { return _fieldValue; }
            set { SetProperty(ref _fieldValue, value); }
        }


        void dynamicObject_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //RaisePropertyChanged(e.PropertyName);
        }

        dynamic _runTimeProps = new ExpandoObject();
        public dynamic RunTimeProps
        {
            get { return _runTimeProps; }
            set { _runTimeProps = value; }
        }

        private UDTAppData _appData = null;

        public DelegateCommand UpdateCommand { get; set; }
        public DelegateCommand CancelCommand { get; set; }

        public DataViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            UpdateCommand = new DelegateCommand(SaveData, canExectueSave).ObservesProperty(() => Name).ObservesProperty(() => Value);
            CancelCommand = new DelegateCommand(CancelEdit, canExecuteCancel).ObservesProperty(() => Name).ObservesProperty(() => Value);
            _appData = new UDTAppData();
            _fieldName = _appData.FieldName;
            _fieldValue = _appData.FieldValue;

            //dynamic _runTimeProps = new ExpandoObject();
            // Create a new event and initialize it with null.  
            //((INotifyPropertyChanged)_runTimeProps).PropertyChanged += new PropertyChangedEventHandler(dynamicObject_PropertyChanged);
            var p = _runTimeProps as IDictionary<String, object>;
            p["newProp"] = "NewProp";
        }


        private void SaveData()
        {
            _appData.FieldName = _fieldName;
            _appData.FieldValue = _fieldValue;
            _appData.SaveData();
            UpdateCommand.RaiseCanExecuteChanged();
            CancelCommand.RaiseCanExecuteChanged();
        }

        private void CancelEdit()
        {
            Name = _appData.FieldName;
            Value = _appData.FieldValue;
            _runTimeProps.newProp = "XXXXX";
        }

        private bool canExecuteCancel()
        {
            return (Name != _appData.FieldName || Value != _appData.FieldValue);
        }

        private bool canExectueSave()
        {
            //return !String.IsNullOrWhiteSpace(Name) || !String.IsNullOrWhiteSpace(Value);
            return canExecuteCancel() && !HasErrors;
        }
    }

}



