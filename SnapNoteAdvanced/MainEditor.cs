using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace SnapNoteAdvanced
{
    public partial class MainEditor : Form
    {
        public string currentDirectory = "C:\\";
        public string ol_rtbtext = "";
        public Color ol_fg_color = Color.White;
        public Color ol_bg_color = Color.FromArgb(255, 64, 64, 64);
        bool directoryView = false;
        public List<string> optionsSettings = [];
        public int currentCol = 0;
        public int currentRow = 0;

        public MainEditor()
        {
            InitializeComponent();

            //Initialising stuff
            InitializeSettings();
            richTextBox1.MouseClick += richTextBox1_MouseClick;
        }

        private void InitializeSettings()
        {
            try
            {
                optionsSettings = File.ReadAllLines("config.txt").ToList<string>();
                foreach (string s in optionsSettings)
                {
                    string s_val = s.Split(":")[1];
                    if (s.StartsWith("bg:"))
                    {
                        ol_bg_color = Color.FromArgb(
                            Convert.ToInt32(s_val.Split(",")[0]),
                            Convert.ToInt32(s_val.Split(",")[1]),
                            Convert.ToInt32(s_val.Split(",")[2]),
                            Convert.ToInt32(s_val.Split(",")[3])
                            );
                    }
                    else if (s.StartsWith("fg:"))
                    {
                        ol_fg_color = Color.FromArgb(
                            Convert.ToInt32(s_val.Split(",")[0]),
                            Convert.ToInt32(s_val.Split(",")[1]),
                            Convert.ToInt32(s_val.Split(",")[2]),
                            Convert.ToInt32(s_val.Split(",")[3])
                            );
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to read options.txt due to the following exception: {Environment.NewLine}{ex.Message}");
            }
            richTextBox1.BackColor = ol_bg_color;
            richTextBox1.ForeColor = ol_fg_color;
        }

        private void richTextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.Control)
            {
                if (e.KeyCode == Keys.D)
                {
                    if (directoryView)
                    {
                        directoryView = false;
                        richTextBox1.Text = ol_rtbtext;
                        richTextBox1.BackColor = ol_bg_color;
                        richTextBox1.ForeColor = ol_fg_color;
                        return;
                    }

                    directoryView = true;
                    ol_bg_color = richTextBox1.BackColor;
                    ol_fg_color = richTextBox1.ForeColor;

                    richTextBox1.BackColor = Color.Black;
                    richTextBox1.ForeColor = Color.White;
                    try
                    {
                        RefreshCurrentDirectoryView();
                    }
                    catch
                    {
                        richTextBox1.Text += ol_rtbtext;
                        directoryView = false;
                    }
                }
            }
            else if (e.Modifiers == Keys.Alt)
            {
                if (e.KeyCode == Keys.S)
                {
                    Settings settings = new Settings();
                    settings.ShowDialog();
                }
            }
            else
            {

            }
        }

        private void richTextBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (directoryView)
                {
                    string clickedLine = "";

                    try
                    {
                        int charIndex = richTextBox1.GetCharIndexFromPosition(e.Location);
                        int lineIndex = richTextBox1.GetLineFromCharIndex(charIndex);
                        clickedLine = richTextBox1.Lines[lineIndex];
                    }
                    catch (Exception ex)
                    {
                        richTextBox1.Text = $"Error {Environment.NewLine}{ex.Message}";
                    }

                    if (clickedLine == "- - - - -") return;
                    else if (clickedLine == "(No Parent Directory)") return;
                    else if (clickedLine == "<No Directories>") return;
                    else if (clickedLine == "<No Files>") return;

                    if (Directory.Exists(clickedLine))
                    {
                        currentDirectory = clickedLine;
                        RefreshCurrentDirectoryView();
                        return;
                    }

                    if (File.Exists(clickedLine))
                    {
                        try
                        {
                            richTextBox1.Text = File.ReadAllText(Path.Combine(currentDirectory, clickedLine));
                        }
                        catch (Exception ex) //for now, not a permanent thing
                        {
                            richTextBox1.Text = $"Error, Couldnt open file{Environment.NewLine}{ex.Message}";
                        }
                        directoryView = false;
                    }
                    else
                    {
                        richTextBox1.Text = $"Error, Couldnt open file{Environment.NewLine}File {clickedLine} does not seem to exist in {currentDirectory}";
                    }
                }
            }
        }

        public void RefreshCurrentDirectoryView()
        {
            List<string> children_list = new List<string>();

            // Add drives
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                children_list.Add(drive.RootDirectory.FullName);
            }

            children_list.Add("- - - - -");

            // Add parent directory if not root directory
            if (currentDirectory != Path.GetPathRoot(currentDirectory))
            {
                string parentDirectory = Directory.GetParent(currentDirectory)?.FullName;
                children_list.Add(parentDirectory ?? "(No Parent Directory)");
            }
            else
            {
                children_list.Add("(No Parent Directory)");
            }

            children_list.Add("- - - - -");
            // Add directories and files
            try
            {
                string[] child_dirs = Directory.GetDirectories(currentDirectory);
                string[] child_files = Directory.GetFiles(currentDirectory);

                if (child_dirs.Length == 0) child_dirs = ["<No Directories>"];
                children_list.AddRange(child_dirs);

                if (child_files.Length == 0) child_files = ["<No Files>"];

                children_list.AddRange(child_files);

                //stupid i know, just comparing the starting of both variabls, but it works! and not much bugs to expect
                if (child_dirs[0] == "<No Directories>" && child_files[0] == "<No Files>")
                {
                    children_list.RemoveRange(children_list.Count - 2, 2);
                    children_list.AddRange(new List<string>() { "<No Directories and Files>" });
                }

                string[] children = children_list.ToArray();
                if (richTextBox1.Text.Split(Environment.NewLine)[0] != DriveInfo.GetDrives()[0].Name)
                {
                    ol_rtbtext = richTextBox1.Text;
                }
                richTextBox1.Text = string.Join(Environment.NewLine, children);
            }
            catch (UnauthorizedAccessException)
            {
                richTextBox1.BackColor = Color.Red;
                richTextBox1.Text = """
                    :(

                    An Unauthorised Exception Occured, The Folder Cant be Accessed

                    """;
                Task.Delay(2000).Wait();
                directoryView = false;
                currentDirectory = Directory.GetParent(currentDirectory).FullName;
                richTextBox1.Text = ol_rtbtext;
                richTextBox1.BackColor = ol_bg_color;
                richTextBox1.ForeColor = ol_fg_color;
                return;

            }
        }

        private void MainEditor_Activated(object sender, EventArgs e)
        {
            InitializeSettings();
        }
    }
}
