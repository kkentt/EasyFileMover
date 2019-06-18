using System;
using System.Windows;
using System.IO;
using WinForms = System.Windows.Forms; //FolderDialog 
using System.Diagnostics;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace FileMover
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        HashSet<FileExtension> objFileExtensionList;
        List<string> checkedExtensions = new List<string>();
        CheckBox checkbox;
        private const string FromFolderDefaultText = "Choose Source Folder";
        private const string ToFolderDefaultText = "Choose Destination Folder";
        private int countSelectedExtension = 0;
        private const string ComboText = " File Extensions Selected";

        public MainWindow()
        {
            InitializeComponent();
            txt_source.Text = FromFolderDefaultText;
            txt_destination.Text = ToFolderDefaultText;
            objFileExtensionList = new HashSet<FileExtension>();
            combo_file_ext.Text = countSelectedExtension + ComboText;
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
        }

        #region From Button
        private void Btn_from_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WinForms.FolderBrowserDialog folderDialog = new WinForms.FolderBrowserDialog();
                folderDialog.ShowNewFolderButton = true;
                folderDialog.SelectedPath = System.AppDomain.CurrentDomain.BaseDirectory;
                WinForms.DialogResult result = folderDialog.ShowDialog();
                if (result == WinForms.DialogResult.OK)
                {
                    if (!String.IsNullOrWhiteSpace(folderDialog.SelectedPath))
                    {
                        if (folderDialog.SelectedPath.Trim() != txt_destination.Text.Trim())
                        {
                            txt_source.Text = folderDialog.SelectedPath.Trim();
                        }
                        else
                        {
                            MessageBox.Show("Source and Destination Folder can't be the same !");
                        }
                    }
                    BindFileExtensionToCombobox(txt_source.Text);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }
        #endregion

        #region To Button
        private void Btn_to_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WinForms.FolderBrowserDialog folderDialog = new WinForms.FolderBrowserDialog();
                folderDialog.ShowNewFolderButton = false;
                folderDialog.SelectedPath = System.AppDomain.CurrentDomain.BaseDirectory;
                WinForms.DialogResult result = folderDialog.ShowDialog();
                if (result == WinForms.DialogResult.OK)
                {
                    String sPath = folderDialog.SelectedPath;
                    if (!String.IsNullOrWhiteSpace(sPath))
                    {
                        if (sPath.Trim() != txt_source.Text.Trim())
                        {
                            txt_destination.Text = sPath.Trim();
                            DirectoryInfo folder = new DirectoryInfo(sPath);
                            if (folder.Exists)
                            {
                                foreach (FileInfo fileInfo in folder.GetFiles())
                                {
                                    String sDate = fileInfo.CreationTime.ToString("yyyy-MM-dd");
                                    Debug.WriteLine("#Debug: File: " + fileInfo.Name + " Date:" + sDate);
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show("Source and Destination Folder can't be the same !");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        #endregion

        #region Move Button
        private void Btn_move_files_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (IsValid())
                {
                    var files = GetFilesToMove(txt_source.Text.Trim(), checkedExtensions);
                    if (files != null)
                    {
                        foreach (var file in files)
                        {
                            var temp_name = Path.Combine(txt_destination.Text.Trim(), file.Name);
                            var name = GetUniqueFileName(fullFilePath:temp_name);
                            file.MoveTo(name);
                        }
                        MessageBox.Show("File Moving Done !");
                        //update extensions from  "source folder" 
                        if (!String.IsNullOrWhiteSpace(txt_source.Text))
                        {
                            BindFileExtensionToCombobox(txt_source.Text.Trim());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        #endregion 

        private void BindFileExtensionToCombobox(string folder)
        {
            //var fileExtensions = new ObservableCollection<FileExtension>(objFileExtensionList);
            //fileExtensions.Clear();
            //combo_file_ext.ItemsSource = fileExtensions;

            ResetCombobox();

            DirectoryInfo sourceFolder = new DirectoryInfo(folder);
            if (sourceFolder.Exists)
            {
                HashSet<string> file_extension_set = new HashSet<string>();
                FileExtension obj;
                if (CheckHaveFileInFolder(sourceFolder))
                {
                    foreach (FileInfo fileInfo in sourceFolder.GetFiles())
                    {
                        file_extension_set.Add(fileInfo.Extension);
                    }
                    foreach (var set in file_extension_set)
                    {
                        obj = new FileExtension();
                        obj.Name = set;
                        objFileExtensionList.Add(obj);
                    }
                    //bind file types to combo box
                    //var fileExtensions = new ObservableCollection<FileExtension>(objFileExtensionList);
                    //combo_file_ext.ItemsSource = fileExtensions;
                    combo_file_ext.ClearValue(ItemsControl.ItemsSourceProperty);
                    combo_file_ext.ItemsSource = objFileExtensionList;
                }
            }
        }

        private bool CheckHaveFileInFolder(DirectoryInfo folder)
        {
            if (folder.GetFiles().Length != 0)
            {
                return true;
            }
            else
            {
                MessageBox.Show("There is no File to move in the source Folder !");
                txt_source.Clear();
                return false;
            }
        }

        private void ResetCombobox()
        {
            checkedExtensions.Clear();
            combo_file_ext.ClearValue(ItemsControl.ItemsSourceProperty);
            objFileExtensionList.Clear();
            combo_file_ext.ItemsSource = objFileExtensionList;
            countSelectedExtension = 0;
            UpdateSelectedFileExtensionStatus(countSelectedExtension);
        }

        private bool IsValid()
        {
            bool isValidated = true;
            if (string.IsNullOrWhiteSpace(txt_source.Text))
            {
                isValidated = false;
                MessageBox.Show("Select Source folder first and make sure it is valid one !");
            }
            else if (string.IsNullOrWhiteSpace(txt_destination.Text))
            {
                isValidated = false;
                MessageBox.Show("Select Destination Folder !");
            }
            else if ((txt_source.Text.Trim() == FromFolderDefaultText) || (txt_destination.Text == ToFolderDefaultText))
            {
                isValidated = false;
                MessageBox.Show("Please choose valid Source Folder and Destination Folder ! ");
            }
            else if (checkedExtensions.Count <= 0)
            {
                isValidated = false;
                MessageBox.Show("Select at least one extension to move !");
            }
            return isValidated;
        }

        private void combo_chk_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                checkbox = e.OriginalSource as CheckBox;
                checkedExtensions.Remove(checkbox.Content.ToString());
                UpdateSelectedFileExtensionStatus(--countSelectedExtension);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void combo_chk_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                checkbox = e.OriginalSource as CheckBox;
                checkedExtensions.Add(checkbox.Content.ToString());
                UpdateSelectedFileExtensionStatus(++countSelectedExtension);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private List<FileInfo> GetFilesToMove(string sourceFolder, List<string> fileExtensions)
        {
            try
            {
                List<FileInfo> chosen_files = new List<FileInfo>();
                DirectoryInfo folder = new DirectoryInfo(sourceFolder);
                foreach (var file in folder.GetFiles())
                {
                    foreach (var extension in fileExtensions)
                    {
                        if (file.Extension == extension)
                        {
                            chosen_files.Add(file);
                        }
                    }
                }
                return chosen_files;
            }
            catch (DirectoryNotFoundException ex)
            {
                MessageBox.Show(ex.Message);
                return null;
            }
        }

        private string GetUniqueFileName(string fullFilePath)
        {
            try
            {
                int fileNo = 1;
                
                // check file already exited or not
                while (File.Exists(fullFilePath))
                {
                    string dir = Path.GetDirectoryName(fullFilePath);
                    string fileName = Path.GetFileNameWithoutExtension(fullFilePath);
                    string fileExt = Path.GetExtension(fullFilePath);
                    //if (fileName.Length > 1 &&  beforeLast == '_'  && Char.IsNumber(last))
                    int fileLength = fileName.Length;
                    char test1 = fileName[fileLength - 2];
                    char test2 = fileName[fileLength - 1];

                    if (fileName.Length > 1 && fileName[fileLength - 2] == '_' && Char.IsNumber(fileName[fileLength - 1]))
                    {
                        char last = fileName[fileLength - 1];
                        fileNo = Int32.Parse(last.ToString());
                        fileNo++;
                        fileName = ReplaceLastOccurrence(fileName, last.ToString(), fileNo.ToString());
                        fullFilePath = Path.Combine(dir, fileName + fileExt);
                    }
                    else
                    {
                        fullFilePath = Path.Combine(dir, fileName + "_" + fileNo + fileExt);
                    }
                }
                return fullFilePath;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return null;
            }
        }
        public string ReplaceLastOccurrence(string Source, string Find, string Replace)
        {
            try
            {
                int Place = Source.LastIndexOf(Find);
                string result = Source.Remove(Place, Find.Length).Insert(Place, Replace);
                return result;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return null;
            }
        }
        private void UpdateSelectedFileExtensionStatus(int noOfSelectedExtension)
        {
            try
            {
                combo_file_ext.Text = noOfSelectedExtension + ComboText;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }

    public class FileExtension
    {
        public FileExtension()
        {
            StatusChecked = false;
        }

        public string Name
        {
            get;
            set;
        }
        public bool StatusChecked
        {
            get;
            set;
        }
    }
}
