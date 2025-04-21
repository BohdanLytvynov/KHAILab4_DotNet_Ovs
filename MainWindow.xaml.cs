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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SunRise_SunDown
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            this.Task_Desc.Text = "Варіант номер 9, Литвинов Б Ю 125 гр\n" + 
                "Схід сонця і захід сонця. Сонце повинне з'являтися в лівій нижній частині вікна, \n" +
                "рухатися у вікні деяким радіусом і сідати в нижній правій частині вікна. \n" +
                "У процесі руху воно має максимально правдоподібно змінювати колір та розмір\n";
        }
    }
}
