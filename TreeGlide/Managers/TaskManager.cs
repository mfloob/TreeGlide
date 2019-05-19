using System.Collections.Generic;
using System.Threading.Tasks;


namespace TreeGlide
{
    abstract public class TaskManager
    {
        private List<Task> taskList = new List<Task>();
        public Logger logger = MainWindow.logger;
        public abstract void OnStart();

        private async Task<T> Run<T>(T x) => await System.Threading.Tasks.Task.Run(() => x);

        public void Add(params Task[] tasks)
        {
            foreach (Task task in tasks)
                taskList.Add(task);
        }

        public async void OnTick()
        {
            foreach(Task task in this.taskList)
            {
                if (await Run(task.Validate()))
                    await Run(task.Execute());
            }
        }
    }
}
