using ImageSorter.Properties;
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace ImageSorter
{
    public partial class Form1 : Form
    {
        String listbox1Entry;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                String dir = folderBrowserDialog1.SelectedPath;
                String[] extensionList = { "*.jpg", "*.png", "*.bmp" };
                foreach (String fileExtension in extensionList)
                {
                    foreach (String file in Directory.GetFiles(dir, fileExtension, SearchOption.AllDirectories))
                    {
                        listBox1.Items.Add(file);
                    }
                }
            }

        }

        private void button5_Click(object sender, EventArgs e)
        {
            listBox1.MoveSelectedItemDown();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            listBox1.MoveSelectedItemUp();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            listbox1Entry = listBox1.GetItemText(listBox1.SelectedItem);
            this.Name = listbox1Entry;
            this.Text = listbox1Entry;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            pictureBox1.Dispose();
            pictureBox1.Load(listbox1Entry);
        }
    }

    public static class ListBoxExtension
    {
        public static void MoveSelectedItemUp(this ListBox listBox)
        {
            _MoveSelectedItem(listBox, -1);
        }

        public static void MoveSelectedItemDown(this ListBox listBox)
        {
            _MoveSelectedItem(listBox, 1);
        }

        static void _MoveSelectedItem(ListBox listBox, int direction)
        {
            // Checking selected item
            if (listBox.SelectedItem == null || listBox.SelectedIndex < 0)
                return; // No selected item - nothing to do

            // Calculate new index using move direction
            int newIndex = listBox.SelectedIndex + direction;

            // Checking bounds of the range
            if (newIndex < 0 || newIndex >= listBox.Items.Count)
                return; // Index out of range - nothing to do

            object selected = listBox.SelectedItem;

            // Save checked state if it is applicable
            var checkedListBox = listBox as CheckedListBox;
            var checkState = CheckState.Unchecked;
            if (checkedListBox != null)
                checkState = checkedListBox.GetItemCheckState(checkedListBox.SelectedIndex);

            // Removing removable element
            listBox.Items.Remove(selected);
            // Insert it in new position
            listBox.Items.Insert(newIndex, selected);
            // Restore selection
            listBox.SetSelected(newIndex, true);

            // Restore checked state if it is applicable
            if (checkedListBox != null)
                checkedListBox.SetItemCheckState(newIndex, checkState);
        }
    }
}
