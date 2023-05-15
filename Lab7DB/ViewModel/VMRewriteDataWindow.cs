using Core0;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Linq;

namespace Lab7DB.ViewModel
{
    public class VMRewriteDataWindow: MyViewModel
    {
        public string NameTable { get; set; }
        public ElementDB Element { get; set; }
        public ObservableCollection<ElementDB> ElementDBs { get; set; }

        private string сontentErrorWindow = CheckError.NotError;
        private DataTable selectedTable;
        private ObservableCollection<string> collectDelete;
        private string selectedDelete;

        public string ContentErrorWindow
        {
            get { return сontentErrorWindow; }
            set
            {
                сontentErrorWindow = value;
                OnPropertyChanged(nameof(ContentErrorWindow));
            }
        }
        public DataTable SelectedTable
        {
            get{ return selectedTable; }
            set
            {
                selectedTable = value;
                OnPropertyChanged(nameof(SelectedTable));
            }
        }
        public ObservableCollection<string> CollectDelete
        {
            get { return collectDelete; }
            set
            {
                collectDelete = value;
                OnPropertyChanged(nameof(CollectDelete));
            }
        }
        public string SelectedDelete
        {
            get { return selectedDelete; }
            set
            {
                selectedDelete = value;
                OnPropertyChanged(nameof(SelectedDelete));
            }
        }


        public VMRewriteDataWindow(ElementDB element, ObservableCollection<ElementDB> elementDBs)
        {
            Element = element;
            NameTable = Element.FullPattern.Pattern.Name;
            SelectedTable = element.Table;
            CollectDelete = CreateCollectionDelete();
            ElementDBs = elementDBs;
        }

        public ICommand Delete
        {
            get
            {
                return new CommandDelegate(parameter =>
                {
                    DataTable rewriteTable = new DataTable(SelectedTable.TableName);
                    rewriteTable = CopyColumns(rewriteTable);
                    foreach (DataRow row in SelectedTable.Rows)
                    {
                        if (row[0].ToString().CompareTo(SelectedDelete) == 0 && IsCanDelete(row))
                            continue;
                        rewriteTable = CopyRow(row, rewriteTable);
                    }
                    SelectedTable = rewriteTable;
                    Element.Table = rewriteTable;
                    //CreateCSV();
                });
            }
        }
        public ICommand Save
        {
            get
            {
                return new CommandDelegate(parameter =>
                {
                    Element.Table = SelectedTable;
                    CollectDelete = CreateCollectionDelete();
                    CreateCSV();
                });
            }
        }

