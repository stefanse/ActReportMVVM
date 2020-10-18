using ActReport.Core.Entities;
using ActReport.Persistence;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using ActReport.Core.Contracts;
using System;
using Microsoft.EntityFrameworkCore.Migrations.Operations;


namespace ActReport.ViewModel
{
    public class EmployeeViewModel : BaseViewModel
    {
        private string _firstName; //Eingabefeld Vorname
        private string _lastName; //Eingabefeld Nachname
        private string _filterText = "";

        private Employee _selectedEmployee; //Aktuell ausgewählter Employee
        private ObservableCollection<Employee> _employees; //Liste aller Miarbeiter

        public string FirstName
        {
            get => _firstName;
            set
            {
                _firstName = value;
                OnPropertyChanged(nameof(FirstName));
            }
        }

        public string LastName
        {
            get => _lastName;
            set
            {
                _lastName = value;
                OnPropertyChanged(nameof(LastName));
            }
        }

        public string FilterText
        {
            get => _filterText;
            set{
                _filterText = value;
                OnPropertyChanged(nameof(FilterText));
            }
        }

    public Employee SelectedEmployee
        {
            get => _selectedEmployee;
            set
            {
                _selectedEmployee = value;
                FirstName = _selectedEmployee?.FirstName;
                LastName = _selectedEmployee?.LastName;
                OnPropertyChanged(nameof(SelectedEmployee));
            }

        }

        public ObservableCollection<Employee> Employees
        {
            get => _employees;
            set
            {
                _employees = value;
                OnPropertyChanged(nameof(Employees));
            }
        }

        public EmployeeViewModel()
        {
            LoadEmployees();
        }

        private void LoadEmployees()
        {
            using (UnitOfWork uow = new UnitOfWork())
            {
                var employees = uow.EmployeeRepository
                    .Get(orderBy:
                    coll => coll.OrderBy(emp => emp.LastName)).ToList();
                Employees = new ObservableCollection<Employee>(employees?.Where(names => names.FirstName.StartsWith(FilterText)) );
                
            }
        }

        private ICommand _cmdSaveChanges;

        public ICommand CmdSaveChanges
        {
            get
            {
                if (_cmdSaveChanges == null)
                {
                    _cmdSaveChanges = new RelayCommand(
                        execute: _ =>
                        {
                            using IUnitOfWork uow = new UnitOfWork();
                            _selectedEmployee.FirstName = _firstName;
                            _selectedEmployee.LastName = _lastName;
                            uow.EmployeeRepository.Update(_selectedEmployee);
                            uow.Save();

                            LoadEmployees();
                        },
                        canExecute: _ => _selectedEmployee != null && _selectedEmployee.LastName.Length > 2) ;
                }
                return _cmdSaveChanges;
            }

            set { _cmdSaveChanges = value; }
        }

        private ICommand _cmdSaveNewEmployee;
        public ICommand CmdSaveNewEmployee
        {
            get
            {
                if (_cmdSaveNewEmployee == null)
                {
                    _cmdSaveNewEmployee = new RelayCommand(
                    execute: _ =>
                    {
                        using IUnitOfWork uow = new UnitOfWork();
                        Employee neuerMA = new Employee
                        {
                            FirstName = _firstName,
                            LastName = _lastName
                        };
                        uow.EmployeeRepository.Insert(neuerMA);

                        uow.Save();

                        LoadEmployees();
                    },
                    canExecute: _ => LastName?.Length > 2 && _selectedEmployee != null);
                }
                return _cmdSaveNewEmployee;
            }

            set { _cmdSaveNewEmployee = value; }
        }
    }
}