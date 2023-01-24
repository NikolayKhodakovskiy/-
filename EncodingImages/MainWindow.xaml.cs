using Microsoft.Win32;
using System;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace EncodingImages
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string projectPath;
        private string imageTitle;
        private string imagePath;

        public MainWindow()
        {
            InitializeComponent();
            projectPath = GetProjectPath();
            imagePath = string.Empty;
        }

        private void LoadFromProject_Click(object sender, RoutedEventArgs e)
        {
            LoadImage(projectPath + "Images");
        }

        private void LoadFromResource_Click(object sender, RoutedEventArgs e)
        {
            LoadImage(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory));
        }

        public string GetProjectPath()
        {
            string path = "";
            string[] parts = Environment.CurrentDirectory.Split(@"\");
            for (int i = 0; i < parts.Length - 3; i++)
            {
                path += parts[i] + @"\";
            }
            return path;
        }

        private void LoadImage(string path)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.InitialDirectory = path;
            if (openFile.ShowDialog() == true)
            {
                imagePath = openFile.FileName;
                imageTitle = imagePath.Split(@"\").Last();
                if (!CustomImage.IsExtensionSupported(imageTitle.Split(".").Last()))
                {
                    MessageBox.Show("Формат выбранного файла не относится к изображениям\nили изображение такого формата не поддерживается", "Неверный формат", MessageBoxButton.OK);
                    EncodedImage.Source = null;
                    return;
                }
                Uri imageUri = new Uri(imagePath);
                InitialImage.Source = new BitmapImage(imageUri);
                EncodedImage.Source = null;
            }
        }
        private void EncodeTextButton_Click(object sender, RoutedEventArgs e)
        {
            if (InitialImage.Source == null)
            {
                MessageBox.Show("Сначала выберите картинку", "Отсутствие источника", MessageBoxButton.OK);
                return;
            }
            if (Message.Text.Length == 0)
            {
                MessageBox.Show("Сначала введите текст", "Неверный ввод", MessageBoxButton.OK);
                return;
            }
            Bitmap imageBitmap = CustomImage.GetImageBitmap(imagePath);
            CustomImage image = new CustomImage(imageBitmap, Message.Text, projectPath, imageTitle, BitLabel.Text);
            
            MessageBoxResult result = image.IsMessageLengthExceeded() ? MessageBox.Show("Превышена длина текста.\nЕсли продолжить, информация будет обрезана", "Ошибка сообщения", MessageBoxButton.YesNo) : MessageBoxResult.Yes;
            if (result == MessageBoxResult.Yes)
            {
                image.EncodeTextIntoImage();
                EncodedImage.Source = CustomImage.ToBitmapImage(imageBitmap);
                Message.Text = "";
            }
        }

        private void DecodeTextButton_Click(object sender, RoutedEventArgs e)
        {
            if (EncodedImage.Source == null)
            {
                MessageBox.Show("Выбранное изображение, не содержало зашифрований текст", "Отсутствие источника", MessageBoxButton.OK);
                return;
            }
            Message.Text = CustomImage.DecodeTextFromImage(projectPath + @"EncodedImages\" + imageTitle); 
        }

        private void ClearTextBox_Click(object sender, RoutedEventArgs e)
        {
            TextBox box = (TextBox)sender;
            box.Text = "";
        }

        private void ClearFields_Click(object sender, RoutedEventArgs e)
        {
            InitialImage.Source = null;
            EncodedImage.Source = null;
            BitPosition.Value = 0;
            Message.Text = "";
        }

        private void ExitWindow_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
