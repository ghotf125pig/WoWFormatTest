﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WoWFormatLib.DBC;
using WoWFormatLib.FileReaders;

namespace WoWFormatUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var basedir = ConfigurationManager.AppSettings["basedir"];
            var reader = new MapReader(basedir);
            Dictionary<int, string> maps = reader.GetMaps();
            foreach (KeyValuePair<int, string> map in maps)
            {
                MapListBox.Items.Add(map.Value);
            }
        }

        private void WDTGrid_MouseLeftButtonUp (object sender, MouseEventArgs e)
        {
            foreach (Rectangle child in WDTGrid.Children)
            {
                Point point = new Point(e.GetPosition(WDTGrid).X, e.GetPosition(WDTGrid).Y);
                //check if there is a rectangle at that point
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var basedir = ConfigurationManager.AppSettings["basedir"];
            if (MapListBox.SelectedValue != null)
            {
                var wdt = new WDTReader(basedir);
                wdt.LoadWDT(MapListBox.SelectedValue.ToString());
                List<int[]> tiles = wdt.getTiles();
                Console.WriteLine(MapListBox.SelectedValue.ToString());
                WDTGrid.Children.Clear();

                foreach(var tile in tiles){
                    var x = tile[0];
                    var y = tile[1];
                    Rectangle rect = new Rectangle();
                    rect.Width = WDTGrid.Width / 64;
                    rect.Height = WDTGrid.Height / 64;
                    rect.VerticalAlignment = VerticalAlignment.Top;
                    rect.HorizontalAlignment = HorizontalAlignment.Left;
                    var xmargin = x * rect.Width;
                    var ymargin = y * rect.Height;
                    rect.Fill = new SolidColorBrush(Color.FromRgb(0, 111, 0));
                    rect.Stroke = new SolidColorBrush(Color.FromRgb(0, 0, 0));
                    rect.Margin = new Thickness(xmargin, ymargin, rect.Margin.Right, rect.Margin.Bottom);
                    WDTGrid.Children.Add(rect);
                }
            }
        }
    }
}