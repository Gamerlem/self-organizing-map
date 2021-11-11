using SOFM;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SOM
{
    public partial class Form1 : Form
    {
        private NeuralNetwork nn;
        private Color[,] matrix;

        private Boolean lightUp = false;
        private Boolean clearPlot = false;
        private Neuron winner;
        private Point point = new Point();
        private Color color = Color.YellowGreen;

        public Form1()
        {
            InitializeComponent();
            nn = new NeuralNetwork(1, Int32.Parse(noOfIteration.Text), Double.Parse(epsilon.Text), Functions.Discrete);
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            //adjust size of boxes
            int zoomFactor = 20;
            Graphics g = e.Graphics;
            if (matrix != null)
            {
                pictureBox1.Height = (int)Math.Sqrt(matrix.Length) * zoomFactor;
                pictureBox1.Width = (int)Math.Sqrt(matrix.Length) * zoomFactor;
                int currentTop, currentLeft;
                for (int i = 0; i < (int)Math.Sqrt(matrix.Length); i++)
                {
                    currentTop = i * zoomFactor;
                    for (int j = 0; j < (int)Math.Sqrt(matrix.Length); j++)
                    { 
                        currentLeft = j * zoomFactor;
                        //draw rectangle for non-winning neurons
                        g.DrawRectangle(new Pen(new SolidBrush(Color.WhiteSmoke), 2), new Rectangle(currentTop, currentLeft, zoomFactor, zoomFactor));
                        //draw rectangle for winning neurons
                        g.FillRectangle(new SolidBrush(matrix[i,j]), new RectangleF(currentTop, currentLeft, zoomFactor, zoomFactor));
                    }
                }
                DrawBorder(g);
            }
            else
            {
                DrawBorder(g);
                g.DrawString("Train/ Load a Model", new Font("Verdana", 10), new SolidBrush(Color.Black), new PointF(this.Width/3 - 150, this.Height/3 + 70));
            }

            if (lightUp)
            {
                //delete old mark
                if (color == Color.YellowGreen)
                {
                    point = new Point(winner.Coordinate.X, winner.Coordinate.Y);
                    color = matrix[winner.Coordinate.X, winner.Coordinate.Y];
                }
                int top = (point.X * zoomFactor);
                int left = (point.Y * zoomFactor);
                g.DrawRectangle(new Pen(new SolidBrush(Color.WhiteSmoke), 2), new Rectangle(top, left, zoomFactor, zoomFactor));
                g.FillRectangle(new SolidBrush(color), new RectangleF(top, left, zoomFactor, zoomFactor));
                DrawBorder(g);
                //add new mark
                top = (winner.Coordinate.X * zoomFactor);
                left = (winner.Coordinate.Y * zoomFactor);
                Rectangle rect = new Rectangle(top + (int)zoomFactor / 2 - (int)zoomFactor / 4, left + (int)zoomFactor / 2 - (int)zoomFactor / 4, (int)zoomFactor / 2, (int)zoomFactor / 2);
                g.DrawEllipse(new Pen(new SolidBrush(Color.Gray), 1), rect);
                g.FillEllipse(new SolidBrush(Color.Orange), rect);
                point.X = winner.Coordinate.X;
                point.Y = winner.Coordinate.Y;
                color = matrix[winner.Coordinate.X, winner.Coordinate.Y];
            }

            if (clearPlot && !point.IsEmpty)
            {
                if (color == Color.YellowGreen)
                {
                    point = new Point(winner.Coordinate.X, winner.Coordinate.Y);
                    color = matrix[winner.Coordinate.X, winner.Coordinate.Y];
                }
                int top = (point.X * zoomFactor);
                int left = (point.Y * zoomFactor);
                g.DrawRectangle(new Pen(new SolidBrush(Color.WhiteSmoke), 2), new Rectangle(top, left, zoomFactor, zoomFactor));
                g.FillRectangle(new SolidBrush(color), new RectangleF(top, left, zoomFactor, zoomFactor));
                DrawBorder(g);
            }
        }
        //for picturebox border
        private void DrawBorder(Graphics g)
        {
            ControlPaint.DrawBorder(g, pictureBox1.ClientRectangle, Color.Black, ButtonBorderStyle.Solid);
        }

        private void displayData()
        {
            if (nn.Patterns!=null)
            {
                listBox1.Items.Clear();
                string patternString;
                for (int i = 0; i < nn.Patterns.Count; i++)
                {
                    patternString = "";
                    patternString += nn.Classes[i] + " ";
                    for (int j = 0; j < nn.InputLayerDimension; j++)
                        patternString += nn.Patterns[i][j].ToString("g2") + " ";
                    listBox1.Items.Add(patternString);
                }
            }
            
        }

        private void AddLegend()
        {
            if (nn.ExistentClasses != null)
            {
                panelLegend.Controls.Clear();
                Label label = new Label();
                label.Name = "lblLegend";
                label.Top = 5;
                label.Left = 5;
                label.Text = "Legend";
                label.AutoSize = true;
                panelLegend.Controls.Add(label);
                for (int i = 0; i < nn.ExistentClasses.Count; i++)
                {
                    Label lbl = new Label();
                    lbl.Name = "lbl" + nn.ExistentClasses.Keys[i];
                    lbl.Text = " - " + nn.ExistentClasses.Keys[i];
                    lbl.Top = 20 * (i + 1);
                    lbl.AutoSize = true;
                    lbl.Left = 15 + (int)lbl.Font.Size;
                    this.panelLegend.Controls.Add(lbl);
                    Panel panel = new Panel();
                    panel.Name = "panel" + nn.ExistentClasses.Keys[i];
                    panel.Top = 20 * (i + 1) + (int)lbl.Font.Size / 2;
                    panel.Left = 15;
                    panel.Width = (int)lbl.Font.Size;
                    panel.Height = (int)lbl.Font.Size;
                    panel.BackColor = nn.UsedColors[i];
                    this.panelLegend.Controls.Add(panel);
                }
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            winner = nn.FindWinner(nn.Patterns[listBox1.SelectedIndex]);
            lightUp = true;
            this.Refresh();
            lightUp = false;
        }
        
        private void button1_Click(object sender, EventArgs e)
        {
            //Load and form map
            int NumberOfCards = (int)Math.Sqrt(Int32.Parse(noOfCompetetive.Text));
            Functions f = Functions.Gaus;
            foreach (Control c in this.panel1.Controls)
            {
                if (c is RadioButton)
                {
                    if (((RadioButton)c).Checked) f = (Functions)Enum.Parse(typeof(Functions), c.Tag.ToString());
                }
            }

            nn = new NeuralNetwork(NumberOfCards, Int32.Parse(noOfIteration.Text), Double.Parse(epsilon.Text), f);
            nn.Normalize = this.chbNormalize.Checked;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                nn.ReadDataFromFile(openFileDialog1.FileName);
                nn.StartLearning();
                matrix = nn.ColorSOFM();
                displayData();
                AddLegend();
                panelLegend.Visible = true;
                this.Refresh();

            }

        }
        private void button2_Click(object sender, EventArgs e)
        {
            //load
            openFileDialog2.ShowDialog();
            displayData();
            AddLegend();
            panelLegend.Visible = true;
            this.Refresh();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //save
            saveFileDialog1.ShowDialog();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //technicals
            button4.BackColor = Color.FromArgb(97, 59, 255);
            button4.ForeColor = Color.White;
            button5.BackColor = Color.FromArgb(238, 235, 254);
            button5.ForeColor = Color.FromArgb(97, 59, 255);
            panel4.Visible = false;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            //input
            button5.BackColor = Color.FromArgb(97, 59, 255);
            button5.ForeColor = Color.White;
            button4.BackColor = Color.FromArgb(238, 235, 254);
            button4.ForeColor = Color.FromArgb(97, 59, 255);
            panel4.Visible = true;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            //plot patient
            if(nn != null)
            {
                int i;
                List<double> patient = new List<double>();
                for (i = 0; i < checkedListBox1.Items.Count; i++)
                {
                    if (checkedListBox1.GetItemChecked(i)) patient.Add(1.0);
                    else patient.Add(0.0);
                }
                for (int j = 0; j < checkedListBox2.Items.Count; j++, i++)
                {
                    if (checkedListBox2.GetItemChecked(j)) patient.Add(1.0);
                    else patient.Add(0.0);
                }
                for (int j = 0; j < checkedListBox3.Items.Count; j++, i++)
                {
                    if (checkedListBox3.GetItemChecked(j)) patient.Add(1.0);
                    else patient.Add(0.0);
                }
                for (int j = 0; j < checkedListBox4.Items.Count; j++, i++)
                {
                    if (checkedListBox4.GetItemChecked(j)) patient.Add(1.0);
                    else patient.Add(0.0);
                }
                for (int j = 0; j < checkedListBox5.Items.Count; j++, i++)
                {
                    if (checkedListBox5.GetItemChecked(j)) patient.Add(1.0);
                    else patient.Add(0.0);
                }
                for (int j = 0; j < checkedListBox6.Items.Count; j++, i++)
                {
                    if (checkedListBox6.GetItemChecked(j)) patient.Add(1.0);
                    else patient.Add(0.0);
                }

                winner = nn.FindWinner(patient);
                lightUp = true;
                this.Refresh();
                lightUp = false;
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            uncheckCB(checkedListBox1);
            uncheckCB(checkedListBox2);
            uncheckCB(checkedListBox3);
            uncheckCB(checkedListBox4);
            uncheckCB(checkedListBox5);
            uncheckCB(checkedListBox6);
            clearPlot = true;
            this.Refresh();
            clearPlot = false;
        }

        private void uncheckCB(CheckedListBox chklistbox)
        {
            for (int i = 0; i < chklistbox.Items.Count; i++)
            {
                if (chklistbox.GetItemChecked(i))
                {
                    chklistbox.SetItemCheckState(i, CheckState.Unchecked);
                }
            }
        }
        //limits the checkbox to 1 check only
        private void max1(CheckedListBox chklistbox)
        {
            int selNdx = chklistbox.SelectedIndex;
            foreach (int cbNdx in chklistbox.CheckedIndices)
            {
                if (cbNdx != selNdx)
                {
                    chklistbox.SetItemChecked(cbNdx, false);
                }
            }
        }

        private void checkedListBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            max1(checkedListBox3);
        }

        private void checkedListBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            max1(checkedListBox4);
        }

        private void checkedListBox5_SelectedIndexChanged(object sender, EventArgs e)
        {
            max1(checkedListBox5);
        }

        private void checkedListBox6_SelectedIndexChanged(object sender, EventArgs e)
        {
            max1(checkedListBox6);
        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            nn.saveMap(saveFileDialog1.FileName);
        }

        private void openFileDialog2_FileOk(object sender, CancelEventArgs e)
        {
            nn.loadMap(openFileDialog2.FileName);
            matrix = nn.getColorMatrix();

            this.Refresh();
        }

        private void checkedListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(checkedListBox1.GetItemChecked(checkedListBox1.Items.Count - 1))
            {
                uncheckCB(checkedListBox1);
                checkedListBox1.SetItemCheckState(checkedListBox1.Items.Count - 1, CheckState.Checked);
            }
        }

        private void checkedListBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (checkedListBox2.GetItemChecked(checkedListBox2.Items.Count - 1))
            {
                uncheckCB(checkedListBox2);
                checkedListBox2.SetItemCheckState(checkedListBox2.Items.Count - 1, CheckState.Checked);
            }
        }
    }
}
