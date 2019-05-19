using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using System.Windows.Threading;
using System.Diagnostics;

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
        private bool localFound;
        private bool localLogged;
        public static MemoryManager memoryManager;
        public static LocalPlayer localPlayer;
        public static Movement movement;
        public static EntityManager entityManager;
        public static Logger logger;
        public List<DispatcherTimer> timerList;

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
            logger.Log("Initializing...");
            timerList = new List<DispatcherTimer>();
            DispatcherTimer processCheckTimer = Timer(500);
            processCheckTimer.Tick += processCheckTimer_Tick;
            processCheckTimer.Start();
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
                ProcessStatus_Label.Content = "Not Found";
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
                    logger.Log("LocalPlayer intitialized.");
                    localLogged = true;
                }
            }
            else
            {
                LocalPlayerStatus_Label.Content = "Not Found";
                LocalPlayerStatus_Label.Foreground = Brushes.Red;
                if (localLogged)
                {
                    logger.Log("LocalPlayer not found.");
                    localLogged = false;
                }
            }
        }
        #endregion

        private DispatcherTimer Timer(int delay)
        {
            DispatcherTimer newTimer = new DispatcherTimer();
            timerList.Add(newTimer);
            newTimer.Interval = new TimeSpan(0, 0, 0, 0, delay);
            return newTimer;
        }

        #region TimerTicks
        private async void processCheckTimer_Tick(object sender, EventArgs e)    //On tick checks if process is open, attaches if it is, then tries to find localPlayer
        {
            this.processOpen = await System.Threading.Tasks.Task.Run(() => ProcessStatus());
            if (SetProcessStatusLabel(processOpen))
            {
                if (!attached)
                {
                    memoryManager = new MemoryManager(PROCESS_NAME, LOCAL_BASE);
                    localPlayer = new LocalPlayer(memoryManager);
                    entityManager = new EntityManager(memoryManager);
                    movement = new Movement(localPlayer);
                    attached = true;
                    logger.Log("Process found. Attached.");
                    attachedLogged = true;
                }
                else
                {                    
                    localFound = localPlayer.IsFound();
                    SetLocalPlayerStatusLabel(localFound);
                }
            }
        }
        private void botTimer_Tick(object sender, EventArgs e, GrindBot bot)
        {
            bot.OnTick();  
        }
        #endregion

        private void Start_Button_Click(object sender, RoutedEventArgs e)
        {
            InputManager.SetActiveWindow("Client_tos");
            GrindBot bot = new GrindBot();
            bot.OnStart();

            DispatcherTimer botTimer = Timer(10);
            botTimer.Tick += (s, e1) => { botTimer_Tick(sender, e, bot); };
            botTimer.Start();

        }       

        private void RefreshEntities()
        {
            List<int> idList = new List<int>(entityManager.GetEntities().Select(x => x.id).Distinct());
            EnemyNearby_ListBox.ItemsSource = idList;
        }

        private void Stop_Button_Click(object sender, RoutedEventArgs e)
        {
            foreach (DispatcherTimer timer in timerList)
                timer.Stop();
        }

        #region UI Controls
        private void LogBox_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            LogScroller.ScrollToBottom();
        }

        private void RefreshEntityList_Button_Click(object sender, RoutedEventArgs e)
        {
            RefreshEntities();
        }

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

        private void EnemyNearby_ListBox_LostFocus(object sender, RoutedEventArgs e)
        {
            EnemyNearby_ListBox.SelectedIndex = -1;
        }

        private void AttackEnemy_ListBox_LostFocus(object sender, RoutedEventArgs e)
        {
            AttackEnemy_ListBox.SelectedIndex = -1;
        }

        private void AttackEnemy_ListBox_MouseDown(object sender, MouseButtonEventArgs e)
        {
            AttackEnemy_ListBox.SelectedIndex = -1;
            EnemyNearby_ListBox.SelectedIndex = -1;
        }

        private void EnemyNearby_ListBox_MouseDown(object sender, MouseButtonEventArgs e)
        {
            AttackEnemy_ListBox.SelectedIndex = -1;
            EnemyNearby_ListBox.SelectedIndex = -1;
        }

        #endregion

    }
}
