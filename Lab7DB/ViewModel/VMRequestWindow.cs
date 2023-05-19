using Core0;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Lab7DB.ViewModel
{
    public class VMRequestWindow: MyViewModel
    {
        private ObservableCollection<ElementDB> elements;

        private DataTable tableOutPut;
        private string windowInPut;
        private string content = CheckError.NotError;

        public DataTable TableOutPut
        {
            get { return tableOutPut; }
            set 
            { 
                tableOutPut = value;
                OnPropertyChanged(nameof(TableOutPut));   
            }
        }
        public string WindowInPut
        {
            get { return windowInPut; }
            set
            {
                windowInPut = value;
                OnPropertyChanged(nameof(WindowInPut));
            }
        }
        public string Content
        {
            get { return content; }
            set
            {
                content = value;
                OnPropertyChanged(nameof(Content));
            }
        }

        public VMRequestWindow(ObservableCollection<ElementDB> elements)
        {
            this.elements = elements;
        }

        public ICommand Execute
        {
            get 
            {
                return new CommandDelegate(parameter => 
                {
                    Request request = new Request(WindowInPut, elements);
                    TableOutPut = request.TableGet;
                    Content = request.State;
                    if(Content.CompareTo(CheckError.NotError) == 0)
                        WindowInPut = "";
                });
            }
        }
    }
}
