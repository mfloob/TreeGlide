using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace TreeGlide
{
    public class Logger
    {
        private ItemsControl itemsControl;

        public Logger(ItemsControl itemsControl)
        {
            this.itemsControl = itemsControl;
        }

        public void Log(string text)
        {
            itemsControl.Items.Add(text);
        }
    }
}
