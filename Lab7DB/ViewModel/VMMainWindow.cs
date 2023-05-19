using Core0;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Printing;
using System.Text.Json;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;

namespace Lab7DB.ViewModel
{
    public class VMMainWindow: MyViewModel
    {
        private ObservableCollection<ElementDB> ElementDBs = new ObservableCollection<ElementDB>();
        private VMCreateWindow VmCreateWindow;
        private VMRewriteTableWindow VmRewriteTableWindow;
        private VMRewriteDataWindow VmRewriteDataWindow;
        private Dictionary<string, DataTable> ForeignColumnWithTable = new Dictionary<string, DataTable>();
        private Dictionary<string, ObservableCollection<string>> ContentForeingColumns = new Dictionary<string, ObservableCollection<string>>();
        private Dictionary<string, DataTable> ForeignCellWithLine = new Dictionary<string, DataTable>();

        private string contentErrorWindow = CheckError.NotError;
        private ObservableCollection<BaseItem> treeElement = new ObservableCollection<BaseItem>();
        private ObservableCollection<string> collectionNameTables = new ObservableCollection<string>();
        private string selectedNameTable;
        private DataTable selectedTable = new DataTable();
        private ObservableCollection<string> nameColumnWithForeignKey = new ObservableCollection<string>();
        private string selectedNameColumnWithForeignKey;
        private DataTable tableForeing = new DataTable();
        private ObservableCollection<string> cellsForeingColumn = new ObservableCollection<string>();
        private string selectedCellsForeing;

