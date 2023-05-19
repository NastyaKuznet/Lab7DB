using Core0;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab7DB.ViewModel
{
    public class Request
    {
        private ObservableCollection<ElementDB> elements;
        private ObservableCollection<ElementDB> workElements = new ObservableCollection<ElementDB>();
        private DataTable tableJoin;
        public DataTable TableGet { get; private set; }
        public string State { get; set; }
        public Request(string textRequest, ObservableCollection<ElementDB> elementDBs) 
        {
            State = CheckError.NotError;
            elements = elementDBs;

            ProcessRequest(textRequest);
        }

        private void ProcessRequest(string textRequest) 
        {
            string[] operations = textRequest.Split('\n');
            foreach (string operation in operations) 
            {
                if (!IsHaveError()) break;
                string[] elementsOperation = operation.Split(' ');
                if(!string.IsNullOrEmpty(operation))
                    NavigateCommand(elementsOperation);
            }
        }
        private void NavigateCommand(string[] elementsOperation)
        {
            switch(elementsOperation[0])
            {
                case "TABLES":
                    DoOperationTABLE(elementsOperation);
                    break;
                case "JOIN":
                    DoOperationJOIN(elementsOperation);
                    break;
                case "GET":
                    DoOperationGET(elementsOperation);
                    break;
                default:
                    State = "Неизвестная операция";
                    break;
            }
        }
        private void DoOperationTABLE(string[] elementsOperation)
        {
            for(int i = 1; i < elementsOperation.Length; i++)
            {
                string elementOp = elementsOperation[i].Trim(',');
                ElementDB searhElement = SearchElementByName(elementOp, elements);
                if (!IsHaveError()) break;
                workElements.Add(searhElement);
            }
        }
        private void DoOperationJOIN(string[] elementsOperation)
        {
            if (IsHaveError(CheckSpecialSymbolJOIN(elementsOperation)))
            {
                string baseName = elementsOperation[1];
                string subName = elementsOperation[3];
                string[] connect = elementsOperation[5].Split('=');
                string baseCondition = tableJoin == null ? connect[0].Split('.')[1] : string.Join('-', connect[0].Split('.'));
                string subCondition = connect[1].Split('.')[1];

                DataTable baseTable = tableJoin == null ? SearchElementByName(baseName, workElements).Table : tableJoin;
                DataTable subTable = SearchElementByName(subName, workElements).Table;

                if (IsHaveError())
                {
                    int baseIndexColumn = SearchIndexColumnByName(baseCondition, baseTable);
                    int subIndexColumn = SearchIndexColumnByName(subCondition, subTable);
                    if(IsHaveError(CheckContentColumnInTable(baseIndexColumn)) && IsHaveError(CheckContentColumnInTable(subIndexColumn)))
                        tableJoin = CreateNewTableFromJoin(baseTable, subTable, baseIndexColumn, subIndexColumn);
                }
            }
        }
        private void DoOperationGET(string[] elementsOperation)
        {
            List<string> namesColumns = CreateListNeededNameColumn(elementsOperation);
            var tuple = AddNeededColumn(tableJoin, namesColumns);
            if (IsHaveError())
            {
                DataTable newTable = tuple.Item1;
                List<int> indexesColumns = tuple.Item2;
                TableGet = AddRows(newTable, indexesColumns);
                TableGet.TableName = "table";
            }
        }

        private ElementDB SearchElementByName(string nameElement, ObservableCollection<ElementDB> _elements)
        {
            foreach (ElementDB element in _elements)
            {
                if (element.FullPattern.Pattern.Name.CompareTo(nameElement) == 0)
                {
                    return element;
                }
            }
            State = $"Нет таблицы {nameElement}";
            return null;
        }
        private int SearchIndexColumnByName(string nameColumn, DataTable table)
        {
            int index = 0;
            foreach (DataColumn columnBase in table.Columns)
            {
                if (columnBase.ColumnName.CompareTo(nameColumn) == 0)
                {
                    return index;
                }
                index++;
            }
            return -1;
        }
        private DataRow SearchRowByCondition(string contentCell, DataTable table, int indexColumn)
        {
            foreach(DataRow row in table.Rows) 
            {
                if (row[indexColumn].ToString().CompareTo(contentCell) == 0)
                {
                    return row;
                }
            }
            return null;
        }
        private DataTable CreateNewTableFromJoin(DataTable baseTable, DataTable subTable, int baseIndexColumn, int subIndexColumn)
        {
            DataTable newTable = ConnectColumnTable(baseTable, subTable);
            newTable = CreateNewRow(baseTable, subTable, baseIndexColumn, subIndexColumn, newTable);
            return newTable;
        }
        private DataTable ConnectColumnTable(DataTable baseTable, DataTable subTable)
        {
            DataTable newTable = new DataTable();
            newTable = AddColimnsFromDifferentTable(newTable, baseTable);
            newTable = AddColimnsFromDifferentTable(newTable, subTable);
            return newTable;
        }
        private DataTable AddColimnsFromDifferentTable(DataTable baseTable, DataTable diffTable)
        {
            foreach(DataColumn column in diffTable.Columns)
            {
                DataColumn newColumn = new DataColumn();
                if (diffTable != tableJoin)
                    newColumn.ColumnName = diffTable.TableName+ "-" + column.ColumnName;//
                else
                    newColumn.ColumnName = column.ColumnName;
                baseTable.Columns.Add(newColumn);
            }
            return baseTable;
        }
        private DataRow AddNewRow(DataRow baseRow, DataRow subRow, DataTable newTable)
        {
            DataRow newRow = newTable.NewRow();
            newRow = AddCellFromDifferentRow(newRow, baseRow);
            int indexAdd = baseRow.ItemArray.Length;
            newRow = AddCellFromDifferentRow(newRow, subRow, indexAdd);
            return newRow;
        }
        private DataRow AddCellFromDifferentRow(DataRow newRow, DataRow diffRow, int indexAdd = 0)
        {
            for(int i = indexAdd; i < diffRow.ItemArray.Length + indexAdd; i++)
            {
                newRow[i] = diffRow.ItemArray[i - indexAdd];
            }
            return newRow;
        }
        private DataTable CreateNewRow(DataTable baseTable, DataTable subTable, int baseIndexColumn, int subIndexColumn, DataTable newTable)
        {
            foreach (DataRow baseRow in baseTable.Rows)
            {
                DataRow subRow = SearchRowByCondition(baseRow[baseIndexColumn].ToString(), subTable, subIndexColumn);
                DataRow newRow = AddNewRow(baseRow, subRow, newTable);
                newTable.Rows.Add(newRow);
            }
            return newTable;
        }
        private string CreateInCorrectFormatNameColumn(string name)
        {
            string[] pathNewName = name.Trim(',').Split('(');
            return pathNewName[0] +"-" + pathNewName[1].Trim(')');
        }
        private List<string> CreateListNeededNameColumn(string[] elementsOperation)
        {
            List<string> namesColumns = new List<string>();
            List<string> parthName = new List<string>();
            for (int i = 1; i < elementsOperation.Length; i++)
            {
                if (IsOneSet(elementsOperation[i]))
                    namesColumns.Add(CreateInCorrectFormatNameColumn(elementsOperation[i]));
                else
                {
                    parthName.Add(elementsOperation[i]);
                    if (IsCloseSet(elementsOperation[i]))
                    {
                        namesColumns = CreateCorrectName(parthName, namesColumns);
                    }
                }
            }
            return namesColumns;
        }
        private bool IsOneSet(string str)
        { 
            return str.Contains('(') && str.Contains(")");
        }
        private bool IsOpenSet(string str)
        {
            return str.Contains('(');
        }
        private bool IsCloseSet(string str)
        {
            return str.Contains(")");
        }
        private List<string> CreateCorrectName(List<string> pathName, List<string> nameColumns)
        {
            string nameTable = " ";
            foreach(string path in pathName)
            {
                if(IsOpenSet(path))
                {
                    string[] content = path.Split('(');
                    nameTable = content[0];
                    nameColumns.Add(CreateInCorrectFormatNameColumn(path).Trim(' '));
                }
                else
                {
                    nameColumns.Add(nameTable + "-" + path.Trim(new char[] { ',', ')'}));
                }
            }
            return nameColumns;
        }
        private (DataTable, List<int>) AddNeededColumn(DataTable searchTable, List<string> namesColumns)
        {
            DataTable newTable = new DataTable();
            List<int> indexesColumns = new List<int>();
            int index = 0;
            int count = 0;
            foreach(DataColumn column in searchTable.Columns)
            {
                if(namesColumns.Contains(column.ColumnName))
                {
                    DataColumn newColumn = new DataColumn(column.ColumnName);
                    newTable.Columns.Add(newColumn);
                    indexesColumns.Add(index);
                    count++;
                }
                index++;
            }
            if (count != namesColumns.Count)
                State = "Не найдена нужная колонка.";
            return (newTable, indexesColumns);
        }
        private DataTable AddRows(DataTable newTable, List<int> indexesColumn)
        {
            foreach(DataRow row in tableJoin.Rows)
            {
                newTable.Rows.Add(CreateNewRowByIndexesColumns(newTable, row, indexesColumn));
            }
            return newTable;
        }
        private DataRow CreateNewRowByIndexesColumns(DataTable table, DataRow row, List<int> indexesColumn)
        { 
            DataRow newRow = table.NewRow();
            int indexNewRow = 0;
            for(int i = 0; i < row.ItemArray.Length; i++)
            {
                if (indexesColumn.Contains(i))
                {
                    newRow[indexNewRow] = row[i];
                    indexNewRow++;
                }
            }
            return newRow;
        }
        private string CheckSpecialSymbolJOIN(string[] str)
        {
            if (str[2].CompareTo("->") != 0 || str[4].CompareTo("on") != 0 || !str[5].Contains('='))
                return "Не хватает специальных символов => и(или) \non и(или) = в операции JOIN.";
            return CheckError.NotError;
        }
        private string CheckContentColumnInTable(int index = -1)
        {
            if (index == -1)
                return "Не найдена нужная колонка.";
            return CheckError.NotError;
        }
        private bool IsHaveError(string state = null)
        {
            if (state != null)
                State = state;
            return State.CompareTo(CheckError.NotError) == 0;
        }
    }
}
