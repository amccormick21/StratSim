using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace TeamStats.View
{
    class TableTextBox : TextBox
    {
        public int Row { get; set; }
        public int Column { get; set; }
        public bool textChanged;

        public TableTextBox(int row, int column)
        {
            Row = row;
            Column = column;
            TextChanged += TableTextBox_TextChanged;
            Leave += TableTextBox_Leave;
            KeyDown += KeyPressed;
        }

        void TableTextBox_TextChanged(object sender, EventArgs e)
        {
            textChanged = true;
        }

        internal event EventHandler MoveToNext;
        protected virtual void KeyPressed(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Down || e.KeyCode == Keys.Space)
            {
                if (MoveToNext != null)
                    MoveToNext(this, new EventArgs());
            }
        }

        void TableTextBox_Leave(object sender, EventArgs e)
        {
            if (ValueChanged != null)
                ValueChanged(Row, Column, this.Text);
        }

        internal delegate void TableTextChangedEventHandler(int row, int column, string value);
        internal event TableTextChangedEventHandler ValueChanged;
    }
}
