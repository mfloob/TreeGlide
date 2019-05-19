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

namespace TreeGlide
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private const string PROCESS_NAME = "Client_tos";
        private const Int32 LOCAL_BASE = 0x1505234;
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
            memoryManager = new MemoryManager(PROCESS_NAME, LOCAL_BASE);
            logger.Log("Process found!");
            localPlayer = new LocalPlayer(memoryManager);
            movement = new Movement(localPlayer);
            entityManager = new EntityManager(memoryManager);
            timerList = new List<DispatcherTimer>();
        }

        private void Start_Button_Click(object sender, RoutedEventArgs e)
        {
            InputManager.SetActiveWindow("Client_tos");
            GrindBot bot = new GrindBot();
            bot.OnStart();

            DispatcherTimer botTimer = new DispatcherTimer();
            timerList.Add(botTimer);
            botTimer.Interval = new TimeSpan(0, 0, 0, 0, 10);
            botTimer.Tick += (s, e1) => { botTimer_Tick(sender, e, bot); };
            botTimer.Start();

        }       

        private void botTimer_Tick(object sender, EventArgs e, GrindBot bot)
        {
            bot.OnTick();  
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
