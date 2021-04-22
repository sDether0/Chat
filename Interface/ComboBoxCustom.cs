using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Interface
{
    public class ComboBoxCustom : ComboBox
    {
        public ComboBoxCustom()
        {
            this.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
        }
        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (this.Items.Count >= 1 & e.Index != -1)
            {
                base.OnDrawItem(e);
                e.DrawBackground();
                ComboBoxItem item = (ComboBoxItem) this.Items[e.Index];
                Brush brush = new SolidBrush(item.ForeColor);
                if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
                {
                    brush = Brushes.Yellow;
                }

                e.Graphics.DrawString(item.Text, this.Font, brush, e.Bounds.X, e.Bounds.Y);
            }
        }
    }
    public class ComboBoxItem
    {
        public ComboBoxItem() { }

        public ComboBoxItem(string pText, object pValue)
        {
            text = pText; val = pValue;
        }

        public ComboBoxItem(string pText, object pValue, Color pColor)
        {
            text = pText; val = pValue; foreColor = pColor;
        }

        string text = "";
        public string Text
        {
            get { return text; }
            set { text = value; }
        }

        object val;
        public object Value
        {
            get { return val; }
            set { val = value; }
        }

        Color foreColor = Color.Black;
        public Color ForeColor
        {
            get { return foreColor; }
            set { foreColor = value; }
        }

        public override string ToString()
        {
            return text;
        }
    }
}
