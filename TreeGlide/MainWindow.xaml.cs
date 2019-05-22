using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using MahApps.Metro.Controls;
using System.Windows.Threading;
using System.Diagnostics;
using TreeGlide.Managers;
using MahApps.Metro.Controls.Dialogs;
using System.IO;
using System.Reflection;

namespace TreeGlide
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private const string PROCESS_NAME = "Client_tos";
        private const Int32 LOCAL_BASE = 0x1505234;
        private bool processOpen;
        private bool attached;
        private bool attachedLogged;
        private bool localLogged;
        private bool running;
        public static bool localFound;
        public static MemoryManager memoryManager;
        public static LocalPlayer localPlayer;
        public static Movement movement;
        public static EntityManager entityManager;
        public static Logger logger;
        public static TimerManager timerManager;
        public static PathManager pathManager;

        #region Offsets
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            logger = new Logger(LogBox);
            logger.Log("Waiting for process...");
            timerManager = new TimerManager();            
            StartProcessCheckTimer();
            StartCoordsTimer();
        }


        private void CreateManagers()
        {
            memoryManager = new MemoryManager(PROCESS_NAME, LOCAL_BASE);
            localPlayer = new LocalPlayer(memoryManager);
            movement = new Movement(localPlayer);
            entityManager = new EntityManager(memoryManager, movement);
            pathManager = new PathManager(timerManager, localPlayer, movement, Path_LogBox);
            FillPathDropDown();
        }

        private void FillPathDropDown()
        {
            string assemblyPath = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string directory = Directory.CreateDirectory(assemblyPath + "/Paths").ToString();
            string[] files = Directory.GetFiles(directory, "*.txt");
            string[] names = new string[files.Length];            
            for (int i = 0; i < names.Length; i++)
                names[i] = System.IO.Path.GetFileName(files[i]).Substring(0, System.IO.Path.GetFileName(files[i]).Length - 4);
            this.Path_DropDown.ItemsSource = names;
        }

        private void StartProcessCheckTimer()
        {
            DispatcherTimer processCheckTimer = timerManager.CreateTimer(500, false);
            processCheckTimer.Tick += processCheckTimer_Tick;
            processCheckTimer.Start();
        }

        private void StartCoordsTimer()
        {
            DispatcherTimer coordsTimer = timerManager.CreateTimer(50, false);
            coordsTimer.Tick += coordsTimer_Tick;
            coordsTimer.Start();
        }

        private bool ProcessStatus()
        {            
            return Process.GetProcessesByName("Client_tos").FirstOrDefault() != null;
        }

        #region Set Labels
        private bool SetProcessStatusLabel(bool processOpen)
        {
            if (processOpen)
            {
                ProcessStatus_Label.Content = "Open";
                ProcessStatus_Label.Foreground = Brushes.LightGreen;
                return true;
            }
            else
            {
                ProcessStatus_Label.Content = "Null";
                ProcessStatus_Label.Foreground = Brushes.Red;
                attached = false;
                if (attachedLogged)
                {
                    logger.Log("Process not found. Detached.");
                    attachedLogged = false;
                }
                return false;
            }
        }

        private void SetLocalPlayerStatusLabel(bool localFound)
        {
            if (localFound)
            {
                LocalPlayerStatus_Label.Content = "Found";
                LocalPlayerStatus_Label.Foreground = Brushes.LightGreen;
                if (!localLogged)
                {
                    logger.Log("LocalPlayer found! Attached to process.");
                    localLogged = true;
                }
            }
            else
            {
                LocalPlayerStatus_Label.Content = "Not Found";
                LocalPlayerStatus_Label.Foreground = Brushes.Red;
                if (localLogged)
                {
                    logger.Log("No LocalPlayer found.");
                    localLogged = false;
                }
            }
        }

        private void SetCoordLabels(float? x, float? y, float? z, bool localFound)
        {
            if (localFound)
            {
                xCoord_Label.Content = x?.ToString("F4");
                yCoord_Label.Content = y?.ToString("F4");
                zCoord_Label.Content = z?.ToString("F4");
                return;
            }
            xCoord_Label.Content = "null";
            yCoord_Label.Content = "null";
            zCoord_Label.Content = "null";
        }
        #endregion        

        #region TimerTicks
        private async void processCheckTimer_Tick(object sender, EventArgs e)    //On tick checks if process is open; attaches if it is then tries to find localPlayer
        {
            this.processOpen = await System.Threading.Tasks.Task.Run(() => ProcessStatus());
            if (SetProcessStatusLabel(processOpen))
            {
                if (!attached)
                {
                    CreateManagers();
                    attached = true;
                    logger.Log("Process found! Waiting for LocalPlayer...");
                    attachedLogged = true;
                    SetLocalPlayerStatusLabel(localFound);
                }
                else
                {                    
                    localFound = localPlayer.IsFound();
                    SetLocalPlayerStatusLabel(localFound);
                }
            }
        }

        private async void coordsTimer_Tick(object sender, EventArgs e)
        {
            if (!localFound)
            {
                SetCoordLabels(null, null, null, false);
                return;
            }

            float x = await System.Threading.Tasks.Task.Run(() => localPlayer.GetX());
            float y = await System.Threading.Tasks.Task.Run(() => localPlayer.GetY());
            float z = await System.Threading.Tasks.Task.Run(() => localPlayer.GetZ());

            if (x == 0 || y == 0 || z == 0)
                Console.WriteLine(String.Format("One of these is zero: {0}, {1}, {2}", x, y, z));

            SetCoordLabels(x, y, z, true);
        }

        private void botTimer_Tick(object sender, EventArgs e, GrindBot bot)
        {
            bot.OnTick();  
        }
        #endregion

        private void Start_Button_Click(object sender, RoutedEventArgs e)
        {
            if (running)
                return;
            else if (!localFound)
            {
                logger.Log("No LocalPlayer found.");
                return;
            }
            //else if (entityManager.AttackListEmpty())
            //{
            //    logger.Log("Attack list empty.");
            //    return;
            //}
            InputManager.SetActiveWindow("Client_tos");
            GrindBot bot = new GrindBot(pathManager);
            DispatcherTimer botTimer = timerManager.CreateTimer(50, true);
            botTimer.Tick += (s, e1) => { botTimer_Tick(s, e1, bot); };
            botTimer.Start();
            running = true;
        }       

        private void RefreshEntities()
        {
            if (!localFound)
                return;
            List<int> idList = new List<int>(entityManager.GetEntities().Select(x => x.id).Distinct());
            EnemyNearby_ListBox.ItemsSource = idList;
        }

        private void Stop_Button_Click(object sender, RoutedEventArgs e)
        {
            if (!running)
                return;
            timerManager.StopTimers();
            running = false;
        }

         #region Controls
        private void LogBox_SizeChanged(object sender, SizeChangedEventArgs e) => LogScroller.ScrollToBottom();
        private void Path_LogBox_SizeChanged(object sender, SizeChangedEventArgs e) => PathLog_Scroller.ScrollToBottom();

        private void RefreshEntityList_Button_Click(object sender, RoutedEventArgs e) => RefreshEntities();

        private void AddEnemy_Button_Click(object sender, RoutedEventArgs e)
        {
            int selectedItem = Convert.ToInt32(EnemyNearby_ListBox.SelectedItem);
            if (selectedItem > 0 && !AttackEnemy_ListBox.Items.Contains(selectedItem))
            {
                AttackEnemy_ListBox.Items.Add(selectedItem);
                entityManager.AddAttackList(selectedItem);
            }
            EnemyNearby_ListBox.SelectedIndex = -1;
        }

        private void DeleteEnemy_Button_Click(object sender, RoutedEventArgs e)
        {
            int selectedIndex = AttackEnemy_ListBox.SelectedIndex;
            if (selectedIndex != -1)
            {
                AttackEnemy_ListBox.Items.RemoveAt(selectedIndex);
                entityManager.RemoveAttackList(Convert.ToInt32(AttackEnemy_ListBox.SelectedItem));
            }
        }

        private void AttackEnemy_ListBox_LostFocus(object sender, RoutedEventArgs e) => AttackEnemy_ListBox.SelectedIndex = -1;
        private void AttackEnemy_ListBox_MouseDown(object sender, MouseButtonEventArgs e)
        {
            AttackEnemy_ListBox.SelectedIndex = -1;
            EnemyNearby_ListBox.SelectedIndex = -1;
        }
        private void EnemyNearby_ListBox_LostFocus(object sender, RoutedEventArgs e) => EnemyNearby_ListBox.SelectedIndex = -1;
        private void EnemyNearby_ListBox_MouseDown(object sender, MouseButtonEventArgs e)
        {
            AttackEnemy_ListBox.SelectedIndex = -1;
            EnemyNearby_ListBox.SelectedIndex = -1;
        }
        #endregion

        private void PathNew_Button_Click(object sender, RoutedEventArgs e)
        {
            if (!localFound)
                return;
            pathManager.StartCreatePath();
            PathStop_Button.Visibility = Visibility.Visible;
        }
        private void PathStop_Button_Click(object sender, RoutedEventArgs e)
        {
            pathManager.StopCreatePath();
            PathStop_Button.Visibility = Visibility.Collapsed;
            PathSave_Button.Visibility = Visibility.Visible;
        }

        private async void PathSave_Button_Click(object sender, RoutedEventArgs e)
        {
            var result = await this.ShowInputAsync("", "Name: ");
            if (result == null)
                return;
            PathSave_Button.Visibility = Visibility.Collapsed;
            pathManager.SavePath(result);
            FillPathDropDown();
        }

        private void Path_DropDown_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (pathManager == null)
            //    return;
            pathManager.SetPath(Path_DropDown.SelectedItem.ToString());
        }
    }
}
