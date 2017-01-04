/*****************************************************************************
 *
 * Copyright 2012-2016 SkillPro Consortium
 *
 * Author: Boris Bocquet, email: b.bocquet@akeoplus.com
 *
 * Date of creation: 2016
 *
 * +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
 *
 * This file is part of the SkillPro Framework. The SkillPro Framework
 * is developed in the SkillPro project, funded by the European FP7
 * programme (Grant Agreement 287733).
 *
 * The SkillPro Framework is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * The SkillPro Framework is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.
 * You should have received a copy of the GNU Lesser General Public License
 * along with the SkillPro Framework.  If not, see <http://www.gnu.org/licenses/>.
*****************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace simpleSEE.UI
{
    /// <summary>
    /// Logique d'interaction pour WindowSetYourProduct.xaml
    /// </summary>
    public partial class WindowSetYourProduct : Window
    {

        private Dictionary<string,string> _AllProductsAndQuantity;

        private Dictionary<string, string> _DataGridSelectedProductsAndQuantity;

        public Dictionary<string, string> DataGridSelectedProductsAndQuantity
        {
            get { return _DataGridSelectedProductsAndQuantity; }
            set { _DataGridSelectedProductsAndQuantity = value; }
        }

        private Dictionary<string, string> _ListBoxProductsAndQuantityAvailable;

        public Dictionary<string, string> ListBoxProductsAndQuantityAvailable
        {
            get { return _ListBoxProductsAndQuantityAvailable; }
            set { _ListBoxProductsAndQuantityAvailable = value; }
        }

        public List<string> ListBoxProductsNamesAvailable
        {
            get { return ListBoxProductsAndQuantityAvailable.Keys.ToList(); }
        }

        public bool Cancelled = true;

        public WindowSetYourProduct(Dictionary<string, string> allProductsAndQuantity, List<KeyValuePair<string, string>> CurrentProductsAndQuantities)
        {
            _AllProductsAndQuantity = allProductsAndQuantity;

            _ListBoxProductsAndQuantityAvailable = new Dictionary<string, string>(_AllProductsAndQuantity);

            _DataGridSelectedProductsAndQuantity = new Dictionary<string, string>();

            for (int i = 0; i < CurrentProductsAndQuantities.Count; i++)
            {
                _DataGridSelectedProductsAndQuantity.Add(CurrentProductsAndQuantities[i].Key, CurrentProductsAndQuantities[i].Value);
                _ListBoxProductsAndQuantityAvailable.Remove(CurrentProductsAndQuantities[i].Key);
            }

            InitializeComponent();

            DataContext = this;
        }


        private void bt_cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void bt_setProduct_click(object sender, RoutedEventArgs e)
        {
            Cancelled = false;
            this.Close();
        }

        private void bt_availableToSelected_Click(object sender, RoutedEventArgs e)
        {
            if (lb_list.SelectedIndex == -1)
                return;

            string selectedItem = (string) lb_list.SelectedItem;

            string quantity = ListBoxProductsAndQuantityAvailable[selectedItem];

            DataGridSelectedProductsAndQuantity.Add(selectedItem, quantity);

            ListBoxProductsAndQuantityAvailable.Remove(selectedItem);

            UpdateSources();
        }

        private void UpdateSources()
        {

            lb_list.ItemsSource = ListBoxProductsNamesAvailable;
            dg_currentProductAndQuantity.ItemsSource = DataGridSelectedProductsAndQuantity;
            dg_currentProductAndQuantity.Items.Refresh();
        }

        private void bt_selectedToAvailable_Click(object sender, RoutedEventArgs e)
        {
            if (dg_currentProductAndQuantity.SelectedIndex == -1)
                return;


            KeyValuePair<string, string> selectedItem = (KeyValuePair<string, string>)dg_currentProductAndQuantity.SelectedItem;

            string quantity;
            bool Exists = _AllProductsAndQuantity.TryGetValue(selectedItem.Key, out quantity);

            if (!Exists)
                quantity = selectedItem.Value;


            ListBoxProductsAndQuantityAvailable.Add(selectedItem.Key,quantity);

            DataGridSelectedProductsAndQuantity.Remove(selectedItem.Key);

            UpdateSources();
        }

        private void bt_add_Click(object sender, RoutedEventArgs e)
        {

            if (string.IsNullOrEmpty(tb_productName.Text))
                return;

            int Quantity;
            if (!int.TryParse(tb_Quantity.Text, out Quantity))
                return;

            string oldQuantity;
            bool exist = DataGridSelectedProductsAndQuantity.TryGetValue(tb_productName.Text, out oldQuantity);

            if (exist)
            {
                DataGridSelectedProductsAndQuantity[tb_productName.Text] = Quantity.ToString();
            }
            else
            {
                DataGridSelectedProductsAndQuantity.Add(tb_productName.Text, Quantity.ToString());
            }


            UpdateSources();

        }

        private void dg_currentProductAndQuantity_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dg_currentProductAndQuantity.SelectedIndex == -1)
                return;


            KeyValuePair<string, string> selectedItem = (KeyValuePair<string, string>)dg_currentProductAndQuantity.SelectedItem;

            tb_productName.Text = selectedItem.Key;
            tb_Quantity.Text = selectedItem.Value;
        }

    }
}
