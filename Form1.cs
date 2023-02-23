using ImageSorter.Properties;
using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace ImageSorter {
    public partial class MainForm : Form {
        private Point _dragStartPoint;
        private int _sourceIndex = -1;
        private Lazy<Image> lazyImage;
        String listbox1Entry;
        bool movetest = false;
        bool deletetest = false;




        public MainForm() {
            InitializeComponent();
            listBox1.MouseDown += ListBox1_MouseDown;
            listBox1.MouseMove += ListBox1_MouseMove;
            listBox1.MouseUp += ListBox1_MouseUp;
            lazyImage = new Lazy<Image>(() => {
                string selectedImagePath = (string)listBox1.SelectedItem;
                return Image.FromFile(selectedImagePath);
            });
        }


        private void Button1_Click(object sender, EventArgs e) {
            string lastPath = Properties.Settings.Default.LastFolderPath;

            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog()) {
                folderDialog.SelectedPath = lastPath;
                folderDialog.ShowNewFolderButton = false;

                DialogResult result = folderDialog.ShowDialog();
                if (result == DialogResult.OK) {
                    string[] imageExtensions = { ".jpg", ".png", ".bmp" };
                    string selectedPath = folderDialog.SelectedPath;
                    Properties.Settings.Default.LastFolderPath = selectedPath;
                    Properties.Settings.Default.Save();
                    listBox1.Items.Clear();
                    foreach (string file in Directory.GetFiles(selectedPath)
                                    .Where(f => imageExtensions.Contains(Path.GetExtension(f).ToLower()))) {
                        listBox1.Items.Add(file);
                    }
                }
            }
        }

        private void Button2_Click(object sender, EventArgs e) {
            string lastOutputPath = Properties.Settings.Default.LastOutputPath;

            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog()) {
                folderDialog.SelectedPath = lastOutputPath;
                folderDialog.ShowNewFolderButton = true;

                DialogResult result = folderDialog.ShowDialog();
                if (result == DialogResult.OK) {
                    string selectedPath = folderDialog.SelectedPath;
                    Properties.Settings.Default.LastOutputPath = selectedPath;
                    Properties.Settings.Default.Save();
                }
            }
        }

        private void ListBox1_MouseUp(object sender, MouseEventArgs e) {
            _sourceIndex = -1;
        }

        private void ListBox1_MouseMove(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Left) {

                int targetIndex = listBox1.IndexFromPoint(e.Location);

                if (targetIndex != -1 && targetIndex != _sourceIndex) {
                    {
                        if (listBox1.Items.Count > 1) {
                            object selectedItem = listBox1.Items[_sourceIndex];
                            listBox1.Items.RemoveAt(_sourceIndex);
                            listBox1.Items.Insert(targetIndex, selectedItem);
                            _sourceIndex = targetIndex;
                        }
                    }
                }
            }
        }
        private void ListBox1_MouseDown(object sender, MouseEventArgs e) {
            _dragStartPoint = new Point(e.X, e.Y);
            _sourceIndex = listBox1.IndexFromPoint(_dragStartPoint);
        }

        private void Button5_Click(object sender, EventArgs e) {
            listBox1.MoveSelectedItemDown();
            if (listBox1.SelectedItem != null) {
                pictureBox1.Image = lazyImage.Value;
            }
        }

        private void Button6_Click(object sender, EventArgs e) {
            listBox1.MoveSelectedItemUp();
            if (listBox1.SelectedItem != null) {
                if (listBox1.SelectedItem != null) {
                    pictureBox1.Image = lazyImage.Value;
                }
            }
        }

        private void ListBox1_SelectedIndexChanged(object sender, EventArgs e) {
            if (listBox1.SelectedIndex >= 0 && listBox1.SelectedIndex < listBox1.Items.Count - 1) {
                button5.Enabled = true;
            }
            else {
                button5.Enabled = false;
            }

            if (listBox1.SelectedIndex <= 0) {
                button6.Enabled = false;
            }
            else {
                button6.Enabled = true;
            }
            if (listBox1.Items.Count <= 0) {
                button3.Enabled = false;
            }
            else {
                button3.Enabled = true;
            }
            // Get the selected item from the list box

            if (pictureBox1 != null && listBox1.SelectedItem != null) {
                string selectedImagePath = (string)listBox1.SelectedItem;
                this.Name = listbox1Entry;
                this.Text = listbox1Entry;
                lazyImage = new Lazy<Image>(() => Image.FromFile(selectedImagePath));
                pictureBox1.Image = lazyImage.Value;
            }
        }

        private void Button4_Click(object sender, EventArgs e) {
            // Create the destination folder
            string destFolder = Properties.Settings.Default.LastOutputPath;
            Directory.CreateDirectory(destFolder);

            // Loop through the files in the list box and move or copy them to the destination folder
            for (int i = 0; i < listBox1.Items.Count; i++) {
                string sourceFile = listBox1.Items[i].ToString();
                string extension = Path.GetExtension(sourceFile);
                string destFile = Path.Combine(destFolder, $"image{(i + 1).ToString("000")}{extension}");
                if (File.Exists(sourceFile)) {
                    try {
                        if (movetest) {
                            pictureBox1.Image.Dispose();
                            pictureBox1.Image = null;
                            pictureBox1.Image = Resources.imagesorterpreview;
                            File.Move(sourceFile, destFile);
                        }
                        else {
                            File.Copy(sourceFile, destFile, true);
                        }
                    }
                    catch (Exception ex) {
                        MessageBox.Show($"Error {(movetest ? "moving" : "copying")} file {sourceFile}: {ex.Message}");
                    }
                }
                else {
                    MessageBox.Show($"File {sourceFile} does not exist");
                }
            }
            if (movetest) { listBox1.Items.Clear(); }

            MessageBox.Show("Done");
            pictureBox1.Image.Dispose();
            pictureBox1.Image = null;
            pictureBox1.Image = Resources.imagesorterpreview;
        }
        private void button3_Click(object sender, EventArgs e) {
            // Get the selected index in the ListBox
            int selectedIndex = listBox1.SelectedIndex;

            if (selectedIndex == -1) return;

            string selectedFile = listBox1.SelectedItem.ToString();

            if (deletetest) {
                if (listBox1.SelectedItem != null) {
                    //pictureBox1.Image = lazyImage.Value;
                    pictureBox1.Image.Dispose();
                    pictureBox1.Image = null;
                }
                try {
                    File.Delete(selectedFile);
                    listBox1.Items.Remove(listBox1.SelectedItem);
                }
                catch (Exception ex) {
                    MessageBox.Show($"Error deleting file {selectedFile}: {ex.Message}");
                    return;
                }
            }
            else {
                listBox1.Items.Remove(listBox1.SelectedItem);
            }

        }


        private void checkBox1_CheckedChanged(object sender, EventArgs e) {
            if (checkBox1.Checked) {
                button4.Text = "Move";
                movetest = true;

            }
            else {
                button4.Text = "Copy";
                movetest = false;
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e) {
            if (checkBox2.Checked) {
                button3.Text = "Delete";
                deletetest = true;

            }
            else {
                button3.Text = "Remove";
                deletetest = false;
            }
        }
    }

    public static class ListBoxExtension {
        public static void MoveSelectedItemUp(this ListBox listBox) {
            MoveSelectedItem(listBox, -1);
        }

        public static void MoveSelectedItemDown(this ListBox listBox) {
            MoveSelectedItem(listBox, 1);
        }

        static void MoveSelectedItem(ListBox listBox, int direction) {
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
            if (checkedListBox == null)
                return;
            checkedListBox.SetItemCheckState(newIndex, checkState);
        }
    }
}