        private ObservableCollection<string> CreateCollectionDelete()
        {
            ObservableCollection<string> collect = new ObservableCollection<string>();
            foreach (DataRow row in Element.Table.Rows)
            {
                collect.Add(row[0].ToString());
            }
            return collect;
        }
        private DataTable CopyColumns(DataTable table)
        {
            foreach (DataColumn column in SelectedTable.Columns)
            {
                table.Columns.Add(column.ColumnName);
            }
            return table;
        }
        private DataTable CopyRow(DataRow row, DataTable table)
        {
            DataRow newRow = table.NewRow();
            foreach (DataColumn column in table.Columns)
            {
                newRow[column.ColumnName] = row[column.ColumnName];
            }
            table.Rows.Add(newRow);
            return table;
        }
        private void CreateCSV()
        {
            string[] lines = new string[SelectedTable.Rows.Count];
            string[][] cellsForElement = new string[SelectedTable.Rows.Count][];
            for (int i = 0; i < lines.Length; i++)
            {
                string[] elements = CreateElementsInLine(i);

                if (!(IsHaveError() && IsHaveError(CheckError.InputErrorProperties(elements, Element.FullPattern.Pattern))))
                    break;

                string line = string.Join(';', elements);
                lines[i] = line;
                cellsForElement[i] = elements;
            }
            if (IsHaveError())
            {
                SaveDataInElement(cellsForElement);
                SaveCSV(lines);
            }
        }
        private string[] CreateElementsInLine(int index)
        {
            DataRow row = SelectedTable.Rows[index];
            int countEl = Element.FullPattern.Pattern.Properties.Count;
            string[] elements = new string[countEl];
            string[] namesColumns = RewriteNamePropFormat();

            for (int j = 0; j < countEl; j++)
            {
                elements[j] = row[j].ToString();
                PatternPropertyDB columnParameters = Element.FullPattern.Pattern.Properties[namesColumns[j]];
                if (!(/*IsHaveError() && */IsHaveError(CheckError.ErrorEmptyString(elements[j])) 
                    && IsHaveError(CheckError.ErrorContentCellIsForeignKey(elements[j], columnParameters.IsForeignKey, 
                    CreateListContentLinkColumn(columnParameters.ForeignTable, columnParameters.ForeignColumn), columnParameters.ForeignTable, columnParameters.ForeignColumn))
                    && IsHaveError(CheckError.ErrorOrderInColumnPrimary(columnParameters.IsPrimaryKey,elements[j], index + 1))))
                    break;
            }
            return elements;
        }
        private string[] RewriteNamePropFormat()
        {
            string[] names = new string[Element.FullPattern.Pattern.Properties.Count];
            int i = 0;
            foreach(string name in Element.FullPattern.Pattern.Properties.Keys) 
            {
                names[i] = name;
                i++;
            }
            return names;
        }
        private List<string> CreateListContentLinkColumn(string nameTable, string nameColumn)
        {
            List<string> listContent = new List<string>();
            foreach(ElementDB element in ElementDBs)
            {
                if(element.FullPattern.Pattern.Name.CompareTo(nameTable) == 0)
                {
                    foreach(DataRow row in element.Table.Rows)
                    {
                        listContent.Add(row[nameColumn].ToString());
                    }
                    return listContent;
                }
            }
            return listContent;
        }
        private void SaveDataInElement(string[][] cells)
        {
            ObjectDB[] objects = new ObjectDB[cells.Length];
            for(int i = 0; i < cells.Length; i++)
            {
                ObjectDB objectDB = new ObjectDB(Element.FullPattern.Pattern, cells[i]);
                objects[i] = objectDB;
            }

            if (Element.Data == null)
                Element.Data = new AdditionalDataObject();
            Element.Data.Objects = objects;
        }
        private void SaveCSV(string[] lines)
        {
            if (IsHaveError())
            {
                if (Element.Data.WayCSV == null)
                {
                    string newWayCSV = Element.FullPattern.WayJson.Substring(0, Element.FullPattern.WayJson.LastIndexOf('\\')) + "\\" + Element.Table.TableName + ".csv";
                    Element.Data.WayCSV = newWayCSV;
                }
                File.WriteAllLines(Element.Data.WayCSV, lines);
                ContentErrorWindow = "Сохранено.";
            }
        }

        public bool IsCanDelete(DataRow rowDelete)
        {
            int indPropDelete = 0;
            foreach (PatternPropertyDB prop in Element.FullPattern.Pattern.Properties.Values)
            {
                if(prop.IsPrimaryKey && prop.ReferencingTables != null && prop.ReferencingTables.Count != 0)
                {
                    foreach(string nameTable in prop.ReferencingTables)
                    {
                        foreach(ElementDB element in ElementDBs)
                        {
                            if(element.FullPattern.Pattern.Name.CompareTo(nameTable) == 0)
                            {
                                int indProp = 0;
                                foreach(PatternPropertyDB propEl in element.FullPattern.Pattern.Properties.Values)
                                {
                                    if(propEl.IsForeignKey && propEl.ForeignTable.CompareTo(Element.FullPattern.Pattern.Name) == 0)
                                    {
                                        foreach(DataRow row in element.Table.Rows)
                                        {
                                            if (row[indProp].ToString().CompareTo(rowDelete[indPropDelete].ToString()) == 0)
                                            {
                                                ContentErrorWindow = "Нельзя удалять стоку на которрую ссылаются \nдругие таблицы.";
                                                return false;
                                            }    
                                        }
                                    }
                                    indProp++;
                                }    
                            }
                        }
                    }
                }
                indPropDelete++;
            }
            return true;
        }
        private bool IsHaveError(string state = null)
        {
            if (state != null)
                ContentErrorWindow = state;
            return ContentErrorWindow.CompareTo(CheckError.NotError) == 0;
        }
    }
}
