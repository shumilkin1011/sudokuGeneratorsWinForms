using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Generator9x9_WF
{
    public partial class Form1 : Form
    {
        int N;
        int[] field;
        int[] closedField;
        static Generator generator;
        Dictionary<int, LevelInfo> list;

        public Form1()
        {
            InitializeComponent();

            generator = new Generator();
            this.comboBox1.Items.Add(DifficultyLevel.EASY_9x9);
            this.comboBox1.Items.Add(DifficultyLevel.MEDIUM_9x9);
            this.comboBox1.Items.Add(DifficultyLevel.HARD_9x9);
            this.comboBox1.Items.Add(DifficultyLevel.EXPERT_9x9);
            this.comboBox1.SelectedIndex = 0;
            
        }

        private void RefreshGrid(int selected)
        {
            dataGridView1.Rows.Clear();
            dataGridView2.Rows.Clear();
            LevelInfo level = list[selected];
            N = (int)level.size;
            field = level.tiles;
            closedField = level.closedTiles;

            dataGridView1.ColumnCount = N;
            dataGridView2.ColumnCount = N;

            for (int i = 0; i < N; i++)
            {
                DataGridViewRow row1 = new DataGridViewRow();
                DataGridViewRow row2 = new DataGridViewRow();
                row1.CreateCells(this.dataGridView1);
                row2.CreateCells(this.dataGridView2);

                for (int j = 0; j < N; j++)
                {
                    row1.Cells[j].Value = field[i*N+j];
                    row2.Cells[j].Value = closedField[i*N+j];
                    if ((int)row2.Cells[j].Value == 0) row2.Cells[j].Style.BackColor = Color.LightGray;

                }

                this.dataGridView1.Rows.Add(row1);
                this.dataGridView2.Rows.Add(row2);
            }

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                row.Height = (dataGridView1.ClientRectangle.Height - dataGridView1.ColumnHeadersHeight) / dataGridView1.Rows.Count;
            }
            foreach (DataGridViewRow row in dataGridView2.Rows)
            {
                row.Height = (dataGridView2.ClientRectangle.Height - dataGridView2.ColumnHeadersHeight) / dataGridView2.Rows.Count;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            label3.Visible = true;
            Refresh();
            switch((DifficultyLevel)this.comboBox1.SelectedItem)
            {
                case DifficultyLevel.EASY_9x9:
                    generator.Generate(DifficultyLevel.EASY_9x9, SudokuSize.SIZE_9x9, (int)numericUpDown1.Value);
                    break;
                case DifficultyLevel.MEDIUM_9x9:
                    generator.Generate(DifficultyLevel.MEDIUM_9x9, SudokuSize.SIZE_9x9, (int)numericUpDown1.Value);
                    break;
                case DifficultyLevel.HARD_9x9:
                    generator.Generate(DifficultyLevel.HARD_9x9, SudokuSize.SIZE_9x9, (int)numericUpDown1.Value);
                    break;
                case DifficultyLevel.EXPERT_9x9:
                    generator.Generate(DifficultyLevel.EXPERT_9x9, SudokuSize.SIZE_9x9, (int)numericUpDown1.Value);
                    break;
            }
            
            RefreshList();
            label3.Visible = false;
        }

        private void RefreshList()
        {
            StreamReader reader = new StreamReader(generator.FilePath);
            string s;
            int count = 0;
            list = new Dictionary<int, LevelInfo>();
            while ( (s= reader.ReadLine() ) != null)  {
                count++;
                list.Add(count, Newtonsoft.Json.JsonConvert.DeserializeObject<LevelInfo>(s));
            }
            listBox1.DataSource = list.Keys.ToList<int>();

        }
        

        private void dataGridView2_SizeChanged(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                row.Height = (dataGridView1.ClientRectangle.Height - dataGridView1.ColumnHeadersHeight) / dataGridView1.Rows.Count;
            }
            foreach (DataGridViewRow row in dataGridView2.Rows)
            {
                row.Height = (dataGridView2.ClientRectangle.Height - dataGridView2.ColumnHeadersHeight) / dataGridView2.Rows.Count;
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selected = (int)this.listBox1.SelectedValue;
            RefreshGrid(selected);
        }

    }
}
