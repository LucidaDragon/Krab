using Lucida.Krab.Characters;
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
            string parseResult = null;
            string sequenceResult = null;
            string errorResult = null;

            try
            {
                var parsed = Parser.Parse(src);
                parseResult = string.Join(Environment.NewLine, parsed);

                var sequence = new Sequencer(parsed);
                sequence.Resolve();
                sequenceResult = string.Join(Environment.NewLine, sequence.Members.Keys);
            }
            catch (SourceError error)
            {
                errorResult = $"\"{error.ErrorSource}\" {error.Message}";
            }
            catch (Exception ex)
            {
                errorResult = $"An error occured while compiling: {ex}";
            }

            Output.Text = $"{parseResult}{Environment.NewLine}{sequenceResult}{Environment.NewLine}{errorResult}";
        }
    }
}
