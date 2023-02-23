using ImageSorter.Properties;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ImageSorter {
    public partial class MainForm : Form {
        private Point _dragStartPoint;
        private int _sourceIndex = -1;
        private Lazy<Image> lazyImage;
#pragma warning disable IDE0044 // Add readonly modifier
        private HashSet<string> loadedFolders = new HashSet<string>();
        bool move = false;
        bool delete = false;
#pragma warning restore IDE0044 // Add readonly modifier


        public MainForm() {
            InitializeComponent();
            this.KeyDown += new KeyEventHandler(Form1_KeyDown);
            this.KeyUp += new KeyEventHandler(Form1_KeyUp);
            listBox1.MouseDown += ListBox1_MouseDown;
            listBox1.MouseMove += ListBox1_MouseMove;
            listBox1.MouseUp += ListBox1_MouseUp;
            lazyImage = new Lazy<Image>(() => {
                string selectedImagePath = (string)listBox1.SelectedItem;
                return Image.FromFile(selectedImagePath);
            });
        }

        private void UpdateMoveButtonsEnabled(ListBox listBox) {
            if (listBox.SelectedItem != null && listBox.Items.Count > 1) {
                int selectedIndex = listBox.SelectedIndex;
                button5.Enabled = (selectedIndex < listBox.Items.Count - 1);
                button6.Enabled = (selectedIndex > 0);
            }
            else {
                button5.Enabled = false;
                button6.Enabled = false;
            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e) {
            // Check if Shift key is held down
            if (e.Shift) {
                // Change button text to "Input File"
                button1.Text = "Input File";
                button3.Text = "Delete";
                button4.Text = "Move";
                button5.Text = "Top";
                button6.Text = "Bottom";
            }
            if (e.KeyCode == Keys.Delete && listBox1.SelectedIndex != -1 && e.Shift) {
                string selectedFile = Path.Combine(Properties.Settings.Default.LastFolderPath, listBox1.SelectedItem.ToString());
                if (listBox1.SelectedItem != null) {
                    try {
                        pictureBox1.Image.Dispose();
                        pictureBox1.Image = null;
                        pictureBox1.Image = Resources.imagesorterpreview;
                        File.Delete(selectedFile);
                        listBox1.Items.Remove(listBox1.SelectedItem);
                    }
                    catch (Exception ex) {
                        MessageBox.Show($"Error deleting file {selectedFile}: {ex.Message}");
                        return;
                    }
                }

            }
            else if (e.KeyCode == Keys.Delete && listBox1.SelectedIndex != -1) {
                pictureBox1.Image.Dispose();
                pictureBox1.Image = null;
                pictureBox1.Image = Resources.imagesorterpreview;
                this.Text = "Image Sorter";
                listBox1.Items.Remove(listBox1.SelectedItem);
            }
            UpdateMoveButtonsEnabled(listBox1);
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e) {
            // Check if Shift key was released
            if (e.KeyCode == Keys.ShiftKey) {
                // Change button text back to original text
                button1.Text = "Input Folder";
                button3.Text = "Remove";
                button4.Text = "Copy";
                button5.Text = "Move Up";
                button6.Text = "Move Down";
            }
        }

        public class ListItem {
            public string Text { get; set; }
            public string Value { get; set; }

            public ListItem(string text, string value) {
                Text = text;
                Value = value;
            }

            public override string ToString() {
                return Text;
            }
        }

        private void Button1_Click(object sender, EventArgs e) {
            // Check if the Shift key is pressed
            if (Control.ModifierKeys == Keys.Shift) {
                // Show the OpenFileDialog
                string lastPath = Properties.Settings.Default.LastFolderPath;
                using (OpenFileDialog openFileDialog = new OpenFileDialog()) {
                    openFileDialog.RestoreDirectory = true;
                    openFileDialog.Filter = "Image files (.jpg, .png, .bmp)|*.jpg;*.png;*.bmp";
                    DialogResult result = openFileDialog.ShowDialog();
                    if (result == DialogResult.OK) {
                        string filePath = openFileDialog.FileName;
                        string fileName = Path.GetFileName(filePath);
                        var item = new ListItem(fileName, filePath);
                        listBox1.Items.Add(item);
                    }
                }
            }
            else {
                // Show the FolderBrowserDialog
                string lastPath = Properties.Settings.Default.LastFolderPath;
                using (FolderBrowserDialog folderDialog = new FolderBrowserDialog()) {
                    folderDialog.SelectedPath = lastPath;
                    folderDialog.ShowNewFolderButton = false;
                    DialogResult result = folderDialog.ShowDialog();
                    if (result == DialogResult.OK) {
                        string[] imageExtensions = { ".jpg", ".png", ".bmp" };
                        string selectedPath = folderDialog.SelectedPath;
                        if (!loadedFolders.Contains(selectedPath)) {
                            Properties.Settings.Default.LastFolderPath = selectedPath;
                            Properties.Settings.Default.Save();
                            listBox1.Items.Clear();
                            Parallel.ForEach(Directory.GetFiles(selectedPath)
                                .Where(f => imageExtensions.Contains(Path.GetExtension(f).ToLower())),
                                new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount - 1 }, filePath => {
                                    string fileName = Path.GetFileName(filePath);
                                    // Create a new ListItem object
                                    var item = new ListItem(fileName, filePath);
                                    // Add the new item to the list box on the UI thread
                                    listBox1.Invoke(new Action(() => {
                                        listBox1.Items.Add(item);
                                    }));
                                });

                        }
                        if (listBox1.Items.Count > 0) {
                            button4.Enabled = true;
                        }
                        else {
                            button4.Enabled = false;
                        }
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
                        if (listBox1.Items.Count > 1) {
                            object selectedItem = listBox1.Items[_sourceIndex];
                            listBox1.Items.RemoveAt(_sourceIndex);
                            listBox1.Items.Insert(targetIndex, selectedItem);
                            _sourceIndex = targetIndex;
                        }
                }
            }
        }
        private void ListBox1_MouseDown(object sender, MouseEventArgs e) {
            _dragStartPoint = new Point(e.X, e.Y);
            _sourceIndex = listBox1.IndexFromPoint(_dragStartPoint);
        }

        private void Button5_Click(object sender, EventArgs e) {
            if (listBox1.SelectedItem != null) {
                bool shiftKeyPressed = Control.ModifierKeys == Keys.Shift;
                listBox1.MoveSelectedItemDown(shiftKeyPressed);
            }
            UpdateMoveButtonsEnabled(listBox1);
            if (listBox1.SelectedItem != null) {
                if (listBox1.SelectedItem != null) {
                    pictureBox1.Image = lazyImage.Value;
                }
            }
        }

        private void Button6_Click(object sender, EventArgs e) {
            if (listBox1.SelectedItem != null) {
                bool shiftKeyPressed = Control.ModifierKeys == Keys.Shift;
                listBox1.MoveSelectedItemUp(shiftKeyPressed);
            }
            UpdateMoveButtonsEnabled(listBox1);
            if (listBox1.SelectedItem != null) {
                    pictureBox1.Image = lazyImage.Value;
            }
        }

        private void ListBox1_SelectedIndexChanged(object sender, EventArgs e) {
            UpdateMoveButtonsEnabled(listBox1); if (listBox1.SelectedItem != null) {
                button3.Enabled = true;
            }
            // Get the selected item from the list box
            if (pictureBox1 != null && listBox1.SelectedItem != null) {
                ListItem selectedItem = (ListItem)listBox1.SelectedItem;
                string selectedImagePath = selectedItem.Value;
                string selectedFileName = selectedItem.Text;
                this.Name = selectedFileName;
                this.Text = selectedFileName;
                lazyImage = new Lazy<Image>(() => Image.FromFile(selectedImagePath));
                pictureBox1.Image = lazyImage.Value;
            }
        }

        private void Button4_Click(object sender, EventArgs e) {
            // Create the destination folder
            string destinationFolder = Properties.Settings.Default.LastOutputPath;
            Directory.CreateDirectory(destinationFolder);

            // Loop through the files in the list box and move or copy them to the destination folder
            for (int i = 0; i < listBox1.Items.Count; i++) {
                string sourceFile = listBox1.Items[i].ToString();
                string extension = Path.GetExtension(sourceFile);
                string destinationFile = Path.Combine(destinationFolder, $"image{(i + 1).ToString("000")}{extension}");
                if (File.Exists(sourceFile)) {
                    try {
                        if (move) {
                            pictureBox1.Image.Dispose();
                            pictureBox1.Image = null;
                            pictureBox1.Image = Resources.imagesorterpreview;
                            this.Text = "Image Sorter";
                            File.Move(sourceFile, destinationFile);
                        }
                        else {
                            File.Copy(sourceFile, destinationFile, true);
                        }
                    }
                    catch (Exception ex) {
                        MessageBox.Show($"Error {(move ? "moving" : "copying")} file {sourceFile}: {ex.Message}");
                    }
                }
                else {
                    MessageBox.Show($"File {sourceFile} does not exist");
                }
            }
            if (move) { listBox1.Items.Clear(); }

            MessageBox.Show("Done");
            pictureBox1.Image.Dispose();
            pictureBox1.Image = null;
            pictureBox1.Image = Resources.imagesorterpreview;
            this.Text = "Image Sorter";
        }
        private void Button3_Click(object sender, EventArgs e) {
            // Get the selected index in the ListBox
            int selectedIndex = listBox1.SelectedIndex;

            if (selectedIndex == -1) return;

            string selectedFile = listBox1.SelectedItem.ToString();

            if (delete) {
                if (listBox1.SelectedItem != null) {
                    //pictureBox1.Image = lazyImage.Value;
                    pictureBox1.Image.Dispose();
                    pictureBox1.Image = null;
                    pictureBox1.Image = Resources.imagesorterpreview;
                    this.Text = "Image Sorter";
                }
                try {
                    File.Delete(selectedFile);
                    listBox1.Items.Remove(listBox1.SelectedItem);
                    this.Text = "Image Sorter";
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
    }




    public static class ListBoxExtension {
        public static void MoveSelectedItemUp(this ListBox listBox, bool shiftKeyPressed) {
            if (listBox.SelectedItem != null) {
                int selectedIndex = listBox.SelectedIndex;
                if (selectedIndex > 0 && shiftKeyPressed) {
                    object selectedItem = listBox.SelectedItem;
                    listBox.Items.RemoveAt(selectedIndex);
                    listBox.Items.Insert(0, selectedItem); // insert at the top
                    listBox.SetSelected(0, true); // select the moved item
                }
            }
        }

        public static void MoveSelectedItemDown(this ListBox listBox, bool shiftKeyPressed) {
            if (listBox.SelectedItem != null) {
                int selectedIndex = listBox.SelectedIndex;
                if (selectedIndex > 0 && shiftKeyPressed) {
                    int lastIndex = listBox.Items.Count - 1; // get the index of the last item
                    if (selectedIndex < lastIndex) { // check if not already at the bottom
                        object selectedItem = listBox.SelectedItem;
                        listBox.Items.RemoveAt(selectedIndex);
                        listBox.Items.Insert(lastIndex, selectedItem); // insert at the bottom
                        listBox.SetSelected(lastIndex, true); // select the moved item
                    }
                }
            }
        }
    }
}