        public string ContentErrorWindow
        {
            get { return contentErrorWindow; }
            set
            {
                contentErrorWindow = value;
                OnPropertyChanged(nameof(ContentErrorWindow));
            }
        }
        public ObservableCollection<BaseItem> TreeElement
        {
            get { return treeElement; }
            set
            {
                treeElement = value;
                OnPropertyChanged(nameof(TreeElement));
            }
        }
        public ObservableCollection<string> CollectionNameTables
        {
            get { return collectionNameTables; }
            set
            {
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
        public DataTable SelectedTable
        {
            get { return selectedTable; }
            set
            {
                selectedTable = value;
                OnPropertyChanged(nameof(SelectedTable));
            }
        }
        public ObservableCollection<string> NameColumnWithForeignKey
        {
            get { return nameColumnWithForeignKey; }
            set
            {
                nameColumnWithForeignKey = value;
                OnPropertyChanged(nameof(NameColumnWithForeignKey));
            }
        }
        public string SelectedNameColumnWithForeignKey
        {
            get { return selectedNameColumnWithForeignKey; }
            set
            {
                selectedNameColumnWithForeignKey = value;
                OnPropertyChanged(nameof(SelectedNameColumnWithForeignKey));
            }
        }
        public DataTable TableForeing
        {
            get { return tableForeing; }
            set
            {
                tableForeing = value;
                OnPropertyChanged(nameof(TableForeing));
            }
        }
        public ObservableCollection<string> CellsForeingColumn
        {
            get { return cellsForeingColumn; }
            set
            {
                cellsForeingColumn = value;
                OnPropertyChanged(nameof(CellsForeingColumn));
            }
        }
        public string SelectedCellsForeing
        {
            get { return selectedCellsForeing; }
            set
            {
                selectedCellsForeing = value;
                OnPropertyChanged(nameof(SelectedCellsForeing));
            }
        }

        public ICommand CreateTable
        {
            get 
            {
                return new CommandDelegate(parametr =>
                {
                    string folder = CallFolderBrowserDialog();
                    if (!string.IsNullOrEmpty(folder)) 
                    {
                        ConnectDataForVMCreate(folder);

                        CreateWindow createWindow = new CreateWindow();
                        VmCreateWindow.CreateWindow = createWindow;
                        VmCreateWindow.ElementDBs = ElementDBs;
                        createWindow.DataContext = VmCreateWindow;
                        createWindow.Show();
                    }
                });
            }
        }
        public ICommand AddFiles
        {
            get
            {
                return new CommandDelegate(parameter =>
                {
                    string folder = CallFolderBrowserDialog();
                    if (!string.IsNullOrEmpty(folder)) 
                    {
                        LoaderFiles model = new LoaderFiles(folder);
                        ContentErrorWindow = model.State;
                        ElementDBs = model.Elements;
                        if (IsHaveError())
                            CreateViewModel();
                    }
                });
            }
        }
        public ICommand Clear
        {
            get
            {
                return new CommandDelegate(parameter =>
                {
                    TreeElement.Clear();
                    CollectionNameTables.Clear();
                    ElementDBs = new ObservableCollection<ElementDB>();
                    SelectedTable = new DataTable();
                    TableForeing = new DataTable();
                });
            }
        }
        public ICommand Update
        {
            get
            {
                return new CommandDelegate(parameter =>
                {
                    SelectedTable = new DataTable();

                    if (VmCreateWindow != null && VmCreateWindow.NewElement != null)
                        UpdateFromCreateWindow();

                    if (VmRewriteTableWindow != null && VmRewriteTableWindow.Element != null)
                        UpdateFromRewriteTableWindow();

                    if (VmRewriteDataWindow != null && VmRewriteDataWindow.Element != null)
                        UpdateFromRewriteDataWindow();
                });
            }
        }
        public ICommand RequestWindow
        {
            get 
            {
                return new CommandDelegate(parameter => 
                {
                    VMRequestWindow vmRequestWindow = new VMRequestWindow(ElementDBs);

                    RequestWindow requestWindow = new RequestWindow();
                    requestWindow.DataContext = vmRequestWindow;
                    requestWindow.Show();
                });
            }
        }

        public ICommand OutputTable
        {
            get
            {
                return new CommandDelegate(parameter =>
                {
                    TableForeing = new DataTable();
                    string namePattern = SelectedNameTable;
                    foreach (ElementDB element in ElementDBs)
                    {
                        if (element.Table.TableName.CompareTo(namePattern) == 0)
                        {
                            SelectedTable = element.Table;
                        }
                    }
                    CreateContentForViewForeign();
                });
            }
        }
        public ICommand RewriteTable
        {
            get
            {
                return new CommandDelegate(parameter =>
                {
                    string namePattern = SelectedNameTable;
                    foreach (ElementDB element in ElementDBs)
                    {
                        if (element.Table.TableName.CompareTo(namePattern) == 0)
                        {
                            if(element.Table.Rows.Count != 0)
                            {
                                ContentErrorWindow = "Нельзя редактировать таблицу, если в ней есть данные.";
                                break;
                            }
                            ConnectDataForVMRewriteTable(element);

                            RewriteTableWindow rewriteTableWindow = new RewriteTableWindow();
                            rewriteTableWindow.DataContext = VmRewriteTableWindow;
                            rewriteTableWindow.Show();
                        }    
                    }
                });
            }
        }
        public ICommand RewriteData
        {
            get
            {
                return new CommandDelegate(parameter =>
                {
                    string namePattern = SelectedNameTable;
                    foreach (ElementDB element in ElementDBs)
                    {
                        if (element.Table.TableName.CompareTo(namePattern) == 0)
                        {
                            VmRewriteDataWindow = new VMRewriteDataWindow(element, ElementDBs);
                            RewriteDataWindow rewriteDataWindow = new RewriteDataWindow();
                            rewriteDataWindow.DataContext = VmRewriteDataWindow;

                            rewriteDataWindow.Show();
                        }
                    }
                });
            }
        }

        public ICommand OutPutForeignTable
        {
            get
            {
                return new CommandDelegate(parameter =>
                {
                    TableForeing = ForeignColumnWithTable[SelectedNameColumnWithForeignKey];
                    CellsForeingColumn = ContentForeingColumns[SelectedNameColumnWithForeignKey];
                });
            }
        }
        public ICommand OutPutForeignLine
        {
            get
            {
                return new CommandDelegate(parameter =>
                {
                    TableForeing = ForeignCellWithLine[SelectedCellsForeing];
                });
            }
        }

        private string CallFolderBrowserDialog()
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            DialogResult result = dialog.ShowDialog();
            string folder = "";
            if (result == DialogResult.OK)
                folder = dialog.SelectedPath;

            return folder;
        }
        private void CreateViewModel()
        {
            foreach(ElementDB element in ElementDBs)
            {
                BaseItem itemTree = new BaseItem(element.FullPattern.Pattern.Name);
                DataTable table = new DataTable(element.FullPattern.Pattern.Name);

                foreach(PatternPropertyDB property in element.FullPattern.Pattern.Properties.Values)
                {
                    itemTree.Children.Add(new BaseItem(property.Name));
                    table.Columns.Add(new DataColumn(property.Name));
                }

                TreeElement.Add(itemTree);
                collectionNameTables.Add(element.FullPattern.Pattern.Name);
                table = CreateRowTable(table, element);
                element.Table = table;
            }
        }
        private DataTable CreateRowTable(DataTable table, ElementDB element)
        {
            if (element.Data != null)
            {
                foreach (ObjectDB objectDB in element.Data.Objects)
                {
                    DataRow row = table.NewRow();
                    int nuberCell = 0;
                    foreach (string text in objectDB.Property.Values)
                    {
                        row[nuberCell] = text;
                        nuberCell++;
                    }
                    table.Rows.Add(row);
                }
            }
            return table;
        }
        private ObservableCollection<string> CreateCollectNameTables(string nameExclusionTable = "")
        {
            ObservableCollection<string> collection = new ObservableCollection<string>();
            foreach (ElementDB element in ElementDBs)
            {
                if (element.FullPattern.Pattern.Name.CompareTo(nameExclusionTable) != 0)
                    collection.Add(element.FullPattern.Pattern.Name);
            }
            return collection;
        }
        private (Dictionary<string, ObservableCollection<string>>, Dictionary<string, Dictionary<string, string>>) CreateCollectColumnsTablesAndWithType()
        {
            Dictionary<string, ObservableCollection<string>> dictionary = new Dictionary<string, ObservableCollection<string>>();
            Dictionary<string, Dictionary<string, string>> dictionaryWithType = new Dictionary<string, Dictionary<string, string>>();
            foreach (ElementDB element in ElementDBs)
            {
                ObservableCollection<string> collect = new ObservableCollection<string>();
                Dictionary<string, string> dict = new Dictionary<string, string>();
                foreach (PatternPropertyDB column in element.FullPattern.Pattern.Properties.Values)
                {
                    if (column.IsPrimaryKey)
                    {
                        collect.Add(column.Name);
                        dict[column.Name] = column.Type;
                    }
                }
                dictionary[element.FullPattern.Pattern.Name] = collect;
                dictionaryWithType[element.FullPattern.Pattern.Name] = dict;
            }
            return (dictionary, dictionaryWithType);
        }
        private BaseItem RewriteBaseItem(DataTable table)
        {
            BaseItem item = new BaseItem(table.TableName);
            foreach (DataColumn column in table.Columns)
            {
                BaseItem subItem = new BaseItem(column.ToString());
                item.Children.Add(subItem);
            }
            return item;
        }
        private void UpdateFromCreateWindow()
        {
            ElementDBs = UpdateForElementsForReasonLink(VmCreateWindow.ElementDBs);
            ElementDBs.Add(VmCreateWindow.NewElement);
            TreeElement.Add(RewriteBaseItem(VmCreateWindow.NewElement.Table));
            CollectionNameTables.Add(VmCreateWindow.NewElement.FullPattern.Pattern.Name);

            VmCreateWindow.NewElement = null;
        }
        private void UpdateFromRewriteTableWindow()
        {
            ElementDBs = UpdateForElementsForReasonLink(VmRewriteTableWindow.ElementDBs);
            for (int i = 0; i < ElementDBs.Count; i++)
            {
                if (ElementDBs[i].Table.TableName.CompareTo(SelectedNameTable) == 0)
                {
                    ElementDBs[i].Table = VmRewriteTableWindow.Element.Table;
                    TreeElement[i] = RewriteBaseItem(ElementDBs[i].Table);
                    CollectionNameTables[i] = ElementDBs[i].Table.TableName;
                    VmRewriteTableWindow.Element = null;
                }
            }
        }
        private void UpdateFromRewriteDataWindow()
        {
            for (int i = 0; i < ElementDBs.Count; i++)
            {
                if (ElementDBs[i].Table.TableName.CompareTo(SelectedNameTable) == 0)
                {
                    ElementDBs[i].Table = VmRewriteDataWindow.Element.Table;
                    TreeElement[i] = RewriteBaseItem(ElementDBs[i].Table);
                    CollectionNameTables[i] = ElementDBs[i].Table.TableName;
                    VmRewriteDataWindow.Element = null;
                }
            }
        }
        private ObservableCollection<ElementDB> UpdateForElementsForReasonLink(ObservableCollection<ElementDB> elements)
        {
            ObservableCollection<ElementDB> updateElements = new ObservableCollection<ElementDB>();
            foreach(ElementDB element in elements)
            {
                if(element.IsNeedUpdate)
                {
                    RewriteJson(element);
                    element.IsNeedUpdate = false;
                }
                updateElements.Add(element);
            }
            return updateElements;
        }
        private void RewriteJson(ElementDB element)
        {
            string json = JsonSerializer.Serialize(element.FullPattern.Pattern);
            File.WriteAllText(element.FullPattern.WayJson, json);
        }
        private void ConnectDataForVMCreate(string folder)
        {
            VmCreateWindow = new VMCreateWindow();
            VmCreateWindow.FullFolderPath = folder;
            VmCreateWindow.CollectionNameTables = CreateCollectNameTables();
            var tuple = CreateCollectColumnsTablesAndWithType();
            VmCreateWindow.CollectionNameColumnsTables = tuple.Item1;
            VmCreateWindow.CollectionNameColumnsTablesWithType = tuple.Item2;
        }
        private void ConnectDataForVMRewriteTable(ElementDB element)
        {
            var tuple = CreateCollectColumnsTablesAndWithType();
            VmRewriteTableWindow = new VMRewriteTableWindow(element, CreateCollectNameTables(element.FullPattern.Pattern.Name),
                tuple.Item1, tuple.Item2);
            VmRewriteTableWindow.ElementDBs = ElementDBs;
        }

        private void CreateContentForViewForeign()
        {
            foreach(ElementDB element in ElementDBs)
            {
                if(element.FullPattern.Pattern.Name.CompareTo(SelectedNameTable) == 0)
                {
                    CreateCollectColumnWithForeignKey(element);
                }
            }
        }
        private void CreateCollectColumnWithForeignKey(ElementDB element)
        {
            ObservableCollection<string> nameColumnWithForeign = new ObservableCollection<string>();
            int index = 0;
            foreach(PatternPropertyDB prop in element.FullPattern.Pattern.Properties.Values)
            {
                if(prop.IsForeignKey)
                {
                    nameColumnWithForeign.Add(prop.Name);
                    ElementDB searchedElement = SearchElement(prop.ForeignTable);
                    ForeignColumnWithTable[prop.Name] = searchedElement.Table;
                    CreateCollectRowFromForeignColumn(searchedElement, index, prop.Name);
                }
                index++;
            }
            NameColumnWithForeignKey = nameColumnWithForeign;
        }
        private ElementDB SearchElement(string nameTable)
        {
            foreach(ElementDB element in ElementDBs)
            {
                if(element.FullPattern.Pattern.Name.CompareTo(nameTable) == 0)
                {
                    return element;
                }
            }
            return new ElementDB();
        }
        private void CreateCollectRowFromForeignColumn(ElementDB searchedElement, int index, string nameColunm)
        {
            ObservableCollection<string> cells = new ObservableCollection<string>();
            foreach(DataRow row in SelectedTable.Rows)
            {
                cells.Add(row[index].ToString());
                ForeignCellWithLine[row[index].ToString()] = CreateCollectForeignCellWithLine(row[index].ToString(), searchedElement);
            }
            ContentForeingColumns[nameColunm] = cells;
        }
        private DataTable CreateCollectForeignCellWithLine(string cell, ElementDB searchedElement)
        {
            DataTable tableForeingLine = CopyColumns(searchedElement.Table);
            int indexColumn = 0;
            foreach(PatternPropertyDB column in searchedElement.FullPattern.Pattern.Properties.Values)
            {
                if(column.IsPrimaryKey)
                {
                    foreach(DataRow row in searchedElement.Table.Rows)
                    {
                        if (row[indexColumn].ToString().CompareTo(cell) == 0)
                        {
                            DataRow newRow = tableForeingLine.NewRow();
                            newRow = CopyRow(newRow, row);
                            tableForeingLine.Rows.Add(newRow);
                        }
                    }
                }
                indexColumn++;
            }
            return tableForeingLine;
        }
        private DataRow CopyRow(DataRow copyRow, DataRow row)
        {
            for(int i = 0; i < row.ItemArray.Length; i++)
            {
                copyRow[i] = row[i].ToString();
            }
            return copyRow;
        }
        private DataTable CopyColumns(DataTable tableForeign)
        {
            DataTable table = new DataTable();
            foreach(DataColumn column in tableForeign.Columns)
            {
                DataColumn newColumn = new DataColumn(column.ColumnName);
                table.Columns.Add(newColumn);
            }
            return table;
        }


        private bool IsHaveError(string state = null)
        {
            if(state != null)
                ContentErrorWindow = state;
            return ContentErrorWindow.CompareTo(CheckError.NotError) == 0;
        }
    }
}
