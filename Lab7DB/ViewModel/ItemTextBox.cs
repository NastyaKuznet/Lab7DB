using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Lab7DB.ViewModel
{
    public class ItemTextBox : MyViewModel
    {
        public ObservableCollection<string> DeferredCollectionNameTables { get; set; }
        private Dictionary<string, ObservableCollection<string>> columnsTables;

        private string name;
        private ObservableCollection<string> collectionTypeColumn = new ObservableCollection<string>() { "int", "string", "dateTime" };
        private string selectedType = "";
        private bool isPrimaryKey;
        private bool isForeignKey;
        private ObservableCollection<string> collectionNameTables;
        private string selectedNameTable;
        private ObservableCollection<string> collectionNameColumns;
        private string selectedNameColumn;

        public string Name
        {
            get { return name; }
            set 
            { 
                name = value; 
                OnPropertyChanged(nameof(Name)); 
            }
        }
        public ObservableCollection<string> CollectionTypeColumn
        {
            get { return collectionTypeColumn; }
            set
            {
                collectionTypeColumn = value;
                OnPropertyChanged(nameof(collectionTypeColumn));
            }
        }
        public string SelectedType
        {
            get { return selectedType; }
            set 
            { 
                selectedType = value; 
                OnPropertyChanged(nameof(SelectedType)); 
            }
        }
        public bool IsPrimaryKey
        {
            get { return isPrimaryKey; }
            set
            {
                isPrimaryKey = value;
                OnPropertyChanged(nameof(IsPrimaryKey));
                CollectionNameTables = new ObservableCollection<string>();
                CollectionNameColumns = new ObservableCollection<string>();
                isForeignKey = false;
            }
        }
        public bool IsForeignKey
        {
            get { return isForeignKey; }
            set
            {
                isForeignKey = value;
                OnPropertyChanged(nameof(IsForeignKey));
                CollectionNameTables = DeferredCollectionNameTables;
            }
        }
        public ObservableCollection<string> CollectionNameTables
        {
            get { return collectionNameTables; }
            set { 
                collectionNameTables = value; 
                OnPropertyChanged(nameof(CollectionNameTables)); 
            }
        }
        public string SelectedNameTable
        {
            get { return selectedNameTable; }
            set 
            { 
                selectedNameTable = value; 
                OnPropertyChanged(nameof(SelectedNameTable)); 
            }
        }
        public ObservableCollection<string> CollectionNameColumns
        {
            get { return collectionNameColumns; }
            set 
            { 
                collectionNameColumns = value; 
                OnPropertyChanged(nameof(CollectionNameColumns)); 
            }
        }
        public string SelectedNameColumn
        {
            get { return selectedNameColumn; }
            set 
            { 
                selectedNameColumn = value; 
                OnPropertyChanged(nameof(SelectedNameColumn)); 
            }
        }

        public ItemTextBox(string _name, ObservableCollection<string> collection, Dictionary<string, ObservableCollection<string>> _columnsTables)
        {
            Name = _name;
            DeferredCollectionNameTables = collection;
            columnsTables = _columnsTables;
            
        }
        public ItemTextBox(string _name, string type,bool isPrimaryKey, bool isForeignKey, ObservableCollection<string> _collectionNameTables, 
            string _selectedNameTable, ObservableCollection<string> _collectionNameColumns, string _selectedNameColumn, Dictionary<string, ObservableCollection<string>> _columnsTables)
        {
            Name = _name;
            SelectedType = type;
            IsPrimaryKey = isPrimaryKey;
            IsForeignKey = isForeignKey;
            DeferredCollectionNameTables = _collectionNameTables;
            SelectedNameTable = _selectedNameTable;
            CollectionNameColumns = _collectionNameColumns;
            SelectedNameColumn = _selectedNameColumn;
            columnsTables = _columnsTables;
        }

        public ICommand SelectTable
        {
            get
            {
                return new CommandDelegate(parametr =>
                {
                    foreach (string nameTable in columnsTables.Keys)
                    {
                        if (nameTable.CompareTo(SelectedNameTable) == 0)
                            CollectionNameColumns = columnsTables[nameTable];
                    }
                });
            }
        }
    }
}
