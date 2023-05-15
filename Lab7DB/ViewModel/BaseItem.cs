using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab7DB.ViewModel
{
    public class BaseItem : MyViewModel
    {
        private string name;
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        private ObservableCollection<BaseItem> children = new ObservableCollection<BaseItem>();

        public ObservableCollection<BaseItem> Children
        {
            get { return children; }
            set
            {
                children = value;
                OnPropertyChanged(nameof(Children));
            }
        }


        public BaseItem(string nameItem)
        {
            Name = nameItem;
        }
    }
}
