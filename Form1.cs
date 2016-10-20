using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Interpreter
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void buttonRun_Click(object sender, EventArgs e)
        {
            //string text = "def main() { i = 4; if (i LESS 4 AND i LESS 5){ i = 40;}else{i = 100 + 2;} }"; 
            string text = "def main() {q = 2; for (i = 0; i LESS 5; i = i + 1) { q = q + 1; } }";
            Interpreter interpreter = new Interpreter(richTextBoxResult);
            interpreter.Execute(text, null);
        }
    }
}
