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
    /// Logique d'interaction pour WindowSkillExecution.xaml
    /// </summary>
    public partial class WindowSkillExecution : Window
    {


        public WindowSkillExecution(string nameOfSkillOnLoaded, string RemainingTimeSecOnLoaded, string StatusOnLoaded)
        {

            DateTime now = DateTime.Now;
            InitializeComponent();
            lbl_remainingTime.Content = RemainingTimeSecOnLoaded;
            lbl_skillName.Content = nameOfSkillOnLoaded;
            lbl_StatusOfExecution.Content = StatusOnLoaded;
            lbl_datetime.Content = "Year : " + now.Year.ToString() + " | Month : " + now.Month.ToString() + " | Day : " + now.Day.ToString() + " | Hour : " + now.Hour.ToString() + " | min : " + now.Minute.ToString() + " | sec : " + now.Second.ToString() + " | ms : " + now.Millisecond.ToString(); 
            
        }



        public void changeTime(string newString)
        {
            lbl_remainingTime.Content = newString;
        }

        public void changeStatus(string newString)
        {
            lbl_StatusOfExecution.Content = newString;
        }
    }
}
