using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TeamStats.View
{
    public class IndexedToolStripButton : ToolStripButton
    {
        int Index { get; set; }

        public IndexedToolStripButton(int index)
        {
            this.Index = index;
            Click += IndexedToolStripButton_Click;
        }

        public event EventHandler<int> ButtonClicked;
        void IndexedToolStripButton_Click(object sender, EventArgs e)
        {
            if (ButtonClicked != null)
                ButtonClicked(this, Index);
        }

    }
}
