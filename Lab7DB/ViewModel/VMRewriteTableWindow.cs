using Core0;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Linq;

namespace Lab7DB.ViewModel
{
    public class VMRewriteTableWindow: MyViewModel
    {
        public ElementDB Element { get; set; }
        public ObservableCollection<ElementDB> ElementDBs { get; set; }
        public ObservableCollection<string> CollectionNameTables { get; set; }
        public Dictionary<string, ObservableCollection<string>> CollectionNameColumnsTables { get; set; }
        public Dictionary<string, Dictionary<string, string>> CollectionNameColumnsTablesWithType { get; set; }

        private string сontentErrorWindow = CheckError.NotError;
        private string nameTable;
        private ObservableCollection<ItemTextBox> columns = new ObservableCollection<ItemTextBox>();
        private string numberColumns;
        private ObservableCollection<string> nameColumns = new ObservableCollection<string>();
        private string selectedColumn;

        public string ContentErrorWindow
        {
            get { return сontentErrorWindow; }
            set
            {
                сontentErrorWindow = value;
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
        public ObservableCollection<ItemTextBox> Columns
        {
            get { return columns; }
            set
            {
                columns = value;
                OnPropertyChanged(nameof(Columns));
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
        public ObservableCollection<string> NameColumns
        {
            get { return nameColumns; }
            set
            {
                nameColumns = value;
                OnPropertyChanged(nameof(NameColumns));
            }
        }
        public string SelectedColumn
        {
            get { return selectedColumn; }
            set 
            { 
                selectedColumn = value; 
                OnPropertyChanged(nameof(SelectedColumn)); 
            }
        }

        public VMRewriteTableWindow(ElementDB element, ObservableCollection<string> collectionNameTables,
            Dictionary<string, ObservableCollection<string>> collectionNameColumnsTables, Dictionary<string, Dictionary<string, string>> collectionNameColumnsTablesWithType)
        {
            Element = element;
            CollectionNameTables = collectionNameTables;
            CollectionNameColumnsTables = collectionNameColumnsTables;
            CollectionNameColumnsTablesWithType = collectionNameColumnsTablesWithType;
            NameTable = element.FullPattern.Pattern.Name;
            Columns = RewriteFormatCollectionList();
            NameColumns = CreateCollectionNameColumns();
        }

        public ICommand RewriteNameTable
        {
            get
            {
                return new CommandDelegate(parameter =>
                {
                    Element.FullPattern.Pattern.Name = NameTable;
                    string upJson = JsonSerializer.Serialize(Element.FullPattern.Pattern);
                    File.WriteAllText(Element.FullPattern.WayJson, upJson);
                });
            }
        }
        public ICommand AddColumn
        {
            get
            {
                return new CommandDelegate(parameter =>
                {
                    if (IsHaveError(CheckError.ErrorEmptyString(NumberColumns)) && IsHaveError(CheckError.InputErrorInt(NumberColumns)))
                    {
                        int numberColumn = int.Parse(NumberColumns);
                        for (int i = 0; i < numberColumn; i++)
                        {
                            Columns.Add(new ItemTextBox("", CollectionNameTables, CollectionNameColumnsTables));
                            NameColumns.Add("");
                        }
                    }
                });
            }
        }
        public ICommand DeleteColumn
        {
            get
            {
                return new CommandDelegate(parameter =>
                {
                    for (int i = 0; i < Columns.Count; i++)
                    {
                        if (!Element.Table.Columns.Contains(Columns[i].Name))
                        {
                            ContentErrorWindow = "Сперва сохраните добавленные столбцы.";
                            break;
                        }
                        if (Columns[i].Name.CompareTo(SelectedColumn) == 0 && IsCanDeleteColumn(Columns[i]))
                        {
                            NameColumns.RemoveAt(i);
                            Element.FullPattern.Pattern.Properties.Remove(Columns[i].Name);
                            Element.Table.Columns.RemoveAt(i);
                            Columns.RemoveAt(i);
                        }
                    }
                });
            }
        }
        public ICommand RewriteColumnTable
        {
            get
            {
                return new CommandDelegate(parameter =>
                {
                    var rewritedTableAndProperies = RewriteTableAndProps();
                    RewriteJson(rewritedTableAndProperies);
                });
            }
        }

        private ObservableCollection<ItemTextBox> RewriteFormatCollectionList()
        {
            ObservableCollection<ItemTextBox> copyCollection = new ObservableCollection<ItemTextBox>();
            foreach (PatternPropertyDB property in Element.FullPattern.Pattern.Properties.Values)
            {
                if (CollectionNameColumnsTables != null && property.ForeignTable !=null)
                {
                    ItemTextBox copyItem = new ItemTextBox(property.Name, property.Type, property.IsPrimaryKey,property.IsForeignKey, CollectionNameTables,
                    property.ForeignTable, CollectionNameColumnsTables[property.ForeignTable], property.ForeignColumn, CollectionNameColumnsTables);
                    copyCollection.Add(copyItem);
                }
                else
                {
                    ItemTextBox copyItem = new ItemTextBox(property.Name, property.Type, property.IsPrimaryKey ,property.IsForeignKey, CollectionNameTables,
                    property.ForeignTable, null, property.ForeignColumn, CollectionNameColumnsTables);
                    copyCollection.Add(copyItem);
                }
                
            }
            return copyCollection;
        }
        private ObservableCollection<string> CreateCollectionNameColumns()
        {
            ObservableCollection<string> names = new ObservableCollection<string>();
            foreach(PatternPropertyDB prop in Element.FullPattern.Pattern.Properties.Values)
            {
                names.Add(prop.Name);
            }
            return names;
        }
        private (DataTable, Dictionary<string, PatternPropertyDB>) RewriteTableAndProps()
        {
            DataTable rewriteTable = new DataTable(Element.Table.TableName);
            Dictionary<string, PatternPropertyDB> rewriteProp = new Dictionary<string, PatternPropertyDB>();
            if (IsHaveError(CheckError.ErrorUniquenessPrimaryKey(CreateListForCheckStatePrimaryKeys())))
            {
                foreach (ItemTextBox contentColumn in Columns)
                {
                    if (IsCorrectColumns(contentColumn))
                    {
                        WriteAnotherTableAboutForeignKey(contentColumn);
                        DataColumn column = new DataColumn(contentColumn.Name);
                        rewriteProp[contentColumn.Name] = new PatternPropertyDB(contentColumn.Name, contentColumn.SelectedType, contentColumn.IsPrimaryKey, contentColumn.IsForeignKey,
                            contentColumn.SelectedNameTable, contentColumn.SelectedNameColumn);
                        rewriteTable.Columns.Add(column);
                    }
                    else { break; }
                }
            }
            return (rewriteTable, rewriteProp);
        }
        private List<bool> CreateListForCheckStatePrimaryKeys()
        {
            List<bool> states = new List<bool>();
            foreach (ItemTextBox column in Columns)
                states.Add(column.IsPrimaryKey);
            return states;
        }
        private void WriteAnotherTableAboutForeignKey(ItemTextBox contentColumn)
        {
            if (contentColumn.IsForeignKey)
            {
                foreach (ElementDB element in ElementDBs)
                {
                    if (element.FullPattern.Pattern.Name.CompareTo(contentColumn.SelectedNameTable) == 0)
                    {
                        element.IsNeedUpdate = true;
                        foreach (PatternPropertyDB prop in element.FullPattern.Pattern.Properties.Values)
                        {
                            if (prop.Name.CompareTo(contentColumn.SelectedNameColumn) == 0)
                            {
                                if (prop.ReferencingTables == null)
                                    prop.ReferencingTables = new List<string>();
                                prop.ReferencingTables.Add(NameTable);
                            }
                        }
                    }
                }
            }
        }
        private void RewriteJson((DataTable, Dictionary<string, PatternPropertyDB>) rewritedTableAndProperies)
        {
            if (IsHaveError())
            {
                Element.FullPattern.Pattern = new PatternObjectDB(NameTable, rewritedTableAndProperies.Item2);
                Element.Table = rewritedTableAndProperies.Item1;

                string upJson = JsonSerializer.Serialize(Element.FullPattern.Pattern);
                File.WriteAllText(Element.FullPattern.WayJson, upJson);
                ContentErrorWindow = "Сохранено.";
            }
        }
        private bool IsCanDeleteColumn(ItemTextBox column)
        {
            if (column.IsPrimaryKey && Element.FullPattern.Pattern.Properties[column.Name].ReferencingTables.Count != 0 && IsStillForeignColumn(column.Name))
                ContentErrorWindow = "Нельзя удалять колонку если на неё ссылаются другие таблицы.";
            
            return IsHaveError();
        }
        private bool IsStillForeignColumn(string nameColumn)
        {
            foreach(PatternPropertyDB prop in Element.FullPattern.Pattern.Properties.Values)
            {
                if(prop.ReferencingTables != null && prop.ReferencingTables.Count !=0)
                {
                    foreach(string nameReferensing in prop.ReferencingTables)
                    {
                        foreach(ElementDB element in ElementDBs)
                        {
                            if(element.FullPattern.Pattern.Name.CompareTo(nameReferensing) == 0)
                            {
                                foreach(PatternPropertyDB propSearh in element.FullPattern.Pattern.Properties.Values)
                                {
                                    if(propSearh.ReferencingTables != null && propSearh.ForeignTable.CompareTo(Element.FullPattern.Pattern.Name) == 0 && propSearh.ForeignColumn.CompareTo(nameColumn) == 0)
                                    {
                                        prop.ReferencingTables.Remove(nameReferensing);
                                        return true;
                                    }
                                        
                                }
                            }
                        }
                    }
                }
            }
            return false;
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
                            && IsHaveError(CheckError.ErrorPrimaryKeyIsNotForeign(contentColumn.IsPrimaryKey, contentColumn.IsForeignKey))
                            && IsHaveError(CheckError.ErrorPrimaryKeyAndReferencingTables(contentColumn.Name, contentColumn.IsPrimaryKey, Element.FullPattern.Pattern.Properties, contentColumn.Name, IsStillForeignColumn(contentColumn.Name)));
        }
        private bool IsHaveError(string state = null)
        {
            if (state != null)
                ContentErrorWindow = state;
            return ContentErrorWindow.CompareTo(CheckError.NotError) == 0;
        }
    }
}
