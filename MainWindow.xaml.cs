using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Diagnostics;
using System.Windows.Threading;
using Snippets;
using System.IO;
using Newtonsoft.Json;
using System.Windows.Media.Animation;
using System.Windows.Controls;
using Microsoft.Win32;
using System.Linq;
using System.Reflection;

namespace WpfApp_AutoPlay
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DispatcherTimer autoStartTimer = new DispatcherTimer(DispatcherPriority.Background);
        DispatcherTimer mouseRecordTimer = new DispatcherTimer(DispatcherPriority.Normal);
        DispatcherTimer setMouseTimer = new DispatcherTimer(DispatcherPriority.Normal);

        float timePaseed = 0;
        //create instance of class from other script
        MouseController mouseController = new MouseController();
        ConsoleLog consoleLog;

        //record mouse click event
        Record record = new Record();
        Stopwatch stopwatch = new Stopwatch();
        const int mouseRecordInterval = 100;
        bool isRecording = false;

        //execute mouse record
        int currentRecord = 0;
        int readXPos = 0;
        int readYPos = 0;
        int setMouseInterval = 0;
        List<Click> readClicks;

        //progress bar
        int percentage;
        double leftOffsetStart = 0;
        double leftOffsetEnd;
        double rightOffsetStart;
        double rightOffsetEnd;
        double fadeValue = 0.02D;
        int startPercentage = 0;

        //save
        string savePath = "";
        string saveName = "";
        string wholeSavePath = "";

        //load
        string loadPath = "";

        //config
        //save setting in root directory of program

        AppSetting appSetting = new AppSetting();
        string settingFolderName = "AutoClickSetting";
        string settingSaveName = "AutoClickSetting.json";
        string settingSavePath = "";
        int autoStartTimeInMilliSecond = 2000;

        //instead of main(general application), WPF launches a main window and uses MainWindow() to start up event.
        public MainWindow()
        {
            //can't put in class initialization since it will be initializated again in runtime
            for (int i = 0; i < appSetting.pathNum; i++)
            {
                appSetting.loadPath.Add("");
            }

            InitializeComponent();

            rightOffsetStart = fadeValue;
            //UI set
            consoleLog = new ConsoleLog();
            HideProgressBar();

            //load save file if no create is needed
            if (!CreateSaveSetting())
            {
                appSetting = JsonConvert.DeserializeObject<AppSetting>(File.ReadAllText(settingSavePath));
                LoadSaveSetting();
            }

            //set saved setting at start

            //record mouse per interval
            mouseRecordTimer.Tick += new EventHandler(RecordTime);
            mouseRecordTimer.Interval = new TimeSpan(0, 0, 0, 0, mouseRecordInterval);
            //set mouse position according to recorder interval and position
            setMouseTimer.Tick += new EventHandler(SetMouseTimer);

            //set template to trigger event on mouse click
            UserActivityHook activity = new UserActivityHook();
            activity.OnMouseActivity += MouseClick;

            //auto start game if available
            if ((bool)autoStartCheckBox.IsChecked)
            {
               
                //2 seconds to uncheck the auto start button
                ConsoleLog.instance.Log("AutoStart");
                autoStartTimer.Tick += new EventHandler(AutoStartGame);
                autoStartTimer.Interval = new TimeSpan(0, 0, 0, 0, autoStartTimeInMilliSecond);
                autoStartTimer.Start();
            }
            else
            {
                ConsoleLog.instance.Log("no auto start");
            }

        }

        public void AutoStartGame(object? sender, EventArgs e)
        {
            Object emptyObject = new Object();
            RoutedEventArgs emptyEvent = new RoutedEventArgs();
            StartGame(emptyObject, emptyEvent);
            autoStartTimer.Stop();
        }

        #region record session
        private void RecordAndStartGame(object sender, RoutedEventArgs e)
        {
            savePath = savePathBox.Text;
            saveName = saveFileName.Text + ".json";

            if (!Directory.Exists(savePath))
            {
                ConsoleLog.instance.Log("save path does not exist");
                return;
            }

            if (saveName == "")
            {
                saveName = "record1";
            }

            if (!isRecording)
            {
                LoadProgram();

                if (record.records.Count > 0)
                {
                    record.records.Clear();
                }
                isRecording = true;
                HideProgressBar();

                stopwatch.Start();
                mouseRecordTimer.Start();
            }
            else
            {
                isRecording = false;
                //remove last item of record if exists
                if (record.records.Count > 0)
                {
                    ConsoleLog.instance.Log("Remove last item");
                    record.records.RemoveAt(record.records.Count - 1);
                    WriteJsonFile(@$"{wholeSavePath}");
                }
                stopwatch.Stop();
                mouseRecordTimer.Stop();
            }
            ConsoleLog.instance.Log("isRecording: " + isRecording.ToString());
        }

        private void MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (isRecording)
            {
                if (e.Button.ToString() != "None" && e.Button.ToString() != "120" && e.Button.ToString() != "-120")
                {
                    //ConsoleLog.instance.Log(e.Button.ToString());
                    RecordToJsonObject(e.X, e.Y);
                    timePaseed = 0.5f;
                }
            }
        }
        public void RecordTime(object? sender, EventArgs e)
        {
            stopwatch.Stop();

            int customOffsetTime = 0;
            //don't know the logic, the delayed tick is consistent, the offset should accumulate.
            //and mouseRecordInterval will exceed 200 and customOffsetTime will be smaller than 0
            //but it doesn't happen
            if (stopwatch.ElapsedMilliseconds != 0 && stopwatch.ElapsedMilliseconds < mouseRecordInterval * 2)
            {
                if ((int)stopwatch.ElapsedMilliseconds > mouseRecordInterval)
                {
                    customOffsetTime = mouseRecordInterval - ((int)stopwatch.ElapsedMilliseconds - mouseRecordInterval);
                }
                else
                {
                    customOffsetTime = mouseRecordInterval + mouseRecordInterval - (int)stopwatch.ElapsedMilliseconds;
                }
                mouseRecordTimer.Interval = new TimeSpan(0, 0, 0, 0, customOffsetTime);
            }
            //ConsoleLog.instance.Log(stopwatch.ElapsedMilliseconds.ToString());
            stopwatch.Restart();
            timePaseed += 0.1f;
        }

        //store record into json object and write the json file
        public void RecordToJsonObject(int _xPos, int _yPos)
        {
            float _timePassed = (float)Decimal.Round((decimal)timePaseed, 3);
            Click _click = new Click((record.records.Count + 1).ToString(), _xPos, _yPos, _timePassed, 0);
            record.records.Add(_click);

            wholeSavePath = savePath + "\\" + saveName;
            WriteJsonFile(@$"{wholeSavePath}");
            ConsoleLog.instance.Log(_timePassed.ToString() + " seconds");
        }

        public void WriteJsonFile(string jsonPath)
        {
            using (StreamWriter file = File.CreateText(jsonPath))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, record.records);
            }
        }

        #endregion

        private void StartGame(object sender, RoutedEventArgs e)
        {
            if (!LoadSaveDirectory())
            {
                consoleLog.Log("no load directory");
                //path cannot be loaded
                return;
            }

            LoadProgram();

            ReadJsonFile(@$"{loadPath}");
            ShowProgressBar();
        }

        private void LoadProgram()
        {
            string programPath = loadProgramText.Text;
            if (programPath != "")
            {
                if ((bool)loadProgramCheckBox.IsChecked)
                {
                    Process _process = new Process();
                    _process.StartInfo = new ProcessStartInfo(@$"{programPath}");
                    _process.StartInfo.UseShellExecute = true;
                    _process.Start();
                    ConsoleLog.instance.Log("Start program");
                }
                else
                {
                    consoleLog.Log("not start program");
                }

            }
            else
            {
                string message = "no program path: " + programPath;
                consoleLog.Log(message);
            }
        }

        private bool LoadSaveDirectory()
        {
            //loop thorugh child and find checked box and get its load path
            for (int i = 0; i < loadStackPanel.Children.Count; i++)
            {
                if (VisualTreeHelper.GetChild(loadStackPanel, i).GetType().Equals(loadStackPanel.GetType()))
                {
                    StackPanel _stackPanelChild = GlobalVisualTreeHelper.FindVisualChild<StackPanel>(loadStackPanel, i);

                    if (_stackPanelChild != null)
                    {
                        CheckBox _checkBoxChild = GlobalVisualTreeHelper.FindVisualChild<CheckBox>(_stackPanelChild, 0);
                        if (_checkBoxChild != null)
                        {
                            if (_checkBoxChild.IsChecked == true)
                            {
                                StackPanel _checkBoxParent = GlobalVisualTreeHelper.FindVisualParent<StackPanel>(_checkBoxChild);
                                TextBox _textBox = GlobalVisualTreeHelper.FindVisualChild<TextBox>(_checkBoxParent, 1);
                                if (_textBox != null)
                                {
                                    loadPath = _textBox.Text;                                   
                                }
                            }
                        }
                    }
                }
            }

            if (!File.Exists(@$"{loadPath}"))
            {
                return false;
            }      
            return true;
        }


        #region execute record session

        #region pregress bar UI
        private void HideProgressBar()
        {
            percentageText.Visibility = Visibility.Hidden;
            ProgressBorder.Visibility = Visibility.Hidden;
        }

        private void ShowProgressBar()
        {
            SetPercentage(percentage = 0);
            percentageText.Visibility = Visibility.Visible;
            ProgressBorder.Visibility = Visibility.Visible;
        }

        private void SetPercentage(int _percentage)
        {
            percentageText.Text = String.Concat(_percentage.ToString(), "%");

            //the fill color
            leftOffsetEnd = (double)_percentage / 100;
            DoubleAnimation offsetAnimationLeft = new DoubleAnimation(leftOffsetStart, leftOffsetEnd, TimeSpan.FromSeconds(0.2));
            leftOffsetStart = leftOffsetEnd;

            //the fill color point to fade out point
            rightOffsetEnd = (double)_percentage / 100 + fadeValue;
            DoubleAnimation offsetAnimationRight = new DoubleAnimation(rightOffsetStart, rightOffsetEnd, TimeSpan.FromSeconds(0.2));
            rightOffsetStart = rightOffsetEnd;

            endGradient.BeginAnimation(GradientStop.OffsetProperty, offsetAnimationRight);
            startGradient.BeginAnimation(GradientStop.OffsetProperty, offsetAnimationLeft);
        }
        #endregion

        private void ReadJsonFile(string jsonPath)
        {
            ConsoleLog.instance.Log(jsonPath);
            readClicks = JsonConvert.DeserializeObject<List<Click>>(File.ReadAllText(jsonPath));
            currentRecord = 0;
            StartAutoClick();
        }

        private void StartAutoClick()
        {
            if (currentRecord < readClicks.Count)
            {
                readXPos = readClicks[currentRecord].xPos;
                readYPos = readClicks[currentRecord].yPos;
                //second to milisecond
                setMouseInterval = (int)Math.Round((readClicks[currentRecord].time + readClicks[currentRecord].customOffsetTime) * 1000);
                setMouseTimer.Interval = new TimeSpan(0, 0, 0, 0, setMouseInterval);
                setMouseTimer.Start();
            }
        }

        private void SetMouseTimer(object? sender, EventArgs e)
        {
            MouseController.instance.SetMousePosition(readXPos, readYPos);

            percentage = (int)Math.Round(((decimal)(currentRecord + 1)) / readClicks.Count * 100);
            SetPercentage(percentage);
            currentRecord++;
            NextClick();
        }
        private void NextClick()
        {
            if (currentRecord < readClicks.Count)
            {
                //second to milisecond
                setMouseInterval = (int)Math.Round((readClicks[currentRecord].time + readClicks[currentRecord].customOffsetTime) * 1000);
                setMouseTimer.Interval = new TimeSpan(0, 0, 0, 0, setMouseInterval);
                readXPos = readClicks[currentRecord].xPos;
                readYPos = readClicks[currentRecord].yPos;
            }
            else
            {
                ConsoleLog.instance.Log("finished excecution");
                setMouseTimer.Stop();
            }
        }

        #endregion

        private void ChooseSaveDirectory(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderBrowserDialog =
                new System.Windows.Forms.FolderBrowserDialog();
            //try to set the text block marking path
            if (Directory.Exists(savePathBox.Text))
            {
                folderBrowserDialog.InitialDirectory = savePathBox.Text;
            }
            else
            {
                //set directory to deafult, if not, keep as the last one
                if (Directory.Exists(@"C:\"))
                {
                    folderBrowserDialog.InitialDirectory = @"C:\";
                }
            }

            folderBrowserDialog.ShowDialog();

            if (folderBrowserDialog.SelectedPath != "")
            {
                savePathBox.Text = folderBrowserDialog.SelectedPath;
                appSetting.savePath = savePathBox.Text;
                UpdateSetting();
            }
        }

        private void ChooseLoadDirectory(object sender, RoutedEventArgs e)
        {
            Button? btn = sender as Button;
            if (btn == null)
            {
                ConsoleLog.instance.Log("button is null");
                return;
            }

            //get sender name
            DependencyObject dpobj = sender as DependencyObject;
            string senderName = dpobj.GetValue(FrameworkElement.NameProperty).ToString();

            OpenFileDialog openFileDialog = new OpenFileDialog();
            StackPanel stackPanel = GlobalVisualTreeHelper.FindVisualParent<StackPanel>(btn);
            if (stackPanel == null)
            {
                ConsoleLog.instance.Log("no parent ");
                return;
            }
            TextBox textBoxPath = GlobalVisualTreeHelper.FindVisualChild<TextBox>(stackPanel, 1);

            if (textBoxPath == null)
            {
                ConsoleLog.instance.Log("no parent ");
                return;
            }

            List<string> fileDirectory = textBoxPath.Text.Split("\\").ToList();
            if (fileDirectory.Count > 1)
            {
                fileDirectory.RemoveAt(fileDirectory.Count - 1);
                string directory = "";
                //int x = 0;
                for (int i = 0; i < fileDirectory.Count; i++)
                {
                    if (i != 0)
                    {
                        directory += "\\";
                    }
                    directory = directory + fileDirectory[i];
                }

                //try to set the text block marking path
                if (File.Exists(textBoxPath.Text))
                {
                    openFileDialog.InitialDirectory = directory;
                    ConsoleLog.instance.Log("find " + directory);
                }
                else
                {
                    //set directory to deafult, if not, keep as the last one
                    if (Directory.Exists(@"C:\"))
                    {
                        openFileDialog.InitialDirectory = @"C:\";
                        ConsoleLog.instance.Log("find " + @"C:\");
                    }
                }
            }
            else
            {
                //set directory to deafult, if not, keep as the last one
                if (Directory.Exists(@"C:\"))
                {
                    openFileDialog.InitialDirectory = @"C:\";
                    ConsoleLog.instance.Log("find " + @"C:\");
                }
            }

            if (senderName != "loadProgram")
            {
                openFileDialog.Filter = "json (*.json)|*.json";
            }
            else
            {
                openFileDialog.Filter = "";
            }

            if (openFileDialog.ShowDialog() == true)
            {
                //ConsoleLog.instance.Log(File.ReadAllText(openFileDialog.FileName));
                textBoxPath.Text = openFileDialog.FileName;

                switch (textBoxPath.Name)
                {
                    case "load1Text":
                        appSetting.loadPath[0] = textBoxPath.Text;
                        break;
                    case "load2Text":
                        appSetting.loadPath[1] = textBoxPath.Text;
                        break;
                    case "load3Text":
                        appSetting.loadPath[2] = textBoxPath.Text;
                        break;
                    case "load4Text":
                        appSetting.loadPath[3] = textBoxPath.Text;
                        break;
                    case "load5Text":
                        appSetting.loadPath[4] = textBoxPath.Text;
                        break;
                    case "loadProgramText":
                        appSetting.programPath = textBoxPath.Text;
                        break;
                }
                UpdateSetting();
            }
        }

        //player checked a load checkbox
        private void CheckLoad(object sender, RoutedEventArgs e)
        {
            //target checkbox
            CheckBox _checkBox = sender as CheckBox;
            if (_checkBox == null)
            {
                return;
            }

            //checkbox is false means it is the one that turns from true to false
            //no need to check other box to false
            if (_checkBox.IsChecked == false)
            {
                appSetting.loadCheckedBox = 0;
                UpdateSetting();
                return;
            }

            //get checkbox parent
            StackPanel _stackPanel = GlobalVisualTreeHelper.FindVisualParent<StackPanel>(_checkBox);
            if (_stackPanel == null)
            {
                return;
            }
            //get checkbox parent parent
            StackPanel _stackPanelParent = GlobalVisualTreeHelper.FindVisualParent<StackPanel>(_stackPanel);
            if (_stackPanelParent == null)
            {
                return;
            }

            //loop thorugh child and find every stack panel and make their checkbox child be false 
            for (int i = 0; i < _stackPanelParent.Children.Count; i++)
            {
                if (VisualTreeHelper.GetChild(_stackPanelParent, i).GetType().Equals(_stackPanelParent.GetType()))
                {
                    StackPanel _stackPanelChild = GlobalVisualTreeHelper.FindVisualChild<StackPanel>(_stackPanelParent, i);
                    
                    if (_stackPanelChild != null)
                    {
                        CheckBox _checkBoxChild = GlobalVisualTreeHelper.FindVisualChild<CheckBox>(_stackPanelChild, 0);
                        if (_checkBoxChild != null)
                        {
                            if (_checkBoxChild.IsChecked == true)
                            {
                                _checkBoxChild.IsChecked = false;
                            }
                        }
                    }
                }
            }

            //turn falsed checkbox to true at last.
            if (_checkBox.IsChecked == false)
            {
                _checkBox.IsChecked = true;
            }

            switch (_checkBox.Name)
            {

                case "load1CheckBox":
                    appSetting.loadCheckedBox = 1;
                    break;
                case "load2CheckBox":
                    appSetting.loadCheckedBox = 2;
                    break;
                case "load3CheckBox":
                    appSetting.loadCheckedBox = 3;
                    break;
                case "load4CheckBox":
                    appSetting.loadCheckedBox = 4;
                    break;
                case "load5CheckBox":
                    appSetting.loadCheckedBox = 5;
                    break;
            }
            UpdateSetting();
        }

        //create save folder and file when start game
        private bool CreateSaveSetting()
        {
            string folderPath = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\" + settingFolderName;
            settingSavePath = folderPath + "\\" + settingSaveName;

            //create folder and file if not exist
            if (!Directory.Exists(folderPath))
            {
                ConsoleLog.instance.Log("Create setting Folder");
                Directory.CreateDirectory(folderPath);
            }

            if (!File.Exists(folderPath + "\\" + settingSaveName))
            {
                using (StreamWriter file = File.CreateText(folderPath + "\\" + settingSaveName))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(file, appSetting);
                }
                appSetting = JsonConvert.DeserializeObject<AppSetting>(File.ReadAllText(settingSavePath));
                ConsoleLog.instance.Log("create default setting");
                return true;
            };

            ConsoleLog.instance.Log("setting already exists");
            return false;
        }

        private void LoadSaveSetting()
        {
            //config convenience
            autoStartTimeInMilliSecond = appSetting.autoStartTimeInMilliSecond;
            settingFolderName = appSetting.settingFolderName;
            settingSaveName = appSetting.settingSaveName;

            //app parameter
            saveFileName.Text = appSetting.saveName;
            savePathBox.Text = appSetting.savePath;
            autoStartCheckBox.IsChecked = appSetting.isAutoStartChecked;

            //assign load path
            loadProgramText.Text = appSetting.programPath;
            loadProgramCheckBox.IsChecked = appSetting.isLoadProgram;
            load1Text.Text = appSetting.loadPath[0];
            load2Text.Text = appSetting.loadPath[1];
            load3Text.Text = appSetting.loadPath[2];
            load4Text.Text = appSetting.loadPath[3];
            load5Text.Text = appSetting.loadPath[4];

            //check the load box
            if (appSetting.loadCheckedBox != 0)
            {
                switch (appSetting.loadCheckedBox)
                {
                    case 1:
                        load1CheckBox.IsChecked = true;
                        break;
                    case 2:
                        load2CheckBox.IsChecked = true;
                        break;
                    case 3:
                        load3CheckBox.IsChecked = true;
                        break;
                    case 4:
                        load4CheckBox.IsChecked = true;
                        break;
                    case 5:
                        load5CheckBox.IsChecked = true;
                        break;
                }
            }
        }

        private void UpdateSetting()
        {
            using (StreamWriter file = File.CreateText(settingSavePath))
            {                
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, appSetting);
            }
        }

        private void CheckAutoStart(object sender, RoutedEventArgs e)
        {
            appSetting.isAutoStartChecked = (bool)autoStartCheckBox.IsChecked;
            UpdateSetting();
        }

        private void CheckProgram(object sender, RoutedEventArgs e)
        {
            appSetting.isLoadProgram = (bool)loadProgramCheckBox.IsChecked;
            UpdateSetting();
        }

        private void UpdateSaveName(object sender, RoutedEventArgs e)
        {
            appSetting.saveName = saveFileName.Text;
            UpdateSetting();
        }

        //debug
        private void ClearDebugBlock(object sender, RoutedEventArgs e)
        {
            debugBox.Text = "";
        }
    }
}

# region Utilities Classes

#region AppSetting

public class AppSetting
{
    public int autoStartTimeInMilliSecond = 2000;
    public string settingFolderName = "AutoClickSetting";
    public string settingSaveName = "AutoClickSetting.json";
    public bool isAutoStartChecked;
    public string saveName = "";
    public string savePath = "";
    public bool isLoadProgram;
    public string programPath = "";
    public int loadCheckedBox = 0;
    public int pathNum = 5;
    public List<string> loadPath = new List<string>();
}

#endregion

#region mouse record json structure
public class Record
{
    public List<Click> records = new List<Click>();
}

public class Click
{
    public string order;
    public int xPos;
    public int yPos;
    public float time;
    public float customOffsetTime;

    public Click(string order, int xPos, int yPos, float time, float customOffsetTime)
    {
        this.order = order;
        this.xPos = xPos;
        this.yPos = yPos;
        this.time = time;
        this.customOffsetTime = customOffsetTime;
    }
}
#endregion

#region Static Methods for Finding element in hierarchy
public static class GlobalVisualTreeHelper
{
    public static T? FindVisualParent<T>(DependencyObject child, bool isKeepFinding = false) where T : DependencyObject
    {
        if (child == null)
        {
            //cannot get child
            return null;
        }

        DependencyObject parentObj = VisualTreeHelper.GetParent(child);

        if (parentObj == null)
        {
            //no parent
            return null;
        }

        T parent = parentObj as T;

        if (parent != null)
        {
            return parent;
        }
        else if (isKeepFinding)
        {
            return FindVisualParent<T>(child, isKeepFinding);
        }
        else
        {
            return null;
        }
    }

    public static T? FindVisualChild<T>(DependencyObject parent, int index) where T : DependencyObject
    {
        if (parent == null)
        {
            return null;
        }
        DependencyObject childObj = VisualTreeHelper.GetChild(parent, index);

        if (childObj != null)
        {
            T child = childObj as T;

            if (child != null)
            {
                return child;
            }
        }
        return null;
    }
}
#endregion

#endregion