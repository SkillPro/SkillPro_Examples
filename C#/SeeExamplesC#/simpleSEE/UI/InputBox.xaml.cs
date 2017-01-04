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
    /// Logique d'interaction pour InputBox.xaml
    /// </summary>
    public partial class InputBox : Window
    {

        public string mdp { get; set; }

        private string _Text;

        public string Text
        {
            get { return _Text; }
            set { _Text = value; }
        }
        

        public InputBox()
        {
            InputBoxConstructor("Enter password : ");
        }

        public InputBox(string text)
        {
            InputBoxConstructor(text);
        }

        private void InputBoxConstructor(string text)
        {
            _Text = text;
            InitializeComponent();
            tb_mdp.Focus();
            DataContext = this;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            mdp = tb_mdp.Password;
            this.Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            mdp = "";
            this.Close();
        }

        private void tb_mdp_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                mdp = tb_mdp.Password;
                this.Close();
            }
        }
    }
}
