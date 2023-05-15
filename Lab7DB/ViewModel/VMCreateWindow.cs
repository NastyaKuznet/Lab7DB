using Core0;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Lab7DB.ViewModel
{
    public class VMCreateWindow: MyViewModel
    {
        public string FullFolderPath { get; set; }
        public ObservableCollection<string> CollectionNameTables { get; set; }
        public Dictionary<string, ObservableCollection<string>> CollectionNameColumnsTables { get; set; }
        public Dictionary<string, Dictionary<string, string>> CollectionNameColumnsTablesWithType { get; set; }
        public ElementDB NewElement = new ElementDB();
        public ObservableCollection<ElementDB> ElementDBs { get; set; }
        public CreateWindow CreateWindow { get; set; }

        private string contentErrorWindow;
        private string nameTable;
        private string numberColumns;
        private ObservableCollection<ItemTextBox> columns = new ObservableCollection<ItemTextBox>();

        public string ContentErrorWindow
        {
            get { return contentErrorWindow; }
            set
            {
                contentErrorWindow = value;
                OnPropertyChanged(nameof(ContentErrorWindow));
            }
        }
        public string NameTable
        {
            get { return nameTable; }
            set
            {
                nameTable = value;
                OnPropertyChanged(nameof(NameTable));
            }
        }
        public string NumberColumns
        {
            get { return numberColumns; }
            set
            {
                numberColumns = value;
                OnPropertyChanged(nameof(NumberColumns));
            }
        }
        public ObservableCollection<ItemTextBox> Columns
        {
            get { return columns; }
            set
            {
                columns = value;
                OnPropertyChanged(nameof(Columns));
            }
        }

        public ICommand CreateTextBoxes
        {
            get
            {
                return new CommandDelegate(parametr =>
                {
                    if (IsHaveError(CheckError.ErrorEmptyString(NumberColumns)) && IsHaveError(CheckError.InputErrorInt(NumberColumns)))
                    {
                        int numberColumn = int.Parse(NumberColumns);
                        for (int i = 0; i < numberColumn; i++)
                        {
                            Columns.Add(new ItemTextBox("", CollectionNameTables, CollectionNameColumnsTables));
                        }
                    }
                });
            }
        }
        public ICommand Save
        {
            get
            {
                return new CommandDelegate(parametr =>
                {
                    if (IsHaveError(CheckError.ErrorEmptyString(NameTable)) && IsHaveError(CheckError.ErrorEmptyString(NumberColumns)))
                    {
                        var newTableAndProperties = CreateTableAndProps();
                        CreateJson(newTableAndProperties);
                    }
                });
            }
        }

        private (DataTable, Dictionary<string, PatternPropertyDB>) CreateTableAndProps()
        {
            DataTable newTable = new DataTable(NameTable);
            Dictionary<string, PatternPropertyDB> props = new Dictionary<string, PatternPropertyDB>();
            if (IsHaveError(CheckError.ErrorUniquenessPrimaryKey(CreateListForCheckStatePrimaryKeys())))
            {
                foreach (ItemTextBox contentColumn in Columns)
                {
                    if (IsCorrectColumns(contentColumn))
                    {
                        WriteAnotherTableAboutForeignKey(contentColumn);
                        DataColumn column = new DataColumn(contentColumn.Name);
                        props[contentColumn.Name] = new PatternPropertyDB(contentColumn.Name, contentColumn.SelectedType, contentColumn.IsPrimaryKey, contentColumn.IsForeignKey,
                            contentColumn.SelectedNameTable, contentColumn.SelectedNameColumn);
                        newTable.Columns.Add(column);
                    }
                    else { break; }
                }
            }
            return (newTable, props);
        }
        private void CreateJson((DataTable, Dictionary<string, PatternPropertyDB>) newTableAndProperties)
        {
            if (IsHaveError())
            {
                NewElement.FullPattern = new AdditionalDataPattern(FullFolderPath, new PatternObjectDB(NameTable, newTableAndProperties.Item2));
                NewElement.Table = newTableAndProperties.Item1;
                string json = JsonSerializer.Serialize(NewElement.FullPattern.Pattern);
                File.WriteAllText($"{FullFolderPath}\\{NameTable}.json", json);
                CreateWindow.Close();
            }
        }
        private List<bool> CreateListForCheckStatePrimaryKeys()
        {
            List<bool> states= new List<bool>();
            foreach(ItemTextBox column in Columns)
                states.Add(column.IsPrimaryKey);
            return states;
        }
        private void WriteAnotherTableAboutForeignKey(ItemTextBox contentColumn)
        {
            if(contentColumn.IsForeignKey)
            {
                foreach(ElementDB element in ElementDBs)
                {
                    if(element.FullPattern.Pattern.Name.CompareTo(contentColumn.SelectedNameTable) == 0)
                    {
                        element.IsNeedUpdate = true;
                        foreach(PatternPropertyDB prop in element.FullPattern.Pattern.Properties.Values)
                        {
                            if(prop.Name.CompareTo(contentColumn.SelectedNameColumn) == 0)
                            {
                                if(prop.ReferencingTables != null)
                                    prop.ReferencingTables.Add(NameTable);
                                else prop.ReferencingTables = new List<string> { NameTable };
                            }
                        }
                    }
                }
            }
        }
        private bool IsCorrectColumns(ItemTextBox contentColumn)
        {
            return IsHaveError(CheckError.ErrorEmptyString(contentColumn.Name))
                            && IsHaveError(CheckError.ErrorEmptyString(contentColumn.SelectedType))
                            && IsHaveError(CheckError.ErrorInstallationForeignKey(contentColumn.IsForeignKey, CollectionNameTables))
                            && IsHaveError(CheckError.ErrorEmptyStringWithForeignKey(contentColumn.IsForeignKey, contentColumn.SelectedNameTable))
                            && IsHaveError(CheckError.ErrorEmptyStringWithForeignKey(contentColumn.IsForeignKey, contentColumn.SelectedNameColumn))
                            && IsHaveError(CheckError.ErrorTypeColumnIsForeignKey(contentColumn.Name, contentColumn.IsForeignKey, contentColumn.SelectedType,
                            CollectionNameColumnsTablesWithType, contentColumn.SelectedNameTable, contentColumn.SelectedNameColumn))
                            && IsHaveError(CheckError.ErrorTypePrimaryKey(contentColumn.IsPrimaryKey, contentColumn.SelectedType))
                            && IsHaveError(CheckError.ErrorPrimaryKeyIsNotForeign(contentColumn.IsPrimaryKey, contentColumn.IsForeignKey));
        }
        private bool IsHaveError(string state = null)
        {
            if (state != null)
                ContentErrorWindow = state;
            return ContentErrorWindow.CompareTo(CheckError.NotError) == 0;
        }

    }
}
