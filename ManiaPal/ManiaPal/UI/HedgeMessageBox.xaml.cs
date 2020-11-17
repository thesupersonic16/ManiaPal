﻿using System;
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

namespace ManiaPal
{
    /// <summary>
    /// Interaction logic for HedgeMessageBox.xaml
    /// </summary>
    public partial class HedgeMessageBox : Window
    {
        public HedgeMessageBox()
        {
            InitializeComponent();
        }

        public HedgeMessageBox(string header, string message, FlowDirection flowDirection = FlowDirection.RightToLeft, TextAlignment textAlignment = TextAlignment.Center)
        {
            InitializeComponent();
            Title = header;
            Header.Text = header;
            Message.Text = message;
            Message.TextAlignment = textAlignment;
            Stack.FlowDirection = flowDirection;
        }

        public void SetWindowSize(double width, double height)
        {
            SetWindowSize(new Size(width, height));
        }

        public void SetWindowSize(Size size)
        {
            Width = size.Width;
            Height = size.Height;
        }

        public void AddButton(string text, Action onClick)
        {
            var btn = new Button()
            {
                Content = $"    {text}    ",
                Width = double.NaN,
                Height = 23,
                Margin = new Thickness(5)
            };

            btn.Click += (caller, args) => { onClick.Invoke(); };
            Stack.Children.Add(btn);
        }
    }
}
