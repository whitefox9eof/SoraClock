using System;
using System.ComponentModel;
using System.Windows.Controls;

namespace SoraClock
{
    class FontFilter
    {
        public FontFilter(ICollectionView collectionView, TextBox textBox)
        {
            string filterText = "";
            collectionView.Filter = delegate (object obj)
            {
                if (String.IsNullOrEmpty(filterText))
                {
                    return true;
                }
                string str = obj.ToString() as string;
                if (String.IsNullOrEmpty(str))
                {
                    return false;
                }
                int index = str.IndexOf(filterText, 0, StringComparison.InvariantCultureIgnoreCase);
                return index > -1;
            };

            textBox.TextChanged += delegate
            {
                filterText = textBox.Text;
                collectionView.Refresh();
            };
        }
    }
}
