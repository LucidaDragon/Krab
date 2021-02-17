﻿using Lucida.Krab.Characters;
using Lucida.Krab.Compiler;
using Lucida.Krab.Parsing;
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

namespace Lucida.Krab.IDE
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Source_TextChanged(object sender, TextChangedEventArgs e)
        {
            var src = new StringSource("src", Source.Text);

            try
            {
                var sequence = new Sequencer(Parser.Parse(src));

                sequence.Resolve();

                Output.Text = string.Join(Environment.NewLine, sequence.Members.Keys);
            }
            catch (SourceError error)
            {
                Output.Text = $"\"{error.ErrorSource}\" {error.Message}";
            }
            catch (Exception ex)
            {
                Output.Text = $"An error occured while compiling: {ex}";
            }
        }
    }
}